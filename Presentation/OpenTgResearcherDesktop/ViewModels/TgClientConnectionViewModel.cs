namespace OpenTgResearcherDesktop.ViewModels;

public sealed partial class TgClientConnectionViewModel : TgPageViewModelBase
{
	#region Fields, properties, constructor

	[ObservableProperty]
	public partial TgEfAppDto? AppDto { get; set; } = new();
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

    private ShellViewModel? _shellVm;

    public IAsyncRelayCommand ClientConnectCommand { get; }
	public IAsyncRelayCommand ClientDisconnectCommand { get; }
	public IAsyncRelayCommand AppSaveCommand { get; }
	public IAsyncRelayCommand AppClearCommand { get; }
	public IAsyncRelayCommand AppDeleteCommand { get; }
    public IAsyncRelayCommand SessionResetCommand { get; }

	public TgClientConnectionViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgClientConnectionViewModel> logger) : base(loadStateService, settingsService, navigationService, logger, nameof(TgClientConnectionViewModel))
	{
        // Команды выполняются только по действию пользователя
		ClientConnectCommand = new AsyncRelayCommand<bool>(ClientConnectAsync);
		ClientDisconnectCommand = new AsyncRelayCommand<bool>(ClientDisconnectAsync);
		AppSaveCommand = new AsyncRelayCommand(AppSaveAsync);
		AppClearCommand = new AsyncRelayCommand(AppClearAsync);
		AppDeleteCommand = new AsyncRelayCommand(AppDeleteAsync);
        SessionResetCommand = new AsyncRelayCommand(SessionResetAsync);
        // Обратные вызовы обновляют интерфейс
        App.BusinessLogicManager.ConnectClient.SetupUpdateException(UpdateException);
        App.BusinessLogicManager.ConnectClient.SetupAfterClientConnect(AfterClientConnectAsync);
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e)
    {
        await LoadStorageDataAsync(LoadDataStorageCoreAsync);
    }

    /// <summary> Объясняет ошибки настройки API и чтения сессии на странице подключения </summary>
    public override void UpdateException(Exception ex)
    {
        base.UpdateException(ex);
        var message = Exception.Message;
        if (message.Contains("Exception while reading session file", StringComparison.OrdinalIgnoreCase))
            Exception.Message = TgResourceExtensions.GetClientSessionReadHelp(SettingsService.AppSession);
        else if (message.Contains("API_ID_INVALID", StringComparison.OrdinalIgnoreCase)
            || message.Contains("API ID Invalid", StringComparison.OrdinalIgnoreCase)
            || message.Contains("API_HASH_INVALID", StringComparison.OrdinalIgnoreCase))
            Exception.Message = TgResourceExtensions.GetClientApiSettingsHelp();
        else if (message.Contains("PHONE_CODE_INVALID", StringComparison.OrdinalIgnoreCase))
            Exception.Message = TgResourceExtensions.GetRpcErrorPhoneCodeInvalid();
        ConnectionMsg = Exception.Message;
    }

    // Обновляем состояние подключения, сохраняя сообщение об ошибке авторизации
    private async Task AfterClientConnectAsync()
    {
        var uid = Guid.NewGuid();
        try
        {
            await LoadStateService.PrepareOnlineTokenAsync(uid);
            var isReady = await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync();

            await TgDesktopUtils.InvokeOnUIThreadAsync(() =>
            {
                ConnectionDt = TgDataFormatUtils.GetDtFormat(DateTime.Now);
                ConnectionMsg = isReady ? TgResourceExtensions.GetClientIsConnected() : TgResourceExtensions.GetClientIsDisconnected();
                if (!isReady && Exception.IsExist) ConnectionMsg = Exception.Message;

                if (AppDto is not null && AppDto.UseClient)
                {
                    var client = App.BusinessLogicManager.ConnectClient.Client;

                    // Показываем понятное сообщение для известных ошибок RPC
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
                        ClearClientDetails();
                    }
                }

                return Task.CompletedTask;
            }, LoadStateService.OnlineToken);
        }
        catch (Exception ex)
        {
            LogError(ex);
            Exception.Set(ex);
        }
        finally
        {
            LoadStateService.StopSoftOnlineProcessing(uid);
        }
    }

    // Ожидаем закрытие вопроса и завершение подтверждённой операции в рамках исходной команды
    private async Task ConfirmActionAsync(Func<Task> action, string title, TgEnumLoadDesktopType loadType, CancellationToken ct = default)
    {
        if (XamlRootVm is null) return;
        try
        {
            ct.ThrowIfCancellationRequested();
            var dialog = new ContentDialog
            {
                XamlRoot = XamlRootVm,
                Title = title,
                PrimaryButtonText = TgResourceExtensions.GetYesButton(),
                CloseButtonText = TgResourceExtensions.GetCancelButton(),
                DefaultButton = ContentDialogButton.Close,
            };
            if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
            ct.ThrowIfCancellationRequested();
            if (loadType == TgEnumLoadDesktopType.Online)
                await LoadOnlineDataAsync(action);
            else
                await LoadStorageDataAsync(action);
        }
        catch (Exception ex)
        {
            LogError(ex);
            Exception.Set(ex);
        }
    }

    private string? ConfigClientDesktop(string what)
	{
        if (AppDto is null) return null;

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

    private async Task ClientConnectAsync(bool isQuestion) => await ClientConnectCoreAsync(isQuestion);

	private async Task ClientConnectCoreAsync(bool isQuestion)
	{
        var uid = Guid.NewGuid();
        try
		{
			Exception.Default();
			DataRequest = string.Empty;
            if (isQuestion == true)
                await ConfirmActionAsync(ClientConnectWithGetMeAsync,
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
            UpdateException(ex);
		}
	}

    // Сохраняем профиль и открываем Избранное только после успешной авторизации
    private async Task ClientConnectWithGetMeAsync()
    {
        if (AppDto is null || AppDto.ApiId <= 0 || AppDto.ApiHash == Guid.Empty)
        {
            UpdateException(new ArgumentException(TgResourceExtensions.GetClientApiSettingsHelp()));
            return;
        }

        try
        {
            await App.BusinessLogicManager.ConnectClient.ConnectSessionAsync(ConfigClientDesktop, ProxyVm?.Dto ?? new(), isDesktop: true);
            if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;
            if (AppDto is not null && App.BusinessLogicManager.ConnectClient.Me is TL.User me)
            {
                AppDto.FirstName = me.first_name ?? string.Empty;
                AppDto.LastName = me.last_name ?? string.Empty;
            }

            await AppSaveCoreAsync();
            _shellVm ??= App.VmLocator.Get<ShellViewModel>();
            _shellVm.UidSavedMessages = await App.BusinessLogicManager.ConnectClient.OpenOrCreateSavedMessagesAsync();
        }
        catch (Exception ex)
        {
            LogError(ex);
            UpdateException(ex);
        }
    }

    private async Task ClientDisconnectAsync(bool isQuestion) => await ClientDisconnectCoreAsync(isQuestion, isRetry: false);

    private async Task ClientDisconnectCoreAsync(bool isQuestion, bool isRetry)
    {
        try
        {
            Exception.Default();
            DataRequest = string.Empty;
            if (isQuestion == true)
                await ConfirmActionAsync(() => App.BusinessLogicManager.ConnectClient.DisconnectClientAsync(),
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
    
    // Сначала загружаем настройки, затем выбираем соответствующий им прокси
    private async Task LoadDataStorageCoreAsync()
	{
        if (!SettingsService.IsExistsAppStorage) return;

        AppDto = await App.BusinessLogicManager.StorageManager.AppRepository.GetCurrentDtoAsync(CancellationToken.None);
        await ReloadProxyAsync();
	}

	public override async Task ReloadUiAsync()
	{
		await base.ReloadUiAsync();

		ClearClientDetails();

		DataRequest = string.Empty;
		DataRequestEmptyResponse = TgResourceExtensions.GetClientDataRequestEmptyResponse();

		await ReloadProxyAsync();
	}

    // Очищаем сведения о клиенте без сброса ошибки подключения
    private void ClearClientDetails()
    {
        UserName = string.Empty;
        MtProxyUrl = string.Empty;
        MaxAutoReconnects = string.Empty;
        FloodRetryThreshold = string.Empty;
        PingInterval = string.Empty;
        MaxCodePwdAttempts = string.Empty;
    }

    // Выбираем сохранённый прокси или существующий пустой элемент списка
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
		// Создаём пустой прокси, только если его ещё нет в списке
		var emptyProxyVm = ProxiesVms.FirstOrDefault(x => x.IsEmptyProxy);
		if (emptyProxyVm is null)
		{
			emptyProxyVm = new(TgGlobalTools.Container, new());
			ProxiesVms.Add(emptyProxyVm);
		}
		// Выбираем сохранённый прокси
		var proxiesUids = ProxiesVms.Select(x => x.Dto.Uid).ToList();
		if (AppDto?.ProxyUid is { } proxyUid && proxiesUids.Contains(proxyUid))
		{
			ProxyVm = ProxiesVms.FirstOrDefault(x => x.Dto.Uid == proxyUid);
		}
		// Используем пустой прокси, если сохранённый отсутствует
		else
		{
			ProxyVm = emptyProxyVm;
		}
	}

	private async Task AppSaveAsync() => 
		await ConfirmActionAsync(AppSaveCoreAsync, TgResourceExtensions.AskSettingsSave(), TgEnumLoadDesktopType.Storage);

    // Сохраняем выбранный прокси и регистрируем ошибки записи настроек
	private async Task AppSaveCoreAsync()
	{
        if (AppDto is null) return;

        try
        {
            AppDto.ProxyUid = ProxyVm?.Dto.Uid ?? Guid.Empty;
            await App.BusinessLogicManager.StorageManager.AppRepository.SaveAsync(AppDto);
            await LoadDataStorageCoreAsync();
        }
        catch (Exception ex)
        {
            LogError(ex);
            Exception = new(ex);
        }
    }

	private async Task AppClearAsync() => 
		await ConfirmActionAsync(AppClearCoreAsync, TgResourceExtensions.AskSettingsClear(), TgEnumLoadDesktopType.Storage);

	private async Task AppClearCoreAsync()
	{
        AppDto = new();
		ProxiesVms.Clear();
		ProxyVm?.Dto = new();

		await ReloadUiAsync();
		Password = string.Empty;
		VerificationCode = string.Empty;
	}

    // Подтверждаем удаление профиля и выбранного файла локальной авторизации
    private async Task AppDeleteAsync() => await ConfirmActionAsync(AppDeleteCoreAsync,
        TgResourceExtensions.AskClientProfileDelete(SettingsService.AppSession), TgEnumLoadDesktopType.Online);

    // Сначала освобождаем и удаляем сессию, затем удаляем профиль из БД
    private async Task AppDeleteCoreAsync()
    {
        if (!await DeleteSessionFileAsync()) return;
        try
        {
            await App.BusinessLogicManager.StorageManager.AppRepository.DeleteAllAsync();
            ClearAuthorizationFields();
            await ReloadUiAsync();
            await LoadDataStorageCoreAsync();
        }
        catch (Exception ex)
        {
            LogError(ex);
            UpdateException(ex);
        }
    }

    // Сбрасываем только локальную сессию после отдельного подтверждения пользователя
    private async Task SessionResetAsync() => await ConfirmActionAsync(SessionResetCoreAsync,
        TgResourceExtensions.AskClientSessionReset(SettingsService.AppSession), TgEnumLoadDesktopType.Online);

    // Сохраняем профиль и прокси для нового входа с теми же API-параметрами
    private async Task SessionResetCoreAsync()
    {
        if (!await DeleteSessionFileAsync()) return;
        ClearAuthorizationFields();
        await base.ReloadUiAsync();
        ClearClientDetails();
        ConnectionMsg = TgResourceExtensions.GetClientSessionResetDone();
    }

    // Удаляем выбранный файл только после завершения клиента и обновляем сведения о его наличии
    private async Task<bool> DeleteSessionFileAsync(CancellationToken ct = default)
    {
        try
        {
            ct.ThrowIfCancellationRequested();
            await App.BusinessLogicManager.ConnectClient.DisconnectClientAsync();
            if (App.BusinessLogicManager.ConnectClient.Client is not null)
                throw new InvalidOperationException("Не удалось завершить Telegram-клиент. Файл сессии и профиль не удалены");
            ct.ThrowIfCancellationRequested();
            if (!string.IsNullOrWhiteSpace(SettingsService.AppSession)) File.Delete(SettingsService.AppSession);
            SettingsService.Load();
            return true;
        }
        catch (Exception ex)
        {
            LogError(ex);
            UpdateException(ex);
            return false;
        }
    }

    // Очищаем данные предыдущей попытки входа, сохраняя API-параметры
    private void ClearAuthorizationFields()
    {
        Password = string.Empty;
        VerificationCode = string.Empty;
        DataRequest = string.Empty;
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
