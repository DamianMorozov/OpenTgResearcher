namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgChatPage
{
    private bool _isSelectingContent;
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

    // Открываем первую вкладку после чтения настроек выбранного чата
    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        PageLoaded(sender, e);
        try { await LoadDetailsAsync(); }
        catch (Exception ex) { ViewModel.LogError(ex); }
    }

    // Не передаём первой вкладке пустой черновик во время загрузки родительской страницы
    private async Task LoadDetailsAsync()
    {
        await ViewModel.InitializationTask;
        ViewModel.ChatDetailsFrame = ChatDetailsFrame;
        var firstItem = ChatDetailsSelectorBar.Items[0];
        if (ChatDetailsSelectorBar.SelectedItem == firstItem)
            ChatDetailsSelectorBar_SelectionChanged(ChatDetailsSelectorBar, null);
        else
            ChatDetailsSelectorBar.SelectedItem = firstItem;
    }

    // Переключаем вкладку, сохраняя общий черновик настроек чата
    private void ChatDetailsSelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs? args)
    {
        var selectedItem = sender.SelectedItem;
        if (selectedItem is null) return;
        ViewModel.IsContentTab = Equals(selectedItem.Tag, nameof(TgChatContentPage));
        switch (selectedItem.Tag)
        {
            case nameof(TgChatSettingsPage):
                ChatDetailsFrame.Navigate(typeof(TgChatSettingsPage), ViewModel.Dto);
                break;
            case nameof(TgChatDownloadPage):
                ChatDetailsFrame.Navigate(typeof(TgChatDownloadPage), ViewModel.Dto);
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
                _isSelectingContent = true;
                ContentViewSelector.SelectedItem = ContentViewSelector.Items[0];
                _isSelectingContent = false;
                ChatDetailsFrame.Navigate(typeof(TgChatContentPage), ViewModel.Uid);
                break;
        }
    }

    // Сохраняем прежнюю страницу текущего пользователя внутри раздела содержимого
    private void ContentViewSelector_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs? args)
    {
        if (_isSelectingContent || !ViewModel.IsContentTab || sender.SelectedItem is not { } item) return;
        var type = Equals(item.Tag, nameof(TgChatMyMessagesPage)) ? typeof(TgChatMyMessagesPage) : typeof(TgChatContentPage);
        ChatDetailsFrame.Navigate(type, ViewModel.Uid);
    }

    #endregion
}
