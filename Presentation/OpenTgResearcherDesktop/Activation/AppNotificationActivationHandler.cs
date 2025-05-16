// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Activation;

public class AppNotificationActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
	//private readonly INavigationService _navigationService;
	//private readonly IAppNotificationService _notificationService;

	public AppNotificationActivationHandler(INavigationService navigationService, IAppNotificationService notificationService)
	{
		//_navigationService = navigationService;
		//_notificationService = notificationService;
	}

	protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
	{
		return AppInstance.GetCurrent().GetActivatedEventArgs()?.Kind == ExtendedActivationKind.AppNotification;
	}

	protected override async Task HandleInternalAsync(LaunchActivatedEventArgs args)
	{
		// TODO: Handle notification activations.

		//// // Access the AppNotificationActivatedEventArgs.
		//// var activatedEventArgs = (AppNotificationActivatedEventArgs)AppInstance.GetCurrent().GetActivatedEventArgs().Data;

		//// // Navigate to a specific page based on the notification arguments.
		//// if (notificationService.ParseArguments(activatedEventArgs.Argument)["action"] == "Settings")
		//// {
		////     // Queue navigation with low priority to allow the UI to initialize.
		////     App.MainWindow.DispatcherQueue.TryEnqueueWithLog(DispatcherQueuePriority.Low, () =>
		////     {
		////         navigationService.NavigateTo(typeof(TgSettingsViewModel).FullName!);
		////     });
		//// }

		App.MainWindow.DispatcherQueue.TryEnqueueWithLog(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
		{
			App.MainWindow.ShowMessageDialogAsync("TODO: Handle notification activations.", "Notification Activation");
		});

		await Task.CompletedTask;
	}
}
