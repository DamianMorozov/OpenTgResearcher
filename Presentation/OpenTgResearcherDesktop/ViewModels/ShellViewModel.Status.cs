namespace OpenTgResearcherDesktop.ViewModels;

public partial class ShellViewModel
{
    public ObservableCollection<TgNetworkChatGroupModel> StatusChatGroups { get; } = [];

    [ObservableProperty]
    public partial string StatusTelegramText { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial string StatusTelegramDetail { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial string StatusStorageText { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial string StatusStorageDetail { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial string StatusActivityText { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial string StatusActivityCount { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial bool IsStatusActivityEmpty { get; private set; } = true;
    [ObservableProperty]
    public partial string StatusFloodText { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial string StatusFloodDetail { get; private set; } = string.Empty;

    /// <summary> Обновляет нижнюю строку по общему состоянию без сетевых запросов и чтения БД </summary>
    public void RefreshStatus()
    {
        StatusTelegramText = (LoadStateService.IsOnlineReady ? "StatusTelegramReady" : "StatusTelegramOffline").GetLocalizedResource();
        StatusTelegramDetail = (LoadStateService.IsOnlineReady ? "StatusTelegramReadyDetail" : "StatusTelegramOfflineDetail").GetLocalizedResource();
        var storageKey = !SettingsService.IsExistsAppStorage ? "StatusStorageMissing"
            : LoadStateService.IsStorageProcessing ? "StatusStorageWriting" : "StatusStorageReady";
        StatusStorageText = storageKey.GetLocalizedResource();
        StatusStorageDetail = $"{storageKey}Detail".GetLocalizedResource();
        StatusFloodText = IsFloodVisible
            ? string.Format(CultureInfo.CurrentCulture, "StatusFloodWaiting".GetLocalizedResource(), Math.Max(0, FloodWaitSeconds))
            : "StatusFloodReady".GetLocalizedResource();
        StatusFloodDetail = IsFloodVisible
            ? string.Format(CultureInfo.CurrentCulture, "StatusFloodWaitingDetail".GetLocalizedResource(), Math.Max(0, FloodWaitSeconds))
            : "StatusFloodReadyDetail".GetLocalizedResource();

        var active = LoadStateService.NetworkActivity.Operations.Where(x => x.State == TgNetworkOperationState.Running).ToArray();
        IsStatusActivityEmpty = active.Length == 0;
        var chats = active.Where(x => x.ChatId.HasValue).Select(x => x.ChatId).Distinct().Count();
        StatusActivityCount = string.Format(CultureInfo.CurrentCulture,
            (chats > 0 ? "StatusChatCount" : "StatusOperationCount").GetLocalizedResource(), chats > 0 ? chats : active.Length);
        StatusActivityText = active.Length == 0 ? "StatusActivityEmpty".GetLocalizedResource()
            : active.All(x => x.IsCancellationRequested) ? "NetworkStateStopping".GetLocalizedResource()
            : active.All(x => x.IsPaused) ? "NetworkStatePaused".GetLocalizedResource()
            : active.Any(x => x.Kind == TgNetworkOperationKind.Parsing) ? "StatusActivityParsing".GetLocalizedResource()
            : active.Any(x => x.Kind == TgNetworkOperationKind.Download) ? "StatusActivityDownloading".GetLocalizedResource()
            : "StatusActivityWorking".GetLocalizedResource();
        UpdateStatusGroups(active);
    }

    // Сохраняем раскрытие и экземпляры неизменных строк, оставляя в панели только выполняющиеся операции
    private void UpdateStatusGroups(IReadOnlyList<TgNetworkOperation> active)
    {
        var groups = active.GroupBy(x => x.ChatId).ToArray();
        for (var index = StatusChatGroups.Count - 1; index >= 0; index--)
            if (!groups.Any(x => x.Key == StatusChatGroups[index].ChatId)) StatusChatGroups.RemoveAt(index);
        for (var index = 0; index < groups.Length; index++)
        {
            var source = groups[index];
            var group = StatusChatGroups.FirstOrDefault(x => x.ChatId == source.Key);
            if (group is null) { group = new(source.Key); StatusChatGroups.Insert(index, group); }
            else if (StatusChatGroups.IndexOf(group) != index) StatusChatGroups.Move(StatusChatGroups.IndexOf(group), index);
            var title = source.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.ChatTitle))?.ChatTitle;
            group.Title = !source.Key.HasValue ? "NetworkGeneralGroup".GetLocalizedResource()
                : !LoadStateService.IsDisplaySensitiveData ? LoadStateService.SensitiveData
                : !string.IsNullOrWhiteSpace(title) ? title
                : string.Format(CultureInfo.CurrentCulture, "NetworkChatGroupFallback".GetLocalizedResource(), source.Key);
            group.Summary = string.Format(CultureInfo.CurrentCulture, "NetworkActiveCount".GetLocalizedResource(), source.Count());
            var rows = source.Select(x => new TgNetworkOperationModel(x, DateTime.UtcNow,
                showSensitiveData: LoadStateService.IsDisplaySensitiveData)).ToArray();
            if (group.Operations.SequenceEqual(rows)) continue;
            group.Operations.Clear();
            foreach (var row in rows) group.Operations.Add(row);
        }
    }

    // Открываем полную активность без изменения сетевых операций
    [RelayCommand]
    private void OpenStatusActivity() => NavigationService.NavigateTo(typeof(TgNetworkActivityViewModel).FullName!);

    // Открываем страницу подключения для восстановления связи
    [RelayCommand]
    private void OpenStatusConnection() => NavigationService.NavigateTo(typeof(TgClientConnectionViewModel).FullName!);

    // Открываем настройки для выбора базы
    [RelayCommand]
    private void OpenStatusSettings() => NavigationService.NavigateTo(typeof(TgSettingsViewModel).FullName!);
}
