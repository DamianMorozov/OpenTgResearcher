param(
    [Parameter(Mandatory = $true)][string]$ExecutablePath,
    [ValidateSet('Russian', 'English')][string]$Language = 'Russian',
    [ValidateSet('Light', 'Dark')][string]$Theme = 'Dark'
)
$ErrorActionPreference = 'Stop'
$caseDirectory = Join-Path $PSScriptRoot ('bin/StatusBar/' + [Guid]::NewGuid().ToString('N'))
$appProcess = $null
$appLog = Join-Path $env:LOCALAPPDATA ('OpenTgResearcherDesktop/current/Log-{0:yyyy-MM-dd}.txt' -f (Get-Date))
$originalLength = if (Test-Path -LiteralPath $appLog) { (Get-Item -LiteralPath $appLog).Length } else { 0 }
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes, System.Drawing, System.Windows.Forms
Add-Type @'
public static class StatusBarTestNative {
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(System.IntPtr window);
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern System.IntPtr SetThreadDpiAwarenessContext(System.IntPtr context);
}
'@
[void][StatusBarTestNative]::SetThreadDpiAwarenessContext([IntPtr](-4))

# Читаем дерево только запущенного тестом процесса
function Get-TestWindow {
    $appProcess.Refresh()
    if ($appProcess.HasExited) { throw "Desktop завершился с кодом $($appProcess.ExitCode)" }
    return [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
}

# Ожидаем элемент после загрузки страницы или перестроения всплывающей панели
function Find-TestElement([string]$id = '', [string]$name = '.*') {
    $deadline = [DateTime]::UtcNow.AddSeconds(25)
    do {
        $appProcess.Refresh()
        if ($appProcess.HasExited) { throw "Desktop завершился до появления элемента $id $name" }
        if ($appProcess.MainWindowHandle -ne [IntPtr]::Zero) {
            try {
                $elements = (Get-TestWindow).FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
                foreach ($element in $elements) {
                    if (($id -eq '' -or $element.Current.AutomationId -eq $id) -and $element.Current.Name -match $name -and
                        -not $element.Current.IsOffscreen) { return $element }
                }
            }
            catch [System.Windows.Automation.ElementNotAvailableException] { }
        }
        Start-Sleep -Milliseconds 100
    } while ([DateTime]::UtcNow -lt $deadline)
    $elements | ForEach-Object { "$($_.Current.AutomationId) | $($_.Current.ClassName) | $($_.Current.Name)" } |
        Set-Content (Join-Path $caseDirectory 'failure-ui.txt') -Encoding utf8
    throw "Не найден элемент $id $name"
}

# Вызываем штатное действие кнопки
function Invoke-TestElement($element) {
    while ($null -ne $element) {
        $pattern = $null
        if ($element.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$pattern)) { $pattern.Invoke(); return }
        if ($element.TryGetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern, [ref]$pattern)) { $pattern.Expand(); return }
        $element = [System.Windows.Automation.TreeWalker]::ControlViewWalker.GetParent($element)
    }
    throw 'Элемент не поддерживает вызов или раскрытие'
}

# Закрываем всплывающую панель стандартной клавишей
function Close-TestFlyout {
    [void][StatusBarTestNative]::SetForegroundWindow($appProcess.MainWindowHandle)
    [System.Windows.Forms.SendKeys]::SendWait('{ESC}')
    Start-Sleep -Milliseconds 150
}

# Сохраняем снимок настоящего окна для проверки отступов
function Save-TestSnapshot([string]$name) {
    [void][StatusBarTestNative]::SetForegroundWindow($appProcess.MainWindowHandle)
    Start-Sleep -Milliseconds 250
    $rect = (Get-TestWindow).Current.BoundingRectangle
    $bitmap = [Drawing.Bitmap]::new([int]$rect.Width, [int]$rect.Height)
    $graphics = [Drawing.Graphics]::FromImage($bitmap)
    try {
        $graphics.CopyFromScreen([int]$rect.Left, [int]$rect.Top, 0, 0, $bitmap.Size)
        $bitmap.Save((Join-Path $caseDirectory ($name + '.png')))
    }
    finally { $graphics.Dispose(); $bitmap.Dispose() }
}

# Проверяем одну строку кнопок и отсутствие обрезания по границам окна
function Assert-TestStatusLayout {
    $window = (Get-TestWindow).Current.BoundingRectangle
    $previous = $null
    foreach ($id in @('StatusTelegram', 'StatusStorage', 'StatusActivity', 'StatusFlood')) {
        $rect = (Find-TestElement $id).Current.BoundingRectangle
        if ($rect.Width -lt 32 -or $rect.Height -lt 32 -or $rect.Left -lt $window.Left -or $rect.Right -gt $window.Right) {
            throw "Кнопка обрезана или слишком мала: $id ($rect)"
        }
        if ($null -ne $previous -and ($rect.Left -lt $previous.Right -or [Math]::Abs($rect.Top - $previous.Top) -gt 2)) {
            throw "Кнопки перекрываются или расположены в разных строках: $id"
        }
        if ([string]::IsNullOrWhiteSpace((Find-TestElement $id).Current.Name)) { throw "Кнопка $id не имеет доступной подписи" }
        $previous = $rect
    }
}

