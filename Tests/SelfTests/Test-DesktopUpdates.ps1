param(
    [Parameter(Mandatory = $true)]
    [string]$ExecutablePath,
    [ValidateSet('Russian', 'English')]
    [string]$Language = 'Russian',
    [switch]$Toolbar
)

$ErrorActionPreference = 'Stop'
$executable = (Resolve-Path -LiteralPath $ExecutablePath).Path
$caseDirectory = Join-Path $PSScriptRoot ('bin/UpdatesPage/' + [Guid]::NewGuid().ToString('N'))
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

# Находим доступный элемент только в окнах тестового процесса, включая всплывающие меню
function Wait-AppElement([string]$namePattern) {
    $deadline = [DateTime]::UtcNow.AddSeconds(30)
    while ([DateTime]::UtcNow -lt $deadline) {
        if ($appProcess.HasExited) { throw "Desktop завершился до появления элемента: $namePattern; код $($appProcess.ExitCode)" }
        $appProcess.Refresh()
        if ($appProcess.MainWindowHandle -ne [IntPtr]::Zero) {
            $window = [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
            try {
                $processCondition = [System.Windows.Automation.PropertyCondition]::new(
                    [System.Windows.Automation.AutomationElement]::ProcessIdProperty, $appProcess.Id)
                $elements = [System.Windows.Automation.AutomationElement]::RootElement.FindAll(
                    [System.Windows.Automation.TreeScope]::Descendants, $processCondition)
            }
            catch {
                # Открытие диалога временно перестраивает дерево UI Automation
                if ($appProcess.HasExited) { throw }
                Start-Sleep -Milliseconds 100
                continue
            }
            foreach ($element in $elements) {
                if ($element.Current.Name -match $namePattern -and $element.Current.IsEnabled -and -not $element.Current.IsOffscreen) { return $element }
            }
        }
        Start-Sleep -Milliseconds 100
    }
    $elements | ForEach-Object { "$($_.Current.ClassName) | $($_.Current.Name) | $($_.Current.IsOffscreen)" } |
        Set-Content -LiteralPath (Join-Path $caseDirectory 'ui.txt') -Encoding utf8
    Save-AppSnapshot 'failure'
    throw "В окне Desktop не найден элемент: $namePattern"
}

# Выполняем штатное действие кнопки или пункта навигации
function Invoke-AppElement($element) {
    $current = $element
    while ($null -ne $current) {
        $pattern = $null
        if ($current.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$pattern)) {
            $deadline = [DateTime]::UtcNow.AddSeconds(5)
            while (-not $current.Current.IsEnabled -and [DateTime]::UtcNow -lt $deadline) { Start-Sleep -Milliseconds 50 }
            if (-not $current.Current.IsEnabled) { throw 'Кнопка не стала доступной после закрытия диалога' }
            Add-Content -LiteralPath (Join-Path $caseDirectory 'actions.log') -Value "Invoke | $($current.Current.ClassName) | $($current.Current.Name)"
            $pattern.Invoke()
            return
        }
        if ($current.TryGetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern, [ref]$pattern)) { $pattern.Select(); return }
        if ($current.TryGetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern, [ref]$pattern)) {
            # Вызываем действие и для уже выбранного варианта, чтобы закрыть меню
            Add-Content -LiteralPath (Join-Path $caseDirectory 'actions.log') -Value "Toggle | $($current.Current.ClassName) | $($current.Current.Name)"
            $pattern.Toggle()
            return
        }
        $current = [System.Windows.Automation.TreeWalker]::ControlViewWalker.GetParent($current)
    }
    throw "Элемент не поддерживает вызов или выбор: $($element.Current.Name)"
}

# Нажимаем кнопки панели мышью, проверяя доступность поверх области перетаскивания
function Invoke-ToolbarButton([string]$id) {
    $deadline = [DateTime]::UtcNow.AddSeconds(5)
    do {
        $button = Get-ToolbarButton $id
        if ($button.Current.IsEnabled) { break }
        Start-Sleep -Milliseconds 50
    } while ([DateTime]::UtcNow -lt $deadline)
    if (-not $button.Current.IsEnabled) { throw "Кнопка панели не стала доступной: $id" }
    # Даём завершиться анимации закрытия предыдущего меню или подтверждения
    Start-Sleep -Milliseconds 350
    [void][ToolbarTestNative]::SetForegroundWindow($appProcess.MainWindowHandle)
    Start-Sleep -Milliseconds 150
    $rect = $button.Current.BoundingRectangle
    Add-Content -LiteralPath (Join-Path $caseDirectory 'actions.log') -Value "Click | $id | $rect"
    [ToolbarTestNative]::Click([int]($rect.Left + $rect.Width / 2), [int]($rect.Top + $rect.Height / 2))
}

