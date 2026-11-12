namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgClientConnectionPage
{
	#region Fields, properties, constructor

	public override TgClientConnectionViewModel ViewModel { get; }

	public TgClientConnectionPage()
	{
		ViewModel = App.GetService<TgClientConnectionViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += OnConnectionLoaded;
        Unloaded += OnConnectionUnloaded;
	}

	#endregion

	#region Methods

    // Подключаем локализованные подписи только для открытой страницы
    private void OnConnectionLoaded(object sender, RoutedEventArgs e) { PageLoaded(sender, e); ViewModel.Activate(); }

    // Переход в другой раздел не отключает текущую сессию
    private void OnConnectionUnloaded(object sender, RoutedEventArgs e) => ViewModel.Deactivate();

	private void OnApiHashTextChanged(object sender, TextChangedEventArgs e) => ViewModel.OnApiHashTextChanged(sender, e);

	private void OnApiIdTextChanged(object sender, TextChangedEventArgs e) => ViewModel.OnApiIdTextChanged(sender, e);

	private void OnPhoneTextChanged(object sender, TextChangedEventArgs e) => ViewModel.OnPhoneTextChanged(sender, e);

    #endregion
}
