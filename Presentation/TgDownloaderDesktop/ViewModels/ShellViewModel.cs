// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using Microsoft.UI.Xaml.Controls.Primitives;
using System.Threading.Tasks;

namespace TgDownloaderDesktop.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
	#region Public and private fields, properties, constructor

	[ObservableProperty]
	public partial bool IsBackEnabled { get; set; }
	[ObservableProperty]
	public partial object? Selected { get; set; }
	[ObservableProperty]
	public partial string AppVersion { get; set; } = string.Empty;
	[ObservableProperty]
	public partial bool IsClientConnected { get; set; }
	private NavigationEventArgs? _eventArgs;

	public IAppNotificationService AppNotificationService { get; }
	public INavigationService NavigationService { get; }
	public INavigationViewService NavigationViewService { get; }
	public IRelayCommand ClientConnectCommand { get; }
	public IRelayCommand ClientDisconnectCommand { get; }

	public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService, IAppNotificationService appNotificationService)
	{
		AppNotificationService = appNotificationService;
		AppNotificationService.ClientConnectionChanged += OnClientConnectionChanged;
		NavigationService = navigationService;
		NavigationService.Navigated += OnNavigated;
		NavigationViewService = navigationViewService;
		ClientConnectCommand = new AsyncRelayCommand(ClientConnectAsync);
		ClientDisconnectCommand = new AsyncRelayCommand(ClientDisconnectAsync);
	}

	#endregion

	#region Public and private methods

	private void OnNavigated(object sender, NavigationEventArgs e)
	{
		_eventArgs = e;
		IsBackEnabled = NavigationService.CanGoBack;
		if (e.SourcePageType == typeof(TgSettingsPage))
		{
			Selected = NavigationViewService.SettingsItem;
			return;
		}
		var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
		if (selectedItem != null)
		{
			Selected = selectedItem;
		}
		AppVersion =
			TgResourceExtensions.GetAppDisplayName() + $" v{TgCommonUtils.GetTrimVersion(Assembly.GetExecutingAssembly().GetName().Version)}";
	}

	private void OnClientConnectionChanged(object? sender, bool isClientConnected)
	{
		IsClientConnected = isClientConnected;
	}

	public async Task ClientConnectAsync()
	{
		if (_eventArgs is null) return;
		await App.MainWindow.DispatcherQueue.TryEnqueueWithLogAsync(async () =>
		{
			var connectViewModel = App.GetService<TgConnectViewModel>();
			await connectViewModel.OnNavigatedToAsync(_eventArgs);
			if (!connectViewModel.IsClientConnected)
			{
				await connectViewModel.ClientConnectAsync();
			}
		});
	}

	public async Task ClientDisconnectAsync()
	{
		if (_eventArgs is null) return;
		await App.MainWindow.DispatcherQueue.TryEnqueueWithLogAsync(async () =>
		{
			var connectViewModel = App.GetService<TgConnectViewModel>();
			await connectViewModel.OnNavigatedToAsync(_eventArgs);
			if (connectViewModel.IsClientConnected)
			{
				await TgGlobalTools.ConnectClient.DisconnectAsync();
			}
		});
	}

	#endregion
}
