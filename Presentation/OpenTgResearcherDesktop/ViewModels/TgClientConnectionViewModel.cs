namespace OpenTgResearcherDesktop.ViewModels;

public sealed partial class TgClientConnectionViewModel : TgPageViewModelBase
{
	#region Fields, properties, constructor

	[ObservableProperty]
	public partial TgEfAppDto AppDto { get; set; } = default!;
	[ObservableProperty]
	public partial TgEfProxyViewModel? ProxyVm { get; set; } = new(TgGlobalTools.Container);
	[ObservableProperty]
	public partial ObservableCollection<TgEfProxyViewModel> ProxiesVms { get; set; } = [];
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

    public IAsyncRelayCommand ClientConnectCommand { get; }
	public IAsyncRelayCommand ClientDisconnectCommand { get; }
	public IAsyncRelayCommand AppSaveCommand { get; }
	public IAsyncRelayCommand AppClearCommand { get; }
	public IAsyncRelayCommand AppDeleteCommand { get; }

	public TgClientConnectionViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgClientConnectionViewModel> logger) : base(loadStateService, settingsService, navigationService, logger, nameof(TgClientConnectionViewModel))
	{
        // Commands
		ClientConnectCommand = new AsyncRelayCommand<bool>(ClientConnectAsync);
		ClientDisconnectCommand = new AsyncRelayCommand<bool>(ClientDisconnectAsync);
		AppSaveCommand = new AsyncRelayCommand(AppSaveAsync);
		AppClearCommand = new AsyncRelayCommand(AppClearAsync);
		AppDeleteCommand = new AsyncRelayCommand(AppDeleteAsync);
        AppClearCommand.Execute(null);
        // Callback updates UI
        App.BusinessLogicManager.ConnectClient.SetupUpdateException(UpdateException);
        App.BusinessLogicManager.ConnectClient.SetupAfterClientConnect(AfterClientConnectAsync);
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e)
    {
        await LoadStorageDataAsync(LoadDataStorageCoreAsync);
    }

    private async Task AfterClientConnectAsync()
    {
        var uid = Guid.NewGuid();
        try
        {
            await LoadStateService.PrepareOnlineTokenAsync(uid);

            await TgDesktopUtils.InvokeOnUIThreadAsync(async () =>
            {
                ConnectionDt = TgDataFormatUtils.GetDtFormat(DateTime.Now);

                if (AppDto.UseClient)
                {
                    var client = App.BusinessLogicManager.ConnectClient.Client;

                    // Check exceptions
                    // https://www.infotelbot.com/2021/06/telegram-error-lists.html
                    if (Exception?.Message is string msg && !string.IsNullOrEmpty(msg))
                    {
                        if (msg.Contains("PHONE_CODE_INVALID", StringComparison.OrdinalIgnoreCase))
                        {
                            ConnectionMsg = TgResourceExtensions.GetRpcErrorPhoneCodeInvalid();
                        }
                        else if (msg.Contains("PASSWORD_HASH_INVALID", StringComparison.OrdinalIgnoreCase))
                        {
                            ConnectionMsg = TgResourceExtensions.GetRpcErrorPasswordHashInvalid();
                        }
                        else if (msg.Contains("FLOOD_WAIT", StringComparison.OrdinalIgnoreCase))
                        {
                            ConnectionMsg = TgResourceExtensions.GetRpcErrorFloodWait();
                        }
                        else if (msg.Contains("PHONE_PASSWORD_FLOOD", StringComparison.OrdinalIgnoreCase))
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
                                ConnectionMsg = TgResourceExtensions.GetClientIsConnected();
                            }
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
            }, LoadStateService.OnlineToken);
        }
        finally
        {
            LoadStateService.StopSoftOnlineProcessing(uid);
        }
    }

    private string? ConfigClientDesktop(string what)
	{
        var response = what switch
		{
			"api_hash" => AppDto.ApiHashString,
			"api_id" => AppDto.ApiId.ToString(),
			"phone_number" => AppDto.PhoneNumber,
			"first_name" => AppDto.FirstName,
			"last_name" => AppDto.LastName,
			"password" => Password,
			"verification_code" => VerificationCode,
			"session_pathname" => SettingsService.AppSession,
			_ => null,
		};
        
        TgDesktopUtils.InvokeOnUIThread(() => { 
		    try
		    {
			    DataRequest = string.IsNullOrEmpty(DataRequest)
				    ? $"{what}: {(string.IsNullOrEmpty(response) ? DataRequestEmptyResponse : response)}"
				    : DataRequest + Environment.NewLine +
					    $"{what}: {(string.IsNullOrEmpty(response) ? DataRequestEmptyResponse : response)}";
		    }
		    catch (Exception ex)
		    {
			    LogError(ex);
		    }
        });
		
        return response;
	}

    private async Task ClientConnectAsync(bool isQuestion) => await ClientConnectCoreAsync(isQuestion, isRetry: false);

	private async Task ClientConnectCoreAsync(bool isQuestion, bool isRetry)
	{
        var uid = Guid.NewGuid();
        try
		{
			Exception.Default();
			DataRequest = string.Empty;
            if (isQuestion == true)
                await ContentDialogAsync(ClientConnectWithGetMeAsync,
                    TgResourceExtensions.AskClientConnect(), TgEnumLoadDesktopType.Online);
            else
            {
                try
                {
                    await LoadStateService.PrepareOnlineTokenAsync(uid);

                    await TgDesktopUtils.InvokeOnUIThreadAsync(ClientConnectWithGetMeAsync, LoadStateService.OnlineToken);
                }
                finally
                {
                    LoadStateService.StopSoftOnlineProcessing(uid);
                }
            }
        }
        catch (Exception ex)
		{
            LogError(ex);
			Exception.Set(ex);
			if (isRetry)
				return;
			if (Exception.Message.Contains("or delete the file to start a new session"))
			{
				await ClientConnectCoreAsync(isQuestion, isRetry: true);
			}
		}
	}

    private async Task ClientConnectWithGetMeAsync()
    {
        await App.BusinessLogicManager.ConnectClient.ConnectSessionAsync(ConfigClientDesktop, ProxyVm?.Dto ?? new(), isDesktop: true);
        if (App.BusinessLogicManager.ConnectClient.Me is TL.User me)
        {
            AppDto.FirstName = me.first_name ?? string.Empty;
            AppDto.LastName = me.last_name ?? string.Empty;
        }

        await AppSaveCoreAsync();
    }

    private async Task ClientDisconnectAsync(bool isQuestion) => await ClientDisconnectCoreAsync(isQuestion, isRetry: false);

    private async Task ClientDisconnectCoreAsync(bool isQuestion, bool isRetry)
    {
        try
        {
            Exception.Default();
            DataRequest = string.Empty;
            if (isQuestion == true)
                await ContentDialogAsync(() => App.BusinessLogicManager.ConnectClient.DisconnectClientAsync(), 
                    TgResourceExtensions.AskClientDisconnect(), TgEnumLoadDesktopType.Online);
            else
            {
                var uid = Guid.NewGuid();
                try
                {
                    await LoadStateService.PrepareOnlineTokenAsync(uid);

                    await TgDesktopUtils.InvokeOnUIThreadAsync(() => App.BusinessLogicManager.ConnectClient.DisconnectClientAsync(), LoadStateService.OnlineToken);
                }
                finally
                {
                    LoadStateService.StopSoftOnlineProcessing(uid);
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex);
            Exception.Set(ex);
            if (isRetry)
                return;
            if (Exception.Message.Contains("or delete the file to start a new session"))
            {
                await ClientDisconnectCoreAsync(isQuestion, isRetry: true);
            }
        }
    }
    
    private async Task LoadDataStorageCoreAsync()
	{
        if (!SettingsService.IsExistsAppStorage) return;
        AppDto = await App.BusinessLogicManager.StorageManager.AppRepository.GetCurrentDtoAsync(CancellationToken.None);
		//await ReloadUiAsync();
	}

	public override async Task ReloadUiAsync()
	{
		await base.ReloadUiAsync();

        UserName = string.Empty;
		MtProxyUrl = string.Empty;
		MaxAutoReconnects = string.Empty;
		FloodRetryThreshold = string.Empty;
		PingInterval = string.Empty;
		MaxCodePwdAttempts = string.Empty;

		DataRequest = string.Empty;
		DataRequestEmptyResponse = TgResourceExtensions.GetClientDataRequestEmptyResponse();

		await ReloadProxyAsync();
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
		if (AppDto.ProxyUid is { } proxyUid && proxiesUids.Contains(proxyUid))
		{
			ProxyVm = ProxiesVms.FirstOrDefault(x => x.Dto.Uid == proxyUid);
		}
		// Select empty proxy
		else
		{
			ProxyVm = emptyProxyVm;
		}
	}

	private async Task AppSaveAsync() => 
        await ContentDialogAsync(AppSaveCoreAsync, TgResourceExtensions.AskSettingsSave(), TgEnumLoadDesktopType.Storage);

	private async Task AppSaveCoreAsync()
	{
        try
        {
            AppDto.ProxyUid = ProxyVm?.Dto.Uid ?? Guid.Empty;
            await App.BusinessLogicManager.StorageManager.AppRepository.SaveAsync(AppDto);
            await LoadDataStorageCoreAsync();
        }
        catch (Exception ex)
        {
            Exception = new(ex);
        }
    }

	private async Task AppClearAsync() => 
        await ContentDialogAsync(AppClearCoreAsync, TgResourceExtensions.AskSettingsClear(), TgEnumLoadDesktopType.Storage);

	private async Task AppClearCoreAsync()
	{
        AppDto = new();
		ProxiesVms.Clear();
		ProxyVm?.Dto = new();

		await ReloadUiAsync();
		Password = string.Empty;
		VerificationCode = string.Empty;
	}

	private async Task AppDeleteAsync() => 
        await ContentDialogAsync(AppDeleteCoreAsync, TgResourceExtensions.AskSettingsDelete(), TgEnumLoadDesktopType.Storage);

	private async Task AppDeleteCoreAsync()
	{
		await App.BusinessLogicManager.StorageManager.AppRepository.DeleteAllAsync();
		await LoadDataStorageCoreAsync();
	}

	public void OnApiHashTextChanged(object sender, TextChangedEventArgs e)
	{
		if (sender is not TextBox textBox)
			return;
		if (TgDataFormatUtils.ParseStringToGuid(textBox.Text) == Guid.Empty)
		{
			return;
		}
	}

	public void OnApiIdTextChanged(object sender, TextChangedEventArgs e)
	{
		if (sender is not TextBox textBox)
			return;
		if (!int.TryParse(textBox.Text, out int id) || id <= 0)
		{
			return;
		}
	}

	public void OnPhoneTextChanged(object sender, TextChangedEventArgs e)
	{
		if (sender is not TextBox textBox)
			return;
		if (string.IsNullOrEmpty(textBox.Text))
		{
			return;
		}
	}

	#endregion
}
