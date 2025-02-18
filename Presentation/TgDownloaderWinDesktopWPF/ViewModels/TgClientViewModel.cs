// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using TgInfrastructure.Enums;

namespace TgDownloaderWinDesktopWPF.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgClientViewModel : TgPageViewModelBase, INavigationAware
{
    #region Public and private fields, properties, constructor

    private TgEfAppRepository AppRepository { get; } = new();
    private TgEfProxyRepository ProxyRepository { get; } = new();
    public TgEfAppViewModel AppVm { get; }
    public TgEfProxyViewModel ProxyVm { get; set; }
    public ObservableCollection<TgEfProxyViewModel> ProxiesVms { get; private set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Notifications { get; set; }
    public string Password { get; set; }
    public string VerificationCode { get; set; }
    public string ServerMessage { get; set; }
    public Brush BackgroundVerificationCode { get; set; }
    public Brush BackgroundPassword { get; set; }
    public Brush BackgroundFirstName { get; set; }
    public Brush BackgroundLastName { get; set; }
    public Brush BackgroundServerMessage { get; set; }
    private bool _isNeedVerificationCode;
    public bool IsNeedVerificationCode
    {
        get => _isNeedVerificationCode;
        set
        {
            _isNeedVerificationCode = value;
            BackgroundVerificationCode = value ? new(Color.Yellow) : new SolidBrush(Color.Transparent);
        }
    }
    private bool _isNeedPassword;
    public bool IsNeedPassword
    {
        get => _isNeedPassword;
        set
        {
            _isNeedPassword = value;
            BackgroundPassword = value ? new(Color.Yellow) : new SolidBrush(Color.Transparent);
        }
    }
    private bool _isNeedFirstName;
    public bool IsNeedFirstName
    {
        get => _isNeedFirstName;
        set
        {
            _isNeedFirstName = value;
            BackgroundFirstName = value ? new(Color.Yellow) : new SolidBrush(Color.Transparent);
        }
    }
    private bool _isNeedLastName;
    public bool IsNeedLastName
    {
        get => _isNeedLastName;
        set
        {
            _isNeedLastName = value;
            BackgroundLastName = value ? new(Color.Yellow) : new SolidBrush(Color.Transparent);
        }
    }

    public TgClientViewModel()
    {
        AppVm = new(AppRepository.GetFirstItem(isReadOnly: false));
        ProxyVm = new(new());
        ProxiesVms = [];

        FirstName = string.Empty;
        LastName = string.Empty;
        Notifications = string.Empty;
        Password = string.Empty;
        VerificationCode = string.Empty;
        BackgroundVerificationCode = new SolidBrush(Color.Transparent);
        BackgroundPassword = new SolidBrush(Color.Transparent);
        BackgroundFirstName = new SolidBrush(Color.Transparent);
        BackgroundLastName = new SolidBrush(Color.Transparent);
        BackgroundServerMessage = new SolidBrush(Color.Transparent);
        StateConnectMsg = string.Empty;
        ServerMessage = string.Empty;
    }

    #endregion

    #region Public and private methods

    public void OnNavigatedTo()
    {
        var task = InitializeViewModelAsync();
		task.Wait();
	}

    public void OnNavigatedFrom() { }

    protected override async Task InitializeViewModelAsync()
    {
        await base.InitializeViewModelAsync();

        IsFileSession = TgAppSettings.AppXml.IsExistsFileSession;
        await LoadProxiesForClientAsync();
    }

    public async Task LoadProxiesForClientAsync()
    {
        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
            await Task.Delay(1);
            var app = AppVm.Dto.GetEntity();
            TgEfProxyEntity proxyNew = await ProxyRepository.GetItemAsync(new TgEfProxyEntity { Uid = app.ProxyUid ?? Guid.Empty });
            ProxiesVms = [];
            foreach (TgEfProxyEntity proxy in (await ProxyRepository.GetListAsync(TgEnumTableTopRecords.All, 0, isReadOnly: false)).Items)
            {
                ProxiesVms.Add(new(proxy));
            }
            if (!ProxiesVms.Select(p => p.Dto.UserName).Contains(proxyNew.UserName))
            {
                ProxyVm = new(proxyNew);
                ProxiesVms.Add(ProxyVm);
            }
        }, false);
    }

    public async Task AfterClientConnectAsync()
    {
        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
            //TgGlobalTools.ConnectClient.UpdateStateConnect(TgGlobalTools.ConnectClient.IsReady
            //    ? TgDesktopUtils.TgLocale.MenuClientIsConnected : TgDesktopUtils.TgLocale.MenuClientIsDisconnected);
            IsFileSession = TgAppSettings.AppXml.IsExistsFileSession;
            if (TgGlobalTools.ConnectClient.IsReady)
                ViewModelClearConfig();
            IsLoad = false;
            await Task.CompletedTask;
        }, false);
    }

    public string ConfigClientDesktop(string what)
    {
        var task = TgGlobalTools.ConnectClient.UpdateStateSourceAsync(0, 0, 0, $"{TgDesktopUtils.TgLocale.MenuClientIsQuery}: {what}");
		task.Wait();
		switch (what)
        {
            case "api_hash":
                string apiHash = TgDataFormatUtils.ParseGuidToString(AppVm.Dto.ApiHash);
                return apiHash;
            case "api_id":
                return AppVm.Dto.ApiId.ToString();
            case "phone_number":
                return AppVm.Dto.PhoneNumber;
            case "notifications":
                return Notifications;
            case "first_name":
                if (string.IsNullOrEmpty(FirstName))
                    IsNeedFirstName = true;
                return FirstName;
            case "last_name":
                if (string.IsNullOrEmpty(LastName))
                    IsNeedLastName = true;
                return LastName;
            case "session_pathname":
                string sessionPath = Path.Combine(Directory.GetCurrentDirectory(), TgAppSettings.AppXml.XmlFileSession);
                return sessionPath;
            case "verification_code":
                if (string.IsNullOrEmpty(VerificationCode))
                    IsNeedVerificationCode = true;
                return VerificationCode;
            case "password":
                if (string.IsNullOrEmpty(Password))
                    IsNeedPassword = true;
                return Password;
            //case "session_key":
            //case "server_address":
            //case "device_model":
            //case "system_version":
            //case "app_version":
            //case "system_lang_code":
            //case "lang_pack":
            //case "lang_code":
            default:
                return string.Empty;
        }
    }

    private void ViewModelClearConfig()
    {
        IsNeedVerificationCode = false;
        VerificationCode = string.Empty;
        IsNeedFirstName = false;
        FirstName = string.Empty;
        IsNeedLastName = false;
        LastName = string.Empty;
        IsNeedPassword = false;
        Password = string.Empty;
    }

    // ClientConnectCommand
    [RelayCommand]
    public async Task OnClientConnectAsync(TgEfProxyViewModel? proxyVm = null)
    {
        await OnAppSaveAsync();

        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
            await Task.Delay(1);
            if (!TgEfUtils.GetEfValid<TgEfAppEntity>(AppVm.Dto.GetEntity()).IsValid)
                return;
            await TgGlobalTools.ConnectClient.ConnectSessionAsync(proxyVm?.Dto.GetEntity() ?? ProxyVm.Dto.GetEntity());
        }, true);

        ServerMessage = TgDesktopUtils.TgClientVm.Exception.IsExist 
            ? TgDesktopUtils.TgClientVm.Exception.Message : string.Empty;
    }

    // ClientDisconnectCommand
    [RelayCommand]
    public async Task OnClientDisconnectAsync()
    {
        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
			await TgGlobalTools.ConnectClient.DisconnectAsync();
        }, false).ConfigureAwait(false);
    }

    // AppLoadCommand
    [RelayCommand]
    public async Task OnAppLoadAsync()
    {
	    await TgDesktopUtils.RunFuncAsync(this, async () =>
	    {
		    await Task.Delay(1);
		    var app = await AppRepository.GetFirstItemAsync(isReadOnly: false);
		    AppVm.Dto = new TgEfAppDto().GetDto(app);
	    }, false).ConfigureAwait(false);
    }

    // AppSaveCommand
	[RelayCommand]
    public async Task OnAppSaveAsync()
    {
        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
            await Task.Delay(1);
            await AppRepository.DeleteAllAsync();
            await AppVm.SaveAsync();
        }, false).ConfigureAwait(false);
    }

    // AppClearCommand
    [RelayCommand]
    public async Task OnAppClearAsync()
    {
        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
            await Task.Delay(1);
            var app = await AppRepository.GetNewItemAsync();
            AppVm.Fill(app);
        }, false).ConfigureAwait(true);
    }

    // AppEmptyCommand
    [RelayCommand]
    public async Task OnAppEmptyAsync()
    {
        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
            await Task.Delay(1);
            await AppRepository.DeleteAllAsync();
        }, false).ConfigureAwait(true);

        await OnAppLoadAsync();
    }
    
    #endregion
}