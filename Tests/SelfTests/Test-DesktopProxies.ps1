param(
    [Parameter(Mandatory = $true)]
    [string]$ExecutablePath,
    [ValidateSet('Russian', 'English')]
    [string]$Language = 'Russian',
    [switch]$NetworkLogo,
    [ValidateSet('Default', 'Light', 'Dark')]
    [string]$Theme = 'Default'
)

$ErrorActionPreference = 'Stop'
$executable = (Resolve-Path -LiteralPath $ExecutablePath).Path
$caseDirectory = Join-Path $PSScriptRoot ('bin/ProxiesPage/' + [Guid]::NewGuid().ToString('N'))
$appProcess = $null
$testListener = $null
$testClients = @()
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

# Читаем значок настоящего окна, который Windows использует для панели задач
function Test-WindowIcon {
    $handle = [NetworkLogoNative]::SendMessage($appProcess.MainWindowHandle, 0x7F, [IntPtr]1, [IntPtr]::Zero)
    if ($handle -eq [IntPtr]::Zero) { throw 'Окно не получило значок OTR' }
    $icon = [Drawing.Icon]::FromHandle($handle)
    $expected = [Drawing.Icon]::new((Join-Path (Split-Path $executable) 'Assets/Otr.ico'), 32, 32)
    $actualBitmap = [Drawing.Bitmap]::new(32, 32)
    $expectedBitmap = [Drawing.Bitmap]::new(32, 32)
    $actualGraphics = [Drawing.Graphics]::FromImage($actualBitmap)
    $expectedGraphics = [Drawing.Graphics]::FromImage($expectedBitmap)
    try {
        $actualGraphics.DrawIcon($icon, [Drawing.Rectangle]::new(0, 0, 32, 32))
        $expectedGraphics.DrawIcon($expected, [Drawing.Rectangle]::new(0, 0, 32, 32))
        $difference = 0
        for ($y = 0; $y -lt 32; $y++) {
            for ($x = 0; $x -lt 32; $x++) {
                if ($actualBitmap.GetPixel($x, $y) -ne $expectedBitmap.GetPixel($x, $y)) { $difference++ }
            }
        }
        $actualBitmap.Save((Join-Path $caseDirectory 'window-icon.png'))
        if ($difference -gt 30) { throw "Значок окна отличается от OTR: $difference пикселей" }
    }
    finally {
        $actualGraphics.Dispose(); $expectedGraphics.Dispose(); $actualBitmap.Dispose(); $expectedBitmap.Dispose(); $expected.Dispose()
    }
}

# Сохраняем только фактически нарисованную область верхнего логотипа
function Get-NetworkLogoFrame([string]$name) {
    $window = [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
    $condition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::AutomationIdProperty, 'ToolbarLogo')
    $rect = $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition).Current.BoundingRectangle
    $bitmap = [Drawing.Bitmap]::new([int]$rect.Width, [int]$rect.Height)
    $graphics = [Drawing.Graphics]::FromImage($bitmap)
    $stream = [IO.MemoryStream]::new()
    try {
        $graphics.CopyFromScreen([int]$rect.Left, [int]$rect.Top, 0, 0, $bitmap.Size)
        $bitmap.Save((Join-Path $caseDirectory ($name + '.png')))
        $bitmap.Save($stream, [Drawing.Imaging.ImageFormat]::Png)
        return [Convert]::ToBase64String($stream.ToArray())
    }
    finally { $stream.Dispose(); $graphics.Dispose(); $bitmap.Dispose() }
}

