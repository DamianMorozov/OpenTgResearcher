namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgChatDownloadPage
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    #region Fields, properties, constructor

    public override TgChatDownloadViewModel ViewModel { get; }

    public TgChatDownloadPage()
    {
        ViewModel = App.GetService<TgChatDownloadViewModel>();

        InitializeComponent();
        DataContext = ViewModel;
        Loaded += OnPageLoaded;
        Unloaded += OnPageUnloaded;
        _timer.Tick += OnTick;
    }

    #endregion

    #region Methods

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        PageLoaded(sender, e);
        ViewModel.RefreshActivity();
        ViewModel.PropertyChanged += OnViewModelChanged;
        Localizer.Get().LanguageChanged += OnLanguageChanged;
        _timer.Start();
    }

    // Скрытая страница не опрашивает сеть и не удерживает обработчики
    private void OnPageUnloaded(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
        ViewModel.PropertyChanged -= OnViewModelChanged;
        Localizer.Get().LanguageChanged -= OnLanguageChanged;
    }

    // Читаем снимок в потоке интерфейса без запросов к Telegram
    private void OnTick(object? sender, object e) => ViewModel.RefreshActivity();

    // Скрываем имена файлов сразу после переключения конфиденциальных данных
    private void OnViewModelChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ViewModel.IsDisplaySensitiveData) or nameof(ViewModel.IsOnlineReady) or nameof(ViewModel.IsOnlineProcessing))
            ViewModel.RefreshActivity();
    }

    // Переводим динамические подписи без повторного открытия страницы
    private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e) => DispatcherQueue.TryEnqueue(ViewModel.RefreshActivity);

    // Загружаем расширенные сведения только при первом раскрытии и доступном подключении
    private async void OnDetailsExpanding(Microsoft.UI.Xaml.Controls.Expander sender, ExpanderExpandingEventArgs args)
    {
        if (ViewModel.HasOnlineDetails || !ViewModel.IsOnlineReady) return;
        try { await ViewModel.LoadDetailsCommand.ExecuteAsync(null); }
        catch (Exception ex) { ViewModel.LogError(ex); }
    }

    #endregion
}
