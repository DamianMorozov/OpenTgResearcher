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

    /// <summary> Загрузка заставки до завершения запуска или закрытия главного окна </summary>
    public async Task LoadSplashScreenAsync(Action backToMainWindow)
    {
        if (App.IsClosing) return;
        ViewModel.BackToMainWindow = backToMainWindow;

        // Загрузка журналирования
        await ViewModel.LoadingLoggingAsync();
        if (App.IsClosing) return;
        Bindings.Update();

        // Загрузка установщика Velopack
        await ViewModel.LoadingVelopackInstallerAsync();
        if (App.IsClosing) return;
        Bindings.Update();

        // Загрузка настроек
        await ViewModel.LoadingSettingsAsync();
        if (App.IsClosing) return;

        // Загрузка хранилища
        await ViewModel.LoadingStorageAsync();
        if (App.IsClosing) return;
        Bindings.Update();

        // Загрузка лицензии
        await ViewModel.LoadingLicenseAsync();
        if (App.IsClosing) return;
        Bindings.Update();

        // Загрузка уведомлений
        await ViewModel.LoadingNotificationsAsync();
        if (App.IsClosing) return;
        Bindings.Update();

        // Загрузка мониторинга оборудования
        ViewModel.LoadingHardwareControl();
        Bindings.Update();

        // Завершение загрузки
        await ViewModel.LoadingCompleteAsync();
        if (App.IsClosing) return;
        Bindings.Update();
    }

    #endregion
}
