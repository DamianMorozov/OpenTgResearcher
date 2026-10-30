param(
    [ValidateRange(0, 60)][int]$CloseDelaySeconds = 2,
    [ValidatePattern('^[0-9A-Fa-f]{64}$')][string]$ExpectedXamlSha256,
    [switch]$StopFailedRun,
    [switch]$StatusOnly
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class ShutdownIdeProbe {
    [DllImport("oleaut32.dll", PreserveSig=false)] private static extern void GetActiveObject(ref Guid clsid, IntPtr reserved,
        [MarshalAs(UnmanagedType.IUnknown)] out object instance);
    [DllImport("ole32.dll", CharSet=CharSet.Unicode, PreserveSig=false)] private static extern void CLSIDFromProgID(string name, out Guid clsid);
    public static object Read(string name) { CLSIDFromProgID(name, out var clsid); GetActiveObject(ref clsid, IntPtr.Zero, out var instance); return instance; }
}
'@

# Запускаем только открытое решение проекта и сохраняем состояние остановленного отладчика для диагностики
$ide = [ShutdownIdeProbe]::Read('VisualStudio.DTE.18.0')
$solutionPath = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../OpenTgResearcher.slnx'))
$designMode = 1
$breakMode = 2
$runMode = 3
if ($StatusOnly) {
    Write-Output ('Solution=' + $ide.Solution.FullName + ' Debugger=' + $ide.Debugger.CurrentMode + ' BreakReason=' + $ide.Debugger.LastBreakReason)
    exit 0
}
if ($StopFailedRun) {
    if ($ide.Solution.FullName -eq $solutionPath -and $ide.Debugger.CurrentMode -ne $designMode) { $ide.Debugger.Stop($false) }
    exit 0
}
if ($ide.Solution.FullName -ne $solutionPath -or $ide.Debugger.CurrentMode -ne $designMode) { throw 'Ожидается открытое решение проекта без активной отладки' }
if (Get-Process OpenTgResearcherDesktop -ErrorAction SilentlyContinue | Where-Object { $_.StartTime }) {
    throw 'Перед селфтестом закройте запущенные экземпляры Desktop'
}
$outputDirectory = Join-Path $PSScriptRoot 'bin/DesktopShutdownVs'
New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
$appLog = Join-Path $env:LOCALAPPDATA ('OpenTgResearcherDesktop/current/Log-{0:yyyy-MM-dd}.txt' -f (Get-Date))
$originalLogLength = if (Test-Path -LiteralPath $appLog) { (Get-Item -LiteralPath $appLog).Length } else { 0 }
$ideProcess = Get-Process devenv | Where-Object { $_.MainWindowHandle -ne 0 } | Select-Object -First 1
$ideWindow = [System.Windows.Automation.AutomationElement]::FromHandle($ideProcess.MainWindowHandle)
$startCondition = [System.Windows.Automation.AndCondition]::new(
    [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::NameProperty, 'Debug Target'),
    [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::SplitButton))
$startButton = $ideWindow.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $startCondition)
if (-not $startButton -or -not $startButton.Current.IsEnabled) { throw 'Не найдена доступная кнопка запуска Desktop в VS' }
if ($startButton.Current.HelpText -notmatch '^OpenTgResearcherDesktop \(') { throw 'В VS выбран другой профиль запуска' }
$startButton.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
$deadline = [DateTime]::UtcNow.AddSeconds(60)
$appProcess = $null
while ([DateTime]::UtcNow -lt $deadline) {
    $appProcess = Get-Process OpenTgResearcherDesktop -ErrorAction SilentlyContinue | Where-Object { $_.StartTime } | Select-Object -First 1
    if ($appProcess -and $appProcess.MainWindowHandle -ne 0) { break }
    Start-Sleep -Milliseconds 100
}
if (-not $appProcess -or $appProcess.MainWindowHandle -eq 0) { throw 'Visual Studio не запустила окно Desktop' }
$null = $appProcess.Handle
Write-Output ('Запущен Desktop под VS: ' + $appProcess.Id)
if ($ide.Debugger.CurrentMode -ne $runMode) { throw 'Desktop запущен без работающего отладчика VS' }
$xamlModule = $appProcess.Modules | Where-Object { $_.ModuleName -ieq 'Microsoft.ui.xaml.dll' } | Select-Object -First 1
if (-not $xamlModule) { throw 'Не найдена загруженная библиотека WinUI' }
$xamlHash = (Get-FileHash -LiteralPath $xamlModule.FileName -Algorithm SHA256).Hash
Write-Output ('WinUI SHA256: ' + $xamlHash + ' | ' + $xamlModule.FileName)
if ($ExpectedXamlSha256 -and $xamlHash -ne $ExpectedXamlSha256) { throw 'VS загрузила другую сборку WinUI' }