# Читаем только журнал изолированного запуска
function Get-TestLog {
    if (-not (Test-Path -LiteralPath $appLog)) { return '' }
    $stream = [IO.File]::Open($appLog, 'Open', 'Read', 'ReadWrite')
    try {
        [void]$stream.Seek($originalLength, 'Begin')
        $reader = [IO.StreamReader]::new($stream)
        try { return $reader.ReadToEnd() } finally { $reader.Dispose() }
    }
    finally { $stream.Dispose() }
}

try {
    if (Get-Process OpenTgResearcherDesktop -ErrorAction SilentlyContinue) { throw 'Для изолированной проверки закройте Desktop' }
    New-Item -ItemType Directory -Path $caseDirectory -Force | Out-Null
    @{
        AppStorage = Join-Path $caseDirectory 'TgStorage.db'; AppSession = Join-Path $caseDirectory 'OpenTgResearcher.session'
        AppLanguage = $Language; AppTheme = $Theme; WindowWidth = '1200'; WindowHeight = '1000'; WindowX = '50'; WindowY = '50'
    } | ConvertTo-Json | Set-Content (Join-Path $caseDirectory 'Settings.json') -Encoding utf8
    $start = [Diagnostics.ProcessStartInfo]::new((Resolve-Path -LiteralPath $ExecutablePath).Path)
    $start.WorkingDirectory = Split-Path -Parent $start.FileName
    $start.UseShellExecute = $false
    $start.Environment['LocalSettingsOptions__ApplicationDataFolder'] = $caseDirectory
    $start.Environment['LocalSettingsOptions__LocalSettingsFile'] = 'Settings.json'
    $start.Environment['DOTNET_ENVIRONMENT'] = 'Production'
    $start.Environment['Logging__LogLevel__Default'] = 'Warning'
    $appProcess = [Diagnostics.Process]::Start($start)
    Invoke-TestElement (Find-TestElement '' '^(Продолжить|Continue)$')
    Assert-TestStatusLayout
    Save-TestSnapshot 'status-wide'
    foreach ($case in @(
        @('StatusTelegram', '^(Подключение к Telegram|Telegram connection)$'),
        @('StatusStorage', '^(Хранилище|Storage)$'),
        @('StatusFlood', '^(Ограничения Telegram|Telegram limits)$'),
        @('StatusActivity', '^(Сейчас нет активных сетевых операций\.|There are no active network operations\.)$')
    )) {
        Invoke-TestElement (Find-TestElement $case[0])
        [void](Find-TestElement '' $case[1])
        Save-TestSnapshot $case[0]
        Close-TestFlyout
    }
    Invoke-TestElement (Find-TestElement 'StatusActivity')
    Invoke-TestElement (Find-TestElement 'StatusOpenActivity')
    [void](Find-TestElement 'NetworkEmpty')
    Write-Output 'PASS: четыре панели подробностей, переход на сетевую активность, пустое состояние'
    $transform = (Get-TestWindow).GetCurrentPattern([System.Windows.Automation.TransformPattern]::Pattern)
    foreach ($width in @(1000, 800)) {
        $transform.Resize($width, 900)
        Start-Sleep -Milliseconds 350
        $actualWidth = (Get-TestWindow).Current.BoundingRectangle.Width
        if ([Math]::Abs($actualWidth - $width) -gt 2) { throw "Окно не приняло ширину $width (получено $actualWidth)" }
        Assert-TestStatusLayout
        Invoke-TestElement (Find-TestElement 'StatusActivity')
        [void](Find-TestElement 'StatusOpenActivity')
        Save-TestSnapshot "status-$width"
        Close-TestFlyout
    }
    [void]$appProcess.CloseMainWindow()
    if (-not $appProcess.WaitForExit(15000) -or $appProcess.ExitCode -ne 0) { throw 'Desktop не завершился штатно' }
    $newLog = Get-TestLog
    $newLog | Set-Content (Join-Path $caseDirectory 'app.log') -Encoding utf8
    if ($newLog -match 'App_UnhandledException|Exception:|Не удалось загрузить контурную иконку') { throw 'В журнале есть исключения' }
    if (-not $newLog.Contains('App closed')) { throw 'Нет записи штатного завершения' }
    Write-Output "PASS: одна строка при ширине 1200, 1000 и 800; закрытие с кодом 0, журнал без исключений ($Language, $Theme); $caseDirectory"
}
finally {
    if ($null -ne $appProcess -and -not $appProcess.HasExited) {
        [void]$appProcess.CloseMainWindow()
        [void]$appProcess.WaitForExit(15000)
    }
}
