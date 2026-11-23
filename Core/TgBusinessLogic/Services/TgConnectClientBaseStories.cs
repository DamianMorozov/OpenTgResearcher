using TL;
using TgStorage.Repositories;

namespace TgBusinessLogic.Services;

public abstract partial class TgConnectClientBase
{
    private sealed record StoryPeer(long Id, string? Name, bool IsChannel, long AccessHash)
    {
        public InputPeer ToInputPeer() => IsChannel ? new InputPeerChannel(Id, AccessHash) : new InputPeerUser(Id, AccessHash);
    }

    private async Task<List<(StoryPeer Peer, StoryItemBase[] Stories)>> ReadActiveStoryFeedAsync(CancellationToken ct)
    {
        var feed = new List<(StoryPeer, StoryItemBase[])>();
        var cursors = new HashSet<string>();
        string? state = null;
        while (await ReadStoriesPageAsync(state, ct) is { } page)
        {
            ct.ThrowIfCancellationRequested();
            foreach (var source in page.peer_stories)
                feed.Add((GetStoryPeer(source.peer, page.UserOrChat(source.peer)), source.stories));
            if (!page.flags.HasFlag(Stories_AllStories.Flags.has_more) || string.IsNullOrEmpty(page.state) || !cursors.Add(page.state)) break;
            state = page.state;
        }
        return feed;
    }

    private static StoryPeer GetStoryPeer(Peer peer, IPeerInfo? info) => info switch
    {
        TL.User user => new(peer.ID, GetStoryUserName(user), false, user.access_hash),
        Channel channel => new(peer.ID, channel.Title, true, channel.access_hash),
        ChatBase chat => new(peer.ID, chat.Title, peer is PeerChannel, 0),
        _ => new(peer.ID, null, peer is PeerChannel, 0),
    };

    public async Task<TgStoryArchiveResult> SyncStoryArchiveAsync(string directory, TgEfStoryDto? selected = null, string? username = null,
        CancellationToken ct = default)
    {
        directory = Path.GetFullPath(directory);
        Directory.CreateDirectory(directory);
        using var repository = new TgEfStoryRepository();
        var total = new TgStoryArchiveResult();
        if (selected is null && string.IsNullOrWhiteSpace(username))
        {
            foreach (var group in (await ReadActiveStoryFeedAsync(ct)).GroupBy(item => item.Peer.Id))
            {
                var peer = group.OrderByDescending(item => item.Peer.AccessHash != 0).First().Peer;
                var stories = IndexStoryItems(group.SelectMany(item => item.Stories));
                if (peer.AccessHash != 0 || Me?.id == peer.Id)
                    await ReadStoryDetailsAsync(ToStoryInputPeer(peer), stories, stories.Values.Where(IsIncompleteStory).Select(x => x.ID).ToArray(), ct);
                total = await SaveStoryArchiveAsync(repository, directory, peer, stories.Values, total, ct);
            }
            return total;
        }

        var author = await ResolveStoryPeerAsync(selected, username, ct);
        var inputPeer = ToStoryInputPeer(author);
        var active = await ReadPeerStoryItemsAsync(inputPeer, ct);
        var items = IndexStoryItems(active);
        await AddStoryProfilePagesAsync(inputPeer, items, archive: false, ct);
        if (!author.IsChannel && author.Id == Me?.id)
            await AddStoryProfilePagesAsync(inputPeer, items, archive: true, ct);
        else if (author.IsChannel && DicChats.TryGetValue(author.Id, out var chat) && chat is Channel channel
            && (channel.flags.HasFlag(Channel.Flags.creator) || channel.admin_rights is not null))
            await AddStoryProfilePagesAsync(inputPeer, items, archive: true, ct);

        // Удаление подтверждает ответ Telegram по ID. Отсутствие в ленте может означать истечение срока
        var known = await repository.GetListDtosAsync(0, 0, x => x.FromId == author.Id, ct);
        var ids = known.Where(x => !x.IsDeleted && x.Id is > 0 and <= int.MaxValue && !items.ContainsKey((int)x.Id))
            .Select(x => (int)x.Id).Concat(items.Values.Where(IsIncompleteStory).Select(x => x.ID)).Distinct().ToArray();
        await ReadStoryDetailsAsync(inputPeer, items, ids, ct);
        return await SaveStoryArchiveAsync(repository, directory, author, items.Values, total, ct);
    }

