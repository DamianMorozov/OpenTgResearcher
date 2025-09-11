// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Views;

public partial class TgStoragePage
{
	#region Fields, properties, constructor

	public override TgStorageViewModel ViewModel { get; }

	public TgStoragePage()
	{
		ViewModel = App.GetService<TgStorageViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += OnPageLoaded;
	}

    #endregion

    #region Methods

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        PageLoaded(sender, e);

        ViewModel.ContentFrame = ContentFrame;
        ContentFrame.Navigate(typeof(TgStorageTablesPage));
    }

    /// <summary> Storage details page </summary>
    private void SelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        var selectedItem = sender.SelectedItem;
        switch (selectedItem.Tag)
        {
            case nameof(TgStorageTablesPage):
                ContentFrame.Navigate(typeof(TgStorageTablesPage));
                break;
            case nameof(TgStorageConfigurationPage):
                ContentFrame.Navigate(typeof(TgStorageConfigurationPage));
                break;
            case nameof(TgStorageAdvancedPage):
                ContentFrame.Navigate(typeof(TgStorageAdvancedPage));
                break;
        }
    }

    #endregion
}
