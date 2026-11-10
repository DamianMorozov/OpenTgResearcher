namespace OpenTgResearcherDesktop.Helpers;

/// <summary> Интеграция параметров программы с Windows и установленной версией Velopack </summary>
internal static class TgProgramIntegration
{
    private const string StartupKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string StartupApprovalKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";
    private const string StartupName = "OpenTgResearcherDesktop";
    private static readonly UISettings _uiSettings = new();
    private static bool _watchingAccent;

    public static string CurrentVersion => Velopack.Locators.VelopackLocator.Current.CurrentlyInstalledVersion?.ToString()
        ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? string.Empty;
    public static bool IsInstalled => Velopack.Locators.VelopackLocator.Current.CurrentlyInstalledVersion is not null
        && !Velopack.Locators.VelopackLocator.Current.IsPortable;

    /// <summary> Читает состояние автозапуска, включая отключение через Диспетчер задач </summary>
    public static bool ReadAutoStart()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(StartupKey);
            using var approval = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(StartupApprovalKey);
            return key?.GetValue(StartupName) is string && !(approval?.GetValue(StartupName) is byte[] state && state.Length > 0 && state[0] == 3);
        }
        catch (Exception ex) { TgLogUtils.WriteException(ex); return false; }
    }

    /// <summary> Регистрирует автозапуск текущего пользователя, отображаемый в Диспетчере задач </summary>
    public static void SetAutoStart(bool enabled)
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(StartupKey);
            if (!enabled) { key.DeleteValue(StartupName, throwOnMissingValue: false); return; }
            if (!IsInstalled) throw new InvalidOperationException("Автозапуск доступен после установки программы");
            var updater = Velopack.Locators.VelopackLocator.Current.UpdateExePath;
            if (string.IsNullOrEmpty(updater) || !File.Exists(updater)) throw new IOException("Не найден загрузчик установленной программы");
            // Стабильный загрузчик сохраняет автозапуск при замене каталога текущей версии
            key.SetValue(StartupName, $"\"{updater}\" start");
            using var approval = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(StartupApprovalKey, writable: true);
            approval?.DeleteValue(StartupName, throwOnMissingValue: false);
        }
        catch (Exception ex) { TgLogUtils.WriteException(ex); throw; }
    }

    /// <summary> Восстанавливает акцент и отслеживает изменение системной палитры </summary>
    public static void InitializeAppearance(ITgSettingsService settings)
    {
        ApplySavedAccent(settings);
        if (_watchingAccent) return;
        _watchingAccent = true;
        _uiSettings.ColorValuesChanged += (_, _) => TgDesktopUtils.InvokeOnUIThread(() =>
        {
            if (!App.IsClosing) ApplySavedAccent(settings);
        });
    }

    // Применяем сохранённую палитру без изменения других параметров
    private static void ApplySavedAccent(ITgSettingsService settings)
    {
        try
        {
            var preferences = new TgProgramPreferences(settings);
            preferences.Load();
            ApplyAccent(preferences.UseSystemAccent, preferences.AccentHex);
        }
        catch (Exception ex) { TgLogUtils.WriteException(ex); }
    }

    /// <summary> Возвращает непрозрачный цвет из пользовательского значения #RRGGBB </summary>
    public static Color ParseAccent(string hex)
    {
        if (hex.Length != 7 || hex[0] != '#') throw new FormatException("Укажите цвет акцента в формате #RRGGBB");
        var rgb = uint.Parse(hex.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
    }

    /// <summary> Применяет системный или пользовательский акцент к ресурсам WinUI </summary>
    public static void ApplyAccent(bool useSystem, string hex)
    {
        var accent = useSystem ? _uiSettings.GetColorValue(UIColorType.Accent) : ParseAccent(hex);
        var resources = Application.Current.Resources;
        resources["SystemAccentColor"] = accent;
        for (var level = 1; level <= 3; level++)
        {
            resources[$"SystemAccentColorLight{level}"] = useSystem
                ? _uiSettings.GetColorValue((UIColorType)((int)UIColorType.AccentLight1 + level - 1)) : Blend(accent, 255, level * 0.18);
            resources[$"SystemAccentColorDark{level}"] = useSystem
                ? _uiSettings.GetColorValue(level switch { 1 => UIColorType.AccentDark1, 2 => UIColorType.AccentDark2, _ => UIColorType.AccentDark3 })
                : Blend(accent, 0, level * 0.18);
        }
        foreach (var name in new[] { "SystemControlForegroundAccentBrush", "SystemControlBackgroundAccentBrush", "AccentFillColorDefaultBrush" })
        {
            if (resources.TryGetValue(name, out var value) && value is SolidColorBrush brush) brush.Color = accent;
            else resources[name] = new SolidColorBrush(accent);
        }
        // Перечитываем ThemeResource у уже созданных контролов
        if (App.MainWindow.Content is FrameworkElement root)
        {
            var theme = root.RequestedTheme;
            root.RequestedTheme = root.ActualTheme == ElementTheme.Dark ? ElementTheme.Light : ElementTheme.Dark;
            root.RequestedTheme = theme;
        }
    }

    // Формируем светлые и тёмные оттенки пользовательского акцента
    private static Color Blend(Color color, byte target, double amount) => Color.FromArgb(255,
        (byte)(color.R + (target - color.R) * amount), (byte)(color.G + (target - color.G) * amount), (byte)(color.B + (target - color.B) * amount));
}
