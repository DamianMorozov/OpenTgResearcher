namespace OpenTgResearcherDesktop.Views;

[SupportedOSPlatform("windows10.0.17763.0")]
public sealed partial class TgSplashScreenPage
{
	#region Fields, properties, constructor

	public override TgSplashScreenViewModel ViewModel { get; }

    public TgSplashScreenPage()
	{
		ViewModel = App.GetService<TgSplashScreenViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        GridContent = MainContent;
        Loaded += PageLoadedWithAnimation;
    }

    #endregion

    #region Methods

    public async Task LoadSplashScreenAsync(Action backToMainWindow)
    {
        ViewModel.BackToMainWindow = backToMainWindow;

        // Loading logging
        await ViewModel.LoadingLoggingAsync();
        Bindings.Update();

        // Loading Velopack Installer
        await ViewModel.LoadingVelopackInstallerAsync();
        Bindings.Update();

        // Loading settings
        await ViewModel.LoadingSettingsAsync();

        // Loading storage
        await ViewModel.LoadingStorageAsync();
        Bindings.Update();

        // Loading license
        await ViewModel.LoadingLicenseAsync();
        Bindings.Update();

        // Loading notifications
        await ViewModel.LoadingNotificationsAsync();
        Bindings.Update();

        // Loading hardware control
        ViewModel.LoadingHardwareControl();
        Bindings.Update();

        // Loading complete
        await ViewModel.LoadingCompleteAsync();
        Bindings.Update();
    }

    #endregion
}
