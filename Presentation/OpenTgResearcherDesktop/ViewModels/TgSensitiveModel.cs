namespace OpenTgResearcherDesktop.ViewModels;

/// <summary> Sensitive model </summary>
public partial class TgSensitiveModel : ObservableRecipient
{
    #region Fields, properties, constructor

    /// <summary> Load state service </summary>
    [ObservableProperty]
    public partial ILoadStateService LoadStateService { get; private set; } = default!;
    /// <summary> Settings service </summary>
    [ObservableProperty]
    public partial ITgSettingsService SettingsService { get; private set; } = default!;

    public bool IsDisplaySensitiveData => LoadStateService.IsDisplaySensitiveData;
    public bool IsExistsAppSession => SettingsService.IsExistsAppSession;
    public bool IsExistsAppStorage => SettingsService.IsExistsAppStorage;
    public bool IsOnlineProcessing => LoadStateService.IsOnlineProcessing;
    public bool IsOnlineReady => LoadStateService.IsOnlineReady;
    public bool IsStorageProcessing => LoadStateService.IsStorageProcessing;
    public string SensitiveData => LoadStateService.SensitiveData;
    public string UserDirectory => SettingsService.UserDirectory;

    public TgSensitiveModel(ILoadStateService loadStateService, ITgSettingsService settingsService) : base()
    {
        ArgumentNullException.ThrowIfNull(loadStateService);
        LoadStateService = loadStateService;
        ArgumentNullException.ThrowIfNull(settingsService);
        SettingsService = settingsService;

        // Callback updates UI: PropertyChanged
        LoadStateService.PropertyChanged += (_, e) =>
        {
            TgDesktopUtils.InvokeOnUIThread(() => { 
                if (e.PropertyName == nameof(LoadStateService.IsStorageProcessing))
                    OnPropertyChanged(nameof(IsStorageProcessing));
                else if (e.PropertyName == nameof(LoadStateService.IsOnlineProcessing))
                    OnPropertyChanged(nameof(IsOnlineProcessing));
                else if (e.PropertyName == nameof(LoadStateService.IsDisplaySensitiveData))
                    OnPropertyChanged(nameof(IsDisplaySensitiveData));
                else if (e.PropertyName == nameof(LoadStateService.IsOnlineReady))
                    OnPropertyChanged(nameof(IsOnlineReady));
            });
        };

        // Callback updates UI: PropertyChanged
        SettingsService.PropertyChanged += (_, e) =>
        {
            TgDesktopUtils.InvokeOnUIThread(() => { 
                if (e.PropertyName == nameof(SettingsService.IsExistsAppStorage))
                    OnPropertyChanged(nameof(IsExistsAppStorage));
                else if (e.PropertyName == nameof(SettingsService.IsExistsAppSession))
                    OnPropertyChanged(nameof(IsExistsAppSession));
                else if (e.PropertyName == nameof(SettingsService.UserDirectory))
                    OnPropertyChanged(nameof(UserDirectory));
            });
        };
    }

    #endregion
}
