﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgContactDetailsViewModel : TgPageViewModelBase
{
    #region Public and private fields, properties, constructor

    private TgEfContactRepository Repository { get; } = new(TgEfUtils.EfContext);
	[ObservableProperty]
	public partial Guid Uid { get; set; } = default!;
	[ObservableProperty]
	public partial TgEfContactDto Dto { get; set; } = default!;
	public IRelayCommand LoadDataStorageCommand { get; }
	public IRelayCommand ClearDataStorageCommand { get; }
	public IRelayCommand UpdateOnlineCommand { get; }

	public TgContactDetailsViewModel(ITgSettingsService settingsService, INavigationService navigationService) : base(settingsService, navigationService)
	{
		// Commands
		ClearDataStorageCommand = new AsyncRelayCommand(ClearDataStorageAsync);
		LoadDataStorageCommand = new AsyncRelayCommand(LoadDataStorageAsync);
		UpdateOnlineCommand = new AsyncRelayCommand(UpdateOnlineAsync);
	}

	#endregion

	#region Public and private methods

	public override async Task OnNavigatedToAsync(NavigationEventArgs e) => await LoadDataAsync(async () =>
		{
			TgEfUtils.AppStorage = SettingsService.AppStorage;
			TgEfUtils.RecreateEfContext();
			Uid = e.Parameter is Guid uid ? uid : Guid.Empty;
			await LoadDataStorageCoreAsync();
			await ReloadUiAsync();
		});

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
		Dto = await Repository.GetDtoAsync(x => x.Uid == Uid);
	}

	private async Task UpdateOnlineAsync() => await ContentDialogAsync(UpdateOnlineCoreAsync, TgResourceExtensions.AskUpdateOnline());

	private async Task UpdateOnlineCoreAsync()
	{
		await LoadDataAsync(async () => {
			if (!await TgDesktopUtils.TgClient.CheckClientIsReadyAsync()) return;
			var tgDownloadSettings = new TgDownloadSettingsViewModel();
			await TgDesktopUtils.TgClient.SearchSourcesTgAsync(tgDownloadSettings, TgEnumSourceType.Contact);
			await LoadDataStorageCoreAsync();
		});
	}

	#endregion
}