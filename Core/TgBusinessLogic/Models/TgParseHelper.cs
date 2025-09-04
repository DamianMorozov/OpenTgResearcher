// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Models;

public sealed class TgParseHelper
{
    public bool IsAccess { get; set; }
    public TL.Channel? Channel { get; set; }
    public TgEfAppDto? AppDto { get; set; }
    public WTelegram.Types.ChatFullInfo? BotChatFullInfo { get; set; }
    public TgChatCache ChatCache { get; set; } = new();
    public TgForumTopicSettings ForumTopicSettings { get; set; } = new();
    public List<Task> DownloadTasks { get; set; } = [];
    public int SourceFirstId { get; set; }
    public int SourceLastId { get; set; }
    public TgMessageSettings MessageSettings { get; set; } = new();
}
