namespace OpenTgResearcherDesktop.Views;

public partial class TgProxiesPage
{
	#region Fields, properties, constructor

	public override TgProxiesViewModel ViewModel { get; }

	public TgProxiesPage()
	{
        ViewModel = App.GetService<TgProxiesViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
	}

	#endregion

    // Выбираем строку под указателем, чтобы меню применяло именно её
    private void OnProxyRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        var element = e.OriginalSource as DependencyObject;
        while (element is not null && element is not DataGridRow) element = VisualTreeHelper.GetParent(element);
        ViewModel.SelectedProxy = (element as DataGridRow)?.DataContext as TgEfProxyDto;
    }
}
