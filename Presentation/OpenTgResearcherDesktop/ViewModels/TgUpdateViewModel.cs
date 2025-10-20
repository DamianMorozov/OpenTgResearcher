namespace OpenTgResearcherDesktop.ViewModels;

public partial class TgUpdateViewModel : TgPageViewModelBase
{
	#region Fields, properties, constructor

	[ObservableProperty]
	public partial string UpdateLog { get; set; } = string.Empty;

	public IAsyncRelayCommand UpdateReleaseCommand { get; }
	public IAsyncRelayCommand UpdatePreviewCommand { get; }

	public TgUpdateViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgUpdateViewModel> logger) : base(loadStateService, settingsService, navigationService, logger, nameof(TgUpdateViewModel))
	{
		// Commands
		UpdateReleaseCommand = new AsyncRelayCommand(UpdateReleaseAsync);
		UpdatePreviewCommand = new AsyncRelayCommand(UpdatePreviewAsync);
	}

	#endregion

	#region Methods

	public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(ReloadUiAsync);

	private async Task UpdateReleaseAsync() => 
        await ContentDialogAsync(() => VelopackUpdateAsync(isPreview: false), TgResourceExtensions.AskUpdateReleaseApp(), TgEnumLoadDesktopType.Online);

	private async Task UpdatePreviewAsync() => 
        await ContentDialogAsync(() => VelopackUpdateAsync(isPreview: true), TgResourceExtensions.AskUpdatePreviewApp(), TgEnumLoadDesktopType.Online);

    /// <summary> Velopack installer update </summary>
    private async Task VelopackUpdateAsync(bool isPreview)
    {
        // Update UI on UI thread
        await TgDesktopUtils.InvokeOnUIThreadAsync(async () =>
        {
            UpdateLog = string.Empty;
            try
            {
                UpdateLog += "Update started" + Environment.NewLine;
                TgAppSettingsHelper.Instance.SetVersion(Assembly.GetExecutingAssembly());
                UpdateLog += $"{TgConstants.OpenTgResearcherDesktop} {TgAppSettingsHelper.Instance.AppVersion}" + Environment.NewLine;
                UpdateLog += "Checking updates on the link github.com" + Environment.NewLine;
                await Task.Delay(TgConstants.TimeOutUIShortMilliseconds);

                var mgr = new UpdateManager(new GithubSource(TgConstants.LinkGitHub, string.Empty, prerelease: isPreview));
                // Check for new version
                var newVersion = await mgr.CheckForUpdatesAsync();
                if (newVersion is null)
                {
                    UpdateLog += "You are using the latest version of the software" + Environment.NewLine;
                    return;
                }

                // Download new version
                UpdateLog += "Download new version" + Environment.NewLine;
                await Task.Delay(TgConstants.TimeOutUIShortMilliseconds);

                await mgr.DownloadUpdatesAsync(newVersion);
                await Task.Delay(TgConstants.TimeOutUIShortMilliseconds);
            }
            // Cannot perform this operation in an application which is not installed
            catch (Exception ex)
            {
                UpdateLog += Environment.NewLine + ex.Message + Environment.NewLine;
                LogError(ex);
            }
        });
    }

    #endregion
}
