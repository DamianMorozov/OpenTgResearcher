namespace OpenTgResearcherDesktop.ViewModels;

public partial class TgSettingsViewModel : TgPageViewModelBase, IDisposable
{
    #region Fields, properties, constructor

    public IAsyncRelayCommand SettingsLoadCommand { get; }
    public IAsyncRelayCommand SettingsDefaultCommand { get; }
    public IAsyncRelayCommand SettingsSaveCommand { get; }
    public TgUpdateViewModel Updates { get; }

    /// <summary> Создаёт общие настройки приложения и каталог аккаунтов Telegram </summary>
    public TgSettingsViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService,
        ILogger<TgSettingsViewModel> logger, TgUpdateViewModel updates)
        : base(loadStateService, settingsService, navigationService, logger, nameof(TgSettingsViewModel))
    {
        Updates = updates;
        Preferences = new TgProgramPreferences(settingsService);
        Preferences.Load();
        // Команды
        SettingsLoadCommand = new AsyncRelayCommand(SettingsLoadAsync, () => !IsProfileBusy);
        SettingsDefaultCommand = new AsyncRelayCommand(SettingsDefaultAsync, () => !IsProfileBusy);
        SettingsSaveCommand = new AsyncRelayCommand(SettingsSaveAsync, () => !IsProfileBusy);
        InitializeProfiles();
    }


    #endregion

    #region IDisposable

    /// <summary> Release managed resources </summary>
    public override void ReleaseManagedResources()
    {
        CheckIfDisposed();

        base.ReleaseManagedResources();
    }

    #endregion

    #region Methods

    /// <summary> Загружает общие параметры и оформление при открытии страницы настроек </summary>
    public override async Task OnNavigatedToAsync(NavigationEventArgs? e)
    {
        SettingsLoadCore();
        await Task.CompletedTask;
    }

    // Загружаем настройки файлов и оформления после подтверждения пользователя
    private async Task SettingsLoadAsync() =>
        await ContentDialogAsync(SettingsLoadCore, TgResourceExtensions.AskSettingsLoad(), TgEnumLoadDesktopType.Storage);

    // Синхронизируем оба блока настроек с сохранёнными значениями
    private void SettingsLoadCore()
    {
        SettingsService.Load();
        LoadProgramSettings();
        LoadProfiles();
    }

    // Запрашиваем подтверждение сброса настроек
    private async Task SettingsDefaultAsync() =>
        await ContentDialogAsync(SettingsDefaultCoreAsync, TgResourceExtensions.AskSettingsDefault(), TgEnumLoadDesktopType.Storage);

    // Сбрасываем общие параметры и черновик оформления без записи в настройки
    private async Task SettingsDefaultCoreAsync()
    {
        SettingsService.AppLanguage = TgEnumLanguage.Default;
        SettingsService.AppTheme = TgEnumTheme.Default;
        LoadProfiles();
        ResetProgramAppearance();
        await Task.CompletedTask;
    }

    // Запрашиваем подтверждение сохранения единой страницы настроек
    private Task SettingsSaveAsync() => SaveSettingsWithProfileAsync();

    #endregion
}
