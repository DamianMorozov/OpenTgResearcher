namespace OpenTgResearcherDesktop.Views;

[SupportedOSPlatform("windows10.0.17763.0")]
public sealed partial class TgSplashScreenPage
{
	#region Fields, properties, constructor

	public override TgSplashScreenViewModel ViewModel { get; }
    private readonly TgSplashAnimationHelper _animation;
    private readonly TgButtonOrbitAnimationHelper _buttonAnimation;

    public TgSplashScreenPage()
	{
		ViewModel = App.GetService<TgSplashScreenViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        GridContent = MainContent;
        _animation = new(MainContent, SplashOrbit, SplashScanBand, SplashContinueButton, SplashLoadingRows);
        _buttonAnimation = new(SplashContinueHost, ContinueOrbitHead, ContinueOrbitTailNear, ContinueOrbitTailMiddle, ContinueOrbitTailFar);
        Loaded += OnSplashLoaded;
        Unloaded += OnSplashUnloaded;
    }

    #endregion

    #region Methods

    // Показываем страницу сразу и передаём декоративное движение композитору Windows
    private void OnSplashLoaded(object sender, RoutedEventArgs e)
    {
        MainContent.Visibility = Visibility.Visible;
        MainContent.UpdateLayout();
        PageLoaded(sender, e);
        var animationsEnabled = new UISettings().AnimationsEnabled;
        _animation.Start(isLoading: !ViewModel.IsLoadComplete, animationsEnabled: animationsEnabled);
        TgLogUtils.WriteLog(animationsEnabled ? "Заставка: мягкое появление и орбита запущены на пять секунд" : "Заставка: анимации отключены настройками Windows");
    }

    // Останавливаем все визуальные эффекты при продолжении работы и закрытии окна
    private void OnSplashUnloaded(object sender, RoutedEventArgs e)
    {
        _animation.Stop();
        _buttonAnimation.Stop();
        TgLogUtils.WriteLog("Заставка: анимации страницы остановлены");
    }

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
        await _animation.WaitForAppearanceAsync();
        if (App.IsClosing) return;
        await ViewModel.LoadingCompleteAsync();
        if (App.IsClosing) return;
        Bindings.Update();
        MainContent.UpdateLayout();
        _buttonAnimation.Start(animationsEnabled: new UISettings().AnimationsEnabled);
        TgLogUtils.WriteLog("Заставка: загрузка завершена, продолжение доступно");
    }

    #endregion
}
