param(
    [Parameter(Mandatory = $true)]
    [string]$ExecutablePath,
    [ValidateSet('English', 'Russian')]
    [string]$Language = 'Russian'
)

$ErrorActionPreference = 'Stop'
$executable = (Resolve-Path -LiteralPath $ExecutablePath).Path
$caseDirectory = Join-Path $PSScriptRoot ('bin/ReleaseStartup/' + [Guid]::NewGuid().ToString('N'))
$process = $null

try {
    if (Get-Process OpenTgResearcherDesktop -ErrorAction SilentlyContinue) { throw 'Перед проверкой закройте запущенный Desktop' }
    New-Item -ItemType Directory -Path $caseDirectory -Force | Out-Null
    $settings = @{
        AppStorage = Join-Path $caseDirectory 'TgStorage.db'
        AppSession = Join-Path $caseDirectory 'OpenTgResearcher.session'
        AppLanguage = $Language
        AppTheme = 'Default'
    }
    $settings | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $caseDirectory 'Settings.json') -Encoding utf8
    $appLog = Join-Path $env:LOCALAPPDATA ('OpenTgResearcherDesktop/current/Log-{0:yyyy-MM-dd}.txt' -f (Get-Date))
    $originalLength = if (Test-Path -LiteralPath $appLog) { (Get-Item -LiteralPath $appLog).Length } else { 0 }
    $start = [Diagnostics.ProcessStartInfo]::new($executable)
    $start.WorkingDirectory = Split-Path -Parent $executable
    $start.UseShellExecute = $false
    $start.Environment['LocalSettingsOptions__ApplicationDataFolder'] = $caseDirectory
    $start.Environment['LocalSettingsOptions__LocalSettingsFile'] = 'Settings.json'
    $start.Environment['DOTNET_ENVIRONMENT'] = 'Production'
    $process = [Diagnostics.Process]::Start($start)
    $deadline = [DateTime]::UtcNow.AddSeconds(35)
    $expectedLanguage = if ($Language -eq 'Russian') { 'ru-RU' } else { 'en-US' }
    $ready = $false
    $newLog = ''
    while ([DateTime]::UtcNow -lt $deadline) {
        if (Test-Path -LiteralPath $appLog) {
            $stream = [IO.File]::Open($appLog, 'Open', 'Read', 'ReadWrite')
            try {
                [void]$stream.Seek($originalLength, 'Begin')
                $reader = [IO.StreamReader]::new($stream)
                $newLog = $reader.ReadToEnd()
                $reader.Dispose()
            }
            finally { $stream.Dispose() }
        }
        if ($process.HasExited) {
            $newLog | Set-Content -LiteralPath (Join-Path $caseDirectory 'startup.log') -Encoding utf8
            throw "Release аварийно завершился при запуске: код $($process.ExitCode); $caseDirectory"
        }
        if ($newLog -match "Language set to $expectedLanguage for unpackaged app") { $ready = $true; break }
        Start-Sleep -Milliseconds 100
    }
    $newLog | Set-Content -LiteralPath (Join-Path $caseDirectory 'startup.log') -Encoding utf8
    if (-not $ready) { throw 'Не подтверждено сохранение языка после полной загрузки заставки' }
    if ($process.WaitForExit(5000)) { throw "Release завершился после заставки: код $($process.ExitCode)" }
    $process.Refresh()
    if (-not $process.CloseMainWindow()) { throw 'Не удалось закрыть окно Release' }
    if (-not $process.WaitForExit(15000)) { throw 'Release не завершил закрытие окна' }
    if ($process.ExitCode -ne 0) { throw "Release закрылся с ошибкой: код $($process.ExitCode)" }
    $saved = Get-Content -LiteralPath (Join-Path $caseDirectory 'Settings.json') -Raw | ConvertFrom-Json
    if ($saved.AppLanguage -ne $Language) { throw 'Сохранённый язык не соответствует выбранному' }
    $database = Join-Path $caseDirectory 'TgStorage.db'
    if (-not (Test-Path -LiteralPath $database) -or (Get-Item -LiteralPath $database).Length -eq 0) { throw 'Не создана изолированная БД Release' }
    Write-Output "PASS: Release ($Language) загрузил заставку, сохранил язык, продолжил работу и закрылся с кодом 0; $caseDirectory"
}
finally {
    if ($process -and -not $process.HasExited) { Stop-Process -Id $process.Id -Force }
    if ($process) { $process.Dispose() }
}
