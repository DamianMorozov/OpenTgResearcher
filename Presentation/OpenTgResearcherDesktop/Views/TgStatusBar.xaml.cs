namespace OpenTgResearcherDesktop.Views;

/// <summary> Компактная строка состояния с подробностями по нажатию </summary>
public sealed partial class TgStatusBar : UserControl
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    public ShellViewModel ViewModel { get; }

    /// <summary> Использует общую модель оболочки и опрашивает операции только в видимом окне </summary>
    public TgStatusBar()
    {
        ViewModel = App.GetService<ShellViewModel>();
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
        _timer.Tick += OnTick;
    }

    // Подключаем мгновенное обновление статусов и переводов при появлении панели
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.PropertyChanged += OnStateChanged;
        Localizer.Get().LanguageChanged += OnLanguageChanged;
        Refresh();
        UpdateFlyoutSize();
        _timer.Start();
    }

    // Скрытая панель не удерживает подписки и не опрашивает операции
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
        ViewModel.PropertyChanged -= OnStateChanged;
        Localizer.Get().LanguageChanged -= OnLanguageChanged;
    }

    // Обновляем прогресс без дополнительных запросов к Telegram
    private void OnTick(object? sender, object e) => Refresh();

    // Немедленно скрываем конфиденциальные данные и отражаем смену состояния подключения
    private void OnStateChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ViewModel.IsDisplaySensitiveData) or nameof(ViewModel.IsOnlineReady) or nameof(ViewModel.IsStorageProcessing)
            or nameof(ViewModel.IsExistsAppStorage) or nameof(ViewModel.IsFloodVisible) or nameof(ViewModel.FloodWaitSeconds)) Refresh();
    }

    // Переводим динамические подписи одновременно с остальными элементами окна
    private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e) => DispatcherQueue.TryEnqueue(Refresh);

    // Синхронизируем подписи доступности, подсказки и состояние индикатора
    private void Refresh()
    {
        ViewModel.RefreshStatus();
        SetCaption(StatusTelegramButton, ViewModel.StatusTelegramDetail);
        SetCaption(StatusStorageButton, ViewModel.StatusStorageDetail);
        SetCaption(StatusActivityButton, $"{ViewModel.StatusActivityText} · {ViewModel.StatusActivityCount}");
        SetCaption(StatusFloodButton, ViewModel.StatusFloodDetail);
        StatusTelegramIcon.Fill = (Brush)Application.Current.Resources[ViewModel.IsOnlineReady
            ? "SystemFillColorSuccessBrush" : "TextFillColorSecondaryBrush"];
    }

    // Оставляем полное описание даже у кнопки без видимого текста
    private static void SetCaption(Control control, string caption)
    {
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(control, caption);
        ToolTipService.SetToolTip(control, caption);
    }

    // Сохраняем подробности в границах узкого окна
    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => UpdateFlyoutSize();

    // Ограничиваем ширину и высоту содержимого всплывающих панелей
    private void UpdateFlyoutSize()
    {
        var width = Math.Clamp((XamlRoot?.Size.Width ?? ActualWidth) - 80, 120, 400);
        foreach (var panel in new[] { StatusTelegramDetails, StatusStorageDetails, StatusActivityDetails, StatusFloodDetails }) panel.Width = width;
    }

    // Закрываем подробности при переходе на полноценную страницу
    private void StatusNavigate_Click(object sender, RoutedEventArgs e)
    {
        foreach (var button in new[] { StatusTelegramButton, StatusStorageButton, StatusActivityButton, StatusFloodButton }) button.Flyout?.Hide();
    }
}
