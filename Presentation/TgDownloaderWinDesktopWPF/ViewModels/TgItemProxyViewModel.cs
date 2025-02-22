// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderWinDesktopWPF.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgItemProxyViewModel : TgPageViewModelBase, INavigationAware
{
    #region Public and private fields, properties, constructor

    private TgEfProxyRepository ProxyRepository { get; } = new();
    public TgEfProxyViewModel ItemProxyVm { get; private set; }
    public IReadOnlyList<TgEnumProxyType> ProxyTypes { get; }
    public TgPageViewModelBase? ViewModel { get; set; }
    private Guid ProxyUid { get; set; }

    public TgItemProxyViewModel()
	{
        ProxyTypes = GetProxyTypes();
        var proxy = new TgEfProxyEntity();
        ProxyRepository.Save(proxy);
		ItemProxyVm = new(proxy);
	}

	#endregion

	#region Public and private methods

	public IReadOnlyList<TgEnumProxyType> GetProxyTypes() =>
		Enum.GetValues(typeof(TgEnumProxyType)).Cast<TgEnumProxyType>().ToList();

    public void OnNavigatedTo()
    {
        var task = InitializeViewModelAsync();
		task.Wait();
	}

    public void OnNavigatedFrom() { }

    protected override async Task InitializeViewModelAsync()
    {
        await base.InitializeViewModelAsync();

        await OnGetProxyFromStorageAsync();
    }

    public void SetItemProxyVm(TgEfProxyViewModel itemProxyVm) =>
        SetItemProxyVm(itemProxyVm.Dto.GetNewEntity());

    public void SetItemProxyVm(TgEfProxyEntity proxy)
    {
        ItemProxyVm.Dto.Copy(proxy, isUidCopy: false);
        TgEfProxyViewModel itemBackup = ItemProxyVm;
		ItemProxyVm = new() { Dto = itemBackup.Dto };
	}

    // GetProxyFromStorageCommand
    [RelayCommand]
    public async Task OnGetProxyFromStorageAsync()
    {
        await TgDesktopUtils.RunFuncAsync(ViewModel ?? this, async () =>
        {
            await Task.Delay(1);
            if (ItemProxyVm.Dto.Uid != Guid.Empty)
                ProxyUid = ItemProxyVm.Dto.Uid;
            TgEfProxyEntity proxy = (await ProxyRepository.GetAsync(new TgEfProxyEntity { Uid = ProxyUid }, isReadOnly: false)).Item;
            SetItemProxyVm(proxy);
        }, true);
    }

    // ClearViewCommand
    [RelayCommand]
    public async Task OnClearViewAsync()
    {
        await TgDesktopUtils.RunFuncAsync(ViewModel ?? this, async () =>
        {
            await Task.Delay(1);
            if (ItemProxyVm.Dto.Uid != Guid.Empty)
                ProxyUid = ItemProxyVm.Dto.Uid;
            var proxy = await ProxyRepository.GetNewItemAsync();
            ItemProxyVm.Fill(proxy);
        }, false);
    }

    // SaveProxyCommand
    [RelayCommand]
    public async Task OnSaveProxyAsync()
    {
        await TgDesktopUtils.RunFuncAsync(ViewModel ?? this, async () =>
        {
            await Task.Delay(1);
            await ProxyRepository.SaveAsync(ItemProxyVm.Dto.GetNewEntity());
        }, false);
    }

    // ReturnToSectionProxiesCommand
    [RelayCommand]
    public async Task OnReturnToSectionProxiesAsync()
    {
        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
            await Task.Delay(1);
            if (Application.Current.MainWindow is MainWindow navigationWindow)
            {
                navigationWindow.ShowWindow();
                navigationWindow.Navigate(typeof(TgProxiesPage));
            }
        }, false);
    }

    #endregion
}