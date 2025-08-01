// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Views;

public sealed partial class ShellPage
{
    #region Public and private fields, properties, constructor

    public ShellViewModel ViewModel { get; }
    private bool _isHandlingToggle = false;

    public ShellPage(ShellViewModel viewModel)
    {
        ViewModel = viewModel;
        
        InitializeComponent();
        DataContext = ViewModel;

        ViewModel.NavigationService.Frame = NavigationFrame;
        ViewModel.NavigationViewService.Initialize(NavigationViewControl);

        // A custom title bar is required for full window theme and Mica support.
        // https://docs.microsoft.com/windows/apps/develop/title-bar?tabs=winui3#full-customization
        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(AppTitleBar);
    }

    #endregion

    #region Public and private methods

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu));
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.GoBack));
    }

    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        AppTitleBar.Margin = new Thickness()
        {
            Left = sender.CompactPaneLength * (sender.DisplayMode == NavigationViewDisplayMode.Minimal ? 2 : 1),
            Top = AppTitleBar.Margin.Top,
            Right = AppTitleBar.Margin.Right,
            Bottom = AppTitleBar.Margin.Bottom
        };
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

    private void ToggleSwitchShowSecretFields_Toggled(object sender, RoutedEventArgs e)
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
        ViewModel.NavigationService.IsDisplaySensitiveData = viewModelBase.IsDisplaySensitiveData = toggleSwitch.IsOn;
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
