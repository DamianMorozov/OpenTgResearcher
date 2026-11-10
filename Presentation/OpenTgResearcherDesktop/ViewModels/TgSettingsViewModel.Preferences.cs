namespace OpenTgResearcherDesktop.ViewModels;

public partial class TgSettingsViewModel
{
    public TgProgramPreferences Preferences { get; private set; } = null!;
    public bool CanConfigureAutoStart => TgProgramIntegration.IsInstalled;

    [ObservableProperty]
    public partial string PreferencesMessage { get; private set; } = string.Empty;

    // Применяем оформление и автозапуск перед сохранением общих настроек приложения
    private void SaveProgramPreferences()
    {
        try
        {
            Preferences.Save(TgProgramIntegration.CurrentVersion, SetProgramAutoStart, TgProgramIntegration.ApplyAccent);
            PreferencesMessage = "ProgramSettingsSaved".GetLocalizedResource();
        }
        catch (Exception ex)
        {
            PreferencesMessage = string.Format(CultureInfo.CurrentCulture, "ProgramSettingsFailed".GetLocalizedResource(), ex.Message);
            throw;
        }
    }

    // Обновляем черновик с учётом сохранённых параметров и выбора языка и темы из верхней панели
    private void LoadProgramSettings()
    {
        Preferences.Load();
        Preferences.AutoStart = CanConfigureAutoStart && TgProgramIntegration.ReadAutoStart();
        PreferencesMessage = Preferences.NeedsSetup(TgProgramIntegration.CurrentVersion, TgProgramIntegration.IsInstalled)
            ? "ProgramSetupWelcome".GetLocalizedResource() : string.Empty;
    }

    // Восстанавливаем оформление по умолчанию, оставляя выбор предварительных выпусков без изменений
    private void ResetProgramAppearance()
    {
        Preferences.LanguageIndex = (int)SettingsService.AppLanguage;
        Preferences.ThemeIndex = (int)TgEnumTheme.Default;
        Preferences.UseSystemAccent = true;
        Preferences.AccentHex = TgProgramPreferences.DefaultAccentHex;
        Preferences.AutoStart = false;
        PreferencesMessage = string.Empty;
    }

    // Не меняем автозапуск установленной программы из среды разработки или переносимого архива
    private void SetProgramAutoStart(bool enabled)
    {
        if (CanConfigureAutoStart) TgProgramIntegration.SetAutoStart(enabled);
    }
}
