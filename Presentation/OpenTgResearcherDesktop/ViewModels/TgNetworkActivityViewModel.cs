namespace OpenTgResearcherDesktop.ViewModels;

/// <summary> Снимок активных сетевых действий и истории текущего запуска </summary>
public sealed partial class TgNetworkActivityViewModel : TgPageViewModelBase
{
    public ObservableCollection<TgNetworkOperationModel> Active { get; } = [];
    public ObservableCollection<TgNetworkOperationModel> History { get; } = [];
    public IRelayCommand ClearHistoryCommand { get; }
    public IAsyncRelayCommand StopParsingCommand { get; }
    public IRelayCommand PauseParsingCommand { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StopParsingCommand), nameof(PauseParsingCommand))]
    public partial bool CanStopParsing { get; private set; }
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StopParsingCommand))]
    public partial bool IsConfirmingStop { get; private set; }
    [ObservableProperty]
    public partial string PauseButtonText { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsActiveEmpty { get; private set; } = true;
    [ObservableProperty]
    public partial bool IsHistoryEmpty { get; private set; } = true;
    [ObservableProperty]
    public partial string ConnectionStatus { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial string ActivitySummary { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial string CompletedSummary { get; private set; } = string.Empty;

    /// <summary> Использует тот же сервис состояния, что и настоящие загрузчики </summary>
    public TgNetworkActivityViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService,
        ILogger<TgNetworkActivityViewModel> logger) : base(loadStateService, settingsService, navigationService, logger, nameof(TgNetworkActivityViewModel))
    {
        ClearHistoryCommand = new RelayCommand(ClearHistory);
        StopParsingCommand = new AsyncRelayCommand(StopParsingAsync, () => CanStopParsing && !IsConfirmingStop);
        PauseParsingCommand = new RelayCommand(PauseParsing, () => CanStopParsing);
    }

    /// <summary> Открывает страницу без подключения к Telegram и чтения пользовательской БД </summary>
    public override Task OnNavigatedToAsync(NavigationEventArgs? e)
    {
        Refresh();
        return Task.CompletedTask;
    }

    /// <summary> Обновляет снимок в потоке страницы, не прерывая выполняющиеся операции </summary>
    public void Refresh()
    {
        var operations = LoadStateService.NetworkActivity.Operations;
        var now = DateTime.UtcNow;
        var active = new List<TgNetworkOperationModel>();
        var history = new List<TgNetworkOperationModel>();
        foreach (var operation in operations)
        {
            var row = new TgNetworkOperationModel(operation, now, showSensitiveData: LoadStateService.IsDisplaySensitiveData);
            if (operation.State == TgNetworkOperationState.Running) active.Add(row); else history.Add(row);
        }
        UpdateRows(Active, active);
        UpdateRows(History, history);
        CanStopParsing = operations.Any(x => x.Kind == TgNetworkOperationKind.Parsing && x.State == TgNetworkOperationState.Running && x.CanCancel);
        PauseButtonText = (operations.Any(x => x.Kind == TgNetworkOperationKind.Parsing && x.IsPaused)
            ? "NetworkResume" : "NetworkPause").GetLocalizedResource();
        IsActiveEmpty = Active.Count == 0;
        IsHistoryEmpty = History.Count == 0;
        ConnectionStatus = (LoadStateService.IsOnlineReady ? "NetworkConnected" : "NetworkDisconnected").GetLocalizedResource();
        ActivitySummary = string.Format(CultureInfo.CurrentCulture, "NetworkActiveCount".GetLocalizedResource(), Active.Count);
        CompletedSummary = string.Format(CultureInfo.CurrentCulture, "NetworkHistoryCount".GetLocalizedResource(), History.Count);
    }

    // Запрашиваем подтверждение перед отменой, не создавая новую сетевую операцию
    private async Task StopParsingAsync(CancellationToken ct = default)
    {
        if (XamlRootVm is null || !CanStopParsing || IsConfirmingStop) return;
        IsConfirmingStop = true;
        try
        {
            var dialog = new ContentDialog
            {
                XamlRoot = XamlRootVm, Title = "NetworkStopQuestion".GetLocalizedResource(),
                PrimaryButtonText = TgResourceExtensions.GetYesButton(), CloseButtonText = TgResourceExtensions.GetCancelButton(),
                DefaultButton = ContentDialogButton.Close,
            };
            if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
            ct.ThrowIfCancellationRequested();
            LoadStateService.NetworkActivity.CancelParsing();
        }
        catch (Exception ex) { LogError(ex); }
        finally { IsConfirmingStop = false; Refresh(); }
    }

    // Переключаем ожидание новых запросов, не отменяя выполняющийся парсинг
    private void PauseParsing()
    {
        try { LoadStateService.NetworkActivity.ToggleParsingPause(); }
        catch (Exception ex) { LogError(ex); }
        finally { Refresh(); }
    }

    // Не пересоздаём неизменные строки при опросе таймера, сохраняя элементы истории
    private static void UpdateRows(ObservableCollection<TgNetworkOperationModel> target, List<TgNetworkOperationModel> rows)
    {
        if (target.SequenceEqual(rows)) return;
        target.Clear();
        foreach (var row in rows) target.Add(row);
    }

    // Очищаем только завершённые строки, рабочие токены и загрузчики не меняем
    private void ClearHistory()
    {
        LoadStateService.NetworkActivity.ClearCompleted();
        Refresh();
    }
}
