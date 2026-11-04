namespace OpenTgResearcherDesktop.Views;

public sealed partial class ShellPage
{
    #region Fields, properties, constructor

    public ShellViewModel ViewModel { get; }
    private bool _isHandlingToggle = false;

    public ShellPage(ShellViewModel viewModel)
    {
        ViewModel = viewModel;
        
        InitializeComponent();
        DataContext = ViewModel;

        ViewModel.NavigationService.Frame = NavigationFrame;
        ViewModel.NavigationViewService.Initialize(NavigationViewControl);

        // Отдельная область перетаскивания сохраняет доступность кнопок верхней панели
        // https://docs.microsoft.com/windows/apps/develop/title-bar?tabs=winui3#full-customization
        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(TitleBarDragRegion);
    }

    #endregion

    #region Methods

    // Подключаем команды и локализацию после появления корня окна
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (KeyboardAccelerators.Count == 0)
        {
            KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu));
            KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.GoBack));
        }
        Localizer.Get().LanguageChanged += OnToolbarLanguageChanged;
        ToolbarUpdateButton.CommandParameter = XamlRoot;
        LocalizeToolbar();
        UpdateTitleBarInsets();
    }

    // Освобождаем подписку при временной замене оболочки заставкой
    private void OnUnloaded(object sender, RoutedEventArgs e) => Localizer.Get().LanguageChanged -= OnToolbarLanguageChanged;

    // Обновляем подписи средств доступности и подсказки одновременно с языком приложения
    private void OnToolbarLanguageChanged(object? sender, LanguageChangedEventArgs e) => TgDesktopUtils.InvokeOnUIThread(LocalizeToolbar);

    // Сохраняем понятные подписи у компактных кнопок без видимого текста
    private void LocalizeToolbar()
    {
        SetToolbarCaption(ToolbarLanguageButton, "ToolbarLanguage");
        SetToolbarCaption(ToolbarThemeButton, "ToolbarTheme");
        SetToolbarCaption(ToolbarUpdateButton, "ToolbarUpdate");
        SetToolbarCaption(ToolbarConnectButton, "ClientConnect");
        SetToolbarCaption(ToolbarDisconnectButton, "ClientDisconnect");
    }

    // Используем одну подпись для подсказки и имени кнопки
    private static void SetToolbarCaption(Button button, string resource)
    {
        var caption = resource.GetLocalizedResource();
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(button, caption);
        ToolTipService.SetToolTip(button, caption);
    }

    // Пересчитываем отступы при изменении режима навигации
    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args) => UpdateTitleBarInsets();

    // Учитываем размер окна и масштаб экрана
    private void AppTitleBar_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateTitleBarInsets();

    // Оставляем системным кнопкам окна отдельное место справа
    private void UpdateTitleBarInsets()
    {
        if (AppTitleBar is null || NavigationViewControl is null) return;
        var scale = XamlRoot?.RasterizationScale ?? 1;
        var right = App.MainWindow.AppWindow.TitleBar.RightInset / scale + 12;
        var left = NavigationViewControl.CompactPaneLength * (NavigationViewControl.DisplayMode == NavigationViewDisplayMode.Minimal ? 2 : 1);
        AppTitleBar.Margin = new Thickness(left, 0, right, 0);
    }

    private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
    {
        var keyboardAccelerator = new KeyboardAccelerator() { Key = key };

        if (modifiers.HasValue)
        {
            keyboardAccelerator.Modifiers = modifiers.Value;
        }
        keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;
        return keyboardAccelerator;
    }

    private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        var navigationService = App.GetService<INavigationService>();
        var result = navigationService.GoBack();
        args.Handled = result;
    }

    private async void ToggleSwitchShowSecretFields_Toggled(object sender, RoutedEventArgs e)
    {
        if (_isHandlingToggle)
            return;

        if (sender is not ToggleSwitch toggleSwitch)
            return;
        if (NavigationFrame.Content is not TgPageBase pageBase)
        {
            DefaultToggleSwitch(toggleSwitch);
            return;
        }
        if (pageBase.ViewModel is not TgPageViewModelBase viewModelBase)
        {
            DefaultToggleSwitch(toggleSwitch);
            return;
        }

        if (ViewModel.NavigationService.Frame is not null)
        {
            var viewModel = await ViewModel.NavigationService.Frame.GetPageViewModelAsync();
            if (viewModel is TgPageViewModelBase pageViewModel)
            {
                await pageViewModel.LoadStorageDataAsync(() =>
                {
                    ViewModel.LoadStateService.SetIsDisplaySensitiveData(toggleSwitch.IsOn);
                });
            }
        }
    }

    private void DefaultToggleSwitch(ToggleSwitch toggleSwitch)
    {
        try
        {
            _isHandlingToggle = true;
            toggleSwitch.IsOn = false;
        }
        finally
        {
            _isHandlingToggle = false;
        }
    }

    private void NavigationViewControl_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer is NavigationViewItem navItem)
        {
            // Get name ViewModel for navigation
            var navigateTo = TgNavigationHelper.GetNavigateTo(navItem);
            var navigationParameter = TgNavigationHelper.GetNavigationParameter(navItem);

            if (navigateTo == null)
                return;

            // Navigation with parameter transmission via NavigationService
            _ = ViewModel.NavigationService.NavigateTo(navigateTo, navigationParameter);
        }
    }

    #endregion
}
