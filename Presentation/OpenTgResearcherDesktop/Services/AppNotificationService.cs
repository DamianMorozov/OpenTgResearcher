// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Services;

public sealed class AppNotificationService : IAppNotificationService
{
	#region Fields, properties, constructor

	private bool _isClientConnected;
	public bool IsClientConnected
	{
		get => _isClientConnected;
		set
		{
			if (_isClientConnected != value)
			{
				_isClientConnected = value;
				ClientConnectionChanged.Invoke(this, value);
			}
		}
	}
	public event EventHandler<bool> ClientConnectionChanged = (_, _) => { };

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
        if (App.MainWindow?.DispatcherQueue is not null)
        {
            var tcs = new TaskCompletionSource();
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    App.MainWindow.ShowMessageDialogAsync(
                        $"{TgLocaleHelper.Instance.AppVersion}: v{TgDataUtils.GetTrimVersion(Assembly.GetExecutingAssembly().GetName().Version)}",
                        TgConstants.OpenTgResearcherDesktop);
                    App.MainWindow.BringToFront();
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
