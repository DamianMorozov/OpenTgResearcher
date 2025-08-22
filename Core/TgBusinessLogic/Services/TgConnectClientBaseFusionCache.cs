// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using TL;

namespace TgBusinessLogic.Services;

public abstract partial class TgConnectClientBase : TgWebDisposable, ITgConnectClient
{
    #region FusionCache

    private readonly IFusionCache Cache = default!;
    private readonly TgBufferCacheHelper<TgEfMessageEntity> _messageBuffer = default!;
    private readonly TgBufferCacheHelper<TgEfMessageRelationEntity> _messageRelationBuffer = default!;
    private readonly TgBufferCacheHelper<TgEfSourceEntity> _chatBuffer = default!;
    private readonly TgBufferCacheHelper<TgEfStoryEntity> _storyBuffer = default!;
    private readonly TgBufferCacheHelper<TgEfUserEntity> _userBuffer = default!;

    private ValueTask<Messages_ChatFull?> GetCachedFullChannelAsync(Channel channel) =>
        Cache.GetOrSetAsync(TgCacheUtils.GetCacheKeyFullChannel(channel.id), factory: _ => TelegramCallAsync(() => Client?.Channels_GetFullChannel(channel) ?? default!), TgCacheUtils.CacheOptionsFullChat);

    private ValueTask<Messages_ChatFull?> GetCachedFullChatAsync(ChatBase chatBase) =>
        Cache.GetOrSetAsync(TgCacheUtils.GetCacheKeyFullChat(chatBase.ID), factory: _ => TelegramCallAsync(() => Client?.GetFullChat(chatBase) ?? default!), TgCacheUtils.CacheOptionsFullChat);

    private ValueTask<Messages_ChatFull?> GetCachedFullChatAsync(long chatId) =>
        Cache.GetOrSetAsync(TgCacheUtils.GetCacheKeyFullChat(chatId), factory: _ => TelegramCallAsync(() => Client?.Messages_GetFullChat(chatId) ?? default!), TgCacheUtils.CacheOptionsFullChat);

    private ValueTask<Messages_MessagesBase> GetCachedChannelMessageAsync(Channel channel, int messageId, Func<Task<Messages_MessagesBase>> factory) =>
        Cache.GetOrSetAsync(TgCacheUtils.GetCacheKeyMessage(channel.id, messageId), factory: _ => factory(), TgCacheUtils.CacheOptionsChannelMessages);

    private ValueTask<int> GetCachedChatLastCountAsync(long chatId, Func<Task<int>> factory) =>
        Cache.GetOrSetAsync(TgCacheUtils.GetCacheKeyChatLastCount(chatId), factory: _ => factory(), TgCacheUtils.CacheOptionsChannelMessages);

