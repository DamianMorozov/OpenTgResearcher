namespace OpenTgResearcherDesktop.ViewModels;

public sealed partial class TgChatStatisticsViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial IAppNotificationService AppNotificationService { get; private set; }

    [ObservableProperty]
    public partial Guid Uid { get; set; } = Guid.Empty!;
    [ObservableProperty]
    public partial TgEfSourceDto Dto { get; set; } = new();
    [ObservableProperty]
    public partial ObservableCollection<TgEfUserDto> UserDtos { get; set; } = new();
    [ObservableProperty]
    public partial Action ScrollRequested { get; set; } = () => { };
    [ObservableProperty]
    public partial TgEfChatStatisticsDto ChatStatisticsDto { get; set; } = new();
    [ObservableProperty]
    public partial ObservableCollection<OpenTgResearcherDesktop.Models.TgChatActivityRow> ActivityRows { get; set; } = new();
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoMessages))]
    public partial int TotalMessages { get; private set; }
    [ObservableProperty]
    public partial int ActiveAuthorsCount { get; private set; }
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoMessages))]
    public partial bool IsCalculated { get; private set; }
    [ObservableProperty]
    public partial string StatisticsError { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial bool IsPeriodExpanded { get; set; } = true;
    public bool HasNoMessages => IsCalculated && TotalMessages == 0;
    public bool HasStatisticsError => !string.IsNullOrEmpty(StatisticsError);

    public IAsyncRelayCommand CalcChatStatisticsCommand { get; }

    public TgChatStatisticsViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgChatStatisticsViewModel> logger, IAppNotificationService appNotificationService)
        : base(loadStateService, settingsService, navigationService, logger, nameof(TgChatStatisticsViewModel))
    {
        AppNotificationService = appNotificationService;
        ChatStatisticsDto.PropertyChanged += OnStatisticsPeriodChanged;
        // Команды
        CalcChatStatisticsCommand = new AsyncRelayCommand(CalcChatStatisticsAsync);
    }

    #endregion

    #region Methods

    /// <summary> Выбирает чат и рассчитывает отсутствующую статистику при активном подключении </summary>
    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(async () =>
    {
        var uid = e?.Parameter is Guid value ? value : Guid.Empty;
        if (Uid != uid)
        {
            Uid = uid;
            UserDtos.Clear();
            IsPeriodExpanded = true;
            ChatStatisticsDto.Default();
            ResetSummary();
        }
        await LoadDataStorageCoreAsync();
        if (!IsCalculated && IsOnlineReady && Dto.Id > 0)
            await LoadOnlineDataAsync(CalcChatStatisticsCoreAsync);
    });

    private async Task LoadDataStorageCoreAsync()
    {
        try
        {
            if (!SettingsService.IsExistsAppStorage) return;

            Dto = await App.BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Uid == Uid);

            IsEmptyData = false;
            ScrollRequested?.Invoke();
        }
        finally
        {
            await ReloadUiAsync();
        }
    }

    private async Task CalcChatStatisticsAsync() => 
        await ContentDialogAsync(CalcChatStatisticsCoreAsync, TgResourceExtensions.AskCalcChatStatistics(), TgEnumLoadDesktopType.Online);
    
    // Подсчитываем уникальных участников и их сообщения последовательными запросами
    private async Task CalcChatStatisticsCoreAsync()
    {
        ResetSummary();
        ChatStatisticsDto.Default();
        if (ChatStatisticsDto.DtStart?.Date > ChatStatisticsDto.DtEnd?.Date)
        {
            StatisticsError = "ChatStatisticsInvalidDates".GetLocalizedResource();
            OnPropertyChanged(nameof(HasStatisticsError));
            return;
        }
        try
        {
            await CalculateStatisticsCoreAsync();
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken.IsCancellationRequested)
        {
            ResetSummary();
        }
        catch (Exception ex)
        {
            LogError(ex);
            StatisticsError = ex.Message;
            OnPropertyChanged(nameof(HasStatisticsError));
        }
    }

    // Подписываемся на заменённый черновик периода и освобождаем прежнюю подписку
    partial void OnChatStatisticsDtoChanged(TgEfChatStatisticsDto oldValue, TgEfChatStatisticsDto newValue)
    {
        if (oldValue is not null) oldValue.PropertyChanged -= OnStatisticsPeriodChanged;
        newValue.PropertyChanged += OnStatisticsPeriodChanged;
        ResetSummary();
    }

    // Изменение периода сбрасывает устаревшие итоги до следующего расчёта
    private void OnStatisticsPeriodChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(TgEfChatStatisticsDto.DtStart) or nameof(TgEfChatStatisticsDto.DtEnd)) ResetSummary();
    }

    // Удаляем итоги прошлого расчёта, сохраняя выбранный период
    private void ResetSummary()
    {
        ActivityRows.Clear();
        TotalMessages = 0;
        ActiveAuthorsCount = 0;
        IsCalculated = false;
        StatisticsError = string.Empty;
        OnPropertyChanged(nameof(HasStatisticsError));
    }

    // Рассчитываем активность по выбранному периоду и строим строки ранжирования
    private async Task CalculateStatisticsCoreAsync()
    {
        ChatStatisticsDto.Default();
        ChatStatisticsDto.UserWithCountDtos.Clear();
        if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;
        
        await MergeUsersWithPlaceholdersAsync();
        await App.BusinessLogicManager.ConnectClient.UpdateUsersAsync([.. UserDtos]);

        // Учитываем каждого пользователя один раз, даже если он найден в Telegram и в БД
        var distinctUsers = UserDtos.GroupBy(u => u.Id).Select(g => g.First()).ToList();

        ChatStatisticsDto.UsersCount = distinctUsers.Count(x => !x.IsBot);
        ChatStatisticsDto.BotsCount = distinctUsers.Count(x => x.IsBot);

        var dtStartRaw = ChatStatisticsDto.DtStart ?? TgEfChatStatisticsDto.SafeMinDate;
        var dtEndRaw = ChatStatisticsDto.DtEnd ?? TgEfChatStatisticsDto.SafeMaxDate;
        // Ограничиваем даты перед преобразованием часового пояса
        var dtStart = dtStartRaw < TgEfChatStatisticsDto.SafeMinDate ? TgEfChatStatisticsDto.SafeMinDate : dtStartRaw;
        var dtEnd = dtEndRaw > TgEfChatStatisticsDto.SafeMaxDate ? TgEfChatStatisticsDto.SafeMaxDate : dtEndRaw;
        DateTimeOffset startDate = dtStart.ToUniversalTime().Date;
        DateTimeOffset tmpEnd = dtEnd.ToUniversalTime().Date;
        DateTimeOffset endDate = (tmpEnd < TgEfChatStatisticsDto.SafeMaxDate.Date) ? tmpEnd.AddDays(1).AddTicks(-1) : tmpEnd;

        
        // Один DbContext не поддерживает параллельное выполнение запросов
        var list = new List<TgEfUserWithCountDto>();
        foreach (var user in distinctUsers)
            list.Add(new() { UserDto = user, Count = await App.BusinessLogicManager.StorageManager.MessageRepository.GetCountAsync(
                x => x.SourceId == Dto.Id && x.UserId == user.Id && x.DtCreated >= startDate.UtcDateTime && x.DtCreated <= endDate.UtcDateTime) });

        foreach (var dto in list.OrderByDescending(x => x.Count).ThenBy(x => x.UserDto.DisplayName, StringComparer.CurrentCultureIgnoreCase))
        {
            ChatStatisticsDto.UserWithCountDtos.Add(dto);
        }
        TotalMessages = list.Sum(x => x.Count);
        ActiveAuthorsCount = list.Count(x => x.Count > 0);
        var maximum = list.Count == 0 ? 0 : list.Max(x => x.Count);
        foreach (var item in ChatStatisticsDto.UserWithCountDtos)
            ActivityRows.Add(new(item.UserDto, item.Count, TotalMessages == 0 ? 0 : item.Count * 100d / TotalMessages,
                maximum == 0 ? 0 : item.Count * 100d / maximum));
        IsCalculated = true;
        IsPeriodExpanded = false;
    }

    private async Task MergeUsersWithPlaceholdersAsync()
    {
        var participantDtos = await App.BusinessLogicManager.ConnectClient.GetParticipantsAsync(Dto.Id);
        var userIds = await App.BusinessLogicManager.StorageManager.MessageRepository.GetUserIdsFromMessagesAsync(x => x.SourceId == Dto.Id);
        var usersDtos = await App.BusinessLogicManager.StorageManager.UserRepository.GetListDtosAsync(take: 0, skip: 0, x => userIds.Contains(x.Id));

        // Собираем все известные идентификаторы
        var existingIds = new HashSet<long>(participantDtos.Select(p => p.Id).Concat(usersDtos.Select(u => u.Id)));
        // Находим отсутствующие идентификаторы
        var missingIds = userIds.Where(id => !existingIds.Contains(id));
        // Создаём записи авторов, ещё не найденных в справочнике
        var placeholderUsers = missingIds.Select(id => new TgEfUserDto
        {
            Id = id,
            IsBot = false,
            IsContact = false
        }).ToList();

        UserDtos = [.. 
            // Участники из Telegram
            participantDtos.Select(x => new TgEfUserDto()
            {
                Id = x.User.id,
                IsUserActive = x.User.IsActive,
                IsBot = x.User.IsBot,
                LastSeenAgo = x.User.LastSeenAgo,
                UserName = x.User.MainUsername,
                AccessHash = x.User.access_hash,
                FirstName = x.User.first_name,
                LastName = x.User.last_name,
                UserNames = x.User.usernames is null ? string.Empty : string.Join("|", x.User.usernames.ToList()),
                PhoneNumber = x.User.phone,
                Status = x.User.status?.ToString() ?? string.Empty,
                RestrictionReason = x.User.restriction_reason is null ? string.Empty : string.Join("|", x.User.restriction_reason.ToList()),
                LangCode = x.User.lang_code,
                IsContact = false,
            }), 
            // Пользователи из таблицы пользователей
            ..usersDtos,
            // Авторы из таблицы сообщений
            ..placeholderUsers
            ];
    }

    /// <summary> Открывает профиль выбранного пользователя </summary>
    internal void OnUserClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        var tag = button.Tag.ToString();
        if (string.IsNullOrEmpty(tag)) return;

        var uid = Guid.Parse(tag);
        NavigationService.NavigateTo(typeof(TgUserDetailsViewModel).FullName!, uid);
    }

    #endregion
}
