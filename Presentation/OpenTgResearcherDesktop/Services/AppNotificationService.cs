namespace OpenTgResearcherDesktop.Services;

public sealed class AppNotificationService : IAppNotificationService
{
    #region Fields, properties, constructor

    /// <summary> Cancellation token source </summary>
    private CancellationTokenSource? _notificationCts;
    /// <summary> Cancellation token </summary>
    private CancellationToken _notificationToken = CancellationToken.None;

    public AppNotificationService()
	{
		//
	}

	~AppNotificationService()
	{
        _notificationCts?.Cancel();
        _notificationCts?.Dispose();
        _notificationCts = null;
        _notificationToken = CancellationToken.None;
        
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
        _notificationCts?.Cancel();
        _notificationCts?.Dispose();
        _notificationCts = new CancellationTokenSource();
        _notificationToken = _notificationCts.Token;

        await TgDesktopUtils.InvokeOnUIThreadAsync(async () =>
        {
            await App.MainWindow.ShowMessageDialogAsync(
                $"{TgLocaleHelper.Instance.AppVersion}: v{TgDataUtils.GetTrimVersion(Assembly.GetExecutingAssembly().GetName().Version)}",
                TgConstants.OpenTgResearcherDesktop);
            App.MainWindow.BringToFront();
        }, _notificationToken);
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

	#endregion
}
