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
        //viewModel.PlayRefreshAnimationAsync += PlayRefreshAnimationAsync;
        ViewModel = viewModel;
        DataContext = ViewModel;
        InitializeComponent();

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
        var settingsService = App.GetService<ITgSettingsService>();
        settingsService.ApplyTheme(settingsService.AppTheme);
        var theme = TgThemeUtils.GetElementTheme(settingsService.AppTheme);
        TgTitleBarHelper.UpdateTitleBar(theme);
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
        viewModelBase.IsDisplaySensitiveField = toggleSwitch.IsOn;
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

    //private async Task PlayRefreshAnimationAsync()
    //{
    //    var player = FindChildOfType<AnimatedVisualPlayer>(NavigationViewControl);
    //    if (player is not null)
    //    {
    //        if (!player.IsLoaded)
    //        {
    //            TaskCompletionSource<bool> tcs = new();
    //            RoutedEventHandler loadedHandler = null!;
    //            loadedHandler = (s, e) =>
    //            {
    //                player.Loaded -= loadedHandler;
    //                tcs.SetResult(true);
    //            };
    //            player.Loaded += loadedHandler;
    //            await tcs.Task;
    //        }
    //        await App.MainWindow.DispatcherQueue.TryEnqueueWithLogAsync(async () =>
    //        {
    //            await player.PlayAsync(0, 480, looped: true);
    //            await Task.CompletedTask;
    //        });
    //    }
    //}

    //public static T? FindChildOfType<T>(DependencyObject depObj) where T : DependencyObject
    //{
    //    if (depObj == null) return null;
    //    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
    //    {
    //        var child = VisualTreeHelper.GetChild(depObj, i);
    //        if (child is T t) return t;
    //        var result = FindChildOfType<T>(child);
    //        if (result != null) return result;
    //    }
    //    return null;
    //}

    #endregion
}
