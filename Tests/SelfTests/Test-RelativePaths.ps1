param()

$ErrorActionPreference = 'Stop'
$testsRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$failures = @()
$files = @(& rg --files --hidden -g '!**/bin/**' -g '!**/obj/**' $testsRoot)
if ($LASTEXITCODE -ne 0 -or $files.Count -eq 0) { throw 'Не найдены исходники тестов' }
$projects = @($files | Where-Object { $_.EndsWith('.csproj') })
foreach ($projectPath in $projects) {
    [xml]$project = Get-Content -LiteralPath $projectPath -Raw
    $projectDirectory = Split-Path -Parent $projectPath
    foreach ($attribute in $project.SelectNodes('//@Include | //@Update | //@Project')) {
        foreach ($path in $attribute.Value.Split(';')) {
            if ($path.Contains('$(') -or $path.Contains('@(')) { continue }
            if ([IO.Path]::IsPathRooted($path)) {
                $failures += "Абсолютная ссылка в проекте ${projectPath}: $path"
                continue
            }
            # Проверяем существование входных файлов, исключая имена пакетов и обновляемые элементы
            if ($attribute.Name -eq 'Update' -or $attribute.OwnerElement.Name -eq 'PackageReference') { continue }
            if ($attribute.OwnerElement.Name -notin @('ProjectReference', 'Compile', 'EmbeddedResource', 'Content', 'None', 'Import')) { continue }
            if (-not (Test-Path -Path (Join-Path $projectDirectory $path))) {
                $failures += "Не найдена относительная ссылка в проекте ${projectPath}: $path"
            }
        }
    }
}

# Исходники тестов должны работать без заранее заданной буквы локального диска
foreach ($file in $files) {
    if ([IO.Path]::GetExtension($file) -notin @('.cs', '.ps1', '.py', '.csproj', '.props', '.targets', '.json', '.runsettings')) { continue }
    $lineNumber = 0
    foreach ($line in Get-Content -LiteralPath $file) {
        $lineNumber++
        if ($line -match '(?<![A-Za-z0-9_])[A-Za-z]:[\\/]') {
            $relativePath = [IO.Path]::GetRelativePath($testsRoot, $file)
            $failures += "Полный путь к диску: ${relativePath}:$lineNumber"
        }
    }
}
if ($failures.Count) { $failures | Write-Output; throw "Не пройдена проверка относительных путей: $($failures.Count)" }
Write-Output "PASS: относительные ссылки во всех $($projects.Count) проектах тестов существуют; полных путей к дискам в исходниках нет"
