namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgChatsPage
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private bool _isLoadingFromScroll;
    private bool _isRegistryLoadQueued;
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
        SearchScopes.SelectedItem = SearchScopes.Items[ViewModel.SearchScope];
        PublicKinds.SelectedItem = PublicKinds.Items[ViewModel.PublicSearchKind];
        ViewModel.RefreshActivity();
        Localizer.Get().LanguageChanged += OnLanguageChanged;
        ViewModel.PropertyChanged += OnSearchViewModelChanged;
        _timer.Start();
    }

    // Освобождаем таймер и подписку при переходе на другую страницу
    private void OnPageUnloaded(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
        ViewModel.CancelDirectorySizeUpdates();
        ViewModel.CancelPublicSearch();
        Localizer.Get().LanguageChanged -= OnLanguageChanged;
        ViewModel.PropertyChanged -= OnSearchViewModelChanged;
    }

    // Не меняем коллекцию внутри подготовки строк и пересчёта прокрутки DataGrid
    private void OnRegistryRowLoading(object sender, DataGridRowEventArgs e) => QueueRegistryLoad();

    // Несколько событий подготовки строк объединяются в одну проверку после текущей разметки
    private void QueueRegistryLoad()
    {
        if (_isRegistryLoadQueued || _isLoadingFromScroll || !IsLoaded) return;
        _isRegistryLoadQueued = true;
        if (!DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, async () =>
        {
            _isRegistryLoadQueued = false;
            await LoadRegistryAtEndAsync();
        })) _isRegistryLoadQueued = false;
    }

    // Конец определяется видимыми строками, а не точным совпадением значения и максимума полосы
    private bool IsRegistryEndVisible() => IsRegistryEndVisible(DataGridChats, ViewModel.RegistryRows.TakeLast(3).ToArray());

    // Ищем только отображённые контейнеры последних строк загруженной порции
    private bool IsRegistryEndVisible(DependencyObject parent, TgChatRegistryRow[] tail)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = VisualTreeHelper.GetChild(parent, index);
            if (child is DataGridRow { Visibility: Visibility.Visible } row && row.DataContext is TgChatRegistryRow item && tail.Contains(item))
            {
                var top = row.TransformToVisual(DataGridChats).TransformPoint(default).Y;
                if (row.ActualHeight > 0 && top < DataGridChats.ActualHeight && top + row.ActualHeight > 0) return true;
            }
            if (IsRegistryEndVisible(child, tail)) return true;
        }
        return false;
    }

    // Повторяем запрос после занятой загрузки, если пользователь уже достиг конца новой порции
    private async Task LoadRegistryAtEndAsync()
    {
        if (_isLoadingFromScroll || !IsLoaded || ViewModel.IsTelegramSearch || !ViewModel.IsDisplaySensitiveData || !ViewModel.HasMoreItems
            || ViewModel.RegistryRows.Count == 0) return;
        _isLoadingFromScroll = true;
        try
        {
            if (!IsRegistryEndVisible()) return;
            if (ViewModel.RefreshRegistryCommand.ExecutionTask is { IsCompleted: false } refresh) await refresh;
            if (IsLoaded && IsRegistryEndVisible()) await ViewModel.LoadNextRegistryPageAsync();
        }
        catch (Exception ex) { ViewModel.LogError(ex); }
        finally { _isLoadingFromScroll = false; }
    }

    // Проверяем конец списка повторно: событие могло прийти во время запроса предыдущей порции
    private void OnTick(object? sender, object e)
    {
        ViewModel.RefreshActivity();
        QueueRegistryLoad();
    }

    // Учитываем доступную ширину страницы, включая развёрнутое боковое меню
    private void OnPageSizeChanged(object sender, SizeChangedEventArgs e) => UpdateColumns();

    // Колонка не входит в визуальное дерево, поэтому меняем её видимость напрямую
    private void UpdateColumns()
    {
        UpdatedColumn.Visibility = ActualWidth >= 900 ? Visibility.Visible : Visibility.Collapsed;
        var isWide = ActualWidth >= 850;
        PublicDetailsColumn.Width = isWide && ViewModel.IsPublicProfileOpen ? new GridLength(Math.Min(420, ActualWidth * 0.43)) : new GridLength(0);
        Grid.SetColumn(PublicSearchPreview, isWide ? 1 : 0);
        PublicSearchResultsList.Visibility = !isWide && ViewModel.IsPublicProfileOpen ? Visibility.Collapsed : Visibility.Visible;
        PublicResultsLayout.ColumnSpacing = isWide && ViewModel.IsPublicProfileOpen ? 12 : 0;
    }

    // Раскрываем правую карточку также при повторном нажатии на выбранный результат
    private void OnPublicResultClicked(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is TgPublicChatSearchRow row) ViewModel.OpenPublicProfile(row);
    }

    // Синхронизируем ширину панели и закрываем прежний блок медиа при смене источника
    private void OnSearchViewModelChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.SelectedPublicChat)) PublicMediaExpander.IsExpanded = false;
        if (e.PropertyName is nameof(ViewModel.IsPublicProfileOpen) or nameof(ViewModel.SelectedPublicChat)) UpdateColumns();
    }

    // Запрашиваем счётчики только при раскрытии медиа выбранного источника
    private async void OnPublicMediaExpanding(Microsoft.UI.Xaml.Controls.Expander sender, ExpanderExpandingEventArgs args)
    {
        try { await ViewModel.LoadPublicMediaCommand.ExecuteAsync(null); }
        catch (Exception ex) { ViewModel.LogError(ex); }
    }

    // Обновляем перевод заголовков и динамических подписей
    private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e) => DispatcherQueue.TryEnqueue(ViewModel.RefreshActivity);

    // Меню относится к строке под указателем, на пустом месте действия недоступны
    private void OnChatRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        var element = e.OriginalSource as DependencyObject;
        while (element is not null && element is not DataGridRow) element = VisualTreeHelper.GetParent(element);
        ViewModel.SelectedRow = (element as DataGridRow)?.DataContext as OpenTgResearcherDesktop.Models.TgChatRegistryRow;
    }

    // Кнопка поиска и Enter получают ввод до потери фокуса поля
    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox field) ViewModel.FilterText = field.Text;
    }

    // Передаём актуальный ввод в поиск выбранной области по Enter
    private async void FieldFilter_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Enter || sender is not TextBox field) return;
        e.Handled = true;
        // Привязка Text обновляет модель при потере фокуса, а Enter оставляет фокус в поле
        ViewModel.FilterText = field.Text;
        if (!ViewModel.SearchChatsCommand.CanExecute(null)) return;
        try { await ViewModel.SearchChatsCommand.ExecuteAsync(null); }
        catch (Exception ex) { ViewModel.LogError(ex); }
    }

    // Сохраняем область поиска без сброса реестра или выполнения сетевого запроса
    private void OnSearchScopeChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        if (!IsLoaded || sender.SelectedItem?.Tag is not string value || !int.TryParse(value, out var scope)) return;
        ViewModel.FilterText = ChatSearchField.Text;
        ViewModel.SearchScope = scope;
    }

    // Отбираем каналы или группы из текущей выдачи Telegram
    private void OnPublicKindChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        if (!IsLoaded || sender.SelectedItem?.Tag is not string value || !int.TryParse(value, out var kind)) return;
        ViewModel.PublicSearchKind = kind;
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
