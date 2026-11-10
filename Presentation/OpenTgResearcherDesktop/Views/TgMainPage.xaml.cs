namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgMainPage
{
	#region Fields, properties, constructor

	public override TgMainViewModel ViewModel { get; }

	public TgMainPage()
	{
		ViewModel = App.GetService<TgMainViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += OnHomeLoaded;
        Unloaded += OnHomeUnloaded;
	}

	#endregion

    // Подключаем обновление состояния только для видимой главной страницы
    private void OnHomeLoaded(object sender, RoutedEventArgs e)
    {
        PageLoaded(sender, e);
        ViewModel.Activate();
    }

    // Освобождаем подписки при переходе в другой раздел
    private void OnHomeUnloaded(object sender, RoutedEventArgs e) => ViewModel.Deactivate();
}
