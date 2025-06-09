// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Views;

[SupportedOSPlatform("windows10.0.17763.0")]
public sealed partial class TgSplashScreenPage
{
	#region Public and private fields, properties, constructor

	public override TgSplashScreenViewModel ViewModel { get; }

    public TgSplashScreenPage()
	{
		ViewModel = App.GetService<TgSplashScreenViewModel>();
		InitializeComponent();

        GridContent = MainContent;
        Loaded += PageLoadedWithAnimation;
    }

    #endregion

    #region Public and private methods

    public async Task LoadSplashScreenAsync(Action backToMainWindow)
    {
        ViewModel.BackToMainWindow = backToMainWindow;

        // Loading Velopack Installer
        await ViewModel.LoadingVelopackInstallerAsync();
        Bindings.Update();
        // Loading license
        await ViewModel.LoadingLicenseAsync();
        Bindings.Update();
        // Loading notifications
        await ViewModel.LoadingNotificationsAsync();
        Bindings.Update();
        // Loading logging
        await ViewModel.LoadingLoggingAsync();
        Bindings.Update();
        // Loading storage
        await ViewModel.LoadingStorageAsync();
        Bindings.Update();
        // Loading complete
        await ViewModel.LoadingCompleteAsync();
        Bindings.Update();
    }

    #endregion
}
