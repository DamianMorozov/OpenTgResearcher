param()

$ErrorActionPreference = 'Stop'
$solutionPath = Join-Path $PSScriptRoot '..\..\OpenTgResearcher.slnx'
$outputDirectory = Join-Path $PSScriptRoot 'bin\DesktopArchitecture'
$validationTargetsPath = Join-Path $outputDirectory 'ValidateArchitecture.targets'

try {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
    # Проверяем результат штатной цели Windows App SDK без компиляции и изменения файлов приложения
    @'
<Project>
  <Target Name="ValidateDesktopArchitecture" AfterTargets="GetWindowsAppSDKNativePlatform">
    <Error Condition="'$(NativePlatform)' != '$(ExpectedSelfTestPlatform)'"
           Text="Неверная архитектура Windows App SDK: $(NativePlatform), ожидалась $(ExpectedSelfTestPlatform)" />
    <Error Condition="'$(WindowsAppSDKSelfContained)' != 'true'" Text="Режим WindowsAppSDKSelfContained должен оставаться включённым" />
    <Message Importance="high" Text="DESKTOP_ARCHITECTURE_SELFTEST_PASS|$(MSBuildProjectName)|$(Configuration)|$(Platform)|$(NativePlatform)" />
  </Target>
</Project>
'@ | Set-Content -LiteralPath $validationTargetsPath -Encoding utf8

    $failures = @()
    foreach ($configuration in @('Debug', 'Release')) {
        foreach ($platform in @('Any CPU', 'x64', 'x86')) {
            $expectedPlatform = if ($platform -eq 'Any CPU') { 'x64' } else { $platform }
            foreach ($project in @('Presentation\OpenTgResearcherDesktop', 'Tests\OpenTgResearcherDesktop.Tests')) {
                $projectTarget = $project.Replace('.', '_')
                $arguments = @(
                    'msbuild', $solutionPath, '-nologo', '-v:minimal'
                    "-t:${projectTarget}:GetWindowsAppSDKNativePlatform"
                    "-p:Configuration=$configuration", "-p:Platform=$platform"
                    "-p:ExpectedSelfTestPlatform=$expectedPlatform"
                    "-p:CustomAfterMicrosoftCommonTargets=$validationTargetsPath"
                )
                $output = & dotnet @arguments 2>&1
                $exitCode = $LASTEXITCODE
                $logName = '{0}-{1}-{2}.log' -f $configuration, $platform.Replace(' ', ''), ($project -split '\\')[-1]
                $logPath = Join-Path $outputDirectory $logName
                $output | Set-Content -LiteralPath $logPath -Encoding utf8
                if ($exitCode -ne 0 -or ($output -join "`n") -notmatch 'DESKTOP_ARCHITECTURE_SELFTEST_PASS\|') {
                    $failures += "$configuration|$platform|$project : $logPath"
                    Write-Output $output
                } else {
                    Write-Output "PASS: $configuration|$platform|$project -> $expectedPlatform"
                }
            }
        }
    }
    if ($failures.Count -gt 0) { throw "Не пройдены проверки архитектуры:`n$($failures -join "`n")" }
    Write-Output "PASS: Проверены 12 сочетаний конфигурации, платформы и проекта. Логи: $outputDirectory"
}
catch {
    Write-Error "Селфтест архитектуры Desktop завершился ошибкой: $_"
    exit 1
}
