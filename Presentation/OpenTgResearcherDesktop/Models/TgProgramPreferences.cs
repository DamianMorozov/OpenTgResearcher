namespace OpenTgResearcherDesktop.Models;

/// <summary> Параметры программы и отметка завершённой настройки установленной версии </summary>
public sealed partial class TgProgramPreferences(ITgSettingsService settings) : ObservableObject
{
    public const string PreferencesKey = "ProgramPreferences";
    public const string ConfiguredVersionKey = "ProgramConfiguredVersion";
    public const string DefaultAccentHex = "#0078D4";

    [ObservableProperty]
    public partial int LanguageIndex { get; set; }
    [ObservableProperty]
    public partial int ThemeIndex { get; set; }
    [ObservableProperty]
    public partial bool UseSystemAccent { get; set; } = true;
    [ObservableProperty]
    public partial string AccentHex { get; set; } = DefaultAccentHex;
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
        ThemeIndex = (int)settings.AppTheme;
        var saved = settings.ReadSetting<Values>(PreferencesKey)
            ?? new Values(UseSystemAccent: true, AccentHex: DefaultAccentHex, AutoStart: false, IncludePreview: false);
        UseSystemAccent = saved.UseSystemAccent;
        AccentHex = saved.AccentHex;
        AutoStart = saved.AutoStart;
        IncludePreview = saved.IncludePreview;
    }

    /// <summary> Применяет параметры и отмечает настройку версии после успешного применения </summary>
    public void Save(string version, Action<bool> setAutoStart, Action<bool, string> applyAccent)
    {
        if (!Enum.IsDefined((TgEnumLanguage)LanguageIndex) || !Enum.IsDefined((TgEnumTheme)ThemeIndex))
            throw new InvalidOperationException("Выберите язык и тему программы");
        if (AccentHex.Length != 7 || AccentHex[0] != '#' || !int.TryParse(AccentHex.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _))
            throw new InvalidOperationException("Укажите цвет акцента в формате #RRGGBB");
        setAutoStart(AutoStart);
        applyAccent(UseSystemAccent, AccentHex);
        settings.SaveSetting(PreferencesKey, new Values(UseSystemAccent, AccentHex, AutoStart, IncludePreview));
        settings.SaveTheme((TgEnumTheme)ThemeIndex);
        settings.SaveLanguage((TgEnumLanguage)LanguageIndex);
        settings.SaveSetting(ConfiguredVersionKey, version);
    }

    /// <summary> Сохраняемые параметры, общие для всех версий приложения </summary>
    public sealed record Values(bool UseSystemAccent, string AccentHex, bool AutoStart, bool IncludePreview);
}
