// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using TgBusinessLogic.Services;

namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public partial class TgUpdateViewModel : TgPageViewModelBase
{
	#region Fields, properties, constructor

	[ObservableProperty]
	public partial string UpdateLog { get; set; } = string.Empty;
	public IRelayCommand UpdateReleaseCommand { get; }
	public IRelayCommand UpdatePreviewCommand { get; }

	public TgUpdateViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgUpdateViewModel> logger) 
		: base(settingsService, navigationService, logger, nameof(TgUpdateViewModel))
	{
		// Commands
		UpdateReleaseCommand = new AsyncRelayCommand(UpdateReleaseAsync);
		UpdatePreviewCommand = new AsyncRelayCommand(UpdatePreviewAsync);
	}

	#endregion

	#region Methods

	public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadDataAsync(async () =>
	{
		await ReloadUiAsync();
	});

	private async Task UpdateReleaseAsync() => await ContentDialogAsync(async () => await VelopackUpdateAsync(isPreview: false), TgResourceExtensions.AskUpdateReleaseApp());

	private async Task UpdatePreviewAsync() => await ContentDialogAsync(async () => await VelopackUpdateAsync(isPreview: true), TgResourceExtensions.AskUpdatePreviewApp());

	/// <summary> Velopack installer update </summary>
	private async Task VelopackUpdateAsync(bool isPreview)
	{
		UpdateLog = string.Empty;
		var log = new StringBuilder();
		try
		{
			log.AppendLine("Update started");
			TgAppSettingsHelper.Instance.SetVersion(Assembly.GetExecutingAssembly());
			log.AppendLine($"{TgConstants.OpenTgResearcherDesktop} {TgAppSettingsHelper.Instance.AppVersion}");
			log.AppendLine("Checking updates on the link github.com");

            // Check the current license for preview
            if (isPreview && App.BusinessLogicManager.LicenseService.CurrentLicense.LicenseType < TgEnumLicenseType.Community)
            {
                log.AppendLine("To check the preview version, you need to obtain a license");
                return;
            }

            var mgr = new UpdateManager(new GithubSource(TgConstants.LinkGitHub, string.Empty, prerelease: isPreview));
			// Check for new version
			var newVersion = await mgr.CheckForUpdatesAsync();
			if (newVersion is null)
			{
				log.AppendLine("You are using the latest version of the software");
				return;
			}
			// Download new version
			log.AppendLine("Download new version");
			await mgr.DownloadUpdatesAsync(newVersion);
			//// Install new version and restart app
			//var prompt = AnsiConsole.Prompt(
			//	new SelectionPrompt<string>()
			//		.Title("  Install new version and restart app?")
			//		.PageSize(Console.WindowHeight - 5)
			//		.MoreChoicesText(TgLocale.MoveUpDown)
			//		.AddChoices(TgLocale.MenuNo, TgLocale.MenuYes));
			//var isYes = prompt.Equals(TgLocale.MenuYes);
			//if (isYes)
			//	mgr.ApplyUpdatesAndRestart(newVersion);
		}
		// Cannot perform this operation in an application which is not installed
		catch (Exception ex)
		{
			log.AppendLine(ex.Message);
			TgLogUtils.WriteException(ex);
		}
		finally
		{
			UpdateLog = log.ToString();
		}
	}

	#endregion
}
