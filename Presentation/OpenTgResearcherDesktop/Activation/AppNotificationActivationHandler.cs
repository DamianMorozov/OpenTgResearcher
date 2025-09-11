// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Activation;

public class AppNotificationActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    public AppNotificationActivationHandler()
    {
        //
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        return AppInstance.GetCurrent().GetActivatedEventArgs()?.Kind == ExtendedActivationKind.AppNotification;
    }

    /// <summary> Handle the activation for app notifications </summary>
	protected override async Task HandleInternalAsync(LaunchActivatedEventArgs args) => 
        await TgDesktopUtils.InvokeOnUIThreadAsync(() => App.MainWindow.ShowMessageDialogAsync("TODO: Handle notification activations.", "Notification Activation"));
}
