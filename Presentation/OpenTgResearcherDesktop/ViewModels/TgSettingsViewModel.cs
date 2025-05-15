// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public partial class TgSettingsViewModel : TgPageViewModelBase
{
    #region Public and private fields, properties, constructor

    [ObservableProperty]
    public partial string AppStorage { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string AppSession { get; set; } = string.Empty;
    [ObservableProperty]
    public partial bool IsExistsAppStorage { get; set; }
    [ObservableProperty]
    public partial bool IsExistsAppSession { get; set; }

    public IRelayCommand SettingsLoadCommand { get; }
	public IRelayCommand SettingsDefaultCommand { get; }
	public IRelayCommand SettingsSaveCommand { get; }

	public TgSettingsViewModel(ITgSettingsService settingsService, INavigationService navigationService, ITgLicenseService licenseService, ILogger<TgSettingsViewModel> logger) 
		: base(settingsService, navigationService, licenseService, logger, nameof(TgSettingsViewModel))
	{
		// Commands
		SettingsLoadCommand = new AsyncRelayCommand(SettingsLoadAsync);
		SettingsDefaultCommand = new AsyncRelayCommand(SettingsDefaultAsync);
		SettingsSaveCommand = new AsyncRelayCommand(SettingsSaveAsync);
	}

	#endregion

	#region Public and private methods

	public override async Task OnNavigatedToAsync(NavigationEventArgs e) => await SettingsLoadCoreAsync();

	private async Task SettingsLoadAsync() => await ContentDialogAsync(SettingsLoadCoreAsync, TgResourceExtensions.AskSettingsLoad());

	private async Task SettingsLoadCoreAsync()
	{
		await SettingsService.LoadAsync();
        LoadSettingsFromService();
    }

	private async Task SettingsDefaultAsync() => await ContentDialogAsync(SettingsDefaultCoreAsync, TgResourceExtensions.AskSettingsDefault());

	private async Task SettingsDefaultCoreAsync()
    {
        SettingsService.Default();
        LoadSettingsFromService();
        await Task.CompletedTask;
    }

    private async Task SettingsSaveAsync()
	{
		await ContentDialogAsync(SettingsSaveCoreAsync, TgResourceExtensions.AskSettingsSave());
	}

	private async Task SettingsSaveCoreAsync()
	{
        SettingsService.AppStorage = AppStorage;
        SettingsService.AppSession = AppSession;
        await SettingsService.SaveAsync();
        LoadSettingsFromService();
        await ContentDialogAsync(async () => { await Windows.ApplicationModel.Core.CoreApplication.RequestRestartAsync(string.Empty); },
			TgResourceExtensions.AskRestartApp(), ContentDialogButton.Primary);
    }

    private void LoadSettingsFromService()
    {
        AppStorage = SettingsService.AppStorage;
        AppSession = SettingsService.AppSession;
        IsExistsAppStorage = SettingsService.IsExistsAppStorage;
        IsExistsAppSession = SettingsService.IsExistsAppSession;
    }

    #endregion
}
