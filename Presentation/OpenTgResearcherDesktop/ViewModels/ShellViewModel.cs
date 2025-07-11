﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

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
    public partial string StorageVersion { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string License { get; set; } = string.Empty;
    [ObservableProperty]
    public partial bool IsClientConnected { get; set; }
    [ObservableProperty]
    public partial bool IsShowSecretFields { get; set; }

    private NavigationEventArgs? _eventArgs;

    public INavigationService NavigationService { get; }
    public INavigationViewService NavigationViewService { get; }
    public IAppNotificationService AppNotificationService { get; }
    public ITgLicenseService LicenseService => App.BusinessLogicManager.LicenseService;

    public IRelayCommand ClientConnectCommand { get; }
    public IRelayCommand ClientDisconnectCommand { get; }
    public IRelayCommand UpdatePageCommand { get; }

    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService, IAppNotificationService appNotificationService)
    {
        AppNotificationService = appNotificationService;
        AppNotificationService.ClientConnectionChanged += OnClientConnectionChanged;
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;

        // Commands
        ClientConnectCommand = new AsyncRelayCommand(ShellClientConnectAsync);
        ClientDisconnectCommand = new AsyncRelayCommand(ShellClientDisconnectAsync);
        UpdatePageCommand = new AsyncRelayCommand(ShellUpdatePageAsync);
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
        if (selectedItem is not null)
        {
            Selected = selectedItem;
        }
        // App version + Storage version + License
        AppVersion = TgResourceExtensions.GetAppDisplayName() + $" v{TgDataUtils.GetTrimVersion(Assembly.GetExecutingAssembly().GetName().Version)}";
        StorageVersion = $"{TgResourceExtensions.GetStorage()} {TgAppSettingsHelper.Instance.StorageVersion}";
        if (App.BusinessLogicManager.LicenseService.CurrentLicense is not null)
        {
            License = App.BusinessLogicManager.LicenseService.CurrentLicense.Description;
            var foo = LicenseService.CurrentLicense.LicenseType;
            // Check paid license
            if (App.BusinessLogicManager.LicenseService.CurrentLicense.CheckPaidLicense())
            {
                //selectionPrompt.AddChoices(tgLocale.MenuMainBotConnection);
            }
        }
    }

    private void OnClientConnectionChanged(object? sender, bool isClientConnected)
    {
        IsClientConnected = isClientConnected;
    }

    private async Task ShellClientConnectAsync()
    {
        if (_eventArgs is null) return;
        await App.MainWindow.DispatcherQueue.TryEnqueueWithLogAsync(async () =>
        {
            // Trying to find an open page with ViewModel
            var frame = NavigationService.Frame;
            if (frame?.Content is TgClientConnectionPage page)
            {
                await page.ViewModel.OnNavigatedToAsync(_eventArgs);
                if (!page.ViewModel.IsClientConnected)
                {
                    page.ViewModel.ClientConnectCommand.Execute(null);
                }
                return;
            }

            // If not found - get through DI
            var vm = App.GetService<TgClientConnectionViewModel>();
            await vm.OnNavigatedToAsync(_eventArgs);
            if (!vm.IsClientConnected)
            {
                vm.ClientConnectCommand.Execute(null);
            }
        });
    }

    private async Task ShellClientDisconnectAsync()
    {
        if (_eventArgs is null) return;
        await App.MainWindow.DispatcherQueue.TryEnqueueWithLogAsync(async () =>
        {
            // Trying to find an open page with ViewModel
            var frame = NavigationService.Frame;
            if (frame?.Content is TgClientConnectionPage page)
            {
                await page.ViewModel.OnNavigatedToAsync(_eventArgs);
                if (page.ViewModel.IsClientConnected)
                {
                    page.ViewModel.ClientDisconnectCommand.Execute(null);
                }
                return;
            }

            // If not found - get through DI
            var vm = App.GetService<TgClientConnectionViewModel>();
            await vm.OnNavigatedToAsync(_eventArgs);
            if (vm.IsClientConnected)
            {
                await App.BusinessLogicManager.ConnectClient.DisconnectClientAsync();
            }
        });
    }

    private async Task ShellUpdatePageAsync()
    {
        if (_eventArgs?.Content is not TgPageBase pageBase) return;
        await pageBase.ViewModel.OnNavigatedToAsync(_eventArgs);
    }

    #endregion
}
