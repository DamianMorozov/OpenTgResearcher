namespace OpenTgResearcherDesktop.ViewModels;

public sealed partial class TgChatsViewModel
{
    private CancellationTokenSource? _publicSearchCancellation;
    private List<OpenTgResearcherDesktop.Models.TgPublicChatSearchRow> _publicSearchRows = [];
    private bool _hasPublicSearch;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsTelegramSearch))]
    [NotifyPropertyChangedFor(nameof(IsLocalSearch))]
    public partial int SearchScope { get; set; }
    public bool IsTelegramSearch => SearchScope == 1;
    public bool IsLocalSearch => !IsTelegramSearch;
    [ObservableProperty]
    public partial int PublicSearchKind { get; set; }
    [ObservableProperty]
    public partial bool IsPublicSearchBusy { get; private set; }
    [ObservableProperty]
    public partial ObservableCollection<OpenTgResearcherDesktop.Models.TgPublicChatSearchRow> PublicSearchRows { get; private set; } = [];
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedPublicChat))]
    [NotifyPropertyChangedFor(nameof(PublicChatActionCaption))]
    public partial OpenTgResearcherDesktop.Models.TgPublicChatSearchRow? SelectedPublicChat { get; set; }
    public bool HasSelectedPublicChat => SelectedPublicChat is not null;
    public string PublicChatActionCaption => (SelectedPublicChat?.IsSaved == true ? "RegistryOpenChat" : "GlobalSearchAdd").GetLocalizedResource();
    [ObservableProperty]
    public partial string PublicSearchStatus { get; private set; } = "GlobalSearchHint".GetLocalizedResource();
    public IAsyncRelayCommand SearchChatsCommand { get; private set; } = null!;

    // Переключаем область без очистки локального реестра и без сетевого запроса
    partial void OnSearchScopeChanged(int value)
    {
        CancelPublicSearch();
        PublicChatError = string.Empty;
    }

    // Фильтруем полученную выдачу без повторного запроса к Telegram
    partial void OnPublicSearchKindChanged(int value) => ApplyPublicSearchFilter();

    // Обновляем доступность действия для выбранного результата
    partial void OnSelectedPublicChatChanged(OpenTgResearcherDesktop.Models.TgPublicChatSearchRow? value)
    {
        AddPublicChatCommand.NotifyCanExecuteChanged();
        IsPublicProfileOpen = value is not null;
        PublicProfileTask = LoadPublicProfileAsync(value);
    }

    // Сохраняем общий текст, но выбираем источник данных явно
    private async Task SearchChatsAsync()
    {
        if (!IsTelegramSearch)
        {
            await RefreshRegistryCommand.ExecuteAsync(null);
            return;
        }
        CancelPublicSearch();
        using var cancellation = new CancellationTokenSource();
        _publicSearchCancellation = cancellation;
        var ct = cancellation.Token;
        var query = FilterText;
        PublicChatError = string.Empty;
        _publicSearchRows = [];
        _hasPublicSearch = false;
        ApplyPublicSearchFilter();
        IsPublicSearchBusy = true;
        try
        {
            await LoadOnlineDataAsync(async () =>
            {
                if (!SettingsService.IsExistsAppStorage)
                    throw new InvalidOperationException("Сначала подключите базу данных в настройках хранилища");
                if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync())
                    throw new InvalidOperationException("Подключитесь к Telegram на странице «Подключение клиента»");
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, LoadStateService.OnlineToken);
                var results = await App.BusinessLogicManager.ConnectClient.SearchPublicChatsAsync(query, linked.Token);
                linked.Token.ThrowIfCancellationRequested();
                _publicSearchRows = [.. results.Select(x => new OpenTgResearcherDesktop.Models.TgPublicChatSearchRow(x))];
                _hasPublicSearch = true;
                ApplyPublicSearchFilter();
                PublicPhotosTask = LoadPublicPhotosAsync(_publicSearchRows.ToArray());
            });
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested || LoadStateService.OnlineToken.IsCancellationRequested) { }
        catch (Exception ex) { PublicChatError = ex.Message; LogError(ex); }
        finally
        {
            IsPublicSearchBusy = false;
            if (ReferenceEquals(_publicSearchCancellation, cancellation)) _publicSearchCancellation = null;
        }
    }

    // Добавляем выбранный источник отдельно от поиска; следующее нажатие открывает пульт
    private async Task AddPublicChatAsync()
    {
        if (SelectedPublicChat is not { } selected) return;
        PublicChatError = string.Empty;
        if (selected.SourceUid is { } uid)
        {
            NavigationService.NavigateTo(typeof(TgChatViewModel).FullName!, uid);
            return;
        }
        try
        {
            await LoadOnlineDataAsync(async () =>
            {
                if (!SettingsService.IsExistsAppStorage)
                    throw new InvalidOperationException("Сначала подключите базу данных в настройках хранилища");
                if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync())
                    throw new InvalidOperationException("Подключитесь к Telegram на странице «Подключение клиента»");
                var source = await App.BusinessLogicManager.ConnectClient.AddPublicChatAsync("@" + selected.UserName, LoadStateService.OnlineToken);
                selected.SourceUid = source.Uid;
                OnPropertyChanged(nameof(PublicChatActionCaption));
                await RefreshRegistryCommand.ExecuteAsync(null);
            });
        }
        catch (OperationCanceledException) when (LoadStateService.OnlineToken.IsCancellationRequested) { }
        catch (Exception ex) { PublicChatError = ex.Message; LogError(ex); }
    }

    // Сохраняем выбранный результат, если он остаётся в текущем фильтре
    private void ApplyPublicSearchFilter()
    {
        var id = SelectedPublicChat?.Id;
        PublicSearchRows = [.. _publicSearchRows.Where(x => PublicSearchKind == 0 || x.IsGroup == (PublicSearchKind == 2))];
        SelectedPublicChat = PublicSearchRows.FirstOrDefault(x => x.Id == id) ?? PublicSearchRows.FirstOrDefault();
        RefreshPublicSearchStatus();
    }

    /// <summary> Отменяет поиск при уходе со страницы, сохраняя работающие парсеры </summary>
    public void CancelPublicSearch()
    {
        _publicSearchCancellation?.Cancel();
        _publicProfileCancellation?.Cancel();
        _publicPhotosCancellation?.Cancel();
        _publicMediaCancellation?.Cancel();
    }

    // Переводим динамические подписи без изменения выбора и повторного запроса
    private void RefreshPublicSearchLanguage()
    {
        foreach (var row in _publicSearchRows) row.RefreshLanguage();
        OnPropertyChanged(nameof(PublicChatActionCaption));
        RefreshPublicSearchStatus();
    }

    // Показываем приглашение, число результатов или пояснение пустой выдачи
    private void RefreshPublicSearchStatus() => PublicSearchStatus = !_hasPublicSearch ? "GlobalSearchHint".GetLocalizedResource()
        : PublicSearchRows.Count == 0 ? "GlobalSearchEmpty".GetLocalizedResource()
        : string.Format("GlobalSearchCount".GetLocalizedResource(), PublicSearchRows.Count);
}
