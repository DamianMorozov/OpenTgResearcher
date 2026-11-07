namespace TgBusinessLogic.Services;

public abstract partial class TgConnectClientBase
{
    /// <summary> Читает описание, количество участников и публичные ссылки выбранного результата </summary>
    public async Task<TgPublicChatProfile> GetPublicChatProfileAsync(long chatId, CancellationToken ct = default)
    {
        try
        {
            var channel = GetPublicSearchChannel(chatId);
            var full = await ReadPublicChatProfileAsync(channel, ct);
            if (full.full_chat is not TL.ChannelFull info)
                throw new InvalidOperationException("Telegram не вернул сведения о выбранном источнике");
            var current = full.chats.GetValueOrDefault(chatId) as TL.Channel ?? channel;
            var names = new[] { current.MainUsername }.Concat(current.usernames?.Where(x => x.flags.HasFlag(TL.Username.Flags.active))
                .Select(x => x.username) ?? []).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            var discussion = full.chats.GetValueOrDefault(info.linked_chat_id)?.MainUsername;
            return new(info.about ?? string.Empty, info.flags.HasFlag(TL.ChannelFull.Flags.has_participants_count) ? info.participants_count : null,
                names, string.IsNullOrWhiteSpace(discussion) ? null : $"https://t.me/{discussion}");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { TgLogUtils.WriteException(ex); throw DescribeChatAccessException(ex); }
    }

    /// <summary> Читает фотографию в память без записи файлов рядом с программой или в каталог чата </summary>
    public async Task<byte[]> GetPublicChatPhotoAsync(long chatId, CancellationToken ct = default)
    {
        try
        {
            var channel = GetPublicSearchChannel(chatId);
            if (channel.photo is not TL.ChatPhoto) return [];
            await using var output = new MemoryStream();
            await DownloadPublicChatPhotoAsync(channel, output, ct);
            ct.ThrowIfCancellationRequested();
            return output.ToArray();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { TgLogUtils.WriteException(ex); throw; }
    }

    /// <summary> Возвращает счётчики Telegram, сохраняя отметку приблизительного количества </summary>
    public async Task<List<TgPublicChatMediaCount>> GetPublicChatMediaCountsAsync(long chatId, CancellationToken ct = default)
    {
        try
        {
            var counts = await ReadPublicChatMediaCountsAsync(GetPublicSearchChannel(chatId), ct);
            return [.. counts.Select(x => new TgPublicChatMediaCount(x.filter switch
            {
                TL.InputMessagesFilterPhotos => "Photos", TL.InputMessagesFilterVideo => "Videos", TL.InputMessagesFilterDocument => "Documents",
                TL.InputMessagesFilterUrl => "Links", TL.InputMessagesFilterMusic => "Music", TL.InputMessagesFilterVoice => "Voice",
                TL.InputMessagesFilterGif => "Gifs", TL.InputMessagesFilterPoll => "Polls", _ => string.Empty
            }, x.count, x.flags.HasFlag(TL.Messages_SearchCounter.Flags.inexact))).Where(x => x.Kind.Length > 0)];
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { TgLogUtils.WriteException(ex); throw DescribeChatAccessException(ex); }
    }

    // Используем собственный адрес найденного канала без обращения к БД и без вступления
    private TL.Channel GetPublicSearchChannel(long chatId)
    {
        if (Client is null || Bot is not null)
            throw new InvalidOperationException("Подключите пользовательский аккаунт Telegram на странице «Подключение клиента»");
        if (_resolvedChats.TryGetValue(chatId, out var chat) && chat is TL.Channel channel) return channel;
        throw new InvalidOperationException("Повторите поиск в Telegram и выберите нужный канал или группу");
    }

    /// <summary> Запрашивает подробности канала по его собственному access_hash </summary>
    protected virtual async Task<TL.Messages_ChatFull> ReadPublicChatProfileAsync(TL.Channel channel, CancellationToken ct = default)
    {
        try { return await TelegramCallAsync(apiCt => TL.SchemaExtensions.Channels_GetFullChannel(Client!, channel), isThrow: true, ct); }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { TgLogUtils.WriteException(ex); throw; }
    }

    /// <summary> Загружает фотографию профиля выбранного публичного источника </summary>
    protected virtual async Task DownloadPublicChatPhotoAsync(TL.Channel channel, Stream output, CancellationToken ct = default)
    {
        try { await TelegramCallAsync(apiCt => Client!.DownloadProfilePhotoAsync(channel, output, big: true), isThrow: true, ct); }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { TgLogUtils.WriteException(ex); throw; }
    }

    /// <summary> Запрашивает счётчики медиа одним запросом без загрузки самих сообщений </summary>
    protected virtual async Task<TL.Messages_SearchCounter[]> ReadPublicChatMediaCountsAsync(TL.Channel channel, CancellationToken ct = default)
    {
        try
        {
            TL.MessagesFilter[] filters = [new TL.InputMessagesFilterPhotos(), new TL.InputMessagesFilterVideo(),
                new TL.InputMessagesFilterDocument(), new TL.InputMessagesFilterUrl(), new TL.InputMessagesFilterMusic(),
                new TL.InputMessagesFilterVoice(), new TL.InputMessagesFilterGif(), new TL.InputMessagesFilterPoll()];
            return await TelegramCallAsync(apiCt => TL.SchemaExtensions.Messages_GetSearchCounters(Client!, channel, filters), isThrow: true, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { TgLogUtils.WriteException(ex); throw; }
    }
}