# Находим кнопку панели по идентификатору независимо от языка и видимых подписей
function Get-ToolbarButton([string]$id) {
    $window = [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
    $condition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::AutomationIdProperty, $id)
    $button = $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
    if ($null -eq $button -or $button.Current.IsOffscreen) { throw "Не найдена кнопка панели: $id" }
    return $button
}

# Проверяем реальную геометрию кнопок относительно окна и друг друга
function Test-ToolbarBounds {
    $window = [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
    $windowRect = $window.Current.BoundingRectangle
    $previousRight = $windowRect.Left
    foreach ($id in @('ToolbarLanguage', 'ToolbarTheme', 'ToggleSwitchShowSensitiveData', 'ToolbarUpdate', 'ToolbarConnect', 'ToolbarDisconnect')) {
        $button = Get-ToolbarButton $id
        $rect = $button.Current.BoundingRectangle
        if ($rect.Width -lt 32 -or $rect.Height -lt 32 -or $rect.Left -lt $previousRight -or $rect.Right -gt $windowRect.Right - 100 -or
            $rect.Top -lt $windowRect.Top -or $rect.Bottom -gt $windowRect.Top + 64) {
            throw "Кнопка $id обрезана или перекрывается: $rect; окно $windowRect"
        }
        if ([string]::IsNullOrWhiteSpace($button.Current.Name)) { throw "У кнопки $id нет доступной подписи" }
        $previousRight = $rect.Right
    }
    $labelCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::AutomationIdProperty, 'ToolbarSensitiveDataLabel')
    $label = $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $labelCondition)
    if ($null -ne $label -and -not $label.Current.IsOffscreen) {
        $toggleRect = (Get-ToolbarButton 'ToggleSwitchShowSensitiveData').Current.BoundingRectangle
        $gap = $label.Current.BoundingRectangle.Left - $toggleRect.Right
        if ($gap -lt 0 -or $gap -gt $toggleRect.Height / 4) { throw "Слишком большой отступ подписи переключателя: $gap" }
    }
}

# Проверяем фактическое состояние общего переключателя после смены страниц и языка
function Assert-SensitiveDataState([bool]$shown) {
    $deadline = [DateTime]::UtcNow.AddSeconds(5)
    do {
        $control = Get-ToolbarButton 'ToggleSwitchShowSensitiveData'
        $pattern = $control.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern)
        $actual = $pattern.Current.ToggleState -eq [System.Windows.Automation.ToggleState]::On
        if ($actual -eq $shown) { break }
        Start-Sleep -Milliseconds 100
    } while ([DateTime]::UtcNow -lt $deadline)
    if ($actual -ne $shown) { throw "Переключатель данных сбросил состояние: ожидалось $shown" }
    if ($control.Current.Name -notmatch '^(Показывать конфиденциальные данные на всех страницах|Show sensitive data on all pages)$') {
        throw "Переключатель данных не локализован: $($control.Current.Name)"
    }
}

# Проверяем результат скрытия на настоящем поле настроек, содержащем путь только к тестовой БД
function Assert-SettingsStorageShown([bool]$shown) {
    $deadline = [DateTime]::UtcNow.AddSeconds(5)
    do {
        $window = [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
        $edits = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants,
            [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty,
                [System.Windows.Automation.ControlType]::Edit))
        $found = $false
        foreach ($edit in $edits) {
            $pattern = $null
            if ($edit.TryGetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern, [ref]$pattern) -and
                $pattern.Current.Value -eq (Join-Path $caseDirectory 'TgStorage.db')) { $found = $true }
        }
        if ($found -eq $shown) { return }
        Start-Sleep -Milliseconds 100
    } while ([DateTime]::UtcNow -lt $deadline)
    throw "Видимость пути тестовой БД не соответствует переключателю: ожидалось $shown"
}

