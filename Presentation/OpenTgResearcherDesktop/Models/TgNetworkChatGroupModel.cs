namespace OpenTgResearcherDesktop.Models;

/// <summary> Раскрывающаяся группа сетевых действий одного чата или общих операций </summary>
public sealed partial class TgNetworkChatGroupModel : ObservableObject
{
    public long? ChatId { get; }
    public string Icon => ChatId.HasValue ? "chats" : "network";
    public ObservableCollection<TgNetworkOperationModel> Operations { get; } = [];

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string Summary { get; set; } = string.Empty;
    [ObservableProperty]
    public partial bool IsExpanded { get; set; }

    /// <summary> Новые чаты свёрнуты, общие операции доступны сразу </summary>
    public TgNetworkChatGroupModel(long? chatId)
    {
        ChatId = chatId;
        IsExpanded = !chatId.HasValue;
    }
}
