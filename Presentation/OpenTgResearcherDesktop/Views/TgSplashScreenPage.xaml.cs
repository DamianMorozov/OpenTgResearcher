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

        await ViewModel.LoadingVelopackInstallAsync();
        Bindings.Update();
        await ViewModel.LoadingNotificationsAsync();
        Bindings.Update();
        await ViewModel.LoadingLoggingAsync();
        Bindings.Update();
        await ViewModel.LoadingStorageAsync();
        Bindings.Update();
        await ViewModel.LoadingLicenseAsync();
        Bindings.Update();
        await ViewModel.LoadingCompleteAsync();
        Bindings.Update();
    }

    #endregion
}
