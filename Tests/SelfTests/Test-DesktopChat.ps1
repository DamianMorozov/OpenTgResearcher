param(
    [Parameter(Mandatory = $true)]
    [string]$ExecutablePath,
    [Parameter(Mandatory = $true)]
    [string]$SelfTestAssembly,
    [string]$SourceDatabase,
    [long]$ChatId = 100,
    [string]$ChatDirectory = '',
    [switch]$CheckMediaPreview,
    [switch]$ContentOnly,
    [string]$VideoFileName = '',
    [switch]$CheckRegistryMenu,
    [switch]$CheckPublicChat,
    [switch]$CheckRegistryPagination,
    [switch]$RegistryPaginationOnly,
    [switch]$SkipSnapshots,
    [ValidateSet('Percent', 'Steps', 'Wheel', 'VisibleTail')]
    [string]$RegistryScrollMode = 'VisibleTail',
    [int]$WindowWidth = 0,
    [int]$WindowHeight = 1000,
    [ValidateSet('Default', 'Light', 'Dark')]
    [string]$Theme = 'Default',
    [ValidateSet('Russian', 'English')]
    [string]$Language = 'Russian'
)

$ErrorActionPreference = 'Stop'
if ($RegistryPaginationOnly) { $CheckRegistryPagination = $true }
$executable = (Resolve-Path -LiteralPath $ExecutablePath).Path
$caseDirectory = Join-Path $PSScriptRoot ('bin/ChatPage/' + [Guid]::NewGuid().ToString('N'))
$appProcess = $null
$appLog = Join-Path $env:LOCALAPPDATA ('OpenTgResearcherDesktop/current/Log-{0:yyyy-MM-dd}.txt' -f (Get-Date))
$originalLength = if (Test-Path -LiteralPath $appLog) { (Get-Item -LiteralPath $appLog).Length } else { 0 }
Add-Type @'
public static class ChatPageTestNative {
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern bool PostMessage(System.IntPtr window, uint message, System.IntPtr wParam, System.IntPtr lParam);
    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    public static extern System.IntPtr FindWindowEx(System.IntPtr parent, System.IntPtr after, string className, string title);
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(System.IntPtr window);
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern System.IntPtr GetForegroundWindow();
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern bool SetCursorPos(int x, int y);
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern void mouse_event(uint flags, uint dx, uint dy, uint data, System.UIntPtr extraInfo);
}
'@

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
            try {
                $elements = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
            }
            catch {
                # Дерево перестраивается при навигации; повторяем только временную ошибку UI Automation живого процесса
                $failure = $_.Exception.GetBaseException()
                if ($appProcess.HasExited -or ($failure -isnot [Runtime.InteropServices.COMException] -and
                    $failure -isnot [System.Windows.Automation.ElementNotAvailableException])) { throw }
                Start-Sleep -Milliseconds 100
                continue
            }
            foreach ($element in $elements) {
                if ($element.Current.Name -match $namePattern -and ($className -eq '' -or $element.Current.ClassName -eq $className) -and
                    ($automationId -eq '' -or $element.Current.AutomationId -eq $automationId) -and
                    $element.Current.IsEnabled -and -not $element.Current.IsOffscreen) { return $element }
            }
        }
        # Прокручиваем обзор к запрошенному действию в одноколоночном представлении
        if ($automationId -like 'Chat*' -and $null -ne $window) {
            $viewer = $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants,
                [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::AutomationIdProperty, 'ChatOverviewScroll'))
            $scroll = $null
            if ($null -ne $viewer -and $viewer.TryGetCurrentPattern([System.Windows.Automation.ScrollPattern]::Pattern, [ref]$scroll) -and
                $scroll.Current.VerticallyScrollable) {
                if ($scroll.Current.VerticalScrollPercent -ge 99) { $scroll.SetScrollPercent(-1, 0) }
                else { $scroll.Scroll([System.Windows.Automation.ScrollAmount]::NoAmount, [System.Windows.Automation.ScrollAmount]::LargeIncrement) }
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

# Нажимаем внутри проверяемого окна, когда изображение не поддерживает InvokePattern
function Invoke-AppPoint([double]$x, [double]$y) {
    [void][ChatPageTestNative]::SetForegroundWindow($appProcess.MainWindowHandle)
    Start-Sleep -Milliseconds 200
    if ([ChatPageTestNative]::GetForegroundWindow() -ne $appProcess.MainWindowHandle) { throw 'Окно перекрыто перед нажатием' }
    $window = [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
    if (-not $window.Current.BoundingRectangle.Contains($x, $y)) { throw 'Координаты нажатия вышли за границы окна' }
    [void][ChatPageTestNative]::SetCursorPos([int]$x, [int]$y)
    [ChatPageTestNative]::mouse_event(2, 0, 0, 0, [UIntPtr]::Zero)
    [ChatPageTestNative]::mouse_event(4, 0, 0, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 250
}

# Проверяем, что увеличенное изображение действительно скрыто, а не закрывает ленту пустой панелью
function Assert-ImageViewerClosed {
    $window = [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
    $images = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants,
        [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::AutomationIdProperty, 'ChatFullScreenImage'))
    if (@($images | Where-Object { -not $_.Current.IsOffscreen }).Count -gt 0) { throw 'Просмотр изображения остался поверх ленты' }
    [void](Wait-AppElement '^preview.png$' '' 'MessageImage')
}

# Сохраняем только окно проверяемого приложения
function Save-AppSnapshot([string]$name) {
    if ($SkipSnapshots) { return }
    Add-Type -AssemblyName System.Drawing
    [void][ChatPageTestNative]::SetForegroundWindow($appProcess.MainWindowHandle)
    Start-Sleep -Milliseconds 400
    if ([ChatPageTestNative]::GetForegroundWindow() -ne $appProcess.MainWindowHandle) {
        throw 'Кадр не сохранён: проверяемое окно перекрыто другим приложением'
    }
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
    if ($CheckRegistryMenu) {
        if ($SourceDatabase) { throw 'Меню проверяется только на синтетической БД' }
        & dotnet (Resolve-Path -LiteralPath $SelfTestAssembly).Path --seed-registry-menu (Join-Path $caseDirectory 'TgStorage.db')
        if ($LASTEXITCODE -ne 0) { throw 'Не удалось добавить вторую строку для проверки меню' }
    }
    if ($CheckRegistryPagination) {
        if ($SourceDatabase -or $CheckRegistryMenu) { throw 'Пагинация использует отдельную синтетическую БД без проверки меню' }
        & dotnet (Resolve-Path -LiteralPath $SelfTestAssembly).Path --seed-registry-pages (Join-Path $caseDirectory 'TgStorage.db')
        if ($LASTEXITCODE -ne 0) { throw 'Не удалось подготовить страницы чатов' }
    }
    $sourceInfo = & dotnet (Resolve-Path -LiteralPath $SelfTestAssembly).Path --inspect-chat (Join-Path $caseDirectory 'TgStorage.db') $ChatId
    if ($LASTEXITCODE -ne 0) { throw 'Не удалось прочитать выбранный чат тестовой БД' }
    $expectedTitle = [regex]::Match(($sourceInfo -join "`n"), 'Title=(.*?); Directory=').Groups[1].Value
    $settings = @{
        AppStorage = Join-Path $caseDirectory 'TgStorage.db'
        AppSession = Join-Path $caseDirectory 'OpenTgResearcher.session'
        AppLanguage = $Language
        AppTheme = $Theme
    }
    if ($WindowWidth -gt 0) {
        $settings.WindowWidth = "$WindowWidth"; $settings.WindowHeight = "$WindowHeight"
        $settings.WindowX = '50'; $settings.WindowY = '50'
    }
    $settings | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $caseDirectory 'Settings.json') -Encoding utf8
    $start = [Diagnostics.ProcessStartInfo]::new($executable)
    $start.WorkingDirectory = Split-Path -Parent $executable
    $start.UseShellExecute = $false
    $start.Environment['LocalSettingsOptions__ApplicationDataFolder'] = $caseDirectory
    $start.Environment['LocalSettingsOptions__LocalSettingsFile'] = 'Settings.json'
    $start.Environment['DOTNET_ENVIRONMENT'] = 'Production'
    $start.Environment['Logging__LogLevel__Default'] = 'Warning'
    $appProcess = [Diagnostics.Process]::Start($start)
    Invoke-AppElement (Wait-AppElement 'Продолжить|Continue')
    Invoke-AppElement (Wait-AppElement '^(Чаты|Chats)$')
    if ($CheckPublicChat) {
        Invoke-AppElement (Wait-AppElement '^(В Telegram|In Telegram)$')
        $reference = Wait-AppElement '.*' '' 'ChatFilter'
        $reference.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue('@durov')
        $search = Wait-AppElement '.*' '' 'ChatSearch'
        $window = [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
        foreach ($control in @($reference, $search)) {
            if (-not $window.Current.BoundingRectangle.Contains($control.Current.BoundingRectangle)) {
                throw 'Поле или кнопка глобального поиска вышли за границы окна'
            }
        }
        if ($reference.Current.BoundingRectangle.IntersectsWith($search.Current.BoundingRectangle)) {
            throw 'Поле поиска перекрывается кнопкой поиска'
        }
        [void](Wait-AppElement '^(Публичные каналы и группы|Public channels and groups)')
        Save-AppSnapshot 'Chats-PublicSource'
        Invoke-AppElement (Wait-AppElement '^(Мои чаты|My chats)$')
        $reference.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue('')
        Write-Output 'PASS: единая строка, переключение области, поле ввода, кнопка и пояснение без вступления; элементы без перекрытий'
    }
    if ($CheckRegistryPagination) {
        $registryStatistics = Wait-AppElement '^(Показано 100 из 943 чатов|Showing 100 of 943 chats)$'
        $registry = Wait-AppElement '.*' '' 'ChatsRegistry'
        $scroll = $registry.GetCurrentPattern([System.Windows.Automation.ScrollPattern]::Pattern)
        if (-not $scroll.Current.VerticallyScrollable) { throw 'У реестра нет вертикальной прокрутки' }
        # Следующая прокрутка выполняется сразу после появления порции, пока ещё считаются размеры каталогов
        foreach ($expectedCount in @(200,300,400,500,600,700,800,900,943)) {
            if ($RegistryScrollMode -eq 'VisibleTail') {
                $scroll.SetScrollPercent(-1, $(if ($expectedCount -le 300) { 100 } else { 99.9 }))
                if ($expectedCount -gt 300) {
                    $previousCount = if ($expectedCount -eq 943) { 900 } else { $expectedCount - 100 }
                    [void](Wait-AppElement ('^ZZ Page {0:000}$' -f ($previousCount - 2)))
                    Write-Output "SCROLL: последняя строка порции $previousCount видна до математического конца полосы"
                }
            }
            elseif ($RegistryScrollMode -eq 'Percent') { $scroll.SetScrollPercent(-1, 100) }
            else {
                # Прокручиваем обычными порциями, не присваивая полосе точный математический максимум
                $unchangedSteps = 0
                for ($step = 0; $step -lt 500; $step++) {
                    $before = $scroll.Current.VerticalScrollPercent
                    if ($RegistryScrollMode -eq 'Wheel') {
                        $bounds = $registry.Current.BoundingRectangle
                        $x = [int]($bounds.Left + $bounds.Width / 2); $y = [int]($bounds.Top + $bounds.Height / 2)
                        $targetWindow = [ChatPageTestNative]::FindWindowEx($appProcess.MainWindowHandle, [IntPtr]::Zero,
                            'Microsoft.UI.Content.DesktopChildSiteBridge', $null)
                        if ($targetWindow -eq [IntPtr]::Zero) { throw 'Не найдено дочернее окно содержимого тестового Desktop' }
                        $inputWindow = [ChatPageTestNative]::FindWindowEx($targetWindow, [IntPtr]::Zero, 'InputSiteWindowClass', $null)
                        if ($inputWindow -eq [IntPtr]::Zero) { throw 'Не найдено дочернее окно ввода тестового Desktop' }
                        if (-not [ChatPageTestNative]::PostMessage($inputWindow, 0x020A, [IntPtr](-120 -shl 16), [IntPtr](($y -shl 16) -bor $x))) {
                            throw 'Не удалось отправить событие колеса тестовому Desktop'
                        }
                    }
                    else { $scroll.Scroll([System.Windows.Automation.ScrollAmount]::NoAmount, [System.Windows.Automation.ScrollAmount]::LargeIncrement) }
                    Start-Sleep -Milliseconds 30
                    $after = $scroll.Current.VerticalScrollPercent
                    if ($registryStatistics.Current.Name -match "^(Показано $expectedCount из|Showing $expectedCount of)") { break }
                    if ($after -ge 99.999) { break }
                    $unchangedSteps = if ([Math]::Abs($after - $before) -lt 0.000001) { $unchangedSteps + 1 } else { 0 }
                    if ($unchangedSteps -ge 10) { break }
                }
                Write-Output "SCROLL: ожидается $expectedCount; процент=$($scroll.Current.VerticalScrollPercent); $($registryStatistics.Current.Name)"
            }
            [void](Wait-AppElement "^(Показано $expectedCount из 943 чатов|Showing $expectedCount of 943 chats)$")
            if ($scroll.Current.VerticalScrollPercent -le 0) { throw "Дозагрузка $expectedCount сбросила прокрутку к первым строкам" }
            Write-Output "PASS: автоматическая дозагрузка $expectedCount / 943"
        }
        $scroll.SetScrollPercent(-1, 100)
        [void](Wait-AppElement '^ZZ Page 941$')
        Write-Output 'PASS: последняя строка списка действительно видна, прокрутка не сбрасывается'
        Save-AppSnapshot 'Chats-Pagination'
        $scroll.SetScrollPercent(-1, 0)
        $filter = Wait-AppElement '.*' '' 'ChatFilter'
        $filter.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue('ZZ Page 941')
        Invoke-AppElement (Wait-AppElement '.*' '' 'ChatSearch')
        [void](Wait-AppElement '^(Показано 1 из 1 чатов|Showing 1 of 1 chats)$')
        $filter.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue('')
        Invoke-AppElement (Wait-AppElement '.*' '' 'ChatSearch')
        [void](Wait-AppElement '^(Показано 100 из 943 чатов|Showing 100 of 943 chats)$')
        Write-Output 'PASS: прокрутка автоматически загрузила 100 → 200 → 300 → … → 943 чата; поиск и очистка сбросили пагинацию'
        if ($RegistryPaginationOnly) {
            if (-not $appProcess.CloseMainWindow() -or -not $appProcess.WaitForExit(15000)) { throw 'Desktop не завершил закрытие окна' }
            if ($appProcess.ExitCode -ne 0) { throw "Desktop закрылся с ошибкой: код $($appProcess.ExitCode)" }
            if ((Get-NewAppLog) -match 'App_UnhandledException|Unhandled exception|Exception:|Failed to enqueue callback') {
                throw 'В журнале пагинации найдено исключение'
            }
            Write-Output "PASS: пагинация, размеры, поиск и штатное закрытие без исключений; $caseDirectory"
            return
        }
    }
    if ($CheckRegistryMenu) {
        $sortControl = Wait-AppElement '.*' '' 'ChatsSort'
        $selection = $sortControl.GetCurrentPattern([System.Windows.Automation.SelectionPattern]::Pattern).Current.GetSelection()
        if ($selection.Count -ne 1 -or $selection[0].Current.Name -notmatch '^(По названию|By title)$') {
            throw 'Начальная сортировка не по названию'
        }
        $registry = Wait-AppElement '.*' '' 'ChatsRegistry'
        $rows = $registry.FindAll([System.Windows.Automation.TreeScope]::Descendants,
            [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty,
                [System.Windows.Automation.ControlType]::DataItem))
        if ($rows.Count -ne 2) { throw "Не получены две строки реестра: $($rows.Count)" }
        $firstTexts = $rows[0].FindAll([System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.Condition]::TrueCondition)
        if ($expectedTitle -notin @($firstTexts | ForEach-Object { $_.Current.Name })) { throw 'Первая строка не соответствует сортировке по названию' }
        # Щелчок по второй строке должен заменить исходный выбор первой
        $target = $rows[1].Current.BoundingRectangle
        [void][ChatPageTestNative]::SetForegroundWindow($appProcess.MainWindowHandle)
        [void][ChatPageTestNative]::SetCursorPos([int]($target.Left+80),[int]($target.Top+$target.Height/2))
        [ChatPageTestNative]::mouse_event(0x0008,0,0,0,[UIntPtr]::Zero)
        [ChatPageTestNative]::mouse_event(0x0010,0,0,0,[UIntPtr]::Zero)
        Start-Sleep -Milliseconds 350
        $selected = $registry.GetCurrentPattern([System.Windows.Automation.SelectionPattern]::Pattern).Current.GetSelection()
        $selectedTexts = $selected[0].FindAll([System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.Condition]::TrueCondition)
        if ('ZZ Context menu chat' -notin @($selectedTexts | ForEach-Object { $_.Current.Name })) { throw 'Меню открылось для другого чата' }
        $elements = ([System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)).FindAll(
            [System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.Condition]::TrueCondition)
        foreach ($menuId in @('ChatRowParsing','ChatRowPause')) {
            $item = @($elements | Where-Object { $_.Current.AutomationId -eq $menuId -and -not $_.Current.IsOffscreen })
            if ($item.Count -ne 1 -or $item[0].Current.IsEnabled) { throw "Меню $menuId не показано или разрешает действие без подключения" }
        }
        Save-AppSnapshot 'Chats-RowMenu'
        [System.Windows.Forms.SendKeys]::SendWait('{ESC}')
        $rows[0].GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
        Write-Output 'PASS: сортировка по названию; меню выбрало вторую строку; парсинг и пауза без подключения недоступны'
    }
    # Проверяем настоящие заголовки: колонки не входят в обычный обход локализатора
    $expectedHeaders = if ($Language -eq 'Russian') {
        @('Чат', 'Сообщения', 'Размер', 'Парсинг', 'Обновление')
    } else { @('Chat', 'Messages', 'Size', 'Parsing', 'Updated') }
    if ($WindowWidth -gt 0 -and $WindowWidth -lt 1000) { $expectedHeaders = $expectedHeaders[0..3] }
    $deadline = [DateTime]::UtcNow.AddSeconds(15)
    do {
        $window = [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
        $headers = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants,
            [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty,
                [System.Windows.Automation.ControlType]::HeaderItem)) | Where-Object { -not $_.Current.IsOffscreen }
        $headerNames = foreach ($header in $headers) {
            $header.Current.Name
            $header.FindAll([System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.Condition]::TrueCondition) |
                ForEach-Object { $_.Current.Name }
        }
        $headersReady = @($headers).Count -eq $expectedHeaders.Count -and @($expectedHeaders | Where-Object { $_ -notin $headerNames }).Count -eq 0
        if (-not $headersReady) { Start-Sleep -Milliseconds 100 }
    } until ($headersReady -or [DateTime]::UtcNow -gt $deadline)
    if (-not $headersReady) {
        Save-AppSnapshot 'Chats-HeadersFailure'
        $headers | ForEach-Object { "$($_.Current.Name): $($_.Current.BoundingRectangle)" } |
            Set-Content -LiteralPath (Join-Path $caseDirectory 'headers-failure.txt') -Encoding utf8
        throw "Не отображены $($expectedHeaders.Count) заголовков реестра: $($headerNames -join ', '); $caseDirectory"
    }
    $headerNames | Set-Content -LiteralPath (Join-Path $caseDirectory 'headers.txt') -Encoding utf8
    Write-Output "PASS: $($expectedHeaders.Count) локализованных заголовков реестра чатов"
    if (-not $SourceDatabase) {
        $bytes = (Get-ChildItem -LiteralPath (Join-Path $caseDirectory 'Media') -File -Recurse | Measure-Object Length -Sum).Sum
        $unit = if ($bytes -lt 1024) { 0 } elseif ($bytes -lt 1048576) { 1 } elseif ($bytes -lt 1073741824) { 2 } else { 3 }
        $units = if ($Language -eq 'Russian') { @('Б','КБ','МБ','ГБ') } else { @('B','KB','MB','GB') }
        $expectedSize = ($bytes / [Math]::Pow(1024, $unit)).ToString('0.##') + ' ' + $units[$unit]
        [void](Wait-AppElement ('^' + [regex]::Escape($expectedSize) + '$'))
        Write-Output "PASS: колонка размера показывает объём настоящего каталога: $expectedSize"
    }
    Save-AppSnapshot 'Chats'
    $open = Wait-AppElement '.*' '' 'ChatOpenChat'
    $rect = $window.Current.BoundingRectangle
    $buttonRect = $open.Current.BoundingRectangle
    if ($buttonRect.Right -gt $rect.Right -or $buttonRect.Left -lt $rect.Left) { throw 'Кнопка открытия чата вышла за границы окна' }
    $filter = Wait-AppElement '.*' '' 'ChatFilter'
    if ($filter.Current.BoundingRectangle.Width -lt 120) { throw 'Поле фильтра чатов сжалось до недоступной ширины' }
    # Проверяем поиск и фильтр без подключения к Telegram
    $filterValue = $filter.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
    $filter.SetFocus()
    # UI Automation вводит текст независимо от русской или английской раскладки Windows
    $filterValue.SetValue('registry_missing_483291')
    if ($filterValue.Current.Value -ne 'registry_missing_483291') { throw 'Тест не ввёл поисковую строку' }
    [System.Windows.Forms.SendKeys]::SendWait('{ENTER}')
    [void](Wait-AppElement '^(Чаты не найдены|No chats found)')
    if ($filterValue.Current.Value -ne 'registry_missing_483291') { throw 'Enter изменил текст поиска' }
    $filter.SetFocus()
    $filterValue.SetValue('')
    [System.Windows.Forms.SendKeys]::SendWait('{ENTER}')
    Write-Output 'PASS: Enter применил поиск без потери фокуса; очистка и Enter вернули список'
    [void](Wait-AppElement '.*' '' 'ChatOpenChat')
    Invoke-AppElement (Wait-AppElement '^(В работе|In progress)$' 'Microsoft.UI.Xaml.Controls.SelectorBarItem')
    [void](Wait-AppElement '^(Чаты не найдены|No chats found)')
    Invoke-AppElement (Wait-AppElement '^(Все|All)$' 'Microsoft.UI.Xaml.Controls.SelectorBarItem')
    [void](Wait-AppElement '.*' '' 'ChatOpenChat')
    $sort = Wait-AppElement '.*' '' 'ChatsSort'
    $sort.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern).Expand()
    Invoke-AppElement (Wait-AppElement '^(По названию|By title)$')
    [void](Wait-AppElement '.*' '' 'ChatOpenChat')
    $details = Wait-AppElement '^(Сведения|Details)$' 'Microsoft.UI.Xaml.Controls.Expander'
    $expand = $details.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
    $expand.Expand()
    Save-AppSnapshot 'Chats-Details'
    $expand.Collapse()
    $privacy = (Wait-AppElement '.*' '' 'ToggleSwitchShowSensitiveData').GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern)
    $privacy.Toggle()
    Start-Sleep -Milliseconds 200
    $window = [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
    foreach ($hiddenId in @('ChatsRegistry', 'ChatSelectionActions')) {
        $hidden = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants,
            [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::AutomationIdProperty, $hiddenId))
        if (@($hidden | Where-Object { -not $_.Current.IsOffscreen }).Count -gt 0) { throw "Конфиденциальные данные не скрыты: $hiddenId" }
    }
    $privacy.Toggle()
    [void](Wait-AppElement '.*' '' 'ChatOpenChat')
    Write-Output 'PASS: поиск, пустой фильтр работающих чатов, сортировка, сведения и конфиденциальность реестра'
    # Без подключения кнопка запуска видна, но недоступна; управление расположено в порядке парсинг, пауза, открытие
    $parsing = $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants,
        [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::AutomationIdProperty, 'ChatRegistryParsing'))
    $pause = $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants,
        [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::AutomationIdProperty, 'ChatRegistryPause'))
    $open = Wait-AppElement '.*' '' 'ChatOpenChat'
    if ($null -eq $parsing -or $null -eq $pause -or $parsing.Current.IsOffscreen -or $parsing.Current.IsEnabled) {
        throw 'Кнопка парсинга не видна или разрешает запуск без подключения'
    }
    $parseRect = $parsing.Current.BoundingRectangle
    $pauseRect = $pause.Current.BoundingRectangle
    $openRect = $open.Current.BoundingRectangle
    if ($parseRect.Right -gt $pauseRect.Left -or $pauseRect.Right -gt $openRect.Left -or $openRect.Right -gt $rect.Right) {
        throw 'Кнопки парсинга, паузы и открытия перекрываются или выходят за границы окна'
    }
    $expectedParseCaption = if ($Language -eq 'Russian') { 'Парсинг' } else { 'Parse' }
    $parseTexts = $parsing.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
    if ($expectedParseCaption -notin @($parsing.Current.Name) + @($parseTexts | ForEach-Object { $_.Current.Name })) {
        throw 'Кнопка парсинга не переведена на выбранный язык'
    }
    Write-Output 'PASS: локализованная кнопка парсинга слева от паузы; без подключения запуск недоступен, кнопки не перекрываются'
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
    Invoke-AppElement (Wait-AppElement '.*' '' 'ChatOpenChat')
    [void](Wait-AppElement '^(Обзор|Overview)$' 'Microsoft.UI.Xaml.Controls.SelectorBarItem')
    Write-Output 'PASS: выбранный чат открыт без диалога подтверждения'
    $chatsMenu = Wait-AppElement '^(Чаты|Chats)$' 'Microsoft.UI.Xaml.Controls.NavigationViewItem'
    if (-not $chatsMenu.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Current.IsSelected) {
        throw 'Обычный чат ошибочно открыт в разделе сохранённых сообщений'
    }
    $captions = if ($Language -eq 'Russian') {
        @('Обзор', 'Содержимое', 'Мой профиль в чате', 'Все сообщения', 'Участники', 'Статистика', 'Обзор')
    } else { @('Overview', 'Content', 'My chat profile', 'All messages', 'Participants', 'Statistics', 'Overview') }
    $models = @('Download', 'Content', 'MyMessages', 'Content', 'Participants', 'Statistics', 'Download')
    if ($ContentOnly) {
        $captions = @(if ($Language -eq 'Russian') { 'Содержимое' } else { 'Content' })
        $models = @('Content')
    }
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
        if ($models[$index] -eq 'Settings') {
            $window = [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
            $edits = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants,
                [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty,
                    [System.Windows.Automation.ControlType]::Edit))
            $values = foreach ($edit in $edits) {
                $pattern = $null
                if ($edit.TryGetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern, [ref]$pattern)) { $pattern.Current.Value }
            }
            if ($expectedTitle -notin $values) { throw "Настройки не отобразили название чата при открытии: $expectedTitle" }
            Write-Output 'PASS: настройки сразу показывают выбранный чат'
            Save-AppSnapshot 'Settings'
        }
        if ($models[$index] -eq 'Download') {
            Save-AppSnapshot ('Overview-' + $index)
            $toggleIds = @('ChatParseMessages', 'ChatDownloadFiles', 'ChatParseComments', 'ChatJoinMessageId',
                'ChatDownloadThumbnail', 'ChatCreateSubdirectories', 'ChatAutoUpdate')
            foreach ($toggleId in $toggleIds) {
                $option = (Wait-AppElement '.*' '' $toggleId).GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern)
                if ($option.Current.ToggleState -eq [System.Windows.Automation.ToggleState]::On) { $option.Toggle() }
            }
            $reset = Wait-AppElement '^(Сброс настроек|Reset settings)$' '' 'ChatResetSettings'
            Invoke-AppElement $reset
            foreach ($toggleId in $toggleIds) {
                $option = (Wait-AppElement '.*' '' $toggleId).GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern)
                if ($option.Current.ToggleState -ne [System.Windows.Automation.ToggleState]::On) { throw "Сброс не включил опцию $toggleId" }
            }
            [void](Wait-AppElement '^(Сетевая активность|Network activity)$' '' 'ChatOpenNetworkActivity')
            Write-Output 'PASS: сброс настроек включил семь переключателей без отдельного диалога; доступна кнопка сетевой активности'
            $percent = Wait-AppElement '.*' '' 'ChatDownloadPercent'
            if ($percent.Current.BoundingRectangle.Width -lt 80) { throw 'Поле процента сжато до нечитаемой ширины' }
            if (-not $percent.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).Current.IsReadOnly) {
                throw 'Поле процента разрешает редактирование'
            }
            if ($percent.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).Current.Value -notmatch '\d+[.,]\d+\s*%') {
                throw 'При открытии вкладки отсутствует процент сохранённого прогресса'
            }
            $toggle = (Wait-AppElement '.*' '' 'ChatAutoUpdate').GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern)
            $originalState = $toggle.Current.ToggleState
            foreach ($attempt in 1..2) {
                $toggle.Toggle()
                $enabled = $toggle.Current.ToggleState -eq [System.Windows.Automation.ToggleState]::On
                Invoke-AppElement (Wait-AppElement '.*' '' 'ChatSaveSettings')
                Invoke-AppElement (Wait-AppElement '^(Да|Yes)$')
                $deadline = [DateTime]::UtcNow.AddSeconds(10)
                do {
                    $savedInfo = & dotnet (Resolve-Path -LiteralPath $SelfTestAssembly).Path --inspect-chat (Join-Path $caseDirectory 'TgStorage.db') $ChatId
                    $saved = ($savedInfo -join "`n") -match ('AutoUpdate=' + $enabled + ';')
                    if (-not $saved) { Start-Sleep -Milliseconds 100 }
                } until ($saved -or [DateTime]::UtcNow -gt $deadline)
                if (-not $saved) { throw "Автообновление не сохранилось в БД: $enabled" }
            }
            if ($toggle.Current.ToggleState -ne $originalState) { throw 'Переключатель не вернулся в исходное состояние' }
            Write-Output 'PASS: поле процента доступно только для чтения; оба состояния автообновления сохранены настоящим интерфейсом'
            Save-AppSnapshot 'Download'
            $details = Wait-AppElement '.*' '' 'ChatDetails'
            $expand = $details.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
            if ($expand.Current.ExpandCollapseState -eq [System.Windows.Automation.ExpandCollapseState]::Collapsed) { $expand.Expand() }
            Start-Sleep -Milliseconds 700
            $window = [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
            $viewer = $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants,
                [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::AutomationIdProperty, 'ChatOverviewScroll'))
            $scroll = $viewer.GetCurrentPattern([System.Windows.Automation.ScrollPattern]::Pattern)
            if ($scroll.Current.VerticallyScrollable) { $scroll.SetScrollPercent(-1, 50) }
            Start-Sleep -Milliseconds 150
            $edits = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants,
                [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty,
                    [System.Windows.Automation.ControlType]::Edit))
            $titleEdit = $null
            $values = foreach ($edit in $edits) {
                $value = $null
                if ($edit.TryGetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern, [ref]$value)) {
                    if ($value.Current.Value -eq $expectedTitle) { $titleEdit = $edit }
                    $value.Current.Value
                }
            }
            if ($expectedTitle -notin $values) { throw "Обзор не отобразил сведения выбранного чата: $expectedTitle" }
            # Фокус удерживает сведения в кадре при активации окна вместо возврата к прежнему полю процента
            $titleEdit.SetFocus()
            Start-Sleep -Milliseconds 200
            if ($titleEdit.Current.IsOffscreen) { throw 'Поле названия чата недоступно в раскрытой секции' }
            Save-AppSnapshot 'Details'
            $expand.Collapse()
            Write-Output 'PASS: сведения выбранного чата доступны внутри обзора без Telegram'
            $advanced = Wait-AppElement '.*' '' 'ChatAdvanced'
            $expand = $advanced.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
            $expand.Expand()
            foreach ($optionId in @('ChatRewriteMessages', 'ChatRewriteFiles', 'ChatFileNameByMessage')) {
                [void](Wait-AppElement '.*' '' $optionId)
            }
            Save-AppSnapshot 'Advanced'
            $expand.Collapse()
            $viewer = $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants,
                [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::AutomationIdProperty, 'ChatOverviewScroll'))
            $scroll = $viewer.GetCurrentPattern([System.Windows.Automation.ScrollPattern]::Pattern)
            if ($scroll.Current.VerticallyScrollable) { $scroll.SetScrollPercent(-1, 0) }
        }
        if ($models[$index] -eq 'MyMessages') {
            if (-not $SourceDatabase) {
                [void](Wait-AppElement '^Alex Researcher$')
                [void](Wait-AppElement '^(Контакты|Contact details)$')
                [void](Wait-AppElement '^(В этом чате|In this chat)$')
                Write-Output 'PASS: карточка моего профиля показывает пользователя, контакты и сведения чата из тестовой БД'
            }
            Save-AppSnapshot 'Profile'
        }
        if ($models[$index] -eq 'Statistics') {
            [void](Wait-AppElement '^(Активность авторов|Author activity)$')
            [void](Wait-AppElement '^(Выберите период и рассчитайте статистику|Select a period and calculate statistics)$')
            $period = Wait-AppElement '.*' '' 'ChatStatisticsPeriod'
            $expand = $period.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
            $expand.Collapse()
            Start-Sleep -Milliseconds 200
            if ($expand.Current.ExpandCollapseState -ne [System.Windows.Automation.ExpandCollapseState]::Collapsed) {
                throw 'Период статистики не сворачивается для просмотра результатов'
            }
            $expand.Expand()
            [void](Wait-AppElement '^(Активность авторов|Author activity)$')
            Save-AppSnapshot 'Statistics'
            Write-Output 'PASS: статистика показывает период и объяснение до расчёта; подключение Telegram не используется'
        }
        if ($models[$index] -eq 'Participants') {
            if ($SourceDatabase) {
                Invoke-AppElement (Wait-AppElement '.*' '' 'ChatParticipantsActions')
                Invoke-AppElement (Wait-AppElement '.*' '' 'ChatParticipantsFromMessages')
                Invoke-AppElement (Wait-AppElement '^(Да|Yes)$')
            }
            $catalog = Wait-AppElement '.*' '' 'ChatParticipantsCatalog'
            $deadline = [DateTime]::UtcNow.AddSeconds(15)
            do {
                $participantRows = $catalog.FindAll([System.Windows.Automation.TreeScope]::Descendants,
                    [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty,
                        [System.Windows.Automation.ControlType]::ListItem))
                if ($participantRows.Count -eq 0) { Start-Sleep -Milliseconds 100 }
            } until ($participantRows.Count -gt 0 -or [DateTime]::UtcNow -gt $deadline)
            if ($participantRows.Count -eq 0) { throw 'Участники не появились в каталоге' }
            [void](Wait-AppElement '.*' '' 'ChatParticipantDossier')
            if (-not $SourceDatabase) {
                if ($participantRows.Count -ne 2) { throw 'Каталог потерял синтетического участника' }
                foreach ($row in $participantRows) {
                    Invoke-AppElement $row
                    Start-Sleep -Milliseconds 200
                    $dossier = Wait-AppElement '.*' '' 'ChatParticipantDossier'
                    $texts = $dossier.FindAll([System.Windows.Automation.TreeScope]::Descendants,
                        [System.Windows.Automation.Condition]::TrueCondition) | ForEach-Object { $_.Current.Name }
                    $rowTexts = $row.FindAll([System.Windows.Automation.TreeScope]::Descendants,
                        [System.Windows.Automation.Condition]::TrueCondition) | ForEach-Object { $_.Current.Name }
                    $expected = if ('Maria Reader' -in $rowTexts) { 'Maria Reader' } else { 'Alex Researcher' }
                    if ($expected -notin $texts) { throw "Выбор участника не обновил досье: $expected" }
                }
            }
            if (-not $SourceDatabase) {
                $search = Wait-AppElement '.*' '' 'ChatParticipantsSearch'
                $search.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue('selftest_maria')
                Invoke-AppElement (Wait-AppElement '.*' '' 'ChatParticipantsFind')
                $deadline = [DateTime]::UtcNow.AddSeconds(10)
                do {
                    $dossier = Wait-AppElement '.*' '' 'ChatParticipantDossier'
                    $texts = $dossier.FindAll([System.Windows.Automation.TreeScope]::Descendants,
                        [System.Windows.Automation.Condition]::TrueCondition) | ForEach-Object { $_.Current.Name }
                    $found = 'Maria Reader' -in $texts -and 'Alex Researcher' -notin $texts
                    if (-not $found) { Start-Sleep -Milliseconds 100 }
                } until ($found -or [DateTime]::UtcNow -gt $deadline)
                if (-not $found) { throw 'Поиск не отфильтровал каталог и досье' }
                $search.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue('')
                Invoke-AppElement (Wait-AppElement '.*' '' 'ChatParticipantsFind')
                [void](Wait-AppElement '^Alex Researcher$')
                Write-Output 'PASS: настоящая команда поиска фильтрует каталог и обновляет досье без исключения'
            }
            Write-Output "PASS: открытие каталога участников и выбор досье; строк: $($participantRows.Count)"
            Save-AppSnapshot 'Participants'
        }
        if ($models[$index] -eq 'Content') {
            $feed = Wait-AppElement '.*' '' 'ChatMessagesFeed'
            $messageRows = $feed.FindAll([System.Windows.Automation.TreeScope]::Descendants,
                [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty,
                    [System.Windows.Automation.ControlType]::ListItem))
            if ($messageRows.Count -eq 0) { throw 'Содержимое не показало сообщения выбранного чата' }
            Write-Output "PASS: непустая лента выбранного чата; видимых контейнеров: $($messageRows.Count)"
            Save-AppSnapshot 'Content'
            if ($VideoFileName) {
                $feed.GetCurrentPattern([System.Windows.Automation.ScrollPattern]::Pattern).SetScrollPercent(-1, 100)
                Start-Sleep -Milliseconds 300
                $image = Wait-AppElement ('(^|[\\/ ])' + [regex]::Escape($VideoFileName) + '$') '' 'MessageThumbnail'
                Save-AppSnapshot 'VideoCaption'
                $beforePlayers = @(Get-Process | Where-Object { $_.MainWindowTitle -like ('*' + $VideoFileName + '*') } | Select-Object -ExpandProperty Id)
                $bounds = $image.Current.BoundingRectangle
                Invoke-AppPoint ($bounds.Left + $bounds.Width / 2) ($bounds.Top + $bounds.Height / 2)
                $deadline = [DateTime]::UtcNow.AddSeconds(15)
                do {
                    $player = Get-Process | Where-Object { $_.MainWindowTitle -like ('*' + $VideoFileName + '*') } | Select-Object -First 1
                    if ($null -eq $player) { Start-Sleep -Milliseconds 100 }
                } until ($null -ne $player -or [DateTime]::UtcNow -gt $deadline)
                if ($null -eq $player) { throw 'Нажатие обложки не открыло оригинал в системном проигрывателе' }
                try {
                    Start-Sleep -Milliseconds 1200
                    $playerWindow = [System.Windows.Automation.AutomationElement]::FromHandle($player.MainWindowHandle)
                    $playerWindow.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition) |
                        ForEach-Object { "$($_.Current.ControlType.ProgrammaticName) | $($_.Current.Name)" } |
                        Set-Content -LiteralPath (Join-Path $caseDirectory 'player.txt') -Encoding utf8
                    Write-Output "PASS: обложка открыла настоящий оригинал в системном проигрывателе: $($player.MainWindowTitle)"
                }
                finally {
                    if ($player.Id -notin $beforePlayers) { [void]$player.CloseMainWindow(); [void]$player.WaitForExit(5000) }
                    $player.Dispose()
                }
            }
        }
        if ($CheckMediaPreview -and $models[$index] -eq 'Content') {
            Add-Type -AssemblyName System.Drawing
            $scroll = $feed.GetCurrentPattern([System.Windows.Automation.ScrollPattern]::Pattern)
            if ($scroll.Current.VerticallyScrollable) { $scroll.SetScrollPercent(-1, 0) }
            Start-Sleep -Milliseconds 200
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
            # Фото открывается внутри ленты; обложка видео теперь запускает оригинал во внешнем проигрывателе
            if ($scroll.Current.VerticallyScrollable) { $scroll.SetScrollPercent(-1, 100) }
            Start-Sleep -Milliseconds 200
            $image = Wait-AppElement '^preview.png$' '' 'MessageImage'
            $bounds = $image.Current.BoundingRectangle
            Invoke-AppPoint ($bounds.Left + $bounds.Width / 2) ($bounds.Top + $bounds.Height / 2)
            $preview = Wait-AppElement '.*' '' 'ChatFullScreenImage'
            $previewBounds = $preview.Current.BoundingRectangle
            $feedBounds = $feed.Current.BoundingRectangle
            $outside = @(
                @(($feedBounds.Left + 4), ($feedBounds.Top + 4)), @(($feedBounds.Right - 4), ($feedBounds.Top + 4)),
                @(($feedBounds.Left + 4), ($feedBounds.Bottom - 4)), @(($feedBounds.Right - 4), ($feedBounds.Bottom - 4))
            ) | Where-Object { -not $previewBounds.Contains($_[0], $_[1]) } | Select-Object -First 1
            if ($null -eq $outside) { throw "Не найден фон вне изображения: preview=$previewBounds; feed=$feedBounds" }
            Invoke-AppPoint $outside[0] $outside[1]
            Assert-ImageViewerClosed
            Invoke-AppPoint ($bounds.Left + $bounds.Width / 2) ($bounds.Top + $bounds.Height / 2)
            $preview = Wait-AppElement '.*' '' 'ChatFullScreenImage'
            $previewBounds = $preview.Current.BoundingRectangle
            Invoke-AppPoint ($previewBounds.Left + $previewBounds.Width / 2) ($previewBounds.Top + $previewBounds.Height / 2)
            Assert-ImageViewerClosed
            Invoke-AppPoint ($bounds.Left + $bounds.Width / 2) ($bounds.Top + $bounds.Height / 2)
            [void](Wait-AppElement '.*' '' 'ChatFullScreenImage')
            Invoke-AppElement (Wait-AppElement '^(Мой профиль в чате|My chat profile)$' 'Microsoft.UI.Xaml.Controls.SelectorBarItem')
            [void](Wait-AppElement '^Alex Researcher$')
            Invoke-AppElement (Wait-AppElement '^(Все сообщения|All messages)$' 'Microsoft.UI.Xaml.Controls.SelectorBarItem')
            [void](Wait-AppElement '^preview.png$' '' 'MessageImage')
            Assert-ImageViewerClosed
            Write-Output 'PASS: просмотр закрывается по фону и изображению; возврат к ленте сбрасывает незакрытый просмотр'
            # Проверяем скрытие всего содержимого, включая обложку, через настоящий переключатель
            $sensitive = Wait-AppElement '.*' '' 'ToggleSwitchShowSensitiveData'
            $toggle = $sensitive.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern)
            $toggle.Toggle()
            Start-Sleep -Milliseconds 200
            $hiddenImages = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants,
                [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::AutomationIdProperty,'MessageImage'))
            if (@($hiddenImages | Where-Object { -not $_.Current.IsOffscreen }).Count -gt 0) { throw 'Обложка не скрылась вместе с конфиденциальными данными' }
            $toggle.Toggle()
            [void](Wait-AppElement '^preview.png$' '' 'MessageImage')
            Invoke-AppElement (Wait-AppElement '.*' '' 'ClearChatContent')
            Invoke-AppElement (Wait-AppElement '^(Да|Yes)$')
            [void](Wait-AppElement '^(Пустые данные|Empty data)$')
            $toggle.Toggle()
            Start-Sleep -Milliseconds 200
            $emptyLabels = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants,
                [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty,
                    [System.Windows.Automation.ControlType]::Text)) | Where-Object { $_.Current.Name -match '^(Пустые данные|Empty data)$' }
            if (@($emptyLabels | Where-Object { -not $_.Current.IsOffscreen }).Count -gt 0) {
                throw 'Пустое состояние перекрывает сообщение конфиденциальности'
            }
            $toggle.Toggle()
            Write-Output 'PASS: скрытие содержимого и подсказка пустого списка после очистки'
        }
    }
    Invoke-AppElement (Wait-AppElement '^(Обзор|Overview)$' 'Microsoft.UI.Xaml.Controls.SelectorBarItem')
    Invoke-AppElement (Wait-AppElement '^(Сетевая активность|Network activity)$' '' 'ChatOpenNetworkActivity')
    [void](Wait-AppElement '^(Операции OpenTgResearcher|OpenTgResearcher operations)$')
    Save-AppSnapshot 'NetworkActivity'
    Write-Output 'PASS: кнопка вкладки загрузки открыла страницу сетевой активности'
    if ($appProcess.WaitForExit(2000)) { throw "Desktop вылетел после проверки вкладок: код $($appProcess.ExitCode)" }
    $log = Get-NewAppLog
    $log | Set-Content -LiteralPath (Join-Path $caseDirectory 'page.log') -Encoding utf8
    if ($log -notmatch 'TgChatViewModel.*Page loaded') { throw 'Журнал не подтвердил загрузку страницы чата' }
    if ($log -match 'Content does not support UIElement|App_UnhandledException|Unhandled exception in|Object reference not set') { throw 'В журнале найдено необработанное исключение' }
    if (-not $appProcess.CloseMainWindow() -or -not $appProcess.WaitForExit(15000)) { throw 'Desktop не завершил закрытие окна' }
    if ($appProcess.ExitCode -ne 0) { throw "Desktop закрылся с ошибкой: код $($appProcess.ExitCode)" }
    if ($ContentOnly) { Write-Output "PASS: содержимое выбранного чата ($Language, $Theme), без исключений; $caseDirectory" }
    else { Write-Output "PASS: пульт чата ($Language, $Theme), четыре вкладки, локальная навигация содержимого, секции настроек, без исключений; $caseDirectory" }
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


