namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgHardwareResourcePage
{
	#region Fields, properties, constructor

	public override TgHardwareResourceViewModel ViewModel { get; }

	public TgHardwareResourcePage()
	{
		ViewModel = App.GetService<TgHardwareResourceViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
        Loaded += OnHardwareLoaded;
        Unloaded += OnHardwareUnloaded;
	}

	#endregion

    // Подключаем локализацию динамических подписей только пока страница видима
    private void OnHardwareLoaded(object sender, RoutedEventArgs e)
    {
        Localizer.Get().LanguageChanged += OnLanguageChanged;
        ViewModel.Dashboard.PropertyChanged += OnDashboardChanged;
        HardwareViewSelector.SelectedItem = ViewModel.Dashboard.IsHistory ? HistoryTab : OverviewTab;
        RefreshCaptions();
    }

    // Убираем обработчики при уходе со страницы
    private void OnHardwareUnloaded(object sender, RoutedEventArgs e)
    {
        Localizer.Get().LanguageChanged -= OnLanguageChanged;
        ViewModel.Dashboard.PropertyChanged -= OnDashboardChanged;
    }

    // Сохраняем измерения и меняем только представление
    private void HardwareViewSelector_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        if (ViewModel is not null) ViewModel.Dashboard.SelectView(isHistory: sender.SelectedItem == HistoryTab);
    }

    // Меняем иконку и доступную подпись вместе с состоянием паузы
    private void OnDashboardChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.Dashboard.IsPaused)) RefreshCaptions();
    }

    // Переводим динамические подписи без перезапуска страницы
    private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e) => DispatcherQueue.TryEnqueue(RefreshCaptions);

    // Текст кнопки и её контур обозначают следующее действие
    private void RefreshCaptions()
    {
        ViewModel.RefreshCaptions();
        PauseIcon.Kind = ViewModel.Dashboard.IsPaused ? "play" : "pause";
    }
}