# Настоящая TCP-проверка ждёт заполненную очередь loopback-сервера, не обращаясь к Telegram
function Test-NetworkLogo {
    [void][NetworkLogoNative]::SetForegroundWindow($appProcess.MainWindowHandle)
    Test-WindowIcon
    Start-Sleep -Milliseconds 300
    $idle = Get-NetworkLogoFrame 'network-idle'
    Invoke-AppElement (Wait-AppElement '^(Проверить все прокси|Check all proxies)$')
    $frames = @()
    for ($index = 0; $index -lt 12; $index++) {
        Start-Sleep -Milliseconds 180
        $frames += Get-NetworkLogoFrame ('network-active-{0:00}' -f $index)
    }
    if (@($frames | Select-Object -Unique).Count -lt 3 -or @($frames | Where-Object { $_ -ne $idle }).Count -lt 2) {
        throw 'Сетевая проверка не запустила видимое движение полосы в логотипе'
    }
    [void](Wait-AppElement '^(Проверить все прокси|Check all proxies)$')
    Start-Sleep -Milliseconds 300
    $stopped = Get-NetworkLogoFrame 'network-stopped'
    Start-Sleep -Milliseconds 400
    $stoppedAgain = Get-NetworkLogoFrame 'network-stopped-again'
    if ($stopped -ne $stoppedAgain -or $stopped -ne $idle) { throw 'После окончания сети полоса не исчезла' }
    Write-Output "PASS: значок окна OTR, движение сканирования во время TCP-проверки и остановка ($Language, $Theme)"
}

try {
    if (Get-Process OpenTgResearcherDesktop -ErrorAction SilentlyContinue) { throw 'Перед селфтестом закройте запущенный Desktop' }
    Add-Type -AssemblyName UIAutomationClient
    Add-Type -AssemblyName UIAutomationTypes
    if ($NetworkLogo) {
        Add-Type -AssemblyName System.Drawing
        Add-Type -TypeDefinition @'
public static class NetworkLogoNative
{
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern System.IntPtr SendMessage(System.IntPtr window, uint message, System.IntPtr first, System.IntPtr second);
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(System.IntPtr window);
}
'@
    }
    New-Item -ItemType Directory -Path $caseDirectory -Force | Out-Null
    @{
        AppStorage = Join-Path $caseDirectory 'TgStorage.db'
        AppSession = Join-Path $caseDirectory 'OpenTgResearcher.session'
        AppLanguage = $Language
        AppTheme = $Theme
    } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $caseDirectory 'Settings.json') -Encoding utf8
    $start = [Diagnostics.ProcessStartInfo]::new($executable)
    $start.WorkingDirectory = Split-Path -Parent $executable
    $start.UseShellExecute = $false
    $start.Environment['LocalSettingsOptions__ApplicationDataFolder'] = $caseDirectory
    $start.Environment['LocalSettingsOptions__LocalSettingsFile'] = 'Settings.json'
    $start.Environment['DOTNET_ENVIRONMENT'] = 'Production'
    $start.Environment['Logging__LogLevel__Default'] = 'Warning'
    $appProcess = [Diagnostics.Process]::Start($start)
    Invoke-AppElement (Wait-AppElement 'Продолжить|Continue')
    if ($NetworkLogo) {
        $testListener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 0)
        $testListener.Start(1)
        $port = $testListener.LocalEndpoint.Port
        for ($index = 0; $index -lt 32; $index++) {
            $client = [Net.Sockets.TcpClient]::new()
            $testClients += $client
            [void]$client.ConnectAsync([Net.IPAddress]::Loopback, $port)
        }
        & dotnet run --project (Join-Path $PSScriptRoot 'Proxies/Proxies.SelfTest.csproj') -c Debug -p:Platform=x64 --no-build -- `
            --seed-network-proxy (Join-Path $caseDirectory 'TgStorage.db') $port
        if ($LASTEXITCODE -ne 0) { throw 'Не удалось подготовить изолированную БД прокси' }
    }
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
    if ($NetworkLogo) { Test-NetworkLogo }
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
    foreach ($client in $testClients) { $client.Dispose() }
    if ($testListener) { $testListener.Stop() }
    if (Test-Path -LiteralPath $caseDirectory) {
        Get-NewAppLog | Set-Content -LiteralPath (Join-Path $caseDirectory 'page.log') -Encoding utf8
    }
    if ($appProcess -and -not $appProcess.HasExited) {
        [void]$appProcess.CloseMainWindow()
        if (-not $appProcess.WaitForExit(5000)) { Stop-Process -Id $appProcess.Id -Force }
    }
    if ($appProcess) { $appProcess.Dispose() }
}
