// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Dtos;

/// <summary> Represents information about a Telegram bot, including its configuration and metadata </summary>
public sealed class TgBotInfoDto
{
    #region Public and private fields, properties, constructor

    public string Username { get; set; } = default!;
    public string Id { get; set; } = default!;
    public string AccessHash { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string IsBot { get; set; } = default!;
    public string IsPremium { get; set; } = default!;
    public string AddedToAttachmentMenu { get; set; } = default!;
    public string CanConnectToBusiness { get; set; } = default!;
    public string CanJoinGroups { get; set; } = default!;
    public string CanReadAllGroupMessages { get; set; } = default!;
    public string HasMainWebApp { get; set; } = default!;
    public string SupportsInlineQueries { get; set; } = default!;
    public bool IsTlUser { get; set; }
    public string IsActive { get; set; } = default!;
    public string LastSeenAgo { get; set; } = default!;
    public string MainUsername { get; set; } = default!;
    public string BotActiveUsers { get; set; } = default!;
    public string BotInfoVersion { get; set; } = default!;
    public string BotInlinePlaceholder { get; set; } = default!;
    public string Flags { get; set; } = default!;
    public int FloodRetryThreshold { get; set; }
    public bool IsMainDC { get; set; }
    public string MTProxyUrl { get; set; } = default!;
    public int MaxAutoReconnects { get; set; }
    public int MaxCodePwdAttempts { get; set; }
    public int PingInterval { get; set; }

    public TgBotInfoDto()
    {
        Username = string.Empty;
        Id = string.Empty;
        AccessHash = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        IsBot = string.Empty;
        IsPremium = string.Empty;
        AddedToAttachmentMenu = string.Empty;
        CanConnectToBusiness = string.Empty;
        CanJoinGroups = string.Empty;
        CanReadAllGroupMessages = string.Empty;
        HasMainWebApp = string.Empty;
        SupportsInlineQueries = string.Empty;
        IsTlUser = false;
        IsActive = string.Empty;
        LastSeenAgo = string.Empty;
        MainUsername = string.Empty;
        BotActiveUsers = string.Empty;
        BotInfoVersion = string.Empty;
        BotInlinePlaceholder = string.Empty;
        Flags = string.Empty;
        FloodRetryThreshold = 0;
        IsMainDC = false;
        MTProxyUrl = string.Empty;
        MaxAutoReconnects = 0;
        MaxCodePwdAttempts = 0;
        PingInterval = 0;
    }

    #endregion
}
