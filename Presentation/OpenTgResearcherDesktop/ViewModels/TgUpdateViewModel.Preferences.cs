namespace OpenTgResearcherDesktop.ViewModels;

public partial class TgUpdateViewModel
{
    public TgProgramPreferences Preferences { get; private set; } = null!;
    public bool CanConfigureAutoStart => TgProgramIntegration.IsInstalled;

    [ObservableProperty]
    public partial string PreferencesMessage { get; private set; } = string.Empty;

    // Сохраняем настройки программы независимо от подключения Telegram
    [RelayCommand]
    private void SaveProgramSettings()
    {
        try
        {
            Preferences.Save(TgProgramIntegration.CurrentVersion, SetProgramAutoStart, TgProgramIntegration.ApplyAccent);
            PreferencesMessage = "ProgramSettingsSaved".GetLocalizedResource();
        }
        catch (Exception ex)
        {
            PreferencesMessage = string.Format(CultureInfo.CurrentCulture, "ProgramSettingsFailed".GetLocalizedResource(), ex.Message);
            LogError(ex);
        }
    }

    // Обновляем черновик при переходе, учитывая смену языка и темы из верхней панели
    private void LoadProgramSettings()
    {
        Preferences.Load();
        Preferences.AutoStart = CanConfigureAutoStart && TgProgramIntegration.ReadAutoStart();
        PreferencesMessage = Preferences.NeedsSetup(TgProgramIntegration.CurrentVersion, TgProgramIntegration.IsInstalled)
            ? "ProgramSetupWelcome".GetLocalizedResource() : string.Empty;
    }

    // Не меняем автозапуск установленной программы при запуске из среды разработки или переносимого архива
    private void SetProgramAutoStart(bool enabled)
    {
        if (CanConfigureAutoStart) TgProgramIntegration.SetAutoStart(enabled);
    }
}
