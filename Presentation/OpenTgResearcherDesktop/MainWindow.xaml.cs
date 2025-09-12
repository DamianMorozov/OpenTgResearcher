namespace OpenTgResearcherDesktop;

public sealed partial class MainWindow : WindowEx
{
    #region Fields, properties, constructor

    private readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue = default!;
    private readonly UISettings _settings = default!;
    private readonly ITgSettingsService _tgSettingsService = default!;

    public MainWindow()
    {
        InitializeComponent();

        try
        {
            if (!string.IsNullOrEmpty(TgFileUtils.BaseDirectory))
                AppWindow.SetIcon(Path.Combine(TgFileUtils.BaseDirectory, "Assets/applicationIcon.ico"));
            Content = null;
            Title = TgResourceExtensions.GetAppDisplayName();

            // Theme change code picked from https://github.com/microsoft/WinUI-Gallery/pull/1239
            _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            _settings = new();
            _settings.ColorValuesChanged += Settings_ColorValuesChanged; // cannot use FrameworkElement.ActualThemeChanged event

            Closed += MainWindow_Closed;
            _tgSettingsService = App.GetService<ITgSettingsService>();
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }

        LoadWindowState();
    }

    #endregion

    #region Methods

    /// <summary> Handle the Closed event </summary>
	private void MainWindow_Closed(object sender, WindowEventArgs args) => SaveWindowState();

    // this handles updating the caption button colors correctly when Windows system theme is changed while the app is open
    private void Settings_ColorValuesChanged(UISettings sender, object args)
    {
        // This calls comes off-thread, hence we will need to dispatch it to current app's thread
        _dispatcherQueue.TryEnqueue(TgTitleBarHelper.ApplySystemThemeToCaptionButtons);
    }

    private void LoadWindowState()
    {
        try
        {
            _tgSettingsService.LoadWindow();

            if (_tgSettingsService.WindowWidth == 0 && _tgSettingsService.WindowHeight == 0 && _tgSettingsService.WindowX == 0 && _tgSettingsService.WindowY == 0)
            {
                DefaultWindowState();
                return;
            }

            if (_tgSettingsService.WindowWidth <= 0 || _tgSettingsService.WindowHeight <= 0)
            {
                DefaultWindowState();
                return;
            }

            //AppWindow.Resize(new Windows.Graphics.SizeInt32(_tgSettingsService.WindowWidth, _tgSettingsService.WindowHeight));
            //AppWindow.Move(new Windows.Graphics.PointInt32(_tgSettingsService.WindowX, _tgSettingsService.WindowY));

            // Get window size and position from settings
            int windowX = _tgSettingsService.WindowX;
            int windowY = _tgSettingsService.WindowY;
            int windowWidth = _tgSettingsService.WindowWidth;
            int windowHeight = _tgSettingsService.WindowHeight;

            // Let's calculate the combined rectangle of all monitors
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            // Get the combined area of all monitors
            var allDisplayRects = Microsoft.UI.Windowing.DisplayArea.FindAll();
            for (int i = 0; i < allDisplayRects.Count; i++)
            {
                var display = allDisplayRects[i];
                var rect = display.WorkArea;
                if (rect.X < minX) minX = rect.X;
                if (rect.Y < minY) minY = rect.Y;
                if (rect.X + rect.Width > maxX) maxX = rect.X + rect.Width;
                if (rect.Y + rect.Height > maxY) maxY = rect.Y + rect.Height;
            }

            // Check and adjust the position and size of the window so that it does not extend beyond the borders of all monitors
            if (windowX < minX) windowX = minX;
            if (windowY < minY) windowY = minY;
            if (windowX + windowWidth > maxX) windowX = maxX - windowWidth;
            if (windowY + windowHeight > maxY) windowY = maxY - windowHeight;

            // In case the window size is larger than the whole area of monitors, set the minimum size
            if (windowWidth > maxX - minX) windowWidth = maxX - minX;
            if (windowHeight > maxY - minY) windowHeight = maxY - minY;

            // Apply window position and size
            AppWindow.Resize(new Windows.Graphics.SizeInt32(windowWidth, windowHeight));
            AppWindow.Move(new Windows.Graphics.PointInt32(windowX, windowY));
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            DefaultWindowState();
        }
    }

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    private void DefaultWindowState()
    {
        try
        {
            // WIN32
            int screenWidth = GetSystemMetrics(0);  // SM_CXSCREEN
            int screenHeight = GetSystemMetrics(1); // SM_CYSCREEN

            int width = screenWidth / 3 * 2;
            int height = screenHeight / 3 * 2;

            int posX = (screenWidth - width) / 3 * 2;
            int posY = (screenHeight - height) / 3 * 2;

            AppWindow.Resize(new Windows.Graphics.SizeInt32(width, height));
            AppWindow.Move(new Windows.Graphics.PointInt32(posX, posY));
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }
    }

    private void SaveWindowState()
    {
        try
        {
            _tgSettingsService.SaveWindow(AppWindow.Size.Width, AppWindow.Size.Height, AppWindow.Position.X, AppWindow.Position.X);
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }
    }

    #endregion
}
