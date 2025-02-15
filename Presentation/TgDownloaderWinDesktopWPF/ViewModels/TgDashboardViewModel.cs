// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderWinDesktopWPF.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgDashboardViewModel : TgPageViewModelBase, INavigationAware
{
    #region Public and private fields, properties, constructor

    public static string AppVersionTitle { get; set; } = string.Empty;
    public static string AppVersionShort { get; set; } = string.Empty;
    public static string AppVersionFull { get; set; } = string.Empty;

    #endregion

    #region Public and private methods

    public TgDashboardViewModel()
    {
        AppVersionTitle = $"{TgConstants.AppTitleDesktop} " +
                          $"v{TgCommonUtils.GetTrimVersion(Assembly.GetExecutingAssembly().GetName().Version)}";
        AppVersionShort = $"v{TgCommonUtils.GetTrimVersion(Assembly.GetExecutingAssembly().GetName().Version)}";
        AppVersionFull = $"{TgDesktopUtils.TgLocale.AppVersion}: {AppVersionShort}";
    }

    public void OnNavigatedTo()
	{
        var task = InitializeViewModelAsync();
        task.Wait();
    }

	public void OnNavigatedFrom() { }

    #endregion

    #region Public and private methods

    // SettingsDefaultCommand
    [RelayCommand]
	public async Task OnSettingsDefaultAsync()
	{
        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
            await Task.Delay(1);
            TgAppSettings.DefaultXmlSettings();
        }, false).ConfigureAwait(false);
	}

    // SettingsSaveCommand
    [RelayCommand]
	public async Task OnSettingsSaveAsync()
	{
        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
            await Task.Delay(1);
            TgAppSettings.StoreXmlSettings();
            TgAppSettings.LoadXmlSettings();
        }, false).ConfigureAwait(false);
	}

	#endregion
}