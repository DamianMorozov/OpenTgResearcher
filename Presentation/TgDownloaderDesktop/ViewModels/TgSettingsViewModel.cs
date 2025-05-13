// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public partial class TgSettingsViewModel : TgPageViewModelBase
{
	#region Public and private fields, properties, constructor

	public IRelayCommand SettingsLoadCommand { get; }
	public IRelayCommand SettingsDefaultCommand { get; }
	public IRelayCommand SettingsSaveCommand { get; }

	public TgSettingsViewModel(ITgSettingsService settingsService, INavigationService navigationService, ITgLicenseService licenseService, ILogger<TgSettingsViewModel> logger) 
		: base(settingsService, navigationService, licenseService, logger, nameof(TgSettingsViewModel))
	{
		SettingsLoadCommand = new AsyncRelayCommand(SettingsLoadAsync);
		SettingsDefaultCommand = new AsyncRelayCommand(SettingsDefaultAsync);
		SettingsSaveCommand = new AsyncRelayCommand(SettingsSaveAsync);
	}

	#endregion

	#region Public and private methods

	public override async Task OnNavigatedToAsync(NavigationEventArgs e) => await SettingsService.LoadAsync();

	private async Task SettingsLoadAsync() => await ContentDialogAsync(SettingsLoadCoreAsync, TgResourceExtensions.AskSettingsLoad());

	private async Task SettingsLoadCoreAsync()
	{
		await SettingsService.LoadAsync();
	}

	private async Task SettingsDefaultAsync() => await ContentDialogAsync(SettingsDefaultCoreAsync, TgResourceExtensions.AskSettingsDefault());

	private async Task SettingsDefaultCoreAsync()
	{
		SettingsService.Default();
		await Task.CompletedTask;
	}

	private async Task SettingsSaveAsync()
	{
		await ContentDialogAsync(SettingsSaveCoreAsync, TgResourceExtensions.AskSettingsSave());
	}

	private async Task SettingsSaveCoreAsync()
	{
		await SettingsService.SaveAsync();
		await ContentDialogAsync(async () => { await Windows.ApplicationModel.Core.CoreApplication.RequestRestartAsync(string.Empty); },
			TgResourceExtensions.AskRestartApp(), ContentDialogButton.Primary);
	}

	#endregion
}
