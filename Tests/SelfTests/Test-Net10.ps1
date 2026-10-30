param()

$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$failures = @()
$projectPaths = @(& rg --files --hidden -g '*.csproj' -g '!**/bin/**' -g '!**/obj/**' $root)
if ($LASTEXITCODE -ne 0 -or $projectPaths.Count -eq 0) { throw 'Не найдены проекты для проверки .NET 10' }
foreach ($projectPath in $projectPaths) {
    [xml]$project = Get-Content -LiteralPath $projectPath -Raw
    $frameworkNodes = $project.SelectNodes('//TargetFramework | //TargetFrameworks')
    if ($frameworkNodes.Count -eq 0) { $failures += "Не задана целевая платформа: $projectPath"; continue }
    foreach ($node in $frameworkNodes) {
        foreach ($framework in $node.InnerText.Split(';')) {
            if ($framework -notmatch '^net10\.0(?:-windows10\.0\.19041\.0)?$') {
                $failures += "Ожидалась .NET 10, получена ${framework}: $projectPath"
            }
        }
    }
}

# Профили публикации не должны переопределять платформу старой версией
$profiles = @(& rg --files --hidden --no-ignore -g '*.pubxml' -g '!**/bin/**' -g '!**/obj/**' (Join-Path $root Presentation))
foreach ($profilePath in $profiles) {
    [xml]$profile = Get-Content -LiteralPath $profilePath -Raw
    foreach ($node in $profile.SelectNodes('//TargetFramework')) {
        if ($node.InnerText -notmatch '^net10\.0(?:-windows10\.0\.19041\.0)?$') { $failures += "Старая платформа публикации: $profilePath" }
    }
}
$workflow = Get-Content (Join-Path $root '.github/workflows/action_build_windows.yml') -Raw
if ($workflow -notmatch "dotnet-version:\s*'10\.0\.x'") { $failures += 'CI не использует SDK .NET 10' }
$dockerfile = Get-Content (Join-Path $root 'Presentation/OpenTgResearcherConsole/Dockerfile') -Raw
if ($dockerfile -notmatch '(?m)^FROM mcr\.microsoft\.com/dotnet/aspnet:10\.0 AS base\s*$') { $failures += 'Docker не использует среду .NET 10' }
if ($failures.Count) { $failures | Write-Output; throw "Не пройдена проверка миграции .NET 10: $($failures.Count)" }
Write-Output "PASS: .NET 10 во всех $($projectPaths.Count) проектах, $($profiles.Count) профилях публикации, CI и Docker"
