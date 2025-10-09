namespace OpenTgResearcherDesktop.ViewModels;

public sealed partial class TgHardwareResourceViewModel : TgPageViewModelBase, IDisposable
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial int CpuApp { get; set; }
    [ObservableProperty]
    public partial int CpuTotal { get; set; }
    [ObservableProperty]
    public partial int MemoryAppUsage { get; set; }
    [ObservableProperty]
    public partial string MemoryAppUsageString { get; set; } = "";
    [ObservableProperty]
    public partial int MemoryTotalUsage { get; set; }
    [ObservableProperty]
    public partial string MemoryTotalUsageString { get; set; } = "";

    private ILifetimeScope Scope { get; } = default!;

    private ITgHardwareResourceMonitoringService HardwareResourceMonitoringService { get; }

    public IAsyncRelayCommand StartMonitorCommand { get; }
    public IAsyncRelayCommand StopMonitorCommand { get; }

    public TgHardwareResourceViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgHardwareResourceViewModel> logger) : base(loadStateService, settingsService, navigationService, logger, nameof(TgHardwareResourceViewModel))
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

    /// <summary> Release managed resources </summary>
    public override void ReleaseManagedResources()
    {
        base.ReleaseManagedResources();

        CheckIfDisposed();

        HardwareResourceMonitoringService.MetricsUpdated -= OnMetricsUpdated;
        HardwareResourceMonitoringService.Dispose();

        Scope.Dispose();
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
            MemoryAppUsageString = TgFileUtils.GetFileSizeAsString(e.MemoryAppGb * 1024 * 1024 * 1024);
            MemoryTotalUsage = (int)e.MemoryTotalPercent;
            MemoryTotalUsageString = TgFileUtils.GetFileSizeAsString(e.MemoryUsedGb * 1024 * 1024 * 1024);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(LoadDataStorageCoreAsync);

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

    private async Task StartMonitorAsync() => 
        await ContentDialogAsync(StartMonitorCoreAsync, TgResourceExtensions.AskStartMonitoring(), TgEnumLoadDesktopType.Online);

    private async Task StartMonitorCoreAsync() => await LoadOnlineDataAsync(() =>
    {
        try
        {
            HardwareResourceMonitoringService.StartMonitoring();
        }
        catch (Exception ex)
        {
            LogError(ex, "Error starting hardware monitoring");
        }
    });

    private async Task StopMonitorAsync() => 
        await ContentDialogAsync(StopMonitorCoreAsync, TgResourceExtensions.AskStopMonitoring(), TgEnumLoadDesktopType.Online);

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
