namespace TgBusinessLogic.Models;

/// <summary> Публичный источник Telegram с признаком сохранения в локальном реестре </summary>
public sealed record TgPublicChatSearchResult(long Id, string Title, string UserName, bool IsGroup, Guid? SourceUid,
    int? ParticipantsCount = null, bool IsVerified = false);

/// <summary> Информация публичного источника без добавления в локальное хранилище </summary>
public sealed record TgPublicChatProfile(string About, int? ParticipantsCount, string[] UserNames, string? DiscussionLink);

/// <summary> Счётчик доступных сообщений выбранного типа из Telegram </summary>
public sealed record TgPublicChatMediaCount(string Kind, int Count, bool IsApproximate);
