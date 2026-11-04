param(
    [Parameter(Mandatory = $true)]
    [string]$ExecutablePath,
    [ValidateSet('Russian', 'English')]
    [string]$Language = 'Russian'
)

$ErrorActionPreference = 'Stop'
$executable = (Resolve-Path -LiteralPath $ExecutablePath).Path
$caseDirectory = Join-Path $PSScriptRoot ('bin/UpdatesPage/' + [Guid]::NewGuid().ToString('N'))
$appProcess = $null
$appLog = Join-Path $env:LOCALAPPDATA ('OpenTgResearcherDesktop/current/Log-{0:yyyy-MM-dd}.txt' -f (Get-Date))
$originalLength = if (Test-Path -LiteralPath $appLog) { (Get-Item -LiteralPath $appLog).Length } else { 0 }

# Читаем только журнал проверяемого запуска
function Get-NewAppLog {
    if (-not (Test-Path -LiteralPath $appLog)) { return '' }
    $stream = [IO.File]::Open($appLog, 'Open', 'Read', 'ReadWrite')
    try {
        [void]$stream.Seek($originalLength, 'Begin')
        $reader = [IO.StreamReader]::new($stream)
        try { return $reader.ReadToEnd() }
        finally { $reader.Dispose() }
    }
    finally { $stream.Dispose() }
}

# Находим доступный элемент только в окне запущенного тестом процесса
function Wait-AppElement([string]$namePattern) {
    $deadline = [DateTime]::UtcNow.AddSeconds(30)
    while ([DateTime]::UtcNow -lt $deadline) {
        if ($appProcess.HasExited) { throw "Desktop завершился до появления элемента: $namePattern; код $($appProcess.ExitCode)" }
        $appProcess.Refresh()
        if ($appProcess.MainWindowHandle -ne [IntPtr]::Zero) {
            $window = [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
            $elements = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
            foreach ($element in $elements) {
                if ($element.Current.Name -match $namePattern -and $element.Current.IsEnabled -and -not $element.Current.IsOffscreen) { return $element }
            }
        }
        Start-Sleep -Milliseconds 100
    }
    $elements | ForEach-Object { "$($_.Current.ClassName) | $($_.Current.Name) | $($_.Current.IsOffscreen)" } |
        Set-Content -LiteralPath (Join-Path $caseDirectory 'ui.txt') -Encoding utf8
    Save-AppSnapshot 'failure'
    throw "В окне Desktop не найден элемент: $namePattern"
}

# Выполняем штатное действие кнопки или пункта навигации
function Invoke-AppElement($element) {
    $current = $element
    while ($null -ne $current) {
        $pattern = $null
        if ($current.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$pattern)) {
            $deadline = [DateTime]::UtcNow.AddSeconds(5)
            while (-not $current.Current.IsEnabled -and [DateTime]::UtcNow -lt $deadline) { Start-Sleep -Milliseconds 50 }
            if (-not $current.Current.IsEnabled) { throw 'Кнопка не стала доступной после закрытия диалога' }
            $pattern.Invoke()
            return
        }
        if ($current.TryGetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern, [ref]$pattern)) { $pattern.Select(); return }
        $current = [System.Windows.Automation.TreeWalker]::ControlViewWalker.GetParent($current)
    }
    throw "Элемент не поддерживает вызов или выбор: $($element.Current.Name)"
}

# Находим журнал по стабильному идентификатору и проверяем доступность копирования
function Get-UpdateLogElement {
    $window = [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
    $condition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::AutomationIdProperty, 'UpdateLog')
    $element = $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
    if ($null -eq $element) { throw 'Поле журнала обновлений не найдено' }
    $value = $element.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
    if (-not $value.Current.IsReadOnly) { throw 'Журнал допускает редактирование' }
    $text = $null
    if (-not $element.TryGetCurrentPattern([System.Windows.Automation.TextPattern]::Pattern, [ref]$text)) {
        throw 'Журнал недоступен для выделения и копирования'
    }
    return $element
}

# Ожидаем наблюдаемый результат команды в настоящем поле журнала
function Wait-UpdateLog([string]$expected) {
    $deadline = [DateTime]::UtcNow.AddSeconds(15)
    do {
        $element = Get-UpdateLogElement
        $value = $element.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).Current.Value
        if ($value -match $expected) { return $value }
        if ($appProcess.HasExited) { throw 'Desktop завершился во время проверки обновлений' }
        Start-Sleep -Milliseconds 100
    } while ([DateTime]::UtcNow -lt $deadline)
    throw "В журнале отсутствует результат $expected : $value"
}

