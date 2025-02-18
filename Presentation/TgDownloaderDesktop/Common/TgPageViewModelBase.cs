// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.Common;

/// <summary> Base class for TgViewModel </summary>
[DebuggerDisplay("{ToDebugString()}")]
public partial class TgPageViewModelBase : ObservableRecipient
{
	#region Public and private fields, properties, constructor

	[ObservableProperty]
	public partial ITgSettingsService SettingsService { get; private set; }
	[ObservableProperty]
	public partial INavigationService NavigationService { get; private set; }
	[ObservableProperty]
	public partial TgLicenseManagerHelper LicenseManager { get; set; } = TgLicenseManagerHelper.Instance;
	[ObservableProperty]
	public partial TgExceptionViewModel Exception { get; set; } = new();
	[ObservableProperty]
	public partial string ConnectionDt { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string ConnectionMsg { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string StateProxyDt { get; set; }= string.Empty;
	[ObservableProperty]
	public partial string StateProxyMsg { get; set; }= string.Empty;
	[ObservableProperty]
	public partial string StateSourceDt { get; set; }= string.Empty;
	[ObservableProperty]
	public partial string StateSourceMsg { get; set; }= string.Empty;
	[ObservableProperty]
	public partial int StateSourceProgress { get; set; } = 0;
	[ObservableProperty]
	public partial string StateSourceProgressString { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string StateSourceDirectory { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string StateSourceDirectorySizeString { get; set; } = string.Empty;
	[ObservableProperty]
	public partial XamlRoot? XamlRootVm { get; set; }
	[ObservableProperty]
	public partial bool IsPageLoad { get; set; }
	[ObservableProperty]
	public partial bool IsOnlineReady { get; set; }
	[ObservableProperty]
	public partial bool IsEnabledContent { get; set; }
	[ObservableProperty]
	public partial bool IsDownloading { get; set; }
	//[ObservableProperty]
	//public partial FileSystemWatcher? DirectorySystemWatcher { get; set; }
	[ObservableProperty]
	public partial TgDownloadSettingsViewModel DownloadSettings { get; set; } = new();

	public TgPageViewModelBase(ITgSettingsService settingsService, INavigationService navigationService)
	{
		SettingsService = settingsService;
		NavigationService = navigationService;
		LicenseManager.ActivateLicense(string.Empty, TgResourceExtensions.GetLicenseFreeDescription(),
			TgResourceExtensions.GetLicensePaidDescription(), TgResourceExtensions.GetLicensePremiumDescription());
	}

	#endregion

	#region Public and private methods

	public virtual string ToDebugString() => TgObjectUtils.ToDebugString(this);

	public virtual void OnLoaded(object parameter)
	{
		if (parameter is XamlRoot xamlRoot)
			XamlRootVm = xamlRoot;
	}

	public virtual async Task OnNavigatedToAsync(NavigationEventArgs e) => await LoadDataAsync(async () => await Task.CompletedTask);

	protected virtual async Task ReloadUiAsync()
	{
		ConnectionDt = string.Empty;
		ConnectionMsg = string.Empty;
		Exception.Default();
		await TgGlobalTools.ConnectClient.CheckClientIsReadyAsync();
		IsOnlineReady = TgGlobalTools.ConnectClient.IsReady;
	}

	/// <summary> Open url </summary>
	public void OpenHyperlink(object sender, RoutedEventArgs e)
	{
		if (sender is not HyperlinkButton hyperlinkButton)
			return;
		if (hyperlinkButton.Tag is not string tag)
			return;
		var url = TgDesktopUtils.ExtractUrl(tag);
		Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
	}
	
	/// <summary> Update state client message </summary>
	public virtual void UpdateStateProxy(string message)
	{
		App.MainWindow.DispatcherQueue.TryEnqueue(() =>
		{
			StateProxyDt = TgDataFormatUtils.GetDtFormat(DateTime.Now);
			StateProxyMsg = message;
		});
	}

	/// <summary> Update exception message </summary>
	public virtual async Task UpdateExceptionAsync(Exception ex)
	{
		Exception = new(ex);
		await Task.CompletedTask;
	}

	/// <summary> Update state source message </summary>
	public async Task UpdateStateSource(long sourceId, int messageId, int count, string message)
	{
		App.MainWindow.DispatcherQueue.TryEnqueue(() =>
		{
			float progress = messageId == 0 || count  == 0 ? 0 : (float) messageId * 100 / count;
			StateSourceProgress = (int)progress;
			StateSourceProgressString = progress == 0 ? $"{0:00.00} %" : $"{progress:#00.00} %";
			StateSourceDt = TgDataFormatUtils.GetDtFormat(DateTime.Now);
			StateSourceMsg = $"{messageId} | {message}";

			//long size = await TgDesktopUtils.CalculateDirSizeAsync(StateSourceDirectory);
			//StateSourceDirectorySizeString = FormatSize(size);
		});
		await Task.CompletedTask;
	}

//	protected void DirectorySystemWatcher_OnChanged(object sender, FileSystemEventArgs e)
//	{
//		App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
//		{
//			try
//			{
//				long size = await TgDesktopUtils.CalculateDirSizeAsync(StateSourceDirectory);
//				StateSourceDirectorySizeString = FormatSize(size);
//				//Debug.WriteLine($"File: {e.FullPath} {e.ChangeType}");
//			}
//			catch (Exception ex)
//			{
//#if DEBUG
//				Debug.WriteLine(ex);
//#endif
//			}
//		});
//	}

	private string FormatSize(long size)
	{
		if (size < 1024)
		{
			return $"{size} B";
		}
		else if (size < 1024 * 1024)
		{
			return $"{size / 1024.0:###.###} KB";
		}
		else if (size < 1024 * 1024 * 1024)
		{
			return $"{size / (1024 * 1024.0):###.###} MB";
		}
		else
		{
			return $"{size / (1024 * 1024 * 1024.0):###.###} GB";
		}
	}

	protected async Task ContentDialogAsync(string title, ContentDialogButton defaultButton = ContentDialogButton.Close)
	{
		if (XamlRootVm is null) return;
		ContentDialog dialog = new()
		{
			XamlRoot = XamlRootVm,
			Title = title,
			PrimaryButtonText = TgResourceExtensions.GetYesButton(),
			CloseButtonText = TgResourceExtensions.GetCancelButton(),
			DefaultButton = defaultButton,
		};
		_ = await dialog.ShowAsync();
	}

	protected async Task ContentDialogAsync(string title, string content)
	{
		if (XamlRootVm is null) return;
		ContentDialog dialog = new()
		{
			XamlRoot = XamlRootVm,
			Title = title,
			Content = content,
			CloseButtonText = TgResourceExtensions.GetOkButton(),
			DefaultButton = ContentDialogButton.Close,
		};
		_ = await dialog.ShowAsync();
	}

	protected async Task ContentDialogAsync(Func<Task> task, string title, ContentDialogButton defaultButton = ContentDialogButton.Close, bool useLoadData = false)
	{
		if (XamlRootVm is null) return;
		ContentDialog dialog = new()
		{
			XamlRoot = XamlRootVm,
			Title = title,
			PrimaryButtonText = TgResourceExtensions.GetYesButton(),
			CloseButtonText = TgResourceExtensions.GetCancelButton(),
			DefaultButton = defaultButton,
			PrimaryButtonCommand = new AsyncRelayCommand(useLoadData ? async () => await LoadDataAsync(task) : task)
		};
		_ = await dialog.ShowAsync();
	}

	protected async Task ContentDialogAsync(Action action, string title, ContentDialogButton defaultButton = ContentDialogButton.Close)
	{
		if (XamlRootVm is null) return;
		ContentDialog dialog = new()
		{
			XamlRoot = XamlRootVm,
			Title = title,
			PrimaryButtonText = TgResourceExtensions.GetYesButton(),
			CloseButtonText = TgResourceExtensions.GetCancelButton(),
			DefaultButton = defaultButton,
			PrimaryButtonCommand = new RelayCommand(action)
		};
		_ = await dialog.ShowAsync();
	}

	protected async Task LoadDataAsync(Func<Task> task)
	{
		try
		{
			IsEnabledContent = false;
			IsPageLoad = true;
			await Task.Delay(100);
			await task();
		}
		finally
		{
			IsPageLoad = false;
			IsEnabledContent = true;
		}
	}

	/// <summary> Write text to clipboard </summary>
	public async void OnClipboardWriteClick(object sender, RoutedEventArgs e)
	{
		if (sender is Button button)
		{
			var address = button.Tag?.ToString();
			if (string.IsNullOrEmpty(address)) return;
			if (!string.IsNullOrEmpty(address))
			{
				var dataPackage = new DataPackage();
				dataPackage.SetText(address);
				Clipboard.SetContent(dataPackage);
				await ContentDialogAsync(TgResourceExtensions.GetClipboard(), address);
			}
		}
	}

	#endregion
}
