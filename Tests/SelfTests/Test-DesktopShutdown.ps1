param(
    [Parameter(Mandatory = $true)]
    [string]$PackageFullName,
    [ValidateRange(0, 60)]
    [int]$CloseDelaySeconds = 0,
    [string]$ReadyPattern = 'License loading|Загрузка лицензии',
    [switch]$CaptureManagedExceptions,
    [switch]$OpenMainPage,
    [string]$DebuggerPath = 'C:\Program Files (x86)\Windows Kits\10\Debuggers\x64\cdb.exe'
)

$ErrorActionPreference = 'Stop'
$outputDirectory = Join-Path $PSScriptRoot 'bin\DesktopShutdown'
$debuggerLog = Join-Path $outputDirectory 'debugger.log'
$debuggerScript = Join-Path $outputDirectory 'debugger.cmd'
$appLog = Join-Path $env:LOCALAPPDATA ('OpenTgResearcherDesktop\current\Log-{0:yyyy-MM-dd}.txt' -f (Get-Date))
$debuggerProcess = $null
$appProcess = $null

try {
    if (Get-Process OpenTgResearcherDesktop -ErrorAction SilentlyContinue | Where-Object { $_.StartTime }) {
        throw 'Перед селфтестом закройте запущенные экземпляры Desktop'
    }
    if (-not (Test-Path -LiteralPath $DebuggerPath)) {
        throw 'Не найден консольный отладчик CDB из Windows SDK'
    }

    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
    $originalLogLength = if (Test-Path -LiteralPath $appLog) { (Get-Item -LiteralPath $appLog).Length } else { 0 }
    $commands = @(
        'sxe -c ".echo SHUTDOWN_SELFTEST_EXCEPTION; .exr -1; kb; !analyze -v; q" 0xc000027b'
        'sxe -c ".echo SHUTDOWN_SELFTEST_EXCEPTION; .exr -1; kb; q" av'
        'sxe -c ".lastevent; q" epr'
    )
    if ($CaptureManagedExceptions) {
        $commands += 'sxe -c "!sos.pe; !sos.clrstack; g" clr'
    }
    $commands += 'g'
    $commands | Set-Content -LiteralPath $debuggerScript
    $arguments = '-o -y . -logo "{0}" -cf "{1}" -plmPackage {2} -plmApp App' -f $debuggerLog, $debuggerScript, $PackageFullName
    $debuggerProcess = Start-Process -FilePath $DebuggerPath -ArgumentList $arguments -WindowStyle Hidden -PassThru `
        -RedirectStandardOutput (Join-Path $outputDirectory 'stdout.txt') -RedirectStandardError (Join-Path $outputDirectory 'stderr.txt')

    # Ждём загрузку приложения, проверяя только новые строки журнала
    $deadline = [DateTime]::UtcNow.AddSeconds(45)
    $isLoaded = $false
    while ([DateTime]::UtcNow -lt $deadline) {
        $appProcess = Get-Process OpenTgResearcherDesktop -ErrorAction SilentlyContinue | Where-Object { $_.StartTime } | Select-Object -First 1
        if (Test-Path -LiteralPath $appLog) {
            $logStream = [System.IO.File]::Open($appLog, 'Open', 'Read', 'ReadWrite')
            try {
                [void]$logStream.Seek($originalLogLength, 'Begin')
                $logReader = [System.IO.StreamReader]::new($logStream)
                $newLog = $logReader.ReadToEnd()
                $logReader.Dispose()
            }
            finally { $logStream.Dispose() }
            if ($appProcess -and $newLog -match $ReadyPattern) {
                $isLoaded = $true
                break
            }
        }
        if ($debuggerProcess.HasExited) { break }
        Start-Sleep -Milliseconds 5
    }
    if (-not $isLoaded) { throw "Не подтверждена загрузка Desktop. Диагностика: $debuggerLog" }

    if ($OpenMainPage) {
        # Открываем главную страницу штатной кнопкой заставки через UI Automation
        Add-Type -AssemblyName UIAutomationClient
        Add-Type -AssemblyName UIAutomationTypes
        $deadline = [DateTime]::UtcNow.AddSeconds(15)
        $continueButton = $null
        while ([DateTime]::UtcNow -lt $deadline -and -not $continueButton) {
            $appProcess.Refresh()
            $window = [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
            if ($window) {
                $condition = [System.Windows.Automation.PropertyCondition]::new(
                    [System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::Button)
                $buttons = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $condition)
                foreach ($button in $buttons) {
                    $texts = $button.FindAll([System.Windows.Automation.TreeScope]::Descendants,
                        [System.Windows.Automation.Condition]::TrueCondition)
                    $buttonText = ($texts | ForEach-Object { $_.Current.Name }) -join ' '
                    if (($button.Current.Name + ' ' + $buttonText) -match 'Продолжить|Continue' -and
                        $button.Current.IsEnabled -and -not $button.Current.IsOffscreen) {
                        $continueButton = $button
                        break
                    }
                }
            }
            if (-not $continueButton) { Start-Sleep -Milliseconds 50 }
        }
        if ($continueButton) {
            $invoke = $continueButton.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
            $invoke.Invoke()
        }
        else { throw 'Не найдена кнопка перехода с заставки на главную страницу' }
        $deadline = [DateTime]::UtcNow.AddSeconds(10)
        do {
            $logStream = [System.IO.File]::Open($appLog, 'Open', 'Read', 'ReadWrite')
            [void]$logStream.Seek($originalLogLength, 'Begin')
            $logReader = [System.IO.StreamReader]::new($logStream)
            $mainPageLog = $logReader.ReadToEnd()
            $logReader.Dispose()
            if ($mainPageLog -match 'TgMainViewModel \| Page loaded') { break }
            Start-Sleep -Milliseconds 5
        } while ([DateTime]::UtcNow -lt $deadline -and -not $debuggerProcess.HasExited)
        if ($mainPageLog -notmatch 'TgMainViewModel \| Page loaded') {
            throw 'Не подтверждён переход с заставки на главную страницу'
        }
        Write-Output 'Подтверждена загрузка главной страницы'
    }

    Start-Sleep -Seconds $CloseDelaySeconds
    $appProcess.Refresh()
    if ($OpenMainPage) {
        $window = [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
        $closeCondition = [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::NameProperty, 'Close')
        $closeButton = $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $closeCondition)
        if (-not $closeButton) { throw 'Не найдена кнопка закрытия окна' }
        $closeButton.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
    }
    elseif (-not $appProcess.CloseMainWindow()) { throw 'Не удалось отправить запрос закрытия главного окна' }
    if (-not $debuggerProcess.WaitForExit(30000)) { throw "Закрытие Desktop не завершилось. Диагностика: $debuggerLog" }

    $debugOutput = Get-Content -LiteralPath $debuggerLog -Raw
    if ($debugOutput -match '(?m)^SHUTDOWN_SELFTEST_EXCEPTION\s*$|HResult: 8000ffff' -or $debugOutput -notmatch 'Exit process [^\r\n]*code 0\b') {
        throw "Сбой закрытия Desktop. Диагностика: $debuggerLog"
    }
    if ($CaptureManagedExceptions -and $debugOutput -match
        '(?s)Exception type:\s+System.ObjectDisposedException(?:(?!Exception object:).)*TgHardwareResourceMonitoringService') {
        throw "Мониторинг обращается к освобождённым ресурсам при закрытии. Диагностика: $debuggerLog"
    }
    Write-Output "PASS: Desktop загрузился и закрылся с кодом 0. Диагностика: $debuggerLog"
}
finally {
    # Завершаем только процессы, запущенные этим селфтестом
    if ($debuggerProcess -and -not $debuggerProcess.HasExited) { Stop-Process -Id $debuggerProcess.Id -Force }
    if ($appProcess -and -not $appProcess.HasExited) { Stop-Process -Id $appProcess.Id -Force }
}
