namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgSettingsPage
{
	#region Fields, properties, constructor

	public override TgSettingsViewModel ViewModel { get; }

	public TgSettingsPage()
	{
		ViewModel = App.GetService<TgSettingsViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += OnSettingsLoaded;
        Unloaded += OnSettingsUnloaded;
	}

	#endregion

    // Подключаем общий журнал обновлений к корню окна и восстанавливаем раскрытую секцию
    private void OnSettingsLoaded(object sender, RoutedEventArgs e)
    {
        PageLoaded(sender, e);
        ViewModel.Updates.XamlRootVm = XamlRoot;
        ViewModel.UpdatesRequested += OnUpdatesRequested;
        if (ViewModel.IsUpdatesExpanded) ShowUpdatesSection();
    }

    // Освобождаем обработчик перехода при уходе со страницы, сохраняя текущую загрузку
    private void OnSettingsUnloaded(object sender, RoutedEventArgs e) => ViewModel.UpdatesRequested -= OnUpdatesRequested;

    // Прокручиваем к обновлениям даже при повторном нажатии кнопки верхней панели
    private void OnUpdatesRequested(object? sender, EventArgs e) => ShowUpdatesSection();

    // После раскрытия секции учитываем её новый размер и показываем начало блока
    private void ShowUpdatesSection()
    {
        UpdatesSection.UpdateLayout();
        UpdatesSection.StartBringIntoView(new BringIntoViewOptions { AnimationDesired = true, VerticalAlignmentRatio = 0 });
    }

    // Синхронизируем палитру с введённым значением при открытии выбора цвета
    private void AccentFlyout_Opening(object sender, object e)
    {
        try { AccentPicker.Color = TgProgramIntegration.ParseAccent(ViewModel.Preferences.AccentHex); }
        catch (Exception ex) { ViewModel.LogError(ex); }
    }

    // Сохраняем выбранный цвет в черновике параметров до нажатия кнопки сохранения
    private void AccentPicker_ColorChanged(Microsoft.UI.Xaml.Controls.ColorPicker sender, ColorChangedEventArgs args) =>
        ViewModel.Preferences.AccentHex = $"#{args.NewColor.R:X2}{args.NewColor.G:X2}{args.NewColor.B:X2}";
}
