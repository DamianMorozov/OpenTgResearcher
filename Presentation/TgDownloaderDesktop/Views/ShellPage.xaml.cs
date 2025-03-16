// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.Views;

public sealed partial class ShellPage
{
	#region Public and private fields, properties, constructor

	public ShellViewModel ViewModel { get; }

	public ShellPage(ShellViewModel viewModel)
	{
		ViewModel = viewModel;
		InitializeComponent();

		ViewModel.NavigationService.Frame = NavigationFrame;
		ViewModel.NavigationViewService.Initialize(NavigationViewControl);

		// TODO: Set the title bar icon by updating /Assets/applicationIcon.ico.
		// A custom title bar is required for full window theme and Mica support.
		// https://docs.microsoft.com/windows/apps/develop/title-bar?tabs=winui3#full-customization
		App.MainWindow.ExtendsContentIntoTitleBar = true;
		App.MainWindow.SetTitleBar(AppTitleBar);
		//App.MainWindow.Activated += MainWindow_Activated;
		//AppTitleBarText.Text = "AppDisplayName".GetLocalized();
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

	#endregion
}
