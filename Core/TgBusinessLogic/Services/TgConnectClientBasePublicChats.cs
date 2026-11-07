namespace TgBusinessLogic.Services;

public abstract partial class TgConnectClientBase
{
    private readonly ConcurrentDictionary<long, TL.ChatBase> _resolvedChats = new();

    /// <summary> Ищет публичные источники по названию, точному имени или ссылке без чтения истории </summary>
    public async Task<List<TgPublicChatSearchResult>> SearchPublicChatsAsync(string query, CancellationToken ct = default)
    {
        try
        {
            if (Client is null || Bot is not null)
                throw new InvalidOperationException("Подключите пользовательский аккаунт Telegram на странице «Подключение клиента»");
            var text = (query ?? string.Empty).Trim();
            if (text.Length < 3) throw new InvalidOperationException("Введите не менее трёх символов названия, публичное имя или ссылку t.me");
            IEnumerable<TL.ChatBase> chats;
            if (text.StartsWith('@') || text.Contains("t.me/", StringComparison.OrdinalIgnoreCase) || text.Contains("://"))
            {
                var resolved = await ResolveChatUsernameAsync(NormalizePublicChatUsername(text), ct);
                chats = resolved.peer is TL.PeerChannel peer && resolved.chats.TryGetValue(peer.channel_id, out var chat) ? [chat] : [];
            }
            else
            {
                var found = await SearchPublicChatPeersAsync(text, ct);
                var ids = found.my_results.Concat(found.results).OfType<TL.PeerChannel>().Select(x => x.channel_id).ToHashSet();
                chats = found.chats.Values.Where(x => ids.Contains(x.ID));
            }
            using var repository = new TgStorage.Repositories.TgEfSourceRepository();
            var results = new List<TgPublicChatSearchResult>();
            foreach (var channel in chats.OfType<TL.Channel>().Where(x => !string.IsNullOrWhiteSpace(x.MainUsername)).DistinctBy(x => x.id))
            {
                ct.ThrowIfCancellationRequested();
                var saved = await repository.GetDtoAsync(x => x.Id == channel.id, ct: ct);
                _resolvedChats[channel.id] = channel;
                results.Add(new(channel.id, channel.Title, channel.MainUsername, channel.flags.HasFlag(TL.Channel.Flags.megagroup),
                    saved.Id == 0 ? null : saved.Uid,
                    channel.flags.HasFlag(TL.Channel.Flags.has_participants_count) ? channel.participants_count : null,
                    channel.flags.HasFlag(TL.Channel.Flags.verified)));
            }
            return results;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { TgLogUtils.WriteException(ex); throw DescribeChatAccessException(ex); }
    }

    /// <summary> Запрашивает ограниченную выдачу Telegram без загрузки личных контактов в реестр </summary>
    protected virtual async Task<TL.Contacts_Found> SearchPublicChatPeersAsync(string query, CancellationToken ct = default)
    {
        try { return await TelegramCallAsync(apiCt => TL.SchemaExtensions.Contacts_Search(Client!, query, limit: 30), isThrow: true, ct); }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { TgLogUtils.WriteException(ex); throw; }
    }

    /// <summary> Добавляет доступный публичный источник по имени или ссылке без вступления в него </summary>
    public async Task<TgEfSourceDto> AddPublicChatAsync(string reference, CancellationToken ct = default)
    {
        try
        {
            if (Client is null || Bot is not null)
                throw new InvalidOperationException("Подключите пользовательский аккаунт Telegram на странице «Подключение клиента»");
            var settings = new TgDownloadSettingsViewModel();
            settings.SourceVm.Dto.UserName = NormalizePublicChatUsername(reference);
            var chat = await GetChatBaseAsync(settings, ct);
            if (chat is not TL.Channel channel)
                throw new InvalidOperationException("Укажите публичный канал или группу, например @durov");
            var history = await ReadChatHistoryAsync(channel, ct);
            using var repository = new TgStorage.Repositories.TgEfSourceRepository();
            var dto = await repository.GetDtoAsync(x => x.Id == channel.id, ct: ct);
            if (dto.Id == 0)
            {
                dto.FirstId = 1;
                dto.IsSaveMessages = true;
                dto.IsSaveFiles = true;
                dto.IsCreatingSubdirectories = true;
            }
            dto.Id = channel.id;
            dto.AccessHash = channel.access_hash;
            dto.UserName = channel.MainUsername;
            dto.Title = channel.Title;
            dto.IsUserAccess = true;
            dto.IsSubscribe = !channel.flags.HasFlag(TL.Channel.Flags.left);
            dto.IsRestrictSavingContent = channel.flags.HasFlag(TL.Channel.Flags.noforwards);
            dto.Count = history.Messages.Select(x => x.ID).DefaultIfEmpty(0).Max();
            dto.DtChanged = DateTime.UtcNow;
            await repository.SaveAsync(dto, ct);
            return await repository.GetDtoAsync(x => x.Id == channel.id, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            throw DescribeChatAccessException(ex);
        }
    }

    /// <summary> Разрешает имя через Telegram, сохраняя объект канала и его собственный access_hash </summary>
    protected virtual async Task<TL.Contacts_ResolvedPeer> ResolveChatUsernameAsync(string username, CancellationToken ct = default)
    {
        try { return await TelegramCallAsync(apiCt => TL.SchemaExtensions.Contacts_ResolveUsername(Client!, username), isThrow: true, ct); }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { TgLogUtils.WriteException(ex); throw; }
    }

    /// <summary> Читает последнее доступное сообщение без изменения подписки и отметки прочтения </summary>
    protected virtual async Task<TL.Messages_MessagesBase> ReadChatHistoryAsync(TL.InputPeer peer, CancellationToken ct = default)
    {
        try { return await TelegramCallAsync(apiCt => TL.SchemaExtensions.Messages_GetHistory(Client!, peer, limit: 1), isThrow: true, ct); }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { TgLogUtils.WriteException(ex); throw; }
    }

    // Принимаем публичное имя или ссылку на сам источник, исключая приглашения и ссылки на сообщения
    private static string NormalizePublicChatUsername(string reference)
    {
        var name = (reference ?? string.Empty).Trim();
        if (name.StartsWith("t.me/", StringComparison.OrdinalIgnoreCase)) name = "https://" + name;
        if (Uri.TryCreate(name, UriKind.Absolute, out var uri))
        {
            if (uri.Scheme is not ("https" or "http") || !uri.Host.Equals("t.me", StringComparison.OrdinalIgnoreCase)
                || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
                throw new InvalidOperationException("Укажите публичное имя, например @durov, или ссылку https://t.me/durov");
            name = uri.AbsolutePath.Trim('/');
        }
        name = name.TrimStart('@');
        if (name.Length == 0 || name.All(char.IsDigit) || name.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '_'))
            throw new InvalidOperationException("Одного ID недостаточно. Укажите публичное имя, например @durov, или ссылку https://t.me/durov");
        return name;
    }

    // Объясняем ограничения Telegram, сохраняя исходную ошибку для журнала
    private static Exception DescribeChatAccessException(Exception ex) => ex is TL.RpcException rpc ? rpc.Message switch
    {
        "USERNAME_INVALID" or "USERNAME_NOT_OCCUPIED" => new InvalidOperationException(
            "Публичный чат не найден. Проверьте публичное имя источника или ссылку на него", ex),
        "CHANNEL_PRIVATE" or "USER_NOT_PARTICIPANT" or "CHAT_FORBIDDEN" => new InvalidOperationException(
            "Telegram не разрешает читать историю этого чата без участия. Вступите в него в Telegram, если у вас есть доступ, и повторите парсинг", ex),
        "CHANNEL_INVALID" or "PEER_ID_INVALID" => new InvalidOperationException(
            "Telegram не распознал источник. Укажите его актуальное публичное имя вместо одного ID, например @durov", ex),
        _ => ex
    } : ex;

    // Запоминаем разрешённый источник независимо от списка подписок и обновляем кеши его адреса
    private void RememberResolvedChat(TL.ChatBase chat, TgDownloadSettingsViewModel settings)
    {
        _resolvedChats[chat.ID] = chat;
        ChatCache[chat.ID] = chat;
        InputChannelCache.TryRemove(chat.ID, out _);
        settings.SourceVm.Dto.Id = chat.ID;
        settings.SourceVm.Dto.UserName = chat.MainUsername ?? string.Empty;
        if (chat is TL.Channel channel) settings.SourceVm.Dto.AccessHash = channel.access_hash;
    }
}
