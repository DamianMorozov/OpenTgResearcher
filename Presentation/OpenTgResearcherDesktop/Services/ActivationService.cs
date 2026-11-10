namespace OpenTgResearcherDesktop.Services;

public sealed class ActivationService(ActivationHandler<LaunchActivatedEventArgs> defaultHandler, IEnumerable<IActivationHandler> activationHandlers,
	ITgSettingsService settingsService) : IActivationService
{
    private UIElement? _content;

    public async Task ActivateAsync(object activationArgs)
    {
        try
        {
            // Загружаем параметры до активации окна
            settingsService.Load();
            TgProgramIntegration.InitializeAppearance(settingsService);
            // Создаём содержимое главного окна
            if (App.MainWindow.Content == null)
            {
                // Загружаем оболочку приложения
                App.MainWindow.Content = App.GetService<ShellPage>();
            }
            // Передаём запуск обработчикам активации
            await HandleActivationAsync(activationArgs);
            if (App.IsClosing) return;
            // Активируем главное окно
            App.MainWindow.Activate();

            // Показываем заставку
            await LoadingSplashScreen();
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }
    }

    // Показываем заставку на время подготовки приложения
    private async Task LoadingSplashScreen()
    {
        _content = App.MainWindow.Content;
        var splashScreenPage = App.GetService<TgSplashScreenPage>();
        if (splashScreenPage is not null)
        {
            App.MainWindow.Content = splashScreenPage;
            await splashScreenPage.LoadSplashScreenAsync(BackToMainWindow);
        }
    }

    // Возвращаем оболочку и открываем параметры новой установленной версии
    private void BackToMainWindow()
    {
        if (_content is null) return;
        App.MainWindow.Content = _content;
        try
        {
            var preferences = new TgProgramPreferences(settingsService);
            if (preferences.NeedsSetup(TgProgramIntegration.CurrentVersion, TgProgramIntegration.IsInstalled))
                App.GetService<INavigationService>().NavigateTo(typeof(TgSettingsViewModel).FullName!);
        }
        catch (Exception ex) { TgLogUtils.WriteException(ex); }
    }

    private async Task HandleActivationAsync(object activationArgs)
	{
		var activationHandler = activationHandlers.FirstOrDefault(h => h.CanHandle(activationArgs));
		if (activationHandler != null)
		{
			await activationHandler.HandleAsync(activationArgs);
		}
		if (defaultHandler.CanHandle(activationArgs))
		{
			await defaultHandler.HandleAsync(activationArgs);
		}
	}
}
