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

    /// <summary> Handle the activation for app notifications </summary>
	protected override async Task HandleInternalAsync(LaunchActivatedEventArgs args)
	{
        if (App.MainWindow?.DispatcherQueue is not null)
        {
            var tcs = new TaskCompletionSource();
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    App.MainWindow.ShowMessageDialogAsync("TODO: Handle notification activations.", "Notification Activation");
                    tcs.SetResult();
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            await tcs.Task;
        }
	}
}