# Проверяем сохранение единственного выбранного параметра без доступа к настройкам пользователя
function Assert-ToolbarSetting([string]$key, [string]$expected) {
    $deadline = [DateTime]::UtcNow.AddSeconds(5)
    do {
        $saved = Get-Content -LiteralPath (Join-Path $caseDirectory 'Settings.json') -Raw | ConvertFrom-Json
        if ($saved.$key -eq $expected) { break }
        Start-Sleep -Milliseconds 50
    } while ([DateTime]::UtcNow -lt $deadline)
    if ($saved.$key -ne $expected) { throw "Выбор панели не сохранён: $key = $($saved.$key), ожидалось $expected" }
    if ($saved.AppStorage -ne (Join-Path $caseDirectory 'TgStorage.db') -or
        $saved.AppSession -ne (Join-Path $caseDirectory 'OpenTgResearcher.session')) { throw 'Панель изменила пути БД или сессии' }
    # UI Automation меняет флажок напрямую; закрываем меню перед следующим действием
    [ToolbarTestNative]::CloseMenu()
    Start-Sleep -Milliseconds 250
}

# Находим журнал по стабильному идентификатору и проверяем доступность копирования
function Get-UpdateLogElement {
    $window = [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
    $condition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::AutomationIdProperty, 'UpdateLog')
    $element = $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
    if ($null -eq $element) { throw 'Поле журнала обновлений не найдено' }
    $value = $element.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
    if (-not $value.Current.IsReadOnly) { throw 'Журнал допускает редактирование' }
    $text = $null
    if (-not $element.TryGetCurrentPattern([System.Windows.Automation.TextPattern]::Pattern, [ref]$text)) {
        throw 'Журнал недоступен для выделения и копирования'
    }
    return $element
}

# Ожидаем наблюдаемый результат команды в настоящем поле журнала
function Wait-UpdateLog([string]$expected) {
    $deadline = [DateTime]::UtcNow.AddSeconds(15)
    do {
        $element = Get-UpdateLogElement
        $value = $element.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).Current.Value
        if ($value -match $expected) { return $value }
        if ($appProcess.HasExited) { throw 'Desktop завершился во время проверки обновлений' }
        Start-Sleep -Milliseconds 100
    } while ([DateTime]::UtcNow -lt $deadline)
    throw "В журнале отсутствует результат $expected : $value"
}

