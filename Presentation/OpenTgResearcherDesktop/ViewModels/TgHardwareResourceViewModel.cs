namespace OpenTgResearcherDesktop.ViewModels;

/// <summary> Обзор ресурсов и динамика нагрузки Desktop без зависимости от БД и Telegram </summary>
public sealed partial class TgHardwareResourceViewModel : TgPageViewModelBase
{
    private readonly ILifetimeScope _scope;
    private readonly ITgHardwareResourceMonitoringService _monitoring;
    private bool _released;

    public TgHardwareDashboardModel Dashboard { get; } = new();
    public IRelayCommand PauseCommand { get; }

    [ObservableProperty]
    public partial string MonitorStatus { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial string PauseCaption { get; private set; } = string.Empty;

    /// <summary> Подписывает страницу на общий мониторинг и создаёт локальную историю </summary>
    public TgHardwareResourceViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService,
        ILogger<TgHardwareResourceViewModel> logger) : base(loadStateService, settingsService, navigationService, logger, nameof(TgHardwareResourceViewModel))
    {
        _scope = TgGlobalTools.Container.BeginLifetimeScope();
        _monitoring = _scope.Resolve<ITgHardwareResourceMonitoringService>();
        _monitoring.MetricsUpdated += OnMetricsUpdated;
        Dashboard.PropertyChanged += OnDashboardChanged;
        PauseCommand = new RelayCommand(Dashboard.TogglePause);
    }

    /// <summary> Освобождает подписку страницы, не останавливая общий мониторинг приложения </summary>
    public override void ReleaseManagedResources()
    {
        _released = true;
        _monitoring.MetricsUpdated -= OnMetricsUpdated;
        Dashboard.PropertyChanged -= OnDashboardChanged;
        _scope.Dispose();
        base.ReleaseManagedResources();
    }

    /// <summary> Открывает оборудование даже без хранилища и подключения Telegram </summary>
    public override Task OnNavigatedToAsync(NavigationEventArgs? e)
    {
        RefreshCaptions();
        return Task.CompletedTask;
    }

    /// <summary> Обновляет динамические подписи после смены языка </summary>
    public void RefreshCaptions()
    {
        PauseCaption = (Dashboard.IsPaused ? "HardwareResume" : "HardwarePause").GetLocalizedResource();
        MonitorStatus = (Dashboard.IsPaused ? "HardwarePaused" : !Dashboard.HasData ? "HardwareWaiting" :
            !Dashboard.HasCpuTotal || !Dashboard.HasMemoryTotal ? "HardwareUnavailable" : "HardwareActive").GetLocalizedResource();
    }

    // Измерения поступают из фоновой задачи, все привязки меняем только в очереди окна
    private void OnMetricsUpdated(object? sender, TgHardwareMetrics metrics)
    {
        App.MainWindow?.DispatcherQueue.TryEnqueue(() =>
        {
            if (_released) return;
            try { Dashboard.Apply(metrics); }
            catch (Exception ex) { LogError(ex, "Не удалось обновить показания оборудования"); }
        });
    }

    // Подписи состояния пересчитываем вместе с измерениями и паузой
    private void OnDashboardChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(Dashboard.IsPaused) or nameof(Dashboard.History)) RefreshCaptions();
    }
}