# Сохраняем снимок только тестового окна
function Save-AppSnapshot([string]$name) {
    Add-Type -AssemblyName System.Drawing
    Start-Sleep -Milliseconds 300
    $window = [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
    $rect = $window.Current.BoundingRectangle
    $bitmap = [Drawing.Bitmap]::new([int]$rect.Width, [int]$rect.Height)
    $graphics = [Drawing.Graphics]::FromImage($bitmap)
    try {
        $graphics.CopyFromScreen([int]$rect.Left, [int]$rect.Top, 0, 0, $bitmap.Size)
        $bitmap.Save((Join-Path $caseDirectory ($name + '.png')))
    }
    finally { $graphics.Dispose(); $bitmap.Dispose() }
}

try {
    if (Get-Process OpenTgResearcherDesktop -ErrorAction SilentlyContinue) { throw 'Перед селфтестом закройте запущенный Desktop' }
    Add-Type -AssemblyName UIAutomationClient
    Add-Type -AssemblyName UIAutomationTypes
    New-Item -ItemType Directory -Path $caseDirectory -Force | Out-Null
    @{
        AppStorage = Join-Path $caseDirectory 'TgStorage.db'
        AppSession = Join-Path $caseDirectory 'OpenTgResearcher.session'
        AppLanguage = $Language
        AppTheme = 'Default'
    } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $caseDirectory 'Settings.json') -Encoding utf8
    $start = [Diagnostics.ProcessStartInfo]::new($executable)
    $start.WorkingDirectory = Split-Path -Parent $executable
    $start.UseShellExecute = $false
    $start.Environment['LocalSettingsOptions__ApplicationDataFolder'] = $caseDirectory
    $start.Environment['LocalSettingsOptions__LocalSettingsFile'] = 'Settings.json'
    $start.Environment['DOTNET_ENVIRONMENT'] = 'Production'
    $start.Environment['Logging__LogLevel__Default'] = 'Warning'
    $appProcess = [Diagnostics.Process]::Start($start)
    Invoke-AppElement (Wait-AppElement '^(Продолжить|Continue)$')
    $window = [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
    $window.GetCurrentPattern([System.Windows.Automation.WindowPattern]::Pattern).SetWindowVisualState(
        [System.Windows.Automation.WindowVisualState]::Normal)
    $window.GetCurrentPattern([System.Windows.Automation.TransformPattern]::Pattern).Resize(1280, 900)
    $settingsCaption = if ($Language -eq 'Russian') { 'Настройки' } else { 'Settings' }
    $settings = Wait-AppElement ('^' + [regex]::Escape($settingsCaption) + '$')
    Save-AppSnapshot 'home'
    Invoke-AppElement $settings
    [void](Wait-AppElement '^(Язык|Language)$')
    Invoke-AppElement (Wait-AppElement '^(Обновление|Update)$')
    $releaseCaption = if ($Language -eq 'Russian') { 'Проверка наличия обновлений релиза' } else { 'Checking for release updates' }
    $previewCaption = if ($Language -eq 'Russian') { 'Проверка обновлений предварительного просмотра' } else { 'Checking for preview updates' }
    $release = Wait-AppElement ('^' + [regex]::Escape($releaseCaption) + '$')
    [void](Wait-AppElement ('^' + [regex]::Escape($previewCaption) + '$'))
    [void](Get-UpdateLogElement)
    Invoke-AppElement $release
    Invoke-AppElement (Wait-AppElement '^(Отмена|Cancel)$')
    if ((Get-UpdateLogElement).GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).Current.Value.Length -ne 0) {
        throw 'Отмена подтверждения изменила журнал обновлений'
    }
    Invoke-AppElement (Wait-AppElement ('^' + [regex]::Escape($releaseCaption) + '$'))
    Invoke-AppElement (Wait-AppElement '^(Да|Yes)$')
    $log = Wait-UpdateLog 'Setup\.exe'
    $expected = if ($Language -eq 'Russian') { 'Встроенное обновление доступно' } else { 'Built-in updates are available' }
    if ($log -notmatch $expected) { throw "Инструкция запуска без установки не переведена: $log" }
    Invoke-AppElement (Wait-AppElement ('^' + [regex]::Escape($previewCaption) + '$'))
    Invoke-AppElement (Wait-AppElement '^(Да|Yes)$')
    $log = Wait-UpdateLog 'Setup\.exe'
    [void](Wait-AppElement ('^' + [regex]::Escape($releaseCaption) + '$'))
    Save-AppSnapshot 'updates'
    $log | Set-Content -LiteralPath (Join-Path $caseDirectory 'update-log.txt') -Encoding utf8
    $newLog = Get-NewAppLog
    if ($newLog -notmatch 'TgUpdateViewModel.*Page loaded') { throw 'Нет подтверждения загрузки страницы обновлений' }
    if ($newLog -match 'App_UnhandledException|COMException|System.InvalidOperationException') { throw 'В журнале найдено исключение' }
    if (-not $appProcess.CloseMainWindow() -or -not $appProcess.WaitForExit(15000)) { throw 'Desktop не завершил закрытие окна' }
    if ($appProcess.ExitCode -ne 0) { throw "Desktop завершился с кодом $($appProcess.ExitCode)" }
    Write-Output "PASS: настройки и обновления ($Language), диалоги, журнал, инструкция установки, завершение с кодом 0; $caseDirectory"
}
catch {
    $_.ScriptStackTrace | Write-Output
    if ($appProcess -and -not $appProcess.HasExited) { Save-AppSnapshot 'failure' }
    throw
}
finally {
    if (Test-Path -LiteralPath $caseDirectory) {
        Get-NewAppLog | Set-Content -LiteralPath (Join-Path $caseDirectory 'page.log') -Encoding utf8
    }
    if ($appProcess -and -not $appProcess.HasExited) {
        [void]$appProcess.CloseMainWindow()
        if (-not $appProcess.WaitForExit(5000)) { Stop-Process -Id $appProcess.Id -Force }
    }
    if ($appProcess) { $appProcess.Dispose() }
}