    private InputPeer ToStoryInputPeer(StoryPeer peer) => !peer.IsChannel && peer.Id == Me?.id ? new InputPeerSelf() : peer.ToInputPeer();

    private async Task<StoryPeer> ResolveStoryPeerAsync(TgEfStoryDto? story, string? username, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(username))
        {
            var name = username.Trim().TrimStart('@');
            if (name.Length == 0 || name.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '_'))
                throw new ArgumentException("Укажите имя пользователя Telegram в формате @username.");
            var response = await TelegramCallAsync(apiCt => Client!.Contacts_ResolveUsername(name), isThrow: true, ct);
            var peer = GetStoryPeer(response.peer, response.UserOrChat);
            if (peer.AccessHash != 0 || peer.Id == Me?.id) return peer;
            throw new InvalidOperationException("Telegram не предоставил доступ к источнику сторис.");
        }

        if (story is null || story.FromId <= 0)
            throw new InvalidOperationException("Выберите сторис автора или укажите его @username.");
        var id = story.FromId;
        if (story.PeerAccessHash != 0 || (!story.IsChannel && Me?.id == id))
            return new(id, story.FromName, story.IsChannel, story.PeerAccessHash);
        if (DicUsers.TryGetValue(id, out var user) && user.access_hash != 0)
            return new(id, GetStoryUserName(user), false, user.access_hash);
        if (DicChats.TryGetValue(id, out var chat) && chat is Channel channel && channel.access_hash != 0)
            return new(id, channel.Title, true, channel.access_hash);
        using var users = new TgEfUserRepository();
        var contact = await users.GetDtoAsync(x => x.Id == id, ct);
        if (contact.AccessHash != 0) return new(id, story.FromName, false, contact.AccessHash);
        using var sources = new TgEfSourceRepository();
        var source = await sources.GetDtoAsync(x => x.Id == id, ct);
        if (source.AccessHash != 0) return new(id, source.Title, true, source.AccessHash);
        throw new InvalidOperationException("Источник старой записи неизвестен. Введите @username автора или обновите ленту из Telegram.");
    }

    private static Dictionary<int, StoryItemBase> IndexStoryItems(IEnumerable<StoryItemBase> items)
    {
        var result = new Dictionary<int, StoryItemBase>();
        AddStoryItems(result, items);
        return result;
    }

    private static void AddStoryItems(Dictionary<int, StoryItemBase> result, IEnumerable<StoryItemBase> items)
    {
        foreach (var story in items)
            if (!result.TryGetValue(story.ID, out var previous) || previous is not StoryItem || !IsIncompleteStory(story))
                result[story.ID] = story;
    }

    private static bool IsIncompleteStory(StoryItemBase item) =>
        item is StoryItemSkipped || item is StoryItem story && story.flags.HasFlag(StoryItem.Flags.min);

    private async Task AddStoryProfilePagesAsync(InputPeer peer, Dictionary<int, StoryItemBase> items, bool archive, CancellationToken ct)
    {
        const int pageSize = 100;
        var offset = 0;
        while (true)
        {
            var page = await ReadStoryProfilePageAsync(peer, offset, pageSize, archive, ct);
            if (page.Length == 0) break;
            AddStoryItems(items, page);
            var next = page[^1].ID;
            if (next <= 0 || (offset != 0 && next >= offset)) break;
            offset = next;
        }
    }

    private async Task ReadStoryDetailsAsync(InputPeer peer, Dictionary<int, StoryItemBase> items, int[] ids, CancellationToken ct)
    {
        foreach (var batch in ids.Chunk(100))
            AddStoryItems(items, await ReadStoryItemsByIdAsync(peer, batch, ct));
    }

    protected virtual async Task<StoryItemBase[]> ReadPeerStoryItemsAsync(InputPeer peer, CancellationToken ct) =>
        (await TelegramCallAsync(apiCt => Client!.Stories_GetPeerStories(peer), isThrow: true, ct)).stories?.stories ?? [];

    protected virtual async Task<StoryItemBase[]> ReadStoryProfilePageAsync(InputPeer peer, int offset, int limit, bool archive, CancellationToken ct) =>
        (await TelegramCallAsync(apiCt => archive ? Client!.Stories_GetStoriesArchive(peer, offset, limit)
            : Client!.Stories_GetPinnedStories(peer, offset, limit), isThrow: true, ct)).stories;

    protected virtual async Task<StoryItemBase[]> ReadStoryItemsByIdAsync(InputPeer peer, int[] ids, CancellationToken ct) =>
        (await TelegramCallAsync(apiCt => Client!.Stories_GetStoriesByID(peer, ids), isThrow: true, ct)).stories;

    private async Task<TgStoryArchiveResult> SaveStoryArchiveAsync(ITgEfStoryRepository repository, string directory, StoryPeer peer,
        IEnumerable<StoryItemBase> items, TgStoryArchiveResult total, CancellationToken ct)
    {
        var scope = await repository.GetListDtosAsync(0, 0, x => x.FromId == peer.Id, ct);
        var known = scope.ToDictionary(x => x.Id);
        foreach (var item in items)
        {
            ct.ThrowIfCancellationRequested();
            var dto = known.GetValueOrDefault(item.ID);
            if (item is StoryItemDeleted)
            {
                if (dto is null) continue;
                dto.IsDeleted = true;
            }
            else if (item is StoryItem story && !IsIncompleteStory(story))
            {
                dto ??= new() { Id = story.id, FromId = peer.Id };
                dto.Date = story.date;
                dto.ExpireDate = story.expire_date;
                dto.Caption = story.caption ?? string.Empty;
                dto.IsDeleted = false;
                try
                {
                    if (!story.flags.HasFlag(StoryItem.Flags.noforwards)
                        && await SaveStoryMediaAsync(directory, dto, peer, story.media, ct))
                        total = total with { Downloaded = total.Downloaded + 1 };
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    TgLogUtils.WriteException(ex);
                    total = total with { Failed = total.Failed + 1 };
                }
            }
            else continue;

            if (!string.IsNullOrWhiteSpace(peer.Name)) dto.FromName = peer.Name;
            if (peer.AccessHash != 0) dto.PeerAccessHash = peer.AccessHash;
            dto.IsChannel = peer.IsChannel;
            await repository.SaveAsync(dto, ct);
            try
            {
                var mediaPath = TgStoryMediaArchive.ResolvePath(directory, dto.MediaPath);
                if (File.Exists(mediaPath))
                {
                    var marker = mediaPath + ".deleted";
                    if (dto.IsDeleted && !File.Exists(marker))
                        await File.WriteAllTextAsync(marker, "Deleted in Telegram", ct);
                    else if (!dto.IsDeleted && File.Exists(marker))
                        File.Delete(marker);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                TgLogUtils.WriteException(ex);
                total = total with { Failed = total.Failed + 1 };
            }
            total = total with { Stories = total.Stories + 1 };
        }
        return total;
    }

    private async Task<bool> SaveStoryMediaAsync(string directory, TgEfStoryDto dto, StoryPeer peer, MessageMedia? media, CancellationToken ct)
    {
        var title = peer.Name ?? string.Empty;
        var folder = FormattableString.Invariant($"{(peer.IsChannel ? "channel-" : "user-")}{peer.Id}");
        string name;
        long size;
        Func<Stream, Task> download;
        if (media is MessageMediaPhoto { photo: Photo photo }
            && photo.sizes.Where(x => x is PhotoSize or PhotoSizeProgressive).MaxBy(x => x.FileSize) is { } photoSize)
        {
            name = FormattableString.Invariant($"{dto.Id}-{photo.id}.jpg");
            size = photoSize.FileSize;
            download = stream => DownloadPhotoMediaAsync(photo, stream, photoSize, peer.Id, title, ct);
        }
        else if (media is MessageMediaDocument { document: Document document })
        {
            var extension = document.mime_type switch
            {
                "video/mp4" => ".mp4", "video/webm" => ".webm", "video/quicktime" => ".mov", "video/x-matroska" => ".mkv",
                "image/jpeg" => ".jpg", "image/png" => ".png", "image/webp" => ".webp", "image/gif" => ".gif", _ => ".bin",
            };
            name = FormattableString.Invariant($"{dto.Id}-{document.id}{extension}");
            size = document.size;
            download = stream => DownloadDocumentMediaAsync(document, stream, null, null, peer.Id, title, ct);
        }
        else return false;

        var relative = Path.Combine(folder, name);
        var path = TgStoryMediaArchive.ResolvePath(directory, relative);
        var downloaded = await TgStoryMediaArchive.SaveAsync(path, size, download, ct);
        dto.MediaPath = relative;
        return downloaded;
    }
}
