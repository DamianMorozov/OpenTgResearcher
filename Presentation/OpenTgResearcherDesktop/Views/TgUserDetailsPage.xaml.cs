namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgUserDetailsPage
{
	#region Fields, properties, constructor

	public override TgUserDetailsViewModel ViewModel { get; }

	public TgUserDetailsPage()
	{
		ViewModel = App.GetService<TgUserDetailsViewModel>();

		InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
	}

    #endregion

    #region Methods

    /// <summary> Click on load messages </summary>
    public async void Button_LoadMessagesClick(object sender, RoutedEventArgs e) => await ViewModel.OnLoadMessagesClickAsync(sender, e);

    /// <summary> View preview click - open image </summary>
    private void OnImagePreviewClick(object sender, RoutedEventArgs e)
    {
        if (sender is Image img && img.Source is BitmapImage bitmap)
        {
            //FullScreenImage.Source = bitmap;
            ViewModel.IsImageViewerVisible = true;
            Bindings.Update();
        }
    }

    /// <summary> Exit from full screen image view </summary>
    private void OnFullScreenImageTapped(object sender, RoutedEventArgs e)
    {
        ViewModel.IsImageViewerVisible = false;
        //FullScreenImage.Source = null;
        Bindings.Update();
    }

    #endregion
}
