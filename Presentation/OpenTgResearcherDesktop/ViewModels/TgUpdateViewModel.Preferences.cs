namespace OpenTgResearcherDesktop.ViewModels;

public partial class TgUpdateViewModel
{
    public TgProgramPreferences Preferences { get; private set; } = null!;

    [ObservableProperty]
    public partial string PreferencesMessage { get; private set; } = string.Empty;

    // Сохраняем настройки программы независимо от подключения Telegram
    [RelayCommand]
    private void SaveProgramSettings()
    {
        try
        {
            Preferences.SaveUpdatePreferences();
            PreferencesMessage = "ProgramSettingsSaved".GetLocalizedResource();
        }
        catch (Exception ex)
        {
            PreferencesMessage = string.Format(CultureInfo.CurrentCulture, "ProgramSettingsFailed".GetLocalizedResource(), ex.Message);
            LogError(ex);
        }
    }

    // Обновляем параметры проверки выпусков при переходе на страницу программы
    private void LoadProgramSettings()
    {
        Preferences.Load();
        PreferencesMessage = string.Empty;
    }
}
