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
    private readonly TgBufferCacheHelper<TgParticipantDto> _tlUserBuffer = default!;

    /// <summary> Release all buffers and cache </summary>
    public async Task ReleaseBuffersAsync()
    {
        _messageBuffer.Dispose();
        _messageRelationBuffer.Dispose();
        _chatBuffer.Dispose();
        _storyBuffer.Dispose();
        _userBuffer.Dispose();
        _tlUserBuffer.Dispose();
        Cache.Dispose();
        await Task.CompletedTask;
    }

    /// <summary> Wait for the transaction to complete </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task WaitTransactionCompleteAsync(CancellationToken ct = default)
    {
        try
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
                if (CheckShouldStop(ct)) return;

                var elapsed = DateTime.UtcNow - start;
                var remaining = timeout - elapsed;

                // Stop waiting if total timeout is exceeded
                if (elapsed > timeout)
                    throw new InvalidOperationException("Transaction is still active after waiting for timeouts!");

                // Log status every 5 seconds for debugging/tracking purposes
                if ((int)elapsed.TotalSeconds % 5 == 0)
                    TgLogUtils.WriteLog($"Still waiting for transaction to complete... elapsed: {elapsed.TotalSeconds:F1}s");

                await Task.Delay(delay, ct);

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
        catch (OperationCanceledException)
        {
            // Quietly exit without logging in
        }
    }

    /// <summary> Flush chat buffer to database </summary>
    private async Task FlushChatBufferAsync(bool isSaveMessages, bool isRewriteMessages, bool isForce, CancellationToken ct = default) =>
        await _chatBuffer.FlushAsync(isSaveMessages, isForce,
            saveAction: async (list) =>
            {
                await WaitTransactionCompleteAsync();
                await StorageManager.SourceRepository.SaveListAsync(list, isRewriteMessages, ct: ct);
            },
            postSaveAction: async list =>
            {
                foreach (var item in list)
                    await Cache.RemoveAsync(TgCacheUtils.GetCacheKeyFullChat(item.Id), token: ct);
            });

    /// <summary> Flush story buffer to database </summary>
    private async Task FlushStoryBufferAsync(bool isSaveMessages, bool isRewriteMessages, bool isForce, CancellationToken ct = default) =>
        await _storyBuffer.FlushAsync(isSaveMessages, isForce,
            saveAction: async (list) =>
            {
                await WaitTransactionCompleteAsync();
                await StorageManager.StoryRepository.SaveListAsync(list, isRewriteMessages, ct: ct);
            },
            postSaveAction: async list =>
            {
                foreach (var item in list)
                    await Cache.RemoveAsync(TgCacheUtils.GetCacheKeyStory(item.FromId ?? 0, item.Id), token: ct);
            });

    /// <summary> Flush user buffer to database </summary>
    private async Task FlushUserBufferAsync(bool isSaveMessages, bool isRewriteMessages, bool isForce, CancellationToken ct = default) =>
        await _userBuffer.FlushAsync(isSaveMessages, isForce,
            saveAction: async (list) =>
            {
                await WaitTransactionCompleteAsync();
                await StorageManager.UserRepository.SaveListAsync(list, isRewriteMessages, ct: ct);
            },
            postSaveAction: async list =>
            {
                foreach (var item in list)
                    await Cache.RemoveAsync(TgCacheUtils.GetCacheKeyUser(item.Id), token: ct);
            });

    /// <summary> Flush message buffer to database </summary>
    private async Task FlushMessageBufferAsync(bool isSaveMessages, bool isRewriteMessages, bool isForce, CancellationToken ct = default)
    {
        var isMessageFinally = false;

        // Flush message buffer to database
        await _messageBuffer.FlushAsync(isSaveMessages, isForce,
            saveAction: async (list) =>
            {
                if (CheckShouldStop(ct)) return;
                await WaitTransactionCompleteAsync();

                if (CheckShouldStop(ct)) return;
                await StorageManager.MessageRepository.SaveListAsync(list, isRewriteMessages, ct: ct);

                if (CheckShouldStop(ct)) return;
                if (_downloadSettings is not null && list.Count > 0)
                {
                    var ownMessages = list.Where(x => x.SourceId == _downloadSettings.SourceVm.Dto.Id);
                    var maxId = ownMessages.Select(x => x.Id).DefaultIfEmpty(0).Max();
                    if (maxId > 0)
                    {
                        var newFirstId = Math.Max(_downloadSettings.SourceVm.Dto.FirstId, maxId);
                        if (newFirstId > _downloadSettings.SourceVm.Dto.FirstId)
                        {
                            _downloadSettings.SourceVm.Dto.FirstId = newFirstId;
                            // Save updated FirstId to database
                            var sourceEntity = TgEfDomainUtils.CreateNewEntity(_downloadSettings.SourceVm.Dto, isUidCopy: true);
                            await StorageManager.SourceRepository.SaveAsync(sourceEntity, ct: ct);
                        }
                    }
                }
            },
            postSaveAction: async list =>
            {
                foreach (var item in list)
                {
                    if (CheckShouldStop(ct)) return;
                    await Cache.RemoveAsync(TgCacheUtils.GetCacheKeyMessage(item.SourceId, item.Id), token: ct);
                }

                if (CheckShouldStop(ct)) return;
                if (_downloadSettings is not null && isForce)
                {
                    var newFirstId = Math.Max(_downloadSettings.SourceVm.Dto.FirstId, _downloadSettings.SourceVm.Dto.Count);
                    if (newFirstId > _downloadSettings.SourceVm.Dto.FirstId)
                    {
                        _downloadSettings.SourceVm.Dto.FirstId = newFirstId;
                        // Save updated FirstId to database
                        var sourceEntity = TgEfDomainUtils.CreateNewEntity(_downloadSettings.SourceVm.Dto, isUidCopy: true);
                        await StorageManager.SourceRepository.SaveAsync(sourceEntity, ct: ct);
                    }
                }

                isMessageFinally = true;
            });

        while (!isMessageFinally)
        {
            if (CheckShouldStop(ct)) return;
            await Task.Delay(TgConstants.TimeOutUIMiddleMilliseconds, ct);
        }

        // Flush message relation buffer to database
        await _messageRelationBuffer.FlushAsync(isSaveMessages, isForce,
            saveAction: async (list) =>
            {
                await WaitTransactionCompleteAsync();
                await StorageManager.MessageRelationRepository.SaveListAsync(list, isRewriteMessages, ct: ct);
            },
            postSaveAction: async list =>
            {
                foreach (var item in list)
                {
                    await Cache.RemoveAsync(TgCacheUtils.GetCacheKeyMessageRelation(item.ParentSourceId, item.ParentMessageId, item.ChildSourceId, item.ChildMessageId),
                        token: ct);
                }
            });
    }

    /// <summary> Fills the buffer with stories for a specific peer </summary>
    public async Task<TgEfStoryEntity?> FillBufferStoriesAsync(long peerId, TL.StoryItem story, CancellationToken ct = default) =>
        await TryGetCatchAsync(async () =>
        {
            TgEfStoryEntity? entity = null;
            if (story.entities is not null)
            {
                foreach (var message in story.entities)
                {
                    entity = await StorageManager.CreateOrGetStoryAsync(peerId, story);
                    await Cache.SetAsync(TgCacheUtils.GetCacheKeyStory(peerId, story.id), entity, TgCacheUtils.CacheOptionsChannelMessages, token: ct);
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
                await Cache.SetAsync(TgCacheUtils.GetCacheKeyStory(peerId, story.id), entity, TgCacheUtils.CacheOptionsChannelMessages, token: ct);
                // Save at memory
                if (_storyBuffer.FirstOrDefault(x => x.Id == story.id && x.FromId == peerId) is null)
                    _storyBuffer.Add(entity);
            }
            return entity;
        }, ct: ct);

    /// <summary> Fills the buffer with users with cancellation and exception handling </summary>
    public async Task<TgEfUserEntity?> FillBufferUsersAsync(TL.User user, bool isContact, CancellationToken ct = default) =>
        await TryGetCatchAsync(async () =>
        {
            // Create or get user entity from storage
            var entity = await StorageManager.CreateOrGetUserAsync(user, isContact, isSave: true, ct);

            await Cache.SetAsync(TgCacheUtils.GetCacheKeyUser(user.id), entity, TgCacheUtils.CacheOptionsChannelMessages, ct);

            // Save to in-memory buffer if not already present
            if (_userBuffer.FirstOrDefault(x => x.Id == user.id) is null)
                _userBuffer.Add(entity);
            return entity;
        }, ct: ct);

    /// <summary> Safely tries to get full channel information, utilizing caching to minimize API calls with cancellation and exception handling </summary>
    private async Task<TL.Messages_ChatFull?> TryGetFullChannelSafeAsync(TL.Channel channel, CancellationToken ct = default)
    {
        var key = TgCacheUtils.GetCacheKeyFullChannel(channel.id);

        try
        {
            // Try to retrieve from cache in a strictly typed manner
            var maybe = await Cache.TryGetAsync<TL.Messages_ChatFull?>(key, TgCacheUtils.CacheOptionsFullChat, ct);
            if (maybe.HasValue)
                return maybe.Value;

            // Cache miss: retrieve from server and save
            return await TryGetFullChannelSafeCoreAsync(channel, key, ct);
        }
        catch (OperationCanceledException)
        {
            // Quietly exit without logging in
            return default;
        }
        catch (InvalidCastException)
        {
            // There is another type in the cache: clear and reload
            await Cache.RemoveAsync(key, token: ct);
            return await TryGetFullChannelSafeCoreAsync(channel, key, ct);
        }
    }

    /// <summary> Retrieves full channel info from Telegram API and caches it </summary>
    private async Task<TL.Messages_ChatFull?> TryGetFullChannelSafeCoreAsync(TL.Channel channel, string key, CancellationToken ct)
    {
        // Call Telegram API with cancellation support
        var fresh = await TelegramCallAsync<TL.Messages_ChatFull?>(
            apiCt => Client?.Channels_GetFullChannel(channel) ?? default!, isThrow: false, ct);

        // Save to cache
        await Cache.SetAsync(key, fresh, TgCacheUtils.CacheOptionsFullChat, ct);
        return fresh;
    }

    /// <summary> Safely tries to get the last message ID of a channel, utilizing caching to minimize API calls with cancellation and exception handling </summary>
    private async Task<int> TryGetCacheChatLastCountSafeAsync(long chatId, CancellationToken ct)
    {
        var key = TgCacheUtils.GetCacheKeyChatLastCount(chatId);

        try
        {
            // Try to retrieve from cache in a strictly typed manner
            var maybe = await Cache.TryGetAsync<int>(key, TgCacheUtils.CacheOptionsChannelMessages, ct);
            if (maybe.HasValue)
                return maybe.Value;

            // Cache miss: compute and set
            return await TryGetCacheChatLastCountSafeCoreAsync(chatId, key, ct);
        }
        catch (OperationCanceledException)
        {
            // Quietly exit without logging in
            return default;
        }
        catch (InvalidCastException)
        {
            // There is another type in the cache: clear and reload
            await Cache.RemoveAsync(key, token: ct);
            return await TryGetCacheChatLastCountSafeCoreAsync(chatId, key, ct);
        }
    }

    /// <summary> Retrieves last message ID of a channel and caches it </summary>
    private async Task<int> TryGetCacheChatLastCountSafeCoreAsync(long chatId, string key, CancellationToken ct)
    {
        // Compute last message ID
        var fresh = await GetChannelMessageIdLastCoreAsync(chatId, ct);

        // Save to cache
        await Cache.SetAsync(key, fresh, TgCacheUtils.CacheOptionsChannelMessages, ct);
        return fresh;
    }

    /// <summary> Wraps a factory with unified cancellation support and safe exception handling </summary>
    private Func<FusionCacheFactoryExecutionContext<T>, CancellationToken, Task<T>> SafeFactory<T>(
        Func<FusionCacheFactoryExecutionContext<T>, CancellationToken, Task<T>> factory)
    {
        return async (ctx, innerCt) =>
        {
            // Combine external cancellation token with download token
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(innerCt, LoadStateService.DownloadToken);
            var ct = linkedCts.Token;

            // Return default if cancellation already requested
            if (CheckShouldStop(ct))
                return default!;

            try
            {
                // Invoke the provided factory with the combined token
                var task = factory?.Invoke(ctx, ct);
                if (task == null)
                    return default!;

                return await task.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Return default on cancellation without throwing
                return default!;
            }
            catch (Exception ex)
            {
                // Log other exceptions and return default
                TgLogUtils.WriteException(ex);
                return default!;
            }
        };
    }

    #endregion
}
