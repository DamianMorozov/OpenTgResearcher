param(
    [Parameter(Mandatory = $true)]
    [string]$ExecutablePath,
    [ValidateSet('Russian', 'English')]
    [string]$Language = 'Russian'
)

$ErrorActionPreference = 'Stop'
$executable = (Resolve-Path -LiteralPath $ExecutablePath).Path
$caseDirectory = Join-Path $PSScriptRoot ('bin/ProxiesPage/' + [Guid]::NewGuid().ToString('N'))
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
    throw "В окне Desktop не найден элемент: $namePattern"
}

# Выполняем штатное действие кнопки или пункта навигации
function Invoke-AppElement($element) {
    $current = $element
    while ($null -ne $current) {
        $pattern = $null
        if ($current.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$pattern)) { $pattern.Invoke(); return }
        if ($current.TryGetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern, [ref]$pattern)) { $pattern.Select(); return }
        $current = [System.Windows.Automation.TreeWalker]::ControlViewWalker.GetParent($current)
    }
    throw "Элемент не поддерживает вызов или выбор: $($element.Current.Name)"
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
    $appProcess = [Diagnostics.Process]::Start($start)
    Invoke-AppElement (Wait-AppElement 'Продолжить|Continue')
    Invoke-AppElement (Wait-AppElement '^(Прокси|Proxies)$')

    # Проверяем перевод подписи кнопки в настоящем окне
    $checkAllCaption = if ($Language -eq 'Russian') { 'Проверить все прокси' } else { 'Check all proxies' }
    $checkAllElement = Wait-AppElement ('^' + [regex]::Escape($checkAllCaption) + '$')
    $checkAllElement.Current.Name | Set-Content -LiteralPath (Join-Path $caseDirectory 'check-all-button.txt') -Encoding utf8

    # Проверяем создание настоящих DataGridColumnHeader и подписи, отображаемые их шаблонами
    $expected = if ($Language -eq 'Russian') { @('Тип', 'Сервер', 'Порт', 'Пользователь', 'Пароль', 'Доступность TCP') }
        else { @('Type', 'Server', 'Port', 'User', 'Password', 'TCP availability') }
    $deadline = [DateTime]::UtcNow.AddSeconds(15)
    $ready = $false
    while ([DateTime]::UtcNow -lt $deadline) {
        if ($appProcess.HasExited) { throw "Страница прокси завершила процесс: код $($appProcess.ExitCode)" }
        $window = [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
        $condition = [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::HeaderItem)
        $headers = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $condition)
        $names = @()
        foreach ($header in $headers) {
            $names += $header.Current.Name
            $names += $header.FindAll([System.Windows.Automation.TreeScope]::Descendants,
                [System.Windows.Automation.Condition]::TrueCondition) | ForEach-Object { $_.Current.Name }
        }
        if ($headers.Count -eq 6 -and @($expected | Where-Object { $_ -notin $names }).Count -eq 0) { $ready = $true; break }
        Start-Sleep -Milliseconds 100
    }
    if (-not $ready) { throw "Не подтверждены все заголовки таблицы: $($names -join ', ')" }
    $names | Set-Content -LiteralPath (Join-Path $caseDirectory 'headers.txt') -Encoding utf8
    if ($appProcess.WaitForExit(2000)) { throw "Desktop вылетел после создания заголовков: код $($appProcess.ExitCode)" }
    $log = Get-NewAppLog
    $log | Set-Content -LiteralPath (Join-Path $caseDirectory 'page.log') -Encoding utf8
    if ($log -notmatch 'TgProxiesViewModel.*Page loaded') { throw 'Журнал не подтвердил загрузку страницы прокси' }
    if ($log -match 'Content does not support UIElement|App_UnhandledException') { throw 'В журнале найдено необработанное исключение' }
    if (-not $appProcess.CloseMainWindow() -or -not $appProcess.WaitForExit(15000)) { throw 'Desktop не завершил закрытие окна' }
    if ($appProcess.ExitCode -ne 0) { throw "Desktop закрылся с ошибкой: код $($appProcess.ExitCode)" }
    Write-Output "PASS: страница прокси ($Language), кнопка '$checkAllCaption', 6 заголовков, без исключений, закрытие с кодом 0; $caseDirectory"
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
