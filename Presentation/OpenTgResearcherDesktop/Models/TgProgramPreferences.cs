namespace OpenTgResearcherDesktop.Models;

/// <summary> Параметры программы и отметка завершённой настройки установленной версии </summary>
public sealed partial class TgProgramPreferences(ITgSettingsService settings) : ObservableObject
{
    public const string PreferencesKey = "ProgramPreferences";
    public const string ConfiguredVersionKey = "ProgramConfiguredVersion";

    [ObservableProperty]
    public partial int LanguageIndex { get; set; }
    [ObservableProperty]
    public partial int ThemeIndex { get; set; } = 1;
    [ObservableProperty]
    public partial bool UseSystemTheme { get; set; } = true;
    [ObservableProperty]
    public partial bool UseSystemAccent { get; set; } = true;
    [ObservableProperty]
    public partial string AccentHex { get; set; } = "#0078D4";
    [ObservableProperty]
    public partial bool AutoStart { get; set; }
    [ObservableProperty]
    public partial bool IncludePreview { get; set; }

    /// <summary> Определяет необходимость настройки при первом запуске установленной версии </summary>
    public bool NeedsSetup(string version, bool installed) => installed && settings.ReadSetting<string>(ConfiguredVersionKey) != version;

    /// <summary> Загружает параметры без изменения сохранённых значений </summary>
    public void Load()
    {
        LanguageIndex = (int)settings.AppLanguage;
        UseSystemTheme = settings.AppTheme == TgEnumTheme.Default;
        ThemeIndex = settings.AppTheme == TgEnumTheme.Dark ? 1 : 0;
        var saved = settings.ReadSetting<Values>(PreferencesKey);
        if (saved is null) return;
        UseSystemAccent = saved.UseSystemAccent;
        AccentHex = saved.AccentHex;
        AutoStart = saved.AutoStart;
        IncludePreview = saved.IncludePreview;
    }

    /// <summary> Применяет параметры и отмечает настройку версии после успешного применения </summary>
    public void Save(string version, Action<bool> setAutoStart, Action<bool, string> applyAccent)
    {
        if (!Enum.IsDefined((TgEnumLanguage)LanguageIndex) || ThemeIndex is < 0 or > 1)
            throw new InvalidOperationException("Выберите язык и тему программы");
        if (AccentHex.Length != 7 || AccentHex[0] != '#' || !int.TryParse(AccentHex.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _))
            throw new InvalidOperationException("Укажите цвет акцента в формате #RRGGBB");
        setAutoStart(AutoStart);
        applyAccent(UseSystemAccent, AccentHex);
        settings.SaveSetting(PreferencesKey, new Values(UseSystemAccent, AccentHex, AutoStart, IncludePreview));
        settings.SaveTheme(UseSystemTheme ? TgEnumTheme.Default : ThemeIndex == 1 ? TgEnumTheme.Dark : TgEnumTheme.Light);
        settings.SaveLanguage((TgEnumLanguage)LanguageIndex);
        settings.SaveSetting(ConfiguredVersionKey, version);
    }

    /// <summary> Сохраняемые параметры, общие для всех версий приложения </summary>
    public sealed record Values(bool UseSystemAccent, string AccentHex, bool AutoStart, bool IncludePreview);
}
