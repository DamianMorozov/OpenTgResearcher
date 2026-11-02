param(
    [Parameter(Mandatory = $true)]
    [string]$ExecutablePath,
    [Parameter(Mandatory = $true)]
    [string]$SelfTestAssembly,
    [string]$SourceDatabase,
    [long]$ChatId = 100,
    [string]$ChatDirectory = '',
    [switch]$CheckMediaPreview,
    [int]$WindowWidth = 0,
    [ValidateSet('Russian', 'English')]
    [string]$Language = 'Russian'
)

$ErrorActionPreference = 'Stop'
$executable = (Resolve-Path -LiteralPath $ExecutablePath).Path
$caseDirectory = Join-Path $PSScriptRoot ('bin/ChatPage/' + [Guid]::NewGuid().ToString('N'))
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
function Wait-AppElement([string]$namePattern, [string]$className = '', [string]$automationId = '') {
    $deadline = [DateTime]::UtcNow.AddSeconds(30)
    while ([DateTime]::UtcNow -lt $deadline) {
        if ($appProcess.HasExited) { throw "Desktop завершился до появления элемента: $namePattern; код $($appProcess.ExitCode)" }
        $appProcess.Refresh()
        if ($appProcess.MainWindowHandle -ne [IntPtr]::Zero) {
            $window = [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
            $elements = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
            foreach ($element in $elements) {
                if ($element.Current.Name -match $namePattern -and ($className -eq '' -or $element.Current.ClassName -eq $className) -and
                    ($automationId -eq '' -or $element.Current.AutomationId -eq $automationId) -and
                    $element.Current.IsEnabled -and -not $element.Current.IsOffscreen) { return $element }
            }
        }
        Start-Sleep -Milliseconds 100
    }
    $elements | ForEach-Object { "$($_.Current.ClassName) | $($_.Current.ControlType.ProgrammaticName) | $($_.Current.Name)" } |
        Set-Content -LiteralPath (Join-Path $caseDirectory 'ui.txt') -Encoding utf8
    throw "В окне Desktop не найден элемент: $namePattern; $caseDirectory"
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

# Сохраняем только окно проверяемого приложения
function Save-AppSnapshot([string]$name) {
    Add-Type -AssemblyName System.Drawing
    Start-Sleep -Milliseconds 400
    $window = [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
    $rect = $window.Current.BoundingRectangle
    $bitmap = [Drawing.Bitmap]::new([int]$rect.Width,[int]$rect.Height)
    $graphics = [Drawing.Graphics]::FromImage($bitmap)
    try {
        $graphics.CopyFromScreen([int]$rect.Left,[int]$rect.Top,0,0,$bitmap.Size)
        $bitmap.Save((Join-Path $caseDirectory ($name + '.png')))
    } finally { $graphics.Dispose(); $bitmap.Dispose() }
}

try {
    if ($CheckMediaPreview -and $SourceDatabase) { throw 'Проверка цветной обложки использует только синтетический чат без SourceDatabase' }
    if (Get-Process OpenTgResearcherDesktop -ErrorAction SilentlyContinue) { throw 'Перед селфтестом закройте запущенный Desktop' }
    Add-Type -AssemblyName UIAutomationClient
    Add-Type -AssemblyName UIAutomationTypes
    New-Item -ItemType Directory -Path $caseDirectory -Force | Out-Null
    $seedArgs = @((Resolve-Path -LiteralPath $SelfTestAssembly).Path, '--seed', (Join-Path $caseDirectory 'TgStorage.db'))
    if ($SourceDatabase) { $seedArgs += @('--source', (Resolve-Path -LiteralPath $SourceDatabase).Path, '--chat', "$ChatId", '--directory', $ChatDirectory) }
    & dotnet @seedArgs
    if ($LASTEXITCODE -ne 0) { throw 'Не удалось создать тестовую БД чата' }
    $settings = @{
        AppStorage = Join-Path $caseDirectory 'TgStorage.db'
        AppSession = Join-Path $caseDirectory 'OpenTgResearcher.session'
        AppLanguage = $Language
        AppTheme = 'Default'
    }
    if ($WindowWidth -gt 0) {
        $settings.WindowWidth = "$WindowWidth"; $settings.WindowHeight = '900'
        $settings.WindowX = '50'; $settings.WindowY = '50'
    }
    $settings | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $caseDirectory 'Settings.json') -Encoding utf8
    $start = [Diagnostics.ProcessStartInfo]::new($executable)
    $start.WorkingDirectory = Split-Path -Parent $executable
    $start.UseShellExecute = $false
    $start.Environment['LocalSettingsOptions__ApplicationDataFolder'] = $caseDirectory
    $start.Environment['LocalSettingsOptions__LocalSettingsFile'] = 'Settings.json'
    $start.Environment['DOTNET_ENVIRONMENT'] = 'Production'
    $appProcess = [Diagnostics.Process]::Start($start)
    Invoke-AppElement (Wait-AppElement 'Продолжить|Continue')
    Invoke-AppElement (Wait-AppElement '^(Чаты|Chats)$')
    # Проверяем настоящие заголовки: колонки не входят в обычный обход локализатора
    $expectedHeaders = if ($Language -eq 'Russian') {
        @('ИД', 'Подписка', 'Доступ', 'Активно', 'Имя пользователя', 'Заголовок', 'Кол-во', 'Прогресс', 'Участники')
    } else { @('ID', 'Subscribed', 'Access', 'Active', 'Username', 'Title', 'Count', 'Progress', 'Participants') }
    $deadline = [DateTime]::UtcNow.AddSeconds(15)
    do {
        $window = [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
        $headers = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants,
            [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty,
                [System.Windows.Automation.ControlType]::HeaderItem))
        $headerNames = foreach ($header in $headers) {
            $header.Current.Name
            $header.FindAll([System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.Condition]::TrueCondition) |
                ForEach-Object { $_.Current.Name }
        }
        $headersReady = $headers.Count -eq 9 -and @($expectedHeaders | Where-Object { $_ -notin $headerNames }).Count -eq 0
        if (-not $headersReady) { Start-Sleep -Milliseconds 100 }
    } until ($headersReady -or [DateTime]::UtcNow -gt $deadline)
    if (-not $headersReady) { throw "Не отображены девять заголовков чатов: $($headerNames -join ', ')" }
    $headerNames | Set-Content -LiteralPath (Join-Path $caseDirectory 'headers.txt') -Encoding utf8
    Write-Output 'PASS: девять локализованных заголовков таблицы чатов'
    Save-AppSnapshot 'Chats'
    $open = Wait-AppElement '^(Открыть/редактировать|Open/edit)$'
    $rect = $window.Current.BoundingRectangle
    $buttonRect = $open.Current.BoundingRectangle
    if ($buttonRect.Right -gt $rect.Right -or $buttonRect.Left -lt $rect.Left) { throw 'Кнопка открытия чата вышла за границы окна' }
    $filter = Wait-AppElement '.*' '' 'ChatFilter'
    if ($filter.Current.BoundingRectangle.Width -lt 120) { throw 'Поле фильтра чатов сжалось до недоступной ширины' }
    $delete = Wait-AppElement '^(Удалить|Delete)$'
    if ($delete.Current.BoundingRectangle.Right -gt $rect.Right) { throw 'Кнопка удаления чата обрезана краем окна' }
    $deadline = [DateTime]::UtcNow.AddSeconds(20)
    $selected = $false
    while ([DateTime]::UtcNow -lt $deadline) {
        $window = [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
        $rows = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants,
            [System.Windows.Automation.PropertyCondition]::new(
                [System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::DataItem))
        foreach ($row in $rows) {
            $pattern = $null
            if ($row.TryGetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern, [ref]$pattern)) {
                $pattern.Select(); $selected = $true; break
            }
        }
        if ($selected) { break }
        Start-Sleep -Milliseconds 100
    }
    if (-not $selected) { throw 'В таблице не найден тестовый чат' }
    Invoke-AppElement (Wait-AppElement '^(Открыть/редактировать|Open/edit)$')
    Invoke-AppElement (Wait-AppElement '^(Да|Yes)$')
    [void](Wait-AppElement '^(Настройки|Settings)$' 'Microsoft.UI.Xaml.Controls.SelectorBarItem')
    $chatsMenu = Wait-AppElement '^(Чаты|Chats)$' 'Microsoft.UI.Xaml.Controls.NavigationViewItem'
    if (-not $chatsMenu.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Current.IsSelected) {
        throw 'Обычный чат ошибочно открыт в разделе сохранённых сообщений'
    }
    $captions = if ($Language -eq 'Russian') {
        @('Настройки', 'Скачать', 'Инфо', 'Мои сообщения', 'Участники', 'Статистика', 'Содержимое', 'Настройки')
    } else { @('Settings', 'Download', 'Info', 'My messages', 'Participants', 'Statistics', 'Content', 'Settings') }
    $models = @('Settings', 'Download', 'Info', 'MyMessages', 'Participants', 'Statistics', 'Content', 'Settings')
    for ($index = 0; $index -lt $captions.Count; $index++) {
        $loadedPattern = 'TgChat' + $models[$index] + 'ViewModel.*Page loaded'
        $previousCount = [regex]::Matches((Get-NewAppLog), $loadedPattern).Count
        Invoke-AppElement (Wait-AppElement ('^' + [regex]::Escape($captions[$index]) + '$') 'Microsoft.UI.Xaml.Controls.SelectorBarItem')
        $deadline = [DateTime]::UtcNow.AddSeconds(15)
        $requiredCount = if ($index -eq 0) { 1 } else { $previousCount + 1 }
        while ([regex]::Matches((Get-NewAppLog), $loadedPattern).Count -lt $requiredCount) {
            if ($appProcess.HasExited) { throw "Desktop вылетел на вкладке $($captions[$index])" }
            if ([DateTime]::UtcNow -gt $deadline) { throw "Нет подтверждения загрузки вкладки $($captions[$index])" }
            Start-Sleep -Milliseconds 100
        }
        if ($models[$index] -eq 'Content') { Save-AppSnapshot 'Content' }
        if ($CheckMediaPreview -and $models[$index] -eq 'Content') {
            Add-Type -AssemblyName System.Drawing
            $image = Wait-AppElement '^sample.mp4$' '' 'MessageThumbnail'
            $bounds = $image.Current.BoundingRectangle
            if ($bounds.Width -lt 100 -or $bounds.Height -lt 50) { throw 'Обложка не получила размер для отображения' }
            $deadline = [DateTime]::UtcNow.AddSeconds(10)
            do {
                $bitmap = [Drawing.Bitmap]::new([int]$bounds.Width, [int]$bounds.Height)
                $graphics = [Drawing.Graphics]::FromImage($bitmap)
                try {
                    $graphics.CopyFromScreen([int]$bounds.Left, [int]$bounds.Top, 0, 0, $bitmap.Size)
                    $color = $bitmap.GetPixel([int]($bitmap.Width / 2), [int]($bitmap.Height / 2))
                    $rendered = $color.R -gt 180 -and $color.G -lt 80 -and $color.B -lt 90
                    $bitmap.Save((Join-Path $caseDirectory 'thumbnail.png'), [Drawing.Imaging.ImageFormat]::Png)
                }
                finally { $graphics.Dispose(); $bitmap.Dispose() }
                if (-not $rendered) { Start-Sleep -Milliseconds 100 }
            } until ($rendered -or [DateTime]::UtcNow -gt $deadline)
            if (-not $rendered) { throw 'Видимый элемент обложки не отрисовал тестовое изображение' }
            Write-Output 'PASS: обложка документа найдена, декодирована и отображена настоящим Image'
            # Проверяем скрытие всего содержимого, включая обложку, через настоящий переключатель
            $sensitive = Wait-AppElement '.*' '' 'ToggleSwitchShowSensitiveData'
            $toggle = $sensitive.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern)
            $toggle.Toggle()
            Start-Sleep -Milliseconds 200
            $hiddenImages = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants,
                [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::AutomationIdProperty,'MessageThumbnail'))
            if (@($hiddenImages | Where-Object { -not $_.Current.IsOffscreen }).Count -gt 0) { throw 'Обложка не скрылась вместе с конфиденциальными данными' }
            $toggle.Toggle()
            [void](Wait-AppElement '^sample.mp4$' '' 'MessageThumbnail')
            Invoke-AppElement (Wait-AppElement '.*' '' 'ClearChatContent')
            Invoke-AppElement (Wait-AppElement '^(Да|Yes)$')
            [void](Wait-AppElement '^(Пустые данные|Empty data)$')
            Write-Output 'PASS: скрытие содержимого и подсказка пустого списка после очистки'
        }
    }
    if ($appProcess.WaitForExit(2000)) { throw "Desktop вылетел после проверки вкладок: код $($appProcess.ExitCode)" }
    $log = Get-NewAppLog
    $log | Set-Content -LiteralPath (Join-Path $caseDirectory 'page.log') -Encoding utf8
    if ($log -notmatch 'TgChatViewModel.*Page loaded') { throw 'Журнал не подтвердил загрузку страницы чата' }
    if ($log -match 'Content does not support UIElement|App_UnhandledException|Unhandled exception in|Object reference not set') { throw 'В журнале найдено необработанное исключение' }
    if (-not $appProcess.CloseMainWindow() -or -not $appProcess.WaitForExit(15000)) { throw 'Desktop не завершил закрытие окна' }
    if ($appProcess.ExitCode -ne 0) { throw "Desktop закрылся с ошибкой: код $($appProcess.ExitCode)" }
    Write-Output "PASS: страница чата ($Language), все 7 вкладок, без исключений, закрытие с кодом 0; $caseDirectory"
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


