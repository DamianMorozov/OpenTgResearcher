// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Services;

public sealed class AppNotificationService : IAppNotificationService
{
	//private readonly INavigationService _navigationService;
	#region Public and private fields, properties, constructor

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

	public AppNotificationService(INavigationService navigationService)
	{
		//_navigationService = navigationService;
	}

	~AppNotificationService()
	{
		Unregister();
	}

	#endregion

	#region Public and private methods

	public void Initialize()
	{
		AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;
		AppNotificationManager.Default.Register();
	}

	public void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
	{
		// TODO: Handle notification invocations when your app is already running.
		//// // Navigate to a specific page based on the notification arguments.
		//// if (ParseArguments(args.Argument)["action"] == "Settings")
		//// {
		////    App.MainWindow.DispatcherQueue.TryEnqueue(() =>
		////    {
		////        _navigationService.NavigateTo(typeof(TgSettingsViewModel).FullName!);
		////    });
		//// }
		App.MainWindow.DispatcherQueue.TryEnqueueWithLog(() =>
		{
			App.MainWindow.ShowMessageDialogAsync(
				$"{TgLocaleHelper.Instance.AppVersion}: v{TgCommonUtils.GetTrimVersion(Assembly.GetExecutingAssembly().GetName().Version)}",
				TgConstants.AppTitleDesktop);
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
