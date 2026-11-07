namespace TgBusinessLogic.Helpers;

/// <summary> Преобразование Telegram в независимый от сетевой библиотеки формат хранения </summary>
public static class TgMessageContentFactory
{
    /// <summary> Сохраняет исходный текст, диапазоны UTF-16 и путь к файлу отдельно от подписи </summary>
    public static string Serialize(TL.MessageBase messageBase, string fileName = "")
    {
        var message = messageBase as TL.Message;
        return new TgMessageContent
        {
            Text = message?.message ?? string.Empty,
            FileName = fileName,
            Entities = message?.entities?.Select(x => new TgMessageEntity(x.GetType().Name.Replace("MessageEntity", string.Empty), x.Offset, x.Length,
                x switch
                {
                    TL.MessageEntityTextUrl url => url.url,
                    TL.MessageEntityPre pre => pre.language,
                    TL.MessageEntityMentionName mention => mention.user_id.ToString(),
                    TL.MessageEntityCustomEmoji emoji => emoji.document_id.ToString(),
                    _ => string.Empty
                })).ToArray() ?? []
        }.Serialize();
    }
}
