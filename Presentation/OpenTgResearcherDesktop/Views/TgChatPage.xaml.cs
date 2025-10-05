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

        ViewModel.ChatDetailsFrame = ChatDetailsFrame;
        ChatDetailsFrame.Navigate(typeof(TgChatSettingsPage), ViewModel.Uid);
    }

    /// <summary> Chat details page </summary>
    private void ChatDetailsSelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        var selectedItem = sender.SelectedItem;
        switch (selectedItem.Tag)
        {
            case nameof(TgChatSettingsPage):
                ChatDetailsFrame.Navigate(typeof(TgChatSettingsPage), ViewModel.Uid);
                break;
            case nameof(TgChatDownloadPage):
                ChatDetailsFrame.Navigate(typeof(TgChatDownloadPage), ViewModel.Uid);
                break;
            case nameof(TgChatInfoPage):
                ChatDetailsFrame.Navigate(typeof(TgChatInfoPage), ViewModel.Uid);
                break;
            case nameof(TgChatMyMessagesPage):
                ChatDetailsFrame.Navigate(typeof(TgChatMyMessagesPage), ViewModel.Uid);
                break;
            case nameof(TgChatParticipantsPage):
                ChatDetailsFrame.Navigate(typeof(TgChatParticipantsPage), ViewModel.Uid);
                break;
            case nameof(TgChatStatisticsPage):
                ChatDetailsFrame.Navigate(typeof(TgChatStatisticsPage), ViewModel.Uid);
                break;
            case nameof(TgChatContentPage):
                ChatDetailsFrame.Navigate(typeof(TgChatContentPage), ViewModel.Uid);
                break;
        }

        ViewModel.LoadDataStorageCommand.Execute(null);
    }

    #endregion
}
