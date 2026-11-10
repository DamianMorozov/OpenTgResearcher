namespace OpenTgResearcherDesktop.ChatSelfTests;

internal static partial class Program
{
    // История главной пополняется именно после загрузки настоящего чата из изолированной SQLite
    private static async Task OpenedChatIsRememberedAsync()
    {
        TgRecentChats.Remember(_settings, Guid.NewGuid(), 999, "Предыдущий чат");
        await Chat.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        var latest = TgRecentChats.Read(_settings).First();
        Assert(latest.Uid == _source.Uid && latest.Id == _source.Id && latest.Title == _source.Title,
            "Открытый чат не добавлен первым в историю главной страницы");
    }
}
