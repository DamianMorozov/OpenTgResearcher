param([string]$ExpectedVersion = '')

$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
[xml]$props = Get-Content -LiteralPath (Join-Path $root 'Directory.Build.props') -Raw
$versionNodes = @($props.SelectNodes('/Project/PropertyGroup/Version'))
if ($versionNodes.Count -ne 1) { throw 'Общая версия должна быть задана один раз в Directory.Build.props' }
$version = $versionNodes[0].InnerText
if ($version -notmatch '^\d+\.\d+\.\d+$') { throw "Общая версия должна состоять из трёх частей: $version" }
if ($ExpectedVersion -and $version -ne $ExpectedVersion) { throw "Ожидалась версия $ExpectedVersion, получена $version" }
$assemblyVersion = "$version.0"
$projectPaths = @(& rg --files --hidden -g '*.csproj' -g '!**/bin/**' -g '!**/obj/**' -g '!.git/**' $root)
if ($LASTEXITCODE -ne 0 -or $projectPaths.Count -eq 0) { throw 'Не найдены проекты для проверки общей версии' }
$failures = @()
foreach ($projectPath in $projectPaths) {
    [xml]$project = Get-Content -LiteralPath $projectPath -Raw
    $overrides = $project.SelectNodes('//PropertyGroup/Version | //PropertyGroup/VersionPrefix | //PropertyGroup/VersionSuffix | ' +
        '//PropertyGroup/AssemblyVersion | //PropertyGroup/FileVersion | //PropertyGroup/PackageVersion | //PropertyGroup/InformationalVersion')
    if ($overrides.Count -gt 0) { $failures += "Проект переопределяет общую версию: $projectPath"; continue }
    foreach ($configuration in @('Debug', 'Release')) {
        # Вычисляем реальные версии после выполнения цели SDK без сборки и восстановления NuGet
        $output = & dotnet msbuild $projectPath -nologo -verbosity:quiet -target:GetAssemblyVersion `
            -getProperty:Version,AssemblyVersion,FileVersion,PackageVersion -p:Platform=x64 "-p:Configuration=$configuration"
        if ($LASTEXITCODE -ne 0) { $failures += "MSBuild не вычислил версии: $projectPath ($configuration)"; continue }
        $properties = ($output -join [Environment]::NewLine | ConvertFrom-Json).Properties
        foreach ($name in @('Version', 'PackageVersion', 'AssemblyVersion', 'FileVersion')) {
            $expected = if ($name -in @('AssemblyVersion', 'FileVersion')) { $assemblyVersion } else { $version }
            if ($properties.$name -ne $expected) {
                $failures += "${projectPath} ($configuration): $name = '$($properties.$name)', ожидалось '$expected'"
            }
        }
    }
}
if ($failures.Count -gt 0) { $failures | Write-Output; throw "Не пройдена проверка общей версии: $($failures.Count)" }
Write-Output "PASS: $($projectPaths.Count) проектов наследуют версию $version; сборки $assemblyVersion, пакеты $version; Debug и Release x64"
