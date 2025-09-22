namespace OpenTgResearcherDesktop.ViewModels;

/// <summary> Sensitive model </summary>
[DebuggerDisplay("{ToDebugString()}")]
public partial class TgSensitiveModel : ObservableRecipient
{
    #region Fields, properties, constructor

    /// <summary> Load state service </summary>
    public ILoadStateService LoadStateService { get; private set; }

    public bool IsStorageProcessing => LoadStateService.IsStorageProcessing;
    public bool IsOnlineProcessing => LoadStateService.IsOnlineProcessing;
    public bool IsDisplaySensitiveData => LoadStateService.IsDisplaySensitiveData;
    public bool IsOnlineReady => LoadStateService.IsOnlineReady;
    public string SensitiveData => LoadStateService.SensitiveData;

    public TgSensitiveModel(ILoadStateService loadStateService) : base()
    {
        ArgumentNullException.ThrowIfNull(loadStateService);
        LoadStateService = loadStateService;

        // Callback updates UI: PropertyChanged for LoadStateService
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
    }

    #endregion
}
