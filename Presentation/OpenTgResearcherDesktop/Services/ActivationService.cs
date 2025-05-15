// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Services;

public sealed class ActivationService(ActivationHandler<LaunchActivatedEventArgs> defaultHandler, IEnumerable<IActivationHandler> activationHandlers,
	ITgSettingsService settingsService) : IActivationService
{
	private UIElement? _shell;

	public async Task ActivateAsync(object activationArgs)
	{
		// Execute tasks before activation
		await settingsService.LoadAsync();
		// Set the MainWindow Content
		if (App.MainWindow.Content == null)
		{
            // ShellPage loading
            _shell = App.GetService<ShellPage>();
			App.MainWindow.Content = _shell ?? new Frame();
        }
        // Handle activation via ActivationHandlers
        await HandleActivationAsync(activationArgs);
		// Activate the MainWindow
		App.MainWindow.Activate();

        // SplashScreen loading
        var content = App.MainWindow.Content;
        var splashScreenPage = App.GetService<TgSplashScreenPage>();
        if (splashScreenPage is not null)
        {
            App.MainWindow.Content = splashScreenPage;
            if (splashScreenPage.ViewModel is TgSplashScreenViewModel splashScreenViewModel)
            {
                await splashScreenViewModel.LoadAsync();
            }
        }
        // Back to ShellPage
        App.MainWindow.Content = content;
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
