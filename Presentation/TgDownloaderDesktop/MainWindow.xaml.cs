// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop;

public sealed partial class MainWindow : WindowEx
{
    private readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue;
    private readonly UISettings _settings;

    public MainWindow()
    {
        InitializeComponent();

        if (!string.IsNullOrEmpty(TgFileUtils.BaseDirectory))
            AppWindow.SetIcon(Path.Combine(TgFileUtils.BaseDirectory, "Assets/applicationIcon.ico"));
        Content = null;
        Title = "AppDisplayName".GetLocalized();

		LoadWindowState();
		Closed += MainWindow_Closed;

		// Theme change code picked from https://github.com/microsoft/WinUI-Gallery/pull/1239
		_dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        _settings = new();
        _settings.ColorValuesChanged += Settings_ColorValuesChanged; // cannot use FrameworkElement.ActualThemeChanged event
    }

	private void LoadWindowState()
	{
		var localSettings = ApplicationData.Current.LocalSettings;
		if (localSettings.Values.TryGetValue("WindowWidth", out var width) &&
			localSettings.Values.TryGetValue("WindowHeight", out var height) &&
			localSettings.Values.TryGetValue("WindowX", out var x) &&
			localSettings.Values.TryGetValue("WindowY", out var y))
		{
			AppWindow.Resize(new Windows.Graphics.SizeInt32((int)width, (int)height));
			AppWindow.Move(new Windows.Graphics.PointInt32((int)x, (int)y));
		}
        else
        {
            // Default
            AppWindow.Resize(new Windows.Graphics.SizeInt32(800, 600));
			AppWindow.Move(new Windows.Graphics.PointInt32(0, 0));
		}
	}

	private void SaveWindowState()
	{
		var localSettings = ApplicationData.Current.LocalSettings;
		localSettings.Values["WindowWidth"] = AppWindow.Size.Width;
		localSettings.Values["WindowHeight"] = AppWindow.Size.Height;
		localSettings.Values["WindowX"] = AppWindow.Position.X;
		localSettings.Values["WindowY"] = AppWindow.Position.Y;
	}

	private void MainWindow_Closed(object sender, WindowEventArgs args) => SaveWindowState();

	// this handles updating the caption button colors correctly when Windows system theme is changed while the app is open
	private void Settings_ColorValuesChanged(UISettings sender, object args)
    {
        // This calls comes off-thread, hence we will need to dispatch it to current app's thread
        _dispatcherQueue.TryEnqueueWithLog(TgTitleBarHelper.ApplySystemThemeToCaptionButtons);
    }
}
