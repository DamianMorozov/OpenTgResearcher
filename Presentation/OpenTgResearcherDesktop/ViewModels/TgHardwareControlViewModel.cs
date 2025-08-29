// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgHardwareControlViewModel : TgPageViewModelBase
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

    private ITgHardwareResourceMonitoringService HardwareResourceMonitoringService { get; }

    public IRelayCommand StartMonitorCommand { get; }
    public IRelayCommand StopMonitorCommand { get; }

    public TgHardwareControlViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgHardwareControlViewModel> logger,
        ITgHardwareResourceMonitoringService tgHardwareControlService)
        : base(settingsService, navigationService, logger, nameof(TgHardwareControlViewModel))
    {
        var scope = TgGlobalTools.Container.BeginLifetimeScope();
        HardwareResourceMonitoringService = scope.Resolve<ITgHardwareResourceMonitoringService>();
        HardwareResourceMonitoringService.MetricsUpdated += OnMetricsUpdated;
        // Commands
        StartMonitorCommand = new AsyncRelayCommand(StartMonitorAsync);
        StopMonitorCommand = new AsyncRelayCommand(StopMonitorAsync);
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

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadDataAsync(async () =>
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
            TgLogUtils.WriteException(ex, "Error starting hardware monitoring");
        }
        finally
        {
            await ReloadUiAsync();
        }
    }

    private async Task StartMonitorAsync() => await ContentDialogAsync(StartMonitorCoreAsync, TgResourceExtensions.AskStartMonitoring());

    private async Task StartMonitorCoreAsync() => await ProcessDataAsync(async () =>
    {
        try
        {
            await HardwareResourceMonitoringService.StartMonitoringAsync();
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex, "Error starting hardware monitoring");
        }
        finally
        {
            //
        }
    }, isDisabledContent: true, isPageLoad: false);

    private async Task StopMonitorAsync() => await ContentDialogAsync(StopMonitorCoreAsync, TgResourceExtensions.AskStopMonitoring());

    private async Task StopMonitorCoreAsync() => await ProcessDataAsync(async () =>
    {
        try
        {
            await HardwareResourceMonitoringService.StopMonitoringAsync(isClose: false);
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex, "Error starting hardware monitoring");
        }
        finally
        {
            //
        }
    }, isDisabledContent: true, isPageLoad: false);

    #endregion
}
