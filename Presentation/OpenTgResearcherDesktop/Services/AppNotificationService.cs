namespace OpenTgResearcherDesktop.Services;

public sealed class AppNotificationService : IAppNotificationService
{
	#region Fields, properties, constructor

	public AppNotificationService()
	{
		//
	}

	~AppNotificationService()
	{
		Unregister();
	}

	#endregion

	#region Methods

	public void Initialize()
	{
		AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;
		AppNotificationManager.Default.Register();
	}

    /// <summary> Handle notification invocations when your app is already running </summary>
	public async void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
	{
        await TgDesktopUtils.InvokeOnUIThreadAsync(async () =>
        {
            await App.MainWindow.ShowMessageDialogAsync(
                $"{TgLocaleHelper.Instance.AppVersion}: v{TgDataUtils.GetTrimVersion(Assembly.GetExecutingAssembly().GetName().Version)}",
                TgConstants.OpenTgResearcherDesktop);
            App.MainWindow.BringToFront();
        });
	}

	public bool Show(string payload)
	{
		var appNotification = new AppNotification(payload);

		AppNotificationManager.Default.Show(appNotification);

		return appNotification.Id != 0;
	}

	public NameValueCollection ParseArguments(string arguments)
	{
		return HttpUtility.ParseQueryString(arguments);
	}

	public void Unregister()
	{
		AppNotificationManager.Default.Unregister();
	}

	public void SetClientIsConnected()
	{

	}

	#endregion
}
