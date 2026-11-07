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

    // Меню относится к строке под указателем, на пустом месте действия недоступны
    private void OnChatRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        var element = e.OriginalSource as DependencyObject;
        while (element is not null && element is not DataGridRow) element = VisualTreeHelper.GetParent(element);
        ViewModel.SelectedRow = (element as DataGridRow)?.DataContext as OpenTgResearcherDesktop.Models.TgChatRegistryRow;
    }

    // Выполняем локальный поиск по Enter
    private async void FieldFilter_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Enter || sender is not TextBox field) return;
        e.Handled = true;
        // Привязка Text обновляет модель при потере фокуса, а Enter оставляет фокус в поле
        ViewModel.FilterText = field.Text;
        if (!ViewModel.RefreshRegistryCommand.CanExecute(null)) return;
        try { await ViewModel.RefreshRegistryCommand.ExecuteAsync(null); }
        catch (Exception ex) { ViewModel.LogError(ex); }
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
