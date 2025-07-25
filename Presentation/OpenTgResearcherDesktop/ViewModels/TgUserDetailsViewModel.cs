﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgUserDetailsViewModel : TgPageViewModelBase
{
    #region Public and private fields, properties, constructor

	[ObservableProperty]
	public partial Guid Uid { get; set; } = Guid.Empty!;
	[ObservableProperty]
	public partial TgEfUserDto Dto { get; set; } = default!;
    [ObservableProperty]
    public partial List<long> ChatIds { get; set; } = [];
    [ObservableProperty]
    public partial ObservableCollection<TgEfUserWithMessagesDto> UserWithMessagesDtos { get; set; } = [];

    public IRelayCommand LoadDataStorageCommand { get; }
	public IRelayCommand ClearDataStorageCommand { get; }
	public IRelayCommand UpdateOnlineCommand { get; }

	public TgUserDetailsViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgUserDetailsViewModel> logger) 
		: base(settingsService, navigationService, logger, nameof(TgUserDetailsViewModel))
	{
		// Commands
		ClearDataStorageCommand = new AsyncRelayCommand(ClearDataStorageAsync);
		LoadDataStorageCommand = new AsyncRelayCommand(LoadDataStorageAsync);
		UpdateOnlineCommand = new AsyncRelayCommand(UpdateOnlineAsync);
        SetDisplaySensitiveCommand = new AsyncRelayCommand(SetDisplaySensitiveAsync);
    }

	#endregion

	#region Public and private methods

	public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadDataAsync(async () =>
		{
			Uid = e?.Parameter is Guid uid ? uid : Guid.Empty;
            if (e?.Parameter is Tuple<long, long> tuple)
            {
                Dto = await App.BusinessLogicManager.StorageManager.UserRepository.GetDtoAsync(x => x.Id == tuple.Item1);
                Uid = Dto.Uid;
                ChatIds.Clear();
                ChatIds.Add(tuple.Item2);
            }
			await LoadDataStorageCoreAsync();
			await ReloadUiAsync();
		});

    private async Task SetDisplaySensitiveAsync()
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

    private async Task ClearDataStorageAsync() => await ContentDialogAsync(ClearDataStorageCoreAsync, TgResourceExtensions.AskDataClear());

	private async Task ClearDataStorageCoreAsync()
	{
		Dto = new();
		await Task.CompletedTask;
	}

	private async Task LoadDataStorageAsync() => await ContentDialogAsync(LoadDataStorageCoreAsync, TgResourceExtensions.AskDataLoad(), useLoadData: true);

	private async Task LoadDataStorageCoreAsync()
	{
		if (!SettingsService.IsExistsAppStorage) return;

		Dto = await App.BusinessLogicManager.StorageManager.UserRepository.GetDtoAsync(x => x.Uid == Uid);

        UserWithMessagesDtos.Clear();
        if (ChatIds is not null && ChatIds.Any())
        {
            foreach (var chatId in ChatIds)
            {
                var messageDtos = await App.BusinessLogicManager.StorageManager.MessageRepository
                    .GetListDtosAsync(0, 0, x => x.SourceId == chatId);
                messageDtos = [.. messageDtos.OrderBy(x => x.Id)];
                var chatDto = await App.BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Id == chatId);
                UserWithMessagesDtos.Add(new TgEfUserWithMessagesDto(Dto, chatDto, messageDtos));
            }
        }
	}

	private async Task UpdateOnlineAsync() => await ContentDialogAsync(UpdateOnlineCoreAsync, TgResourceExtensions.AskUpdateOnline());

	private async Task UpdateOnlineCoreAsync()
	{
		await LoadDataAsync(async () => {
			if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;

			await App.BusinessLogicManager.ConnectClient.SearchSourcesTgAsync(DownloadSettings, TgEnumSourceType.Contact);
			
            await LoadDataStorageCoreAsync();
		});
	}

	#endregion
}