// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public partial class TgLogsViewModel : TgPageViewModelBase
{
	#region Public and private fields, properties, constructor

	[ObservableProperty]
	public partial ObservableCollection<string> CollectionLogs { get; private set; } = new();
	public IRelayCommand UpdateFromFileCommand { get; }

	public TgLogsViewModel(ITgSettingsService settingsService, INavigationService navigationService) : base(settingsService, navigationService)
	{
		// Commands
		UpdateFromFileCommand = new AsyncRelayCommand(UpdateFromFileAsync);
	}

	#endregion

	#region Public and private methods

	public override async Task OnNavigatedToAsync(NavigationEventArgs e) => await LoadDataAsync(async () =>
	{
		await UpdateFromFileCoreAsync();
		await ReloadUiAsync();
	});

	private async Task UpdateFromFileAsync() => await UpdateFromFileCoreAsync();

	private async Task UpdateFromFileCoreAsync()
	{
		try
		{
			var appFolder = SettingsService.AppFolder;
			var storageFolder = await StorageFolder.GetFolderFromPathAsync(appFolder);
			var storageFile = await storageFolder.GetFileAsync(TgFileUtils.FileLog);
			var lines = await FileIO.ReadLinesAsync(storageFile);

			CollectionLogs.Clear();
			foreach (var line in lines)
			{
				CollectionLogs.Add(line);
			}
		}
		catch (Exception ex)
		{
			await TgDesktopUtils.FileLogAsync(ex, "An error occurred while loading error logs.");
		}
	}

	#endregion
}
