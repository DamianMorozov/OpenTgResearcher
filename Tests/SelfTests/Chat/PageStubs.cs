namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgChatPage : TgPageBase
{
    public Frame ChatDetailsFrame { get; } = new();
    public SelectorBar ChatDetailsSelectorBar { get; } = new();
    public SelectorBar ContentViewSelector { get; } = new();
    // Заменяем генерацию элементов XAML тестовыми элементами
    private void InitializeComponent() => ContentViewSelector.SelectionChanged = () => ContentViewSelector_SelectionChanged(ContentViewSelector, null);
    /// <summary> Вызывает исходный обработчик выбора вкладки </summary>
    public void Select(SelectorBarItem? item)
    {
        ChatDetailsSelectorBar.SelectedItem = item!;
        ChatDetailsSelectorBar_SelectionChanged(ChatDetailsSelectorBar, new());
    }
}
public sealed class TgChatSettingsPage;
public sealed class TgChatDownloadPage;
public sealed class TgChatInfoPage;
public sealed class TgChatMyMessagesPage;
public sealed class TgChatParticipantsPage;
public sealed class TgChatStatisticsPage;
public sealed class TgChatContentPage;
