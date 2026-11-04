namespace OpenTgResearcherDesktop.ViewModels;

public partial class ShellViewModel
{
    // Применяем язык из меню верхней панели
    [RelayCommand]
    private void ChangeLanguage(string? value)
    {
        if (Enum.TryParse<TgEnumLanguage>(value, out var language) && Enum.IsDefined(language)) SettingsService.SaveLanguage(language);
    }

    // Применяем тему из меню верхней панели
    [RelayCommand]
    private void ChangeTheme(string? value)
    {
        if (Enum.TryParse<TgEnumTheme>(value, out var theme) && Enum.IsDefined(theme)) SettingsService.SaveTheme(theme);
    }

    // Открываем общий журнал и ожидаем штатную проверку стабильного выпуска
    [RelayCommand]
    private async Task CheckForUpdatesAsync(XamlRoot? root, CancellationToken ct = default)
    {
        if (root is null || ct.IsCancellationRequested) return;
        NavigationService.NavigateTo(typeof(TgUpdateViewModel).FullName!);
        if (Updates.IsUpdating) return;
        Updates.XamlRootVm = root;
        await Updates.UpdateReleaseCommand.ExecuteAsync(null);
    }

    /// <summary> Отмечает выбранный язык при изменении настроек из любой страницы </summary>
    public bool IsLanguageSelected(TgEnumLanguage language, string value) => language.ToString() == value;

    /// <summary> Отмечает выбранную тему при изменении настроек из любой страницы </summary>
    public bool IsThemeSelected(TgEnumTheme theme, string value) => theme.ToString() == value;
}
