namespace TgBusinessLogic.Helpers;

/// <summary> Fusion cache helper </summary>
public static class TgCacheUtils
{
    #region Fields, properties, constructor

    public static SemaphoreSlim SaveLock { get; } = new(initialCount: 1, maxCount: 1);
    public static string GetCacheKeyChatLastCount(long chatId) => $"{GetCacheKeyChatLastCountPrefix}:{chatId}";
    public static string GetCacheKeyChatLastCountPrefix() => $"chatLastCount";
    public static string GetCacheKeyChatPrefix() => $"chat";
    public static string GetCacheKeyFullChannel(long peerId) => $"{GetCacheKeyFullChannelPrefix}:{peerId}";
    public static string GetCacheKeyFullChannelPrefix() => $"fullChannel";
    public static string GetCacheKeyFullChat(long peerId) => $"{GetCacheKeyFullChatPrefix}:{peerId}";
    public static string GetCacheKeyFullChatPrefix() => $"fullChat";
    public static string GetCacheKeyMessage(long peerId, int messageId) => $"{GetCacheKeyMessagePrefix}:{peerId}:{messageId}";
    public static string GetCacheKeyMessagePrefix() => $"message";
    public static string GetCacheKeyMessageProcessed(long chatId, int messageId) => $"messageProcessed:{chatId}:{messageId}";
    public static string GetCacheKeyMessageRelation(long parentSourceId, int parentMessageId, long childSourceId, int childMessageId) => $"{GetCacheKeyMessageRelationPrefix}:{parentSourceId}:{parentMessageId}:{childSourceId}:{childMessageId}";
    public static string GetCacheKeyMessageRelationPrefix() => $"messageRelation";
    public static string GetCacheKeyMessages(long peerId, int messageIdStart, int messageIdEnd) => $"{GetCacheKeyMessagesPrefix}:{peerId}:{messageIdStart}-{messageIdEnd}";
    public static string GetCacheKeyMessagesPrefix() => $"messages";
    public static string GetCacheKeyStory(long peerId, long storyId) => $"{GetCacheKeyStoryPrefix}:{peerId}:{storyId}";
    public static string GetCacheKeyStoryPrefix() => $"story";
    public static string GetCacheKeyUser(long id) => $"{GetCacheKeyUserPrefix}:{id}";
    public static string GetCacheKeyUserPrefix() => $"user";

    public static readonly FusionCacheEntryOptions CacheOptionsFullChat = new()
    {
        Duration = TimeSpan.FromSeconds(60),
        JitterMaxDuration = TimeSpan.FromSeconds(3),
        IsFailSafeEnabled = true,
        FailSafeMaxDuration = TimeSpan.FromMinutes(2),
        EagerRefreshThreshold = 0.8f,
        FactorySoftTimeout = TimeSpan.FromSeconds(5),
        FactoryHardTimeout = TimeSpan.FromSeconds(15)
    };

    public static readonly FusionCacheEntryOptions CacheOptionsChannelMessages = new()
    {
        Duration = TimeSpan.FromSeconds(30),
        JitterMaxDuration = TimeSpan.FromSeconds(2),
        IsFailSafeEnabled = true,
        FailSafeMaxDuration = TimeSpan.FromMinutes(1),
        EagerRefreshThreshold = 0.8f,
        FactorySoftTimeout = TimeSpan.FromSeconds(5),
        FactoryHardTimeout = TimeSpan.FromSeconds(15)
    };

    public static readonly FusionCacheEntryOptions CacheOptionsProcessMessage = new()
    {
        Duration = TimeSpan.FromMinutes(10),
        JitterMaxDuration = TimeSpan.FromSeconds(5),
        IsFailSafeEnabled = true,
        FailSafeMaxDuration = TimeSpan.FromMinutes(5),
        EagerRefreshThreshold = 0.9f,
        FactorySoftTimeout = TimeSpan.FromSeconds(5),
        FactoryHardTimeout = TimeSpan.FromSeconds(15)
    };

    #endregion

    #region Methods

    public static async Task ClearAllAsync(this IFusionCache cache, CancellationToken ct = default) => await cache.RemoveAsync("*", token: ct);

    public static void ClearAll(this IFusionCache cache) => cache.Remove("*");

    #endregion
}
