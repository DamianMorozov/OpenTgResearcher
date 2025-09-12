namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgUserDetailsViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

	[ObservableProperty]
	public partial Guid Uid { get; set; } = Guid.Empty!;
	[ObservableProperty]
	public partial TgEfUserDto Dto { get; set; } = default!;
    [ObservableProperty]
    public partial List<long> ListIds { get; set; } = [];
    [ObservableProperty]
    public partial ObservableCollection<TgEfUserWithMessagesDto> UserWithMessagesDtos { get; set; } = [];

    public IRelayCommand LoadDataStorageCommand { get; }
	public IRelayCommand ClearViewCommand { get; }
	public IRelayCommand UpdateOnlineCommand { get; }

	public TgUserDetailsViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgUserDetailsViewModel> logger) 
		: base(settingsService, navigationService, logger, nameof(TgUserDetailsViewModel))
	{
		// Commands
		ClearViewCommand = new AsyncRelayCommand(ClearViewAsync);
		LoadDataStorageCommand = new AsyncRelayCommand(LoadDataStorageAsync);
		UpdateOnlineCommand = new AsyncRelayCommand(UpdateOnlineAsync);
    }

	#endregion

	#region Methods

	public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(async () =>
		{
			Uid = e?.Parameter is Guid uid ? uid : Guid.Empty;
			await LoadDataStorageCoreAsync();
			await ReloadUiAsync();
		});

    protected override async Task SetDisplaySensitiveAsync()
    {
        Dto.IsDisplaySensitiveData = IsDisplaySensitiveData;

        foreach (var userWithMessagesDto in UserWithMessagesDtos)
        {
            userWithMessagesDto.IsDisplaySensitiveData = IsDisplaySensitiveData;
            foreach (var messageDto in userWithMessagesDto.MessageDtos)
            {
                messageDto.IsDisplaySensitiveData = IsDisplaySensitiveData;
            }
        }

        await Task.CompletedTask;
    }

    private async Task ClearViewAsync() => await ContentDialogAsync(ClearDataStorageCore, TgResourceExtensions.AskDataClear(), TgEnumLoadDesktopType.Storage);

    private void ClearDataStorageCore() => Dto = new();

    private async Task LoadDataStorageAsync() => await ContentDialogAsync(LoadDataStorageCoreAsync, TgResourceExtensions.AskDataLoad(), TgEnumLoadDesktopType.Storage);

	private async Task LoadDataStorageCoreAsync()
	{
		if (!SettingsService.IsExistsAppStorage) return;

        UserWithMessagesDtos.Clear();
        ListIds.Clear();

		Dto = await App.BusinessLogicManager.StorageManager.UserRepository.GetDtoAsync(x => x.Uid == Uid);
        ListIds = [.. (await App.BusinessLogicManager.StorageManager.MessageRepository.GetListDtosAsync(0, 0, x => x.UserId == Dto.Id)).Select(x => x.SourceId).Distinct()];

        if (ListIds is not null && ListIds.Count != 0)
        {
            foreach (var chatId in ListIds)
            {
                var messageDtos = await App.BusinessLogicManager.StorageManager.MessageRepository
                    .GetListDtosAsync(0, 0, x => x.SourceId == chatId && x.UserId == Dto.Id);
                messageDtos = [.. messageDtos.OrderBy(x => x.Id)];
                var chatDto = await App.BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Id == chatId);
                UserWithMessagesDtos.Add(new TgEfUserWithMessagesDto(Dto, chatDto, messageDtos));
            }
        }
	}

	private async Task UpdateOnlineAsync() => await ContentDialogAsync(UpdateOnlineCoreAsync, TgResourceExtensions.AskUpdateOnline(), TgEnumLoadDesktopType.Online);

    private async Task UpdateOnlineCoreAsync() => await LoadOnlineDataAsync(async () =>
    {
        try
        {
            if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;

            await App.BusinessLogicManager.ConnectClient.SearchSourcesTgAsync(DownloadSettings, TgEnumSourceType.Contact, [Dto.Id]);

            await LoadDataStorageCoreAsync();
        }
        finally
        {
            await LoadDataStorageCoreAsync();
        }
    });

	#endregion
}
