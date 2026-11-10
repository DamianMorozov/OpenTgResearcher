namespace OpenTgResearcherDesktop.ViewModels;

/// <summary> Общие команды немедленного применения и сохранения языка и темы </summary>
public sealed partial class TgAppearanceViewModel(ITgSettingsService settingsService)
{
    // Применяем язык без записи черновиков остальных настроек
    [RelayCommand]
    private void ChangeLanguage(string? value)
    {
        if (Enum.TryParse<TgEnumLanguage>(value, out var language) && Enum.IsDefined(language)) settingsService.SaveLanguage(language);
    }

    // Применяем системную, светлую или тёмную тему к текущему окну
    [RelayCommand]
    private void ChangeTheme(string? value)
    {
        if (Enum.TryParse<TgEnumTheme>(value, out var theme) && Enum.IsDefined(theme)) settingsService.SaveTheme(theme);
    }

    /// <summary> Отмечает текущий язык при изменении настроек на любой странице </summary>
    public bool IsLanguageSelected(TgEnumLanguage language, string value) => language.ToString() == value;

    /// <summary> Отмечает текущую тему при изменении настроек на любой странице </summary>
    public bool IsThemeSelected(TgEnumTheme theme, string value) => theme.ToString() == value;
}
