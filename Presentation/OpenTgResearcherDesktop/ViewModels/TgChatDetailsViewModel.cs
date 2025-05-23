// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgChatDetailsViewModel : TgPageViewModelBase
{
    #region Public and private fields, properties, constructor

    private TgEfSourceRepository Repository { get; } = new();
    private TgEfMessageRepository MessageRepository { get; } = new();
	[ObservableProperty]
	public partial Guid Uid { get; set; } = Guid.Empty!;
	[ObservableProperty]
	public partial TgEfSourceDto Dto { get; set; } = null!;
	[ObservableProperty]
	public partial ObservableCollection<TgEfMessageDto> Messages { get; set; } = null!;
	[ObservableProperty]
	public partial bool EmptyData { get; set; } = true;
	[ObservableProperty]
	public partial Action ScrollRequested { get; set; } = () => { };
	public IRelayCommand LoadDataStorageCommand { get; }
	public IRelayCommand ClearDataStorageCommand { get; }
	public IRelayCommand UpdateOnlineCommand { get; }
	public IRelayCommand StopDownloadingCommand { get; }

	public TgChatDetailsViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgChatDetailsViewModel> logger) 
		: base(settingsService, navigationService, logger, nameof(TgChatDetailsViewModel))
	{
		// Commands
		ClearDataStorageCommand = new AsyncRelayCommand(ClearDataStorageAsync);
		LoadDataStorageCommand = new AsyncRelayCommand(LoadDataStorageAsync);
		UpdateOnlineCommand = new AsyncRelayCommand(UpdateOnlineAsync);
		StopDownloadingCommand = new AsyncRelayCommand(StopDownloadingAsync);
		// Updates
		App.BusinessLogicManager.ConnectClient.SetupUpdateStateSource(UpdateStateSource);
	}

	#endregion

	#region Public and private methods

	public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadDataAsync(async () =>
		{
			Uid = e?.Parameter is Guid uid ? uid : Guid.Empty;
			await LoadDataStorageCoreAsync();
			await ReloadUiAsync();
		});

	private async Task ClearDataStorageAsync() => await ContentDialogAsync(ClearDataStorageCoreAsync, TgResourceExtensions.AskDataClear());

	private async Task ClearDataStorageCoreAsync()
	{
		Dto = new();
		Messages = [];
		EmptyData = true;
		await Task.CompletedTask;
	}

	private async Task LoadDataStorageAsync() => await ContentDialogAsync(LoadDataStorageCoreAsync, TgResourceExtensions.AskDataLoad(), useLoadData: true);

	private async Task LoadDataStorageCoreAsync()
	{
		await ReloadUiAsync();
		if (!SettingsService.IsExistsAppStorage) return;
		Dto = await Repository.GetDtoAsync(x => x.Uid == Uid);
		Messages = [.. await MessageRepository.GetListDtosDescAsync(take: 100, skip: 0, x => x.SourceId == Dto.Id, isReadOnly: true)];
		EmptyData = !Messages.Any();
		ScrollRequested?.Invoke();
	}

	private async Task UpdateOnlineAsync() => await ContentDialogAsync(UpdateOnlineCoreAsync, TgResourceExtensions.AskUpdateOnline());

	private async Task UpdateOnlineCoreAsync()
	{
		await LoadDataAsync(async () => {
			IsDownloading = true;
			if (!await App.BusinessLogicManager.ConnectClient.CheckClientIsReadyAsync()) return;
			var entity = Dto.GetNewEntity();
			DownloadSettings.SourceVm.Fill(entity);
			DownloadSettings.SourceVm.Dto.DtChanged = DateTime.Now;
			await DownloadSettings.UpdateSourceWithSettingsAsync();

			StateSourceDirectory = Dto.Directory;

			await App.BusinessLogicManager.ConnectClient.DownloadAllDataAsync(DownloadSettings);
			await DownloadSettings.UpdateSourceWithSettingsAsync();
			//await BusinessLogicManager.ConnectClient.UpdateStateSourceAsync(DownloadSettings.SourceVm.Dto.Id, DownloadSettings.SourceVm.Dto.FirstId, TgLocale.SettingsSource);
			await LoadDataStorageCoreAsync();
			IsDownloading = false;
		});
	}

	private async Task StopDownloadingAsync() => await ContentDialogAsync(StopDownloadingCoreAsync, TgResourceExtensions.AskStopDownloading());

	private async Task StopDownloadingCoreAsync()
	{
		if (!await App.BusinessLogicManager.ConnectClient.CheckClientIsReadyAsync()) return;
        App.BusinessLogicManager.ConnectClient.SetForceStopDownloading();
	}

	#endregion
}