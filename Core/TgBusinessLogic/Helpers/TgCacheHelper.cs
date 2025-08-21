// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Helpers;

/// <summary> Fusion cache helper </summary>
public static class TgCacheHelper
{
    #region Public and private fields, properties, constructor

    public static string GetCacheKeyChat() => $"chat";
    public static string GetCacheKeyChatLastCount() => $"chat:last-count";
    public static string GetCacheKeyChatLastCount(long chatId) => $"chat:last-count:{chatId}";
    public static string GetCacheKeyFullChannel() => $"fullChannel";
    public static string GetCacheKeyFullChannel(long peerId) => $"fullChannel:{peerId}";
    public static string GetCacheKeyFullChat() => $"fullChat";
    public static string GetCacheKeyFullChat(long peerId) => $"fullChat:{peerId}";
    public static string GetCacheKeyMessage() => $"message";
    public static string GetCacheKeyMessage(long peerId, int messageId) => $"message:{peerId}:{messageId}";
    public static string GetCacheKeyMessages() => $"messages";
    public static string GetCacheKeyMessages(long peerId, int messageIdStart, int messageIdEnd) => $"messages:{peerId}:{messageIdStart}-{messageIdEnd}";
    public static string GetCacheKeyStoryDb() => $"story:db";
    public static string GetCacheKeyStoryDb(long peerId, long storyId) => $"story:db:{peerId}:{storyId}";
    public static string GetCacheKeyStoryProc() => $"story:proc";
    public static string GetCacheKeyStoryProc(long peerId, long storyId) => $"story:proc:{peerId}:{storyId}";
    public static string GetCacheKeyUserDb() => $"user:db";
    public static string GetCacheKeyUserDb(long id) => $"user:db:{id}";
    public static string GetCacheKeyUserProc() => $"user:proc";
    public static string GetCacheKeyUserProc(long id) => $"user:proc:{id}";

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
}
