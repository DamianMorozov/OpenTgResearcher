// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgBotConnectionViewModel : TgPageViewModelBase
{
	#region Fields, properties, constructor

	private IAppNotificationService AppNotificationService { get; }
	[ObservableProperty]
	public partial TgEfAppEntity AppEntity { get; set; } = null!;
	[ObservableProperty]
	public partial TgEfProxyViewModel? ProxyVm { get; set; } = new(TgGlobalTools.Container);
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
	public partial bool UseBot { get; set; }
	[ObservableProperty]
	public partial string BotTokenKey { get; set; } = string.Empty;
    [ObservableProperty]
    public partial bool UseClient { get; set; }

    public IRelayCommand ClientConnectCommand { get; }
	public IRelayCommand ClientDisconnectCommand { get; }
	public IRelayCommand AppLoadCommand { get; }
	public IRelayCommand AppSaveCommand { get; }
	public IRelayCommand AppClearCommand { get; }
	public IRelayCommand AppDeleteCommand { get; }

	public TgBotConnectionViewModel(ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgBotConnectionViewModel> logger, IAppNotificationService appNotificationService) 
		: base(settingsService, navigationService, logger, nameof(TgBotConnectionViewModel))
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
        App.BusinessLogicManager.ConnectClient.SetupUpdateException(UpdateExceptionAsync);
        App.BusinessLogicManager.ConnectClient.SetupAfterClientConnect(AfterClientConnectAsync);
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadDataAsync(AppLoadCoreAsync);

	private async Task AfterClientConnectAsync()
	{
		ConnectionDt = TgDataFormatUtils.GetDtFormat(DateTime.Now);
		IsClientConnected = false;
		if (UseClient)
		{
			var client = App.BusinessLogicManager.ConnectClient.Client;
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
		await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync();
		IsOnlineReady = App.BusinessLogicManager.ConnectClient.IsReady;

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
			TgLogUtils.WriteException(ex);
		}
		return response;
	}

    private async Task ClientConnectAsync() => await ClientConnectCoreAsync(isRetry: false);

	private async Task ClientConnectCoreAsync(bool isRetry)
	{
		try
		{
			Exception.Default();
			DataRequest = string.Empty;
			await App.BusinessLogicManager.ConnectClient.ConnectSessionDesktopAsync(ProxyVm?.Dto ?? new(), ConfigClientDesktop);
        }
        catch (Exception ex)
		{
			Exception.Set(ex);
			TgLogUtils.WriteException(ex);
			if (isRetry)
				return;
			if (Exception.Message.Contains("or delete the file to start a new session"))
			{
				await TgDesktopUtils.DeleteFileAsync(SettingsService.AppSession);
				await ClientConnectCoreAsync(isRetry: true);
			}
		}
	}

	private async Task ClientDisconnectAsync() => await ContentDialogAsync(App.BusinessLogicManager.ConnectClient.DisconnectClientAsync, TgResourceExtensions.AskClientDisconnect());

	private async Task AppLoadAsync() => await ContentDialogAsync(AppLoadCoreAsync, TgResourceExtensions.AskSettingsLoad());

	private async Task AppLoadCoreAsync()
	{
		var appResult = await App.BusinessLogicManager.StorageManager.AppRepository.GetCurrentAppAsync(isReadOnly: false);
		AppEntity = appResult.IsExists && appResult.Item is not null ? appResult.Item : new();

		await ReloadUiAsync();
	}

    public override async Task ReloadUiAsync()
    {
        await base.ReloadUiAsync();

        ApiHash = TgDataFormatUtils.ParseGuidToString(AppEntity.ApiHash);
        ApiId = AppEntity.ApiId;
        PhoneNumber = AppEntity.PhoneNumber;
        FirstName = AppEntity.FirstName;
        LastName = AppEntity.LastName;
        UseBot = AppEntity.UseBot;
        BotTokenKey = AppEntity.BotTokenKey;
        UseClient = AppEntity.UseClient;

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
        var storageResult = await App.BusinessLogicManager.StorageManager
            .ProxyRepository.GetListAsync(TgEnumTableTopRecords.All, 0, isReadOnly: false);
        if (storageResult.IsExists)
        {
            foreach (TgEfProxyEntity proxy in storageResult.Items)
            {
                ProxiesVms.Add(new(TgGlobalTools.Container, proxy));
            }
        }
        // Insert empty proxy if not exists
        TgEfProxyViewModel? emptyProxyVm = null;
        var proxiesVmsEmpty = ProxiesVms.Where(x => x.IsEmptyProxy);
        if (!proxiesVmsEmpty.Any())
        {
            emptyProxyVm = new(TgGlobalTools.Container, new());
            ProxiesVms.Add(emptyProxyVm);
        }
        // Select proxy
        var proxiesUids = ProxiesVms.Select(x => x.Dto.Uid).ToList();
        if (AppEntity.ProxyUid is { } proxyUid && proxiesUids.Contains(proxyUid))
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
		await App.BusinessLogicManager.StorageManager.AppRepository.DeleteAllAsync();

		AppEntity.ApiHash = TgDataFormatUtils.ParseStringToGuid(ApiHash);
		AppEntity.ApiId = ApiId;
		AppEntity.FirstName = FirstName;
		AppEntity.LastName = LastName;
		AppEntity.PhoneNumber = PhoneNumber;
		AppEntity.ProxyUid = ProxyVm?.Dto.Uid;
		AppEntity.UseBot = UseBot;
		AppEntity.BotTokenKey = BotTokenKey;
        AppEntity.UseClient = UseClient;
        if (AppEntity.ProxyUid is null || AppEntity.ProxyUid == Guid.Empty)
			AppEntity.Proxy = null;

		await App.BusinessLogicManager.StorageManager.AppRepository.SaveAsync(AppEntity);
	}

	private async Task AppClearAsync() => await ContentDialogAsync(AppClearCoreAsync, TgResourceExtensions.AskSettingsClear());

	private async Task AppClearCoreAsync()
	{
		AppEntity = new();
		ProxiesVms.Clear();
		ProxyVm?.Dto = new();

		await ReloadUiAsync();
		Password = string.Empty;
		VerificationCode = string.Empty;
	}

	private async Task AppDeleteAsync() => await ContentDialogAsync(AppDeleteCoreAsync, TgResourceExtensions.AskSettingsDelete());

	private async Task AppDeleteCoreAsync()
	{
		await App.BusinessLogicManager.StorageManager.AppRepository.DeleteAllAsync();
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
		if (sender is not TextBox textBox) return;
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