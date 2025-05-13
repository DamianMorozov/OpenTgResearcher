// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgConnectViewModel : TgPageViewModelBase
{
	#region Public and private fields, properties, constructor

	private IAppNotificationService AppNotificationService { get; }
	private TgEfAppRepository AppRepository { get; } = new();
	private TgEfProxyRepository ProxyRepository { get; } = new();
	[ObservableProperty]
	public partial TgEfAppEntity App { get; set; } = null!;
	[ObservableProperty]
	public partial TgEfProxyViewModel? ProxyVm { get; set; } = new();
	[ObservableProperty]
	public partial ObservableCollection<TgEfProxyViewModel> ProxiesVms { get; set; } = [];
	[ObservableProperty]
	public partial string ApiHash { get; set; } = string.Empty;
	[ObservableProperty]
	public partial int ApiId { get; set; } = 0;
	[ObservableProperty]
	public partial string PhoneNumber { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string FirstName { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string LastName { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string Password { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string VerificationCode { get; set; } = string.Empty;
	[ObservableProperty]
	public partial bool IsNotReady { get; set; }
	[ObservableProperty]
	public partial string MtProxyUrl { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string UserName { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string MaxAutoReconnects { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string FloodRetryThreshold { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string PingInterval { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string MaxCodePwdAttempts { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string DataRequest { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string DataRequestEmptyResponse { get; set; } = string.Empty;
	[ObservableProperty]
	public partial bool IsFieldsCheck { get; set; }
	[ObservableProperty]
	public partial bool IsBot { get; set; }
	[ObservableProperty]
	public partial string BotTokenKey { get; set; } = string.Empty;

	public IRelayCommand ClientConnectCommand { get; }
	public IRelayCommand ClientDisconnectCommand { get; }
	public IRelayCommand AppLoadCommand { get; }
	public IRelayCommand AppSaveCommand { get; }
	public IRelayCommand AppClearCommand { get; }
	public IRelayCommand AppDeleteCommand { get; }


	public TgConnectViewModel(ITgSettingsService settingsService, INavigationService navigationService, IAppNotificationService appNotificationService,
		ITgLicenseService licenseService, ILogger<TgConnectViewModel> logger) 
		: base(settingsService, navigationService, licenseService, logger, nameof(TgConnectViewModel))
	{
		AppNotificationService = appNotificationService;
		IsClientConnected = AppNotificationService.IsClientConnected;

		var task = AppClearCoreAsync();
		task.Wait();
		// Commands
		ClientConnectCommand = new AsyncRelayCommand(ClientConnectAsync);
		ClientDisconnectCommand = new AsyncRelayCommand(ClientDisconnectAsync);
		AppLoadCommand = new AsyncRelayCommand(AppLoadAsync);
		AppSaveCommand = new AsyncRelayCommand(AppSaveAsync);
		AppClearCommand = new AsyncRelayCommand(AppClearAsync);
		AppDeleteCommand = new AsyncRelayCommand(AppDeleteAsync);
		// Delegates
		//TgGlobalTools.ConnectClient.SetupUpdateStateConnect(UpdateStateConnectAsync);
		//TgGlobalTools.ConnectClient.SetupUpdateStateProxy(UpdateStateProxyAsync);
		//TgGlobalTools.ConnectClient.SetupUpdateStateSource(UpdateStateSourceAsync);
		//TgGlobalTools.ConnectClient.SetupUpdateStateMessage(UpdateStateMessageAsync);
		TgGlobalTools.ConnectClient.SetupUpdateException(UpdateExceptionAsync);
		//TgGlobalTools.ConnectClient.SetupUpdateStateExceptionShort(UpdateStateExceptionShortAsync);
		TgGlobalTools.ConnectClient.SetupAfterClientConnect(AfterClientConnectAsync);
		//TgGlobalTools.ConnectClient.SetupGetClientDesktopConfig(ConfigClientDesktop);
	}

	#endregion

	#region Public and private methods

	public override async Task OnNavigatedToAsync(NavigationEventArgs e) => await LoadDataAsync(AppLoadCoreAsync);

	private async Task AfterClientConnectAsync()
	{
		ConnectionDt = TgDataFormatUtils.GetDtFormat(DateTime.Now);
		IsClientConnected = false;
		if (!IsBot)
		{
			var client = TgGlobalTools.ConnectClient.Client;
			// Check exceptions
			// https://www.infotelbot.com/2021/06/telegram-error-lists.html
			if (Exception.Message.Contains("PHONE_CODE_INVALID", StringComparison.InvariantCultureIgnoreCase))
			{
				ConnectionMsg = TgResourceExtensions.GetRpcErrorPhoneCodeInvalid();
			}
			else if (Exception.Message.Contains("PASSWORD_HASH_INVALID", StringComparison.InvariantCultureIgnoreCase))
			{
				ConnectionMsg = TgResourceExtensions.GetRpcErrorPasswordHashInvalid();
			}
			else if (Exception.Message.Contains("FLOOD_WAIT", StringComparison.InvariantCultureIgnoreCase))
			{
				ConnectionMsg = TgResourceExtensions.GetRpcErrorFloodWait();
			}
			else if (Exception.Message.Contains("PHONE_PASSWORD_FLOOD", StringComparison.InvariantCultureIgnoreCase))
			{
				ConnectionMsg = TgResourceExtensions.GetRpcErrorPhonePasswordFlood();
			}
			else
			{
				if (client is null || client.Disconnected)
				{
					ConnectionMsg = TgResourceExtensions.GetClientIsDisconnected();
				}
				else
				{
					IsClientConnected = true;
					ConnectionMsg = TgResourceExtensions.GetClientIsConnected();
				}
			}
			if (client is not null)
			{
				UserName = client.User?.MainUsername ?? string.Empty;
				MtProxyUrl = client.MTProxyUrl;
				MaxAutoReconnects = client.MaxAutoReconnects.ToString();
				FloodRetryThreshold = client.FloodRetryThreshold.ToString();
				PingInterval = client.PingInterval.ToString();
				MaxCodePwdAttempts = client.MaxCodePwdAttempts.ToString();
			}
			else
			{
				await ReloadUiAsync();
			}
		}
		
		// Update connection buttons
		await TgGlobalTools.ConnectClient.CheckClientIsReadyAsync();
		IsOnlineReady = TgGlobalTools.ConnectClient.IsReady;

		// Set app client state
		AppNotificationService.IsClientConnected = IsClientConnected;
	}

	private string? ConfigClientDesktop(string what)
	{
		var response = what switch
		{
			"api_hash" => ApiHash,
			"api_id" => ApiId.ToString(),
			"phone_number" => PhoneNumber,
			"first_name" => FirstName,
			"last_name" => LastName,
			"password" => Password,
			"verification_code" => VerificationCode,
			"session_pathname" => SettingsService.AppSession,
			_ => null,
		};
		try
		{
			DataRequest = string.IsNullOrEmpty(DataRequest)
				? $"{what}: {(string.IsNullOrEmpty(response) ? DataRequestEmptyResponse : response)}"
				: DataRequest + Environment.NewLine +
					$"{what}: {(string.IsNullOrEmpty(response) ? DataRequestEmptyResponse : response)}";
		}
		catch (Exception ex)
		{
			TgLogUtils.LogFatal(ex);
		}
		return response;
	}

	public async Task ClientConnectAsync() => await ClientConnectCoreAsync(isRetry: false);

	private async Task ClientConnectCoreAsync(bool isRetry)
	{
		try
		{
			Exception.Default();
			DataRequest = string.Empty;
			if (!IsBot)
				await TgGlobalTools.ConnectClient.ConnectSessionDesktopAsync(ProxyVm?.Dto.GetNewEntity(), ConfigClientDesktop);
			else
				await TgGlobalTools.ConnectClient.ConnectBotDesktopAsync(BotTokenKey, ApiId, ApiHash, ApplicationData.Current.LocalFolder.Path);
		}
		catch (Exception ex)
		{
			Exception.Set(ex);
			TgLogUtils.LogFatal(ex);
			if (isRetry)
				return;
			if (Exception.Message.Contains("or delete the file to start a new session"))
			{
				await TgDesktopUtils.DeleteFileStorageExistsAsync(SettingsService.AppSession);
				await ClientConnectCoreAsync(isRetry: true);
			}
		}
	}

	public async Task ClientDisconnectAsync() => await ContentDialogAsync(TgGlobalTools.ConnectClient.DisconnectAsync, TgResourceExtensions.AskClientDisconnect());

	private async Task AppLoadAsync() => await ContentDialogAsync(AppLoadCoreAsync, TgResourceExtensions.AskSettingsLoad());

	private async Task AppLoadCoreAsync()
	{
		var storageResult = await AppRepository.GetCurrentAppAsync(isReadOnly: false);
		App = storageResult.IsExists ? storageResult.Item : new();

		await ReloadUiAsync();
	}

	protected override async Task ReloadUiAsync()
	{
		await base.ReloadUiAsync();

		ApiHash = TgDataFormatUtils.ParseGuidToString(App.ApiHash);
		ApiId = App.ApiId;
		PhoneNumber = App.PhoneNumber;
		FirstName = App.FirstName;
		LastName = App.LastName;
		IsBot = App.UseBot;
		BotTokenKey = App.BotTokenKey;

		UserName = string.Empty;
		MtProxyUrl = string.Empty;
		MaxAutoReconnects = string.Empty;
		FloodRetryThreshold = string.Empty;
		PingInterval = string.Empty;
		MaxCodePwdAttempts = string.Empty;

		DataRequest = string.Empty;
		DataRequestEmptyResponse = TgResourceExtensions.GetClientDataRequestEmptyResponse();

		await ReloadProxyAsync();
		OnFieldsTextChangedCore();
	}

	private async Task ReloadProxyAsync()
	{
		ProxiesVms.Clear();
		var storageResult = await ProxyRepository.GetListAsync(TgEnumTableTopRecords.All, 0, isReadOnly: false);
		if (storageResult.IsExists)
		{
			foreach (TgEfProxyEntity proxy in storageResult.Items)
			{
				ProxiesVms.Add(new(proxy));
			}
		}
		// Insert empty proxy if not exists
		TgEfProxyViewModel? emptyProxyVm = null;
		var proxiesVmsEmpty = ProxiesVms.Where(x =>
			x.Dto.Type == TgEnumProxyType.None && (x.Dto.UserName == "No user" || x.Dto.Password == "No password"));
		if (!proxiesVmsEmpty.Any())
		{
			emptyProxyVm = new(new());
			ProxiesVms.Add(emptyProxyVm);
		}
		// Select proxy
		var proxiesUids = ProxiesVms.Select(x => x.Dto.Uid).ToList();
		if (App.ProxyUid is { } proxyUid && proxiesUids.Contains(proxyUid))
		{
			ProxyVm = ProxiesVms.FirstOrDefault(x => x.Dto.Uid == proxyUid);
		}
		// Select empty proxy
		else
		{
			ProxyVm = emptyProxyVm;
		}
	}

	private async Task AppSaveAsync() => await ContentDialogAsync(AppSaveCoreAsync, TgResourceExtensions.AskSettingsSave());

	private async Task AppSaveCoreAsync()
	{
		await AppRepository.DeleteAllAsync();

		App.ApiHash = TgDataFormatUtils.ParseStringToGuid(ApiHash);
		App.ApiId = ApiId;
		App.FirstName = FirstName;
		App.LastName = LastName;
		App.PhoneNumber = PhoneNumber;
		App.ProxyUid = ProxyVm?.Dto.Uid;
		App.UseBot = IsBot;
		App.BotTokenKey = BotTokenKey;
		if (App.ProxyUid is null || App.ProxyUid == Guid.Empty)
			App.Proxy = null;

		await AppRepository.SaveAsync(App);
	}

	private async Task AppClearAsync() => await ContentDialogAsync(AppClearCoreAsync, TgResourceExtensions.AskSettingsClear());

	private async Task AppClearCoreAsync()
	{
		App = new();
		ProxiesVms.Clear();
		if (ProxyVm is not null)
			ProxyVm.Dto = new();

		await ReloadUiAsync();
		Password = string.Empty;
		VerificationCode = string.Empty;
	}

	private async Task AppDeleteAsync() => await ContentDialogAsync(AppDeleteCoreAsync, TgResourceExtensions.AskSettingsDelete());

	private async Task AppDeleteCoreAsync()
	{
		await AppRepository.DeleteAllAsync();
		await AppLoadCoreAsync();
	}

	public void OnFieldsTextChangedCore()
	{
		if (TgDataFormatUtils.ParseStringToGuid(ApiHash) == Guid.Empty ||
			ApiId <= 0 || string.IsNullOrEmpty(PhoneNumber))
		{
			IsFieldsCheck = false;
			return;
		}
		IsFieldsCheck = true;
	}

	public void OnApiHashTextChanged(object sender, TextChangedEventArgs e)
	{
		if (sender is not TextBox textBox)
			return;
		if (TgDataFormatUtils.ParseStringToGuid(textBox.Text) == Guid.Empty)
		{
			IsFieldsCheck = false;
			return;
		}
		if (ApiId > 0 && !string.IsNullOrEmpty(PhoneNumber))
			IsFieldsCheck = true;
	}

	public void OnApiIdTextChanged(object sender, TextChangedEventArgs e)
	{
		if (sender is not TextBox textBox)
			return;
		if (!int.TryParse(textBox.Text, out int id) || id <= 0)
		{
			IsFieldsCheck = false;
			return;
		}
		if (TgDataFormatUtils.ParseStringToGuid(ApiHash) != Guid.Empty && !string.IsNullOrEmpty(PhoneNumber))
			IsFieldsCheck = true;
	}

	public void OnPhoneTextChanged(object sender, TextChangedEventArgs e)
	{
		if (sender is not TextBox textBox)
			return;
		if (string.IsNullOrEmpty(textBox.Text))
		{
			IsFieldsCheck = false;
			return;
		}
		if (TgDataFormatUtils.ParseStringToGuid(ApiHash) != Guid.Empty && ApiId > 0)
			IsFieldsCheck = true;
	}

	#endregion
}