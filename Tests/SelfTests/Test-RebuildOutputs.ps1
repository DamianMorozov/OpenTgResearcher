param()

$ErrorActionPreference = 'Stop'
$outputDirectory = Join-Path $PSScriptRoot 'bin/RebuildOutputs'
New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
$projectPath = Join-Path $outputDirectory 'RebuildOutputs.proj'
$targetsPath = [Security.SecurityElement]::Escape([IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../Directory.Build.targets')))
$targetDirectory = [Security.SecurityElement]::Escape((Join-Path $outputDirectory 'output/'))
# Повторяем отложенный запуск цели очистки ссылки на проект после Rebuild
@"
<Project>
  <PropertyGroup><TargetDir>$targetDirectory</TargetDir></PropertyGroup>
  <Import Project="$targetsPath" />
  <Target Name="BeforeRebuild" />
  <Target Name="AfterRebuild" />
  <Target Name="Clean" />
  <Target Name="Build">
    <MakeDir Directories="`$(TargetDir)" />
    <WriteLinesToFile File="`$(TargetDir)result.txt" Lines="compiled" Overwrite="true" />
  </Target>
  <Target Name="Rebuild" DependsOnTargets="BeforeRebuild;Clean;Build;AfterRebuild" />
  <Target Name="Verify">
    <Error Condition="!Exists('`$(TargetDir)result.txt')" Text="Повторный Clean удалил результат завершённого Rebuild" />
  </Target>
</Project>
"@ | Set-Content -LiteralPath $projectPath -Encoding utf8
$driverPath = Join-Path $outputDirectory 'Driver.proj'
$escapedProjectPath = [Security.SecurityElement]::Escape($projectPath)
@"
<Project>
  <Target Name="Probe">
    <MSBuild Projects="$escapedProjectPath" Targets="Rebuild" />
    <MSBuild Projects="$escapedProjectPath" Targets="CleanTargetDir" />
    <MSBuild Projects="$escapedProjectPath" Targets="Verify" />
  </Target>
</Project>
"@ | Set-Content -LiteralPath $driverPath -Encoding utf8
$output = & dotnet msbuild $driverPath -nologo -t:Probe -v:minimal 2>&1
$exitCode = $LASTEXITCODE
$output | Set-Content -LiteralPath (Join-Path $outputDirectory 'result.log') -Encoding utf8
if ($exitCode -ne 0) { $output | Write-Output; throw 'Не пройдена проверка сохранности результата Rebuild' }
Write-Output 'PASS: Повторный Clean не удаляет результат завершённого Rebuild'