# Сохраняем снимок только тестового окна
function Save-AppSnapshot([string]$name) {
    Add-Type -AssemblyName System.Drawing
    Start-Sleep -Milliseconds 300
    $window = [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
    $rect = $window.Current.BoundingRectangle
    $bitmap = [Drawing.Bitmap]::new([int]$rect.Width, [int]$rect.Height)
    $graphics = [Drawing.Graphics]::FromImage($bitmap)
    try {
        $graphics.CopyFromScreen([int]$rect.Left, [int]$rect.Top, 0, 0, $bitmap.Size)
        $bitmap.Save((Join-Path $caseDirectory ($name + '.png')))
    }
    finally { $graphics.Dispose(); $bitmap.Dispose() }
}

try {
    if (Get-Process OpenTgResearcherDesktop -ErrorAction SilentlyContinue) { throw 'Перед селфтестом закройте запущенный Desktop' }
    Add-Type -AssemblyName UIAutomationClient
    Add-Type -AssemblyName UIAutomationTypes
    Add-Type -TypeDefinition @'
public static class ToolbarTestNative
{
    /// <summary> Активирует только тестовое окно перед нажатием </summary>
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(System.IntPtr window);
    // Перемещаем указатель к проверяемой кнопке
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);
    // Передаём нажатие мыши в тестовое окно
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern void mouse_event(uint flags, uint x, uint y, uint data, System.UIntPtr extra);
    // Передаём клавишу закрытия открытого меню
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern void keybd_event(byte key, byte scan, uint flags, System.UIntPtr extra);
    /// <summary> Закрывает меню после выбора через UI Automation </summary>
    public static void CloseMenu()
    {
        keybd_event(0x1B, 0, 0, System.UIntPtr.Zero);
        keybd_event(0x1B, 0, 0x0002, System.UIntPtr.Zero);
    }
    /// <summary> Нажимает элемент по его фактическим координатам </summary>
    public static void Click(int x, int y)
    {
        SetCursorPos(x, y);
        mouse_event(0x0002, 0, 0, 0, System.UIntPtr.Zero);
        mouse_event(0x0004, 0, 0, 0, System.UIntPtr.Zero);
    }
}
'@
    New-Item -ItemType Directory -Path $caseDirectory -Force | Out-Null
    @{
        AppStorage = Join-Path $caseDirectory 'TgStorage.db'
        AppSession = Join-Path $caseDirectory 'OpenTgResearcher.session'
        AppLanguage = $Language
        AppTheme = 'Default'
    } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $caseDirectory 'Settings.json') -Encoding utf8
    $start = [Diagnostics.ProcessStartInfo]::new($executable)
    $start.WorkingDirectory = Split-Path -Parent $executable
    $start.UseShellExecute = $false
    $start.Environment['LocalSettingsOptions__ApplicationDataFolder'] = $caseDirectory
    $start.Environment['LocalSettingsOptions__LocalSettingsFile'] = 'Settings.json'
    $start.Environment['DOTNET_ENVIRONMENT'] = 'Production'
    $start.Environment['Logging__LogLevel__Default'] = 'Warning'
    $appProcess = [Diagnostics.Process]::Start($start)
    Invoke-AppElement (Wait-AppElement '^(Продолжить|Continue)$')
    $window = [System.Windows.Automation.AutomationElement]::FromHandle($appProcess.MainWindowHandle)
    $window.GetCurrentPattern([System.Windows.Automation.WindowPattern]::Pattern).SetWindowVisualState(
        [System.Windows.Automation.WindowVisualState]::Normal)
    $window.GetCurrentPattern([System.Windows.Automation.TransformPattern]::Pattern).Resize(1280, 900)
    if ($Toolbar) {
        Start-Sleep -Milliseconds 400
        Test-ToolbarBounds
        Save-AppSnapshot 'toolbar-wide'
        Assert-SensitiveDataState $true
        Invoke-ToolbarButton 'ToggleSwitchShowSensitiveData'
        Assert-SensitiveDataState $false
        $window.GetCurrentPattern([System.Windows.Automation.TransformPattern]::Pattern).Resize(800, 800)
        Start-Sleep -Milliseconds 400
        Test-ToolbarBounds
        Save-AppSnapshot 'toolbar-compact'
        Invoke-ToolbarButton 'ToolbarLanguage'
        Invoke-AppElement (Wait-AppElement '^English$')
        Assert-ToolbarSetting 'AppLanguage' 'English'
        $deadline = [DateTime]::UtcNow.AddSeconds(5)
        do {
            $name = (Get-ToolbarButton 'ToolbarUpdate').Current.Name
            if ($name -eq 'Check for updates') { break }
            Start-Sleep -Milliseconds 50
        } while ([DateTime]::UtcNow -lt $deadline)
        if ($name -ne 'Check for updates') { throw 'Подпись верхней панели не переключилась на английский' }
        Assert-SensitiveDataState $false
        Invoke-ToolbarButton 'ToolbarTheme'
        Save-AppSnapshot 'toolbar-theme-menu'
        Invoke-AppElement (Wait-AppElement '^Dark$')
        Assert-ToolbarSetting 'AppTheme' 'Dark'
        Save-AppSnapshot 'toolbar-dark'
        Invoke-ToolbarButton 'ToolbarTheme'
        Invoke-AppElement (Wait-AppElement '^Light$')
        Assert-ToolbarSetting 'AppTheme' 'Light'
        Save-AppSnapshot 'toolbar-light'
        Invoke-ToolbarButton 'ToolbarTheme'
        Invoke-AppElement (Wait-AppElement '^System theme$')
        Assert-ToolbarSetting 'AppTheme' 'Default'
        Invoke-ToolbarButton 'ToolbarLanguage'
        Save-AppSnapshot 'toolbar-language-menu'
        Invoke-AppElement (Wait-AppElement '^Русский$')
        Assert-ToolbarSetting 'AppLanguage' 'Russian'
        [void](Wait-AppElement '^Проверить обновления$')
        if ($Language -eq 'English') {
            Invoke-ToolbarButton 'ToolbarLanguage'
            Invoke-AppElement (Wait-AppElement '^English$')
            Assert-ToolbarSetting 'AppLanguage' 'English'
            [void](Wait-AppElement '^Check for updates$')
        }
        Invoke-ToolbarButton 'ToolbarUpdate'
        Invoke-AppElement (Wait-AppElement '^(Отмена|Cancel)$')
        Assert-SensitiveDataState $false
        Invoke-ToolbarButton 'ToggleSwitchShowSensitiveData'
        Assert-SensitiveDataState $true
        Invoke-ToolbarButton 'ToggleSwitchShowSensitiveData'
        Assert-SensitiveDataState $false
        if ((Get-UpdateLogElement).GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).Current.Value.Length -ne 0) {
            throw 'Отказ от обновления из панели изменил журнал'
        }
        Invoke-ToolbarButton 'ToolbarUpdate'
        Invoke-AppElement (Wait-AppElement '^(Да|Yes)$')
        [void](Wait-UpdateLog 'Setup\.exe')
        Test-ToolbarBounds
        Save-AppSnapshot 'toolbar-update'
        $window.GetCurrentPattern([System.Windows.Automation.TransformPattern]::Pattern).Resize(1280, 900)
        Start-Sleep -Milliseconds 400
        Test-ToolbarBounds
    }
    $settingsCaption = if ($Language -eq 'Russian') { 'Настройки' } else { 'Settings' }
    $settings = Wait-AppElement ('^' + [regex]::Escape($settingsCaption) + '$')
    Save-AppSnapshot 'home'
    Invoke-AppElement $settings
    [void](Wait-AppElement '^(Язык|Language)$')
    if ($Toolbar) {
        Assert-SensitiveDataState $false
        Invoke-ToolbarButton 'ToggleSwitchShowSensitiveData'
        Assert-SensitiveDataState $true
        Assert-SettingsStorageShown $true
        Save-AppSnapshot 'settings-data-shown'
        Invoke-ToolbarButton 'ToggleSwitchShowSensitiveData'
        Assert-SensitiveDataState $false
        Assert-SettingsStorageShown $false
        Save-AppSnapshot 'settings-data-hidden'
    }
    Invoke-AppElement (Wait-AppElement '^(Обновление|Update)$')
    if ($Toolbar) { Assert-SensitiveDataState $false }
    $releaseCaption = if ($Language -eq 'Russian') { 'Проверка наличия обновлений релиза' } else { 'Checking for release updates' }
    $previewCaption = if ($Language -eq 'Russian') { 'Проверка обновлений предварительного просмотра' } else { 'Checking for preview updates' }
    $release = Wait-AppElement ('^' + [regex]::Escape($releaseCaption) + '$')
    [void](Wait-AppElement ('^' + [regex]::Escape($previewCaption) + '$'))
    [void](Get-UpdateLogElement)
    $logBeforeCancel = (Get-UpdateLogElement).GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).Current.Value
    Invoke-AppElement $release
    Invoke-AppElement (Wait-AppElement '^(Отмена|Cancel)$')
    if ((Get-UpdateLogElement).GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).Current.Value -ne $logBeforeCancel) {
        throw 'Отмена подтверждения изменила журнал обновлений'
    }
    Invoke-AppElement (Wait-AppElement ('^' + [regex]::Escape($releaseCaption) + '$'))
    Invoke-AppElement (Wait-AppElement '^(Да|Yes)$')
    $log = Wait-UpdateLog 'Setup\.exe'
    $expected = if ($Language -eq 'Russian') { 'Встроенное обновление доступно' } else { 'Built-in updates are available' }
    if ($log -notmatch $expected) { throw "Инструкция запуска без установки не переведена: $log" }
    Invoke-AppElement (Wait-AppElement ('^' + [regex]::Escape($previewCaption) + '$'))
    Invoke-AppElement (Wait-AppElement '^(Да|Yes)$')
    $log = Wait-UpdateLog 'Setup\.exe'
    [void](Wait-AppElement ('^' + [regex]::Escape($releaseCaption) + '$'))
    Save-AppSnapshot 'updates'
    $log | Set-Content -LiteralPath (Join-Path $caseDirectory 'update-log.txt') -Encoding utf8
    $newLog = Get-NewAppLog
    if ($newLog -notmatch 'TgUpdateViewModel.*Page loaded') { throw 'Нет подтверждения загрузки страницы обновлений' }
    if ($newLog -match 'App_UnhandledException|COMException|System.InvalidOperationException') { throw 'В журнале найдено исключение' }
    if (-not $appProcess.CloseMainWindow() -or -not $appProcess.WaitForExit(15000)) { throw 'Desktop не завершил закрытие окна' }
    if ($appProcess.ExitCode -ne 0) { throw "Desktop завершился с кодом $($appProcess.ExitCode)" }
    if ($Toolbar) {
        Write-Output "PASS: верхняя панель ($Language), язык, все темы, обновление, ширина 800 и 1280"
        Write-Output 'PASS: общий переключатель скрывает поля и сохраняет состояние при навигации без подключения Telegram'
    }
    Write-Output "PASS: настройки и обновления ($Language), диалоги, журнал, инструкция установки, завершение с кодом 0; $caseDirectory"
}
catch {
    $_.ScriptStackTrace | Write-Output
    if ($appProcess -and -not $appProcess.HasExited) { Save-AppSnapshot 'failure' }
    throw
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
