namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgHardwareControlViewModel : TgPageViewModelBase, IDisposable
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial int CpuApp { get; set; }
    [ObservableProperty]
    public partial int CpuTotal { get; set; }
    [ObservableProperty]
    public partial int MemoryAppUsage { get; set; }
    [ObservableProperty]
    public partial int MemoryTotalUsage { get; set; }

    private ILifetimeScope Scope { get; } = default!;

    private ITgHardwareResourceMonitoringService HardwareResourceMonitoringService { get; }

    public IRelayCommand StartMonitorCommand { get; }
    public IRelayCommand StopMonitorCommand { get; }

    public TgHardwareControlViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgHardwareControlViewModel> logger)
        : base(settingsService, navigationService, logger, nameof(TgHardwareControlViewModel))
    {
        Scope = TgGlobalTools.Container.BeginLifetimeScope();
        HardwareResourceMonitoringService = Scope.Resolve<ITgHardwareResourceMonitoringService>();
        HardwareResourceMonitoringService.MetricsUpdated += OnMetricsUpdated;
        // Commands
        StartMonitorCommand = new AsyncRelayCommand(StartMonitorAsync);
        StopMonitorCommand = new AsyncRelayCommand(StopMonitorAsync);
    }

    #endregion

    #region IDisposable

    /// <summary> To detect redundant calls </summary>
    private bool _disposed;

    /// <summary> Finalizer </summary>
	~TgHardwareControlViewModel() => Dispose(false);

    /// <summary> Throw exception if disposed </summary>
    public void CheckIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    /// <summary> Release managed resources </summary>
    public void ReleaseManagedResources()
    {
        CheckIfDisposed();

        HardwareResourceMonitoringService.MetricsUpdated -= OnMetricsUpdated;
        HardwareResourceMonitoringService.Dispose();

        Scope.Dispose();
    }

    /// <summary> Release unmanaged resources </summary>
    public void ReleaseUnmanagedResources()
    {
        CheckIfDisposed();

        //
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

    private void OnMetricsUpdated(object? sender, TgHardwareMetrics e)
    {
        try
        {
            CpuApp = (int)e.CpuAppPercent;
            CpuTotal = (int)e.CpuTotalPercent;
            MemoryAppUsage = (int)e.MemoryAppPercent;
            MemoryTotalUsage = (int)e.MemoryTotalPercent;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(async () =>
    {
        await LoadDataStorageCoreAsync();
    });

    private async Task LoadDataStorageCoreAsync()
    {
        try
        {
            if (!SettingsService.IsExistsAppStorage) return;

            IsEmptyData = false;
        }
        catch (Exception ex)
        {
            LogError(ex, "Error starting hardware monitoring");
        }
        finally
        {
            await ReloadUiAsync();
        }
    }

    private async Task StartMonitorAsync() => await ContentDialogAsync(StartMonitorCoreAsync, TgResourceExtensions.AskStartMonitoring(), TgEnumLoadDesktopType.Online);

    private async Task StartMonitorCoreAsync() => await LoadOnlineDataAsync(async () =>
    {
        try
        {
            await HardwareResourceMonitoringService.StartMonitoringAsync();
        }
        catch (Exception ex)
        {
            LogError(ex, "Error starting hardware monitoring");
        }
    });

    private async Task StopMonitorAsync() => await ContentDialogAsync(StopMonitorCoreAsync, TgResourceExtensions.AskStopMonitoring(), TgEnumLoadDesktopType.Online);

    private async Task StopMonitorCoreAsync() => await LoadOnlineDataAsync(async () =>
    {
        try
        {
            await HardwareResourceMonitoringService.StopMonitoringAsync(isClose: false);
        }
        catch (Exception ex)
        {
            LogError(ex, "Error starting hardware monitoring");
        }
    });

    #endregion
}
