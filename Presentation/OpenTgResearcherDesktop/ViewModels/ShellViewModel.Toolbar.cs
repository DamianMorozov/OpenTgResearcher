namespace OpenTgResearcherDesktop.ViewModels;

public partial class ShellViewModel
{
    private TgAppearanceViewModel? _appearance;
    public TgAppearanceViewModel Appearance => _appearance ??= new(SettingsService);

    // Применяем язык из меню верхней панели
    [RelayCommand]
    private void ChangeLanguage(string? value)
    {
        Appearance.ChangeLanguageCommand.Execute(value);
    }

    // Применяем тему из меню верхней панели
    [RelayCommand]
    private void ChangeTheme(string? value)
    {
        Appearance.ChangeThemeCommand.Execute(value);
    }

    // Открываем общий журнал и ожидаем штатную проверку стабильного выпуска
    [RelayCommand]
    private async Task CheckForUpdatesAsync(XamlRoot? root, CancellationToken ct = default)
    {
        if (root is null || ct.IsCancellationRequested) return;
        NavigationService.NavigateTo(typeof(TgSettingsViewModel).FullName!);
        Settings.ShowUpdates();
        if (Updates.IsUpdating) return;
        Updates.XamlRootVm = root;
        if (SettingsService.ReadSetting<TgProgramPreferences.Values>(TgProgramPreferences.PreferencesKey)?.IncludePreview == true)
            await Updates.UpdatePreviewCommand.ExecuteAsync(null);
        else await Updates.UpdateReleaseCommand.ExecuteAsync(null);
    }

    /// <summary> Отмечает выбранный язык при изменении настроек из любой страницы </summary>
    public bool IsLanguageSelected(TgEnumLanguage language, string value) => Appearance.IsLanguageSelected(language, value);

    /// <summary> Отмечает выбранную тему при изменении настроек из любой страницы </summary>
    public bool IsThemeSelected(TgEnumTheme theme, string value) => Appearance.IsThemeSelected(theme, value);
}