    /// <summary> Wait for the transaction to complete </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task WaitTransactionCompleteAsync(CancellationToken cancellationToken = default)
    {
        // Calculate total timeout based on configured wait times
        var maxWaitSeconds = FloodControlService.WaitSeconds.Sum();
        var timeout = TimeSpan.FromSeconds(maxWaitSeconds);
        var start = DateTime.UtcNow;
        // Delay tuning parameters
        var minDelay = TimeSpan.FromMilliseconds(100); // shortest delay between checks
        var maxDelay = TimeSpan.FromSeconds(2);        // longest delay allowed
        var delay = minDelay;

        // Threshold for final phase (e.g., last 15% of timeout)
        var finalPhaseThreshold = TimeSpan.FromSeconds(maxWaitSeconds * 0.15);

        while (StorageManager.EfContext.Database.CurrentTransaction is not null)
        {
            var elapsed = DateTime.UtcNow - start;
            var remaining = timeout - elapsed;

            // Stop waiting if total timeout is exceeded
            if (elapsed > timeout)
                throw new InvalidOperationException("Transaction is still active after waiting for timeouts!");

            // Log status every 5 seconds for debugging/tracking purposes
            if ((int)elapsed.TotalSeconds % 5 == 0)
                TgLogUtils.WriteLog($"Still waiting for transaction to complete... elapsed: {elapsed.TotalSeconds:F1}s");

            await Task.Delay(delay, cancellationToken);

            // Adjust delay dynamically:
            if (remaining <= finalPhaseThreshold)
            {
                // Final phase: reduce delay to check more often
                delay = minDelay;
            }
            else
            {
                // Ramp-up phase: gradually increase delay until maxDelay
                if (delay < maxDelay)
                    delay = TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds * 2, maxDelay.TotalMilliseconds));
            }
        }
    }

    /// <summary> Flush chat buffer to database </summary>
    private async Task FlushChatBufferAsync(bool isSaveMessages, bool isRewriteMessages, bool isForce) =>
        await _chatBuffer.FlushAsync(isSaveMessages, isForce,
            saveAction: async (list) =>
            {
                await WaitTransactionCompleteAsync();
                await StorageManager.SourceRepository.SaveListAsync(list, isRewriteMessages);
            },
            postSaveAction: async list =>
            {
                foreach (var item in list)
                    await Cache.RemoveAsync(TgCacheUtils.GetCacheKeyFullChat(item.Id));
            });

    /// <summary> Flush story buffer to database </summary>
    private async Task FlushStoryBufferAsync(bool isSaveMessages, bool isRewriteMessages, bool isForce) =>
        await _storyBuffer.FlushAsync(isSaveMessages, isForce,
            saveAction: async (list) =>
            {
                await WaitTransactionCompleteAsync();
                await StorageManager.StoryRepository.SaveListAsync(list, isRewriteMessages);
            },
            postSaveAction: async list =>
            {
                foreach (var item in list)
                    await Cache.RemoveAsync(TgCacheUtils.GetCacheKeyStory(item.FromId ?? 0, item.Id));
            });

    /// <summary> Flush user buffer to database </summary>
    private async Task FlushUserBufferAsync(bool isSaveMessages, bool isRewriteMessages, bool isForce) =>
        await _userBuffer.FlushAsync(isSaveMessages, isForce,
            saveAction: async (list) =>
            {
                await WaitTransactionCompleteAsync();
                await StorageManager.UserRepository.SaveListAsync(list, isRewriteMessages);
            },
            postSaveAction: async list =>
            {
                foreach (var item in list)
                    await Cache.RemoveAsync(TgCacheUtils.GetCacheKeyUser(item.Id));
            });

    /// <summary> Flush message buffer to database </summary>
    private async Task FlushMessageBufferAsync(bool isSaveMessages, bool isRewriteMessages, bool isForce) =>
        await _messageBuffer.FlushAsync(isSaveMessages, isForce,
            saveAction: async (list) =>
            {
                await WaitTransactionCompleteAsync();
                await StorageManager.MessageRepository.SaveListAsync(list, isRewriteMessages);
            },
            postSaveAction: async list =>
            {
                foreach (var item in list)
                {
                    await Cache.RemoveAsync(TgCacheUtils.GetCacheKeyMessage(item.SourceId, item.Id));
                }
            });

    /// <summary> Flush message buffer to database </summary>
    private async Task FlushMessageRelationBufferAsync(bool isSaveMessages, bool isRewriteMessages, bool isForce) =>
        await _messageRelationBuffer.FlushAsync(isSaveMessages, isForce,
            saveAction: async (list) =>
            {
                await WaitTransactionCompleteAsync();
                await StorageManager.MessageRelationRepository.SaveListAsync(list, isRewriteMessages);
            },
            postSaveAction: async list =>
            {
                foreach (var item in list)
                {
                    await Cache.RemoveAsync(TgCacheUtils.GetCacheKeyMessageRelation(item.ParentSourceId, item.ParentMessageId, item.ChildSourceId, item.ChildMessageId));
                }
            });

    /// <summary> Fills the buffer with stories for a specific peer </summary>
    public async Task<TgEfStoryEntity?> FillBufferStoriesAsync(long peerId, StoryItem story) =>
        await TryCatchAsync(async () =>
        {
            TgEfStoryEntity? entity = null;
            if (story.entities is not null)
            {
                foreach (var message in story.entities)
                {
                    entity = await StorageManager.CreateOrGetStoryAsync(peerId, story);
                    await Cache.SetAsync(TgCacheUtils.GetCacheKeyStory(peerId, story.id), entity, TgCacheUtils.CacheOptionsChannelMessages);
                    if (message is not null)
                    {
                        entity.Type = message.Type;
                        entity.Offset = message.Offset;
                        entity.Length = message.Length;
                        TgEfStoryEntityByMessageType(entity, message);
                    }
                    // Switch media type
                    TgEfStoryEntityByMediaType(entity, story.media);
                    // Save at memory
                    if (_storyBuffer.FirstOrDefault(x => x.Id == story.id && x.FromId == peerId) is null)
                        _storyBuffer.Add(entity);
                }
            }
            else
            {
                entity = await StorageManager.CreateOrGetStoryAsync(peerId, story);
                await Cache.SetAsync(TgCacheUtils.GetCacheKeyStory(peerId, story.id), entity, TgCacheUtils.CacheOptionsChannelMessages);
                // Save at memory
                if (_storyBuffer.FirstOrDefault(x => x.Id == story.id && x.FromId == peerId) is null)
                    _storyBuffer.Add(entity);
            }
            return entity;
        });

    /// <summary> Fills the buffer with users </summary>
    public async Task<TgEfUserEntity?> FillBufferUsersAsync(TL.User user, bool isContact) =>
        await TryCatchAsync(async () =>
        {
            var entity = await StorageManager.CreateOrGetUserAsync(user, isContact);
            await Cache.SetAsync(TgCacheUtils.GetCacheKeyUser(user.id), entity, TgCacheUtils.CacheOptionsChannelMessages);
            // Save at memory
            if (_userBuffer.FirstOrDefault(x => x.Id == user.id) is null)
                _userBuffer.Add(entity);
            return entity;
        });

    #endregion
}