# Находим кнопку заставки по её текстовому содержимому
$continueButton = $null
$deadline = [DateTime]::UtcNow.AddSeconds(30)
while ([DateTime]::UtcNow -lt $deadline -and -not $continueButton) {
    $appProcess.Refresh()
    $window = [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
    if ($window) {
        $condition = [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::Button)
        $buttons = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $condition)
        foreach ($button in $buttons) {
            $texts = $button.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
            $buttonText = ($texts | ForEach-Object { $_.Current.Name }) -join ' '
            if (($button.Current.Name + ' ' + $buttonText) -match 'Продолжить|Continue' -and
                $button.Current.IsEnabled -and -not $button.Current.IsOffscreen) { $continueButton = $button; break }
        }
    }
    if (-not $continueButton) { Start-Sleep -Milliseconds 50 }
}
if (-not $continueButton) { throw 'Не найдена кнопка перехода с заставки' }
$continueButton.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
$deadline = [DateTime]::UtcNow.AddSeconds(10)
do {
    $logStream = [IO.File]::Open($appLog, 'Open', 'Read', 'ReadWrite')
    try {
        [void]$logStream.Seek($originalLogLength, 'Begin')
        $logReader = [IO.StreamReader]::new($logStream)
        $newLog = $logReader.ReadToEnd()
        $logReader.Dispose()
    }
    finally { $logStream.Dispose() }
    if ($newLog -match 'TgMainViewModel \| Page loaded') { break }
    Start-Sleep -Milliseconds 50
} while ([DateTime]::UtcNow -lt $deadline)
if ($newLog -notmatch 'TgMainViewModel \| Page loaded') { throw 'Не подтверждена загрузка главной страницы' }
Write-Output 'Подтверждена загрузка главной страницы'
Start-Sleep -Seconds $CloseDelaySeconds
$appProcess.Refresh()
$window = [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
$closeCondition = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::NameProperty, 'Close')
$closeButton = $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $closeCondition)
if (-not $closeButton) { throw 'Не найдена кнопка закрытия' }
$closeButton.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()

$deadline = [DateTime]::UtcNow.AddSeconds(20)
while ([DateTime]::UtcNow -lt $deadline) {
    if ($ide.Debugger.CurrentMode -eq $breakMode) {
        $frames = @($ide.Debugger.CurrentThread.StackFrames | ForEach-Object { $_.FunctionName })
        @('ProcessId=' + $appProcess.Id, 'BreakReason=' + $ide.Debugger.LastBreakReason, 'WinUI SHA256=' + $xamlHash) + $frames |
            Set-Content (Join-Path $outputDirectory 'break.txt')
        $frames | Write-Output
        throw 'Отладчик VS остановился при закрытии Desktop; состояние сохранено в bin/DesktopShutdownVs/break.txt'
    }
    if ($appProcess.HasExited) {
        if ($appProcess.ExitCode -ne 0) { throw ('Сбой закрытия Desktop под VS: ' + $appProcess.ExitCode) }
        Write-Output 'PASS: Desktop под VS открыл главную страницу и закрылся с кодом 0'
        exit 0
    }
    Start-Sleep -Milliseconds 50
}
@('ProcessId=' + $appProcess.Id, 'Debugger=' + $ide.Debugger.CurrentMode, 'WinUI SHA256=' + $xamlHash,
    'Закрытие не завершилось за отведённое время') | Set-Content (Join-Path $outputDirectory 'timeout.txt')
throw 'Закрытие Desktop под VS не завершилось за отведённое время; состояние сохранено в bin/DesktopShutdownVs/timeout.txt'
