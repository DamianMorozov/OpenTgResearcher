namespace OpenTgResearcherDesktop.Views;

public sealed partial class ShellPage
{
    #region Fields, properties, constructor

    public ShellViewModel ViewModel { get; }
    private TgNetworkAnimationHelper? _networkAnimation;

    public ShellPage(ShellViewModel viewModel)
    {
        ViewModel = viewModel;
        
        InitializeComponent();
        DataContext = ViewModel;

        ViewModel.NavigationService.Frame = NavigationFrame;
        ViewModel.NavigationViewService.Initialize(NavigationViewControl);
        // Стандартный шаблон WinUI при первом измерении может раскрыть левую панель
        NavigationViewControl.IsPaneOpen = false;

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
        _networkAnimation ??= new(ToolbarScanBand, ViewModel.LoadStateService, action => DispatcherQueue.TryEnqueue(() => action()),
            () => new UISettings().AnimationsEnabled);
        _networkAnimation.Start();
        ViewModel.LoadStateService.PropertyChanged += OnSensitiveDataChanged;
        ActualThemeChanged += OnShellThemeChanged;
        UpdateNavigationIcons();
        UpdateSensitiveDataButton();
        NavigationViewControl.IsPaneOpen = false;
    }

    // Освобождаем подписку при временной замене оболочки заставкой
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        Localizer.Get().LanguageChanged -= OnToolbarLanguageChanged;
        _networkAnimation?.Stop();
        ViewModel.LoadStateService.PropertyChanged -= OnSensitiveDataChanged;
        ActualThemeChanged -= OnShellThemeChanged;
    }

    // Обновляем подписи средств доступности и подсказки одновременно с языком приложения
    private void OnToolbarLanguageChanged(object? sender, LanguageChangedEventArgs e) => TgDesktopUtils.InvokeOnUIThread(LocalizeToolbar);

    // Сохраняем понятные подписи у компактных элементов без видимого текста
    private void LocalizeToolbar()
    {
        SetToolbarCaption(ToolbarLanguageButton, "ToolbarLanguage");
        SetToolbarCaption(ToolbarThemeButton, "ToolbarTheme");
        UpdateSensitiveDataButton();
        SetToolbarCaption(ToolbarUpdateButton, "ToolbarUpdate");
        SetToolbarCaption(ToolbarConnectButton, "ClientConnect");
        SetToolbarCaption(ToolbarDisconnectButton, "ClientDisconnect");
    }

    // Используем одну подпись для подсказки и доступного имени элемента
    private static void SetToolbarCaption(Control control, string resource)
    {
        var caption = resource.GetLocalizedResource();
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(control, caption);
        ToolTipService.SetToolTip(control, caption);
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

    // Меняем общее состояние без обращения к текущей странице, хранилищу и Telegram
    private void SensitiveDataButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Microsoft.UI.Xaml.Controls.Primitives.ToggleButton button)
            ViewModel.LoadStateService.SetIsDisplaySensitiveData(button.IsChecked == true);
    }

    // Отражаем внешнее изменение общего состояния на кнопке и её подсказке
    private void OnSensitiveDataChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.IsDisplaySensitiveData)) DispatcherQueue.TryEnqueue(UpdateSensitiveDataButton);
    }

    // Глаз показывает текущее состояние, подпись доступности описывает следующее действие
    private void UpdateSensitiveDataButton()
    {
        var shown = ViewModel.IsDisplaySensitiveData;
        SensitiveDataIcon.Kind = shown ? "eye" : "eye-off";
        SetToolbarCaption(ToggleSwitchShowSensitiveData, shown ? "ToolbarSensitiveDataHide" : "ToolbarSensitiveDataShow");
    }

    // Иконки стандартного меню обновляем вместе с темой приложения
    private void OnShellThemeChanged(FrameworkElement sender, object args) => UpdateNavigationIcons();

    // Сохраняем стандартную навигацию WinUI и меняем только контурные изображения
    private void UpdateNavigationIcons()
    {
        var theme = ActualTheme == ElementTheme.Dark ? "dark" : "light";
        foreach (var item in NavigationViewControl.MenuItems.OfType<NavigationViewItem>())
        {
            if (item.Icon is ImageIcon icon && icon.Source is BitmapImage bitmap)
            {
                var name = Path.GetFileNameWithoutExtension(bitmap.UriSource.AbsolutePath);
                name = name[..name.LastIndexOf('-')];
                icon.Source = new BitmapImage(new Uri($"ms-appx:///Assets/Icons/Lucide/{name}-{theme}.png"));
            }
        }
        if (NavigationViewControl.SettingsItem is NavigationViewItem settings)
            settings.Icon = new ImageIcon { Source = new BitmapImage(new Uri($"ms-appx:///Assets/Icons/Lucide/settings-{theme}.png")) };
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
