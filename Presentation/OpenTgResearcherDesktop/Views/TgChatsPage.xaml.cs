namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgChatsPage
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
	#region Fields, properties, constructor

	public override TgChatsViewModel ViewModel { get; }

	public TgChatsPage()
	{
		ViewModel = App.GetService<TgChatsViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += OnPageLoaded;
        Unloaded += OnPageUnloaded;
        SizeChanged += OnPageSizeChanged;
        _timer.Tick += OnTick;
	}

    #endregion

    #region Methods

    // Подключаем обновление состояний только на открытой странице
    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        PageLoaded(sender, e);
        UpdateColumns();
        RegistryFilters.SelectedItem = RegistryFilters.Items[ViewModel.RegistryFilter];
        ViewModel.RefreshActivity();
        Localizer.Get().LanguageChanged += OnLanguageChanged;
        _timer.Start();
    }

    // Освобождаем таймер и подписку при переходе на другую страницу
    private void OnPageUnloaded(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
        Localizer.Get().LanguageChanged -= OnLanguageChanged;
    }

    // Отображаем состояние парсера без чтения хранилища каждую секунду
    private void OnTick(object? sender, object e) => ViewModel.RefreshActivity();

    // Учитываем доступную ширину страницы, включая развёрнутое боковое меню
    private void OnPageSizeChanged(object sender, SizeChangedEventArgs e) => UpdateColumns();

    // Колонка не входит в визуальное дерево, поэтому меняем её видимость напрямую
    private void UpdateColumns() => UpdatedColumn.Visibility = ActualWidth >= 900 ? Visibility.Visible : Visibility.Collapsed;

    // Обновляем перевод заголовков и динамических подписей
    private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e) => DispatcherQueue.TryEnqueue(ViewModel.RefreshActivity);

    // Выполняем локальный поиск по Enter
    private void FieldFilter_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter && ViewModel.RefreshRegistryCommand.CanExecute(null))
        {
            ViewModel.RefreshRegistryCommand.Execute(null);
            e.Handled = true;
        }
    }

    // Фильтруем сохранённые строки независимо от занятости Telegram
    private async void OnRegistryFilterChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        if (!IsLoaded || sender.SelectedItem?.Tag is not string value || !int.TryParse(value, out var filter)) return;
        if (ViewModel.RegistryFilter == filter) return;
        ViewModel.RegistryFilter = filter;
        try { await ViewModel.RefreshRegistryCommand.ExecuteAsync(null); }
        catch (Exception ex) { ViewModel.LogError(ex); }
    }

    // Пересчитываем порядок перед чтением первой порции
    private async void OnRegistrySortChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || sender is not ComboBox combo || combo.SelectedIndex < 0) return;
        ViewModel.RegistrySort = combo.SelectedIndex;
        try { await ViewModel.RefreshRegistryCommand.ExecuteAsync(null); }
        catch (Exception ex) { ViewModel.LogError(ex); }
    }

    #endregion
}
