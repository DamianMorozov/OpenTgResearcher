//// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
//// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

//namespace TgDownloaderDesktop.ViewModels;

//[DebuggerDisplay("{ToDebugString()}")]
//public sealed partial class TgBotDetailsViewModel : TgPageViewModelBase
//{
//    #region Public and private fields, properties, constructor

//	[ObservableProperty]
//	public partial Guid Uid { get; set; } = default!;
//	[ObservableProperty]
//	public partial Action ScrollRequested { get; set; } = () => { };
//	public IRelayCommand LoadDataStorageCommand { get; }
//	public IRelayCommand ClearDataStorageCommand { get; }
//	public IRelayCommand UpdateOnlineCommand { get; }
//	public IRelayCommand StopDownloadingCommand { get; }

//	public TgBotDetailsViewModel(ITgSettingsService settingsService, INavigationService navigationService) : base(settingsService, navigationService)
//	{
//		// Commands
//		ClearDataStorageCommand = new AsyncRelayCommand(ClearDataStorageAsync);
//		LoadDataStorageCommand = new AsyncRelayCommand(LoadDataStorageAsync);
//		UpdateOnlineCommand = new AsyncRelayCommand(UpdateOnlineAsync);
//		StopDownloadingCommand = new AsyncRelayCommand(StopDownloadingAsync);
//		// Updates
//		TgGlobalTools.ConnectClient.SetupUpdateStateSource(UpdateStateSource);
//	}

//	#endregion

//	#region Public and private methods

//	public override async Task OnNavigatedToAsync(NavigationEventArgs e) => await LoadDataAsync(async () =>
//		{
//			Uid = e.Parameter is Guid uid ? uid : Guid.Empty;
//			await LoadDataStorageCoreAsync();
//			await ReloadUiAsync();
//		});

//	private async Task ClearDataStorageAsync() => await ContentDialogAsync(ClearDataStorageCoreAsync, TgResourceExtensions.AskDataClear());

//	private async Task ClearDataStorageCoreAsync()
//	{
//		Dto = new();
//		await Task.CompletedTask;
//	}

//	private async Task LoadDataStorageAsync() => await ContentDialogAsync(LoadDataStorageCoreAsync, TgResourceExtensions.AskDataLoad(), useLoadData: true);

//	private async Task LoadDataStorageCoreAsync()
//	{
//		if (!SettingsService.IsExistsAppStorage) return;
//		//Dto = await Repository.GetDtoAsync(x => x.Uid == Uid);
//		ScrollRequested?.Invoke();
//	}

//	private async Task UpdateOnlineAsync() => await ContentDialogAsync(UpdateOnlineCoreAsync, TgResourceExtensions.AskUpdateOnline());

//	private async Task UpdateOnlineCoreAsync()
//	{
//		await LoadDataAsync(async () => {
//			IsDownloading = true;
//			if (!await TgGlobalTools.ConnectClient.CheckClientIsReadyAsync()) return;
//			//var entity = Dto.GetEntity();
//			//BotVm.Fill(entity);
//			await DownloadSettings.UpdateSourceWithSettingsAsync();

//			await TgGlobalTools.ConnectClient.DownloadAllDataAsync(DownloadSettings);
//			await DownloadSettings.UpdateSourceWithSettingsAsync();
//			await LoadDataStorageCoreAsync();
//			IsDownloading = false;
//		});
//	}

//	private async Task StopDownloadingAsync() => await ContentDialogAsync(StopDownloadingCoreAsync, TgResourceExtensions.AskStopDownloading());

//	private async Task StopDownloadingCoreAsync()
//	{
//		if (!await TgGlobalTools.ConnectClient.CheckClientIsReadyAsync()) return;
//		TgGlobalTools.ConnectClient.SetForceStopDownloading();
//	}

//	#endregion
//}