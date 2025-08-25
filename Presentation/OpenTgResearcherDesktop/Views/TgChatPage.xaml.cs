// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgChatPage
{
    #region Fields, properties, constructor

    public override TgChatViewModel ViewModel { get; }

    public TgChatPage()
    {
        ViewModel = App.GetService<TgChatViewModel>();

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
    }

    /// <summary> Change chat details page </summary>
    private void SelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        var selectedItem = sender.SelectedItem;
        switch (selectedItem.Tag)
        {
            case nameof(TgChatDetailsInfoPage):
                ContentFrame.Navigate(typeof(TgChatDetailsInfoPage), ViewModel.Uid);
                break;
            case nameof(TgChatDetailsParticipantsPage):
                ContentFrame.Navigate(typeof(TgChatDetailsParticipantsPage), ViewModel.Uid);
                break;
            case nameof(TgChatDetailsStatisticsPage):
                ContentFrame.Navigate(typeof(TgChatDetailsStatisticsPage), ViewModel.Uid);
                break;
            case nameof(TgChatDetailsContentPage):
                ContentFrame.Navigate(typeof(TgChatDetailsContentPage), ViewModel.Uid);
                break;
        }
    }

    #endregion
}
