namespace OpenTgResearcherDesktop.Views;

public partial class TgStoragePage
{
    private bool _ready;
    public override TgStorageViewModel ViewModel { get; }

    public TgStoragePage()
    {
        ViewModel = App.GetService<TgStorageViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
        _ready = true;
        Loaded += OnPageLoaded;
        Unloaded += OnPageUnloaded;
    }

    // Каждое открытие страницы начинается с паспорта, выбранная таблица сохраняется в модели
    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        PageLoaded(sender, e);
        ViewModel.Activate();
        StorageSelectorBar.SelectedItem = StorageSelectorBar.Items[0];
        ShowSection("Overview");
    }

    // Скрытая страница не удерживает подписку локализатора
    private void OnPageUnloaded(object sender, RoutedEventArgs e) => ViewModel.Deactivate();

    // Меняем только внутренний раздел, не затрагивая меню приложения
    private void StorageSelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        if (_ready && sender.SelectedItem?.Tag is string section) ShowSection(section);
    }

    // Показываем выбранный раздел паспорта
    private void ShowSection(string section)
    {
        OverviewSection.Visibility = section == "Overview" ? Visibility.Visible : Visibility.Collapsed;
        DataSection.Visibility = section == "Data" ? Visibility.Visible : Visibility.Collapsed;
        BackupsSection.Visibility = section == "Backups" ? Visibility.Visible : Visibility.Collapsed;
        MaintenanceSection.Visibility = section == "Maintenance" ? Visibility.Visible : Visibility.Collapsed;
    }

    // Выбор таблицы сбрасывает поиск и начинает чтение с первой страницы
    private async void Tables_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_ready || ViewModel.IsBusy || ViewModel.SelectedTable is null) return;
        ViewModel.Search = string.Empty;
        await ViewModel.LoadRecordsAsync(reset: true);
    }

    // Поиск выполняется только по подтверждению, без запроса на каждый введённый символ
    private async void Search_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        => await ViewModel.SearchCommand.ExecuteAsync(null);
}
