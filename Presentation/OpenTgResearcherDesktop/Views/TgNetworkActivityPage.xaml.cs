namespace OpenTgResearcherDesktop.Views;

/// <summary> Отображение сетевой активности с обновлением только пока страница открыта </summary>
public sealed partial class TgNetworkActivityPage
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    public override TgNetworkActivityViewModel ViewModel { get; }

    /// <summary> Создаёт страницу с общим состоянием операций </summary>
    public TgNetworkActivityPage()
    {
        ViewModel = App.GetService<TgNetworkActivityViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        _timer.Tick += OnTick;
    }

    // Обновляем страницу сразу и подписываемся на локализацию
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Localizer.Get().LanguageChanged += OnLanguageChanged;
        ViewModel.PropertyChanged += OnViewModelChanged;
        ViewModel.Refresh();
        _timer.Start();
    }

    // Скрытая страница не опрашивает сервис и не удерживает обработчик локализации
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
        Localizer.Get().LanguageChanged -= OnLanguageChanged;
        ViewModel.PropertyChanged -= OnViewModelChanged;
    }

    // Считываем безопасный снимок операций из потока интерфейса
    private void OnTick(object? sender, object e) => ViewModel.Refresh();

    // Скрываем названия сразу при переключении конфиденциальных данных
    private void OnViewModelChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.IsDisplaySensitiveData)) ViewModel.Refresh();
    }

    // Переводим строки даже без новых сетевых событий
    private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e) => DispatcherQueue.TryEnqueue(ViewModel.Refresh);
}
