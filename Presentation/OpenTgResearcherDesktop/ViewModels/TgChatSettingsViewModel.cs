namespace OpenTgResearcherDesktop.ViewModels;

public sealed partial class TgChatSettingsViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial Guid Uid { get; set; } = Guid.Empty!;
    [ObservableProperty]
    public partial TgEfSourceDto Dto { get; set; } = new();
    [ObservableProperty]
    public partial TgEfSourceDto DiscussionDto { get; set; } = new();
    [ObservableProperty]
    public partial bool IsDiscussionDtoExists { get; set; }
    
    public IAsyncRelayCommand OpenDiscussionChatCommand { get; }

    public TgChatSettingsViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgChatSettingsViewModel> logger)
        : base(loadStateService, settingsService, navigationService, logger, nameof(TgChatSettingsViewModel))
    {
        // Команда перехода к обсуждению
        OpenDiscussionChatCommand = new AsyncRelayCommand(OpenDiscussionChatAsync);
    }

    #endregion

    #region Methods

    /// <summary> Принимает общий черновик чата или загружает настройки по идентификатору </summary>
    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => 
        await LoadStorageDataAsync(async () =>
        {
            Dto = e?.Parameter as TgEfSourceDto ?? new();
            Uid = e?.Parameter is Guid uid ? uid : Dto.Uid;
            DiscussionDto = new();
            IsDiscussionDtoExists = false;
            if (!SettingsService.IsExistsAppStorage) return;
            if (e?.Parameter is not TgEfSourceDto)
                Dto = await App.BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Uid == Uid);
            DiscussionDto = await App.BusinessLogicManager.StorageManager.SourceRepository.FindCommentDtoSourceAsync(Dto.Id);
            IsDiscussionDtoExists = DiscussionDto.Id > 0;
        });

    private async Task OpenDiscussionChatAsync()
    {
        // Переходим в существующий чат обсуждения
        if (IsDiscussionDtoExists)
        {
            NavigationService.NavigateTo(typeof(TgChatViewModel).FullName!, DiscussionDto.Uid);
        }
        // Сообщаем об отсутствии чата обсуждения
        else
        {
            await ContentDialogAsync(TgResourceExtensions.Navigation(), TgResourceExtensions.DiscussionChatNotFound());
        }
    }

    #endregion
}
