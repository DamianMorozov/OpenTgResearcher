using System.ComponentModel;

namespace OpenTgResearcherDesktop.ViewModels;

/// <summary> Sensitive model </summary>
public partial class TgSensitiveModel : ObservableRecipient, IDisposable
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
        LoadStateService.PropertyChanged += OnLoadStateServiceChanged;
        SettingsService.PropertyChanged += OnSettingsServiceChanged;
    }

    #endregion

    #region IDisposable

    /// <summary> To detect redundant calls </summary>
    private bool _disposed;

    /// <summary> Finalizer </summary>
	~TgSensitiveModel() => Dispose(false);

    /// <summary> Throw exception if disposed </summary>
    public void CheckIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    /// <summary> Release managed resources </summary>
    public virtual void ReleaseManagedResources()
    {
        CheckIfDisposed();
        if (LoadStateService is not null)
            LoadStateService.PropertyChanged -= OnLoadStateServiceChanged;
        if (SettingsService is not null)
            SettingsService.PropertyChanged -= OnSettingsServiceChanged;
    }

    /// <summary> Release unmanaged resources </summary>
    public virtual void ReleaseUnmanagedResources()
    {
        CheckIfDisposed();
    }

    /// <summary> Dispose pattern </summary>
    public void Dispose()
    {
        // Dispose of unmanaged resources
        Dispose(true);
        // Suppress finalization
        GC.SuppressFinalize(this);
    }

    /// <summary> Dispose pattern </summary>
    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        // Release managed resources
        if (disposing)
            ReleaseManagedResources();
        // Release unmanaged resources
        ReleaseUnmanagedResources();
        // Flag
        _disposed = true;
    }

    #endregion

    #region Methods

    private void OnLoadStateServiceChanged(object? sender, PropertyChangedEventArgs e)
    {
        TgDesktopUtils.InvokeOnUIThread(() => {
            try
            {
                if (e.PropertyName == nameof(LoadStateService.IsStorageProcessing))
                    OnPropertyChanged(nameof(IsStorageProcessing));
                else if (e.PropertyName == nameof(LoadStateService.IsOnlineProcessing))
                    OnPropertyChanged(nameof(IsOnlineProcessing));
                else if (e.PropertyName == nameof(LoadStateService.IsDisplaySensitiveData))
                    OnPropertyChanged(nameof(IsDisplaySensitiveData));
                else if (e.PropertyName == nameof(LoadStateService.IsOnlineReady))
                    OnPropertyChanged(nameof(IsOnlineReady));
            }
            catch (Exception ex)
            {
                TgLogUtils.WriteException(ex);
            }
        });
    }

    private void OnSettingsServiceChanged(object? sender, PropertyChangedEventArgs e)
    {
        TgDesktopUtils.InvokeOnUIThread(() => {
            if (e.PropertyName == nameof(SettingsService.IsExistsAppStorage))
                OnPropertyChanged(nameof(IsExistsAppStorage));
            else if (e.PropertyName == nameof(SettingsService.IsExistsAppSession))
                OnPropertyChanged(nameof(IsExistsAppSession));
            else if (e.PropertyName == nameof(SettingsService.UserDirectory))
                OnPropertyChanged(nameof(UserDirectory));
        });
    }

    #endregion
}
