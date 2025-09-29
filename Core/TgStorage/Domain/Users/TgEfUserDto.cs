namespace TgStorage.Domain.Users;

/// <summary> EF user DTO </summary>
public sealed partial class TgEfUserDto : TgDtoBase, ITgEfUserDto
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial DateTime DtChanged { get; set; }
    public string DtChangedString => $"{DtChanged:yyyy-MM-dd HH:mm:ss}";
    [ObservableProperty]
    public partial long Id { get; set; }
    [ObservableProperty]
    public partial long AccessHash { get; set; }
    [ObservableProperty]
    public partial bool IsUserActive { get; set; }
    [ObservableProperty]
    public partial bool IsBot { get; set; }
    [ObservableProperty]
    public partial TimeSpan LastSeenAgo { get; set; }
    public string LastSeenAgoAsString => TgDtUtils.FormatLastSeenAgo(LastSeenAgo);
    [ObservableProperty]
    public partial string FirstName { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string LastName { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string UserName { get; set; } = string.Empty;
    public string MainUserName => string.IsNullOrEmpty(UserName) ? string.Empty : $"@{UserName}";
    [ObservableProperty]
    public partial string UserNames { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string PhoneNumber { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string Status { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string RestrictionReason { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string LangCode { get; set; } = string.Empty;
    [ObservableProperty]
    public partial bool IsContact { get; set; }
    [ObservableProperty]
    public partial bool IsDeleted { get; set; }
    [ObservableProperty]
    public partial int StoriesMaxId { get; set; }
    [ObservableProperty]
    public partial string BotInfoVersion { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string BotInlinePlaceholder { get; set; } = string.Empty;
    [ObservableProperty]
    public partial int BotActiveUsers { get; set; }
    [ObservableProperty]
    public partial bool IsDownload { get; set; }
    [ObservableProperty]
    public partial int MessagesCount { get; set; }

    public string DisplayName => !string.IsNullOrWhiteSpace(FirstName) || !string.IsNullOrWhiteSpace(LastName) ? $"{FirstName} {LastName}".Trim(): UserName;

    public TgEfUserDto() : base()
    {
        DtChanged = DateTime.MinValue;
        Id = 0;
        AccessHash = 0;
        IsUserActive = false;
        IsBot = false;
        LastSeenAgo = TimeSpan.Zero;
        FirstName = string.Empty;
        LastName = string.Empty;
        UserName = string.Empty;
        UserNames = string.Empty;
        PhoneNumber = string.Empty;
        Status = string.Empty;
        RestrictionReason = string.Empty;
        LangCode = string.Empty;
        IsContact = false;
        IsDeleted = false;
        StoriesMaxId = 0;
        BotInfoVersion = string.Empty;
        BotInlinePlaceholder = string.Empty;
        BotActiveUsers = 0;
        IsDownload = false;
        MessagesCount = 0;
    }

    #endregion

    #region Private methods

    /// <inheritdoc />
    public override string ToString() => $"{Id} | {AccessHash}";

    /// <inheritdoc />
    public override string ToConsoleString()
    {
        var name = TgDataFormatUtils.GetFormatString(FirstName, 30).TrimEnd();
        name += TgDataFormatUtils.GetFormatString(LastName, 30).TrimEnd();
        return
            $"{Id,11} | " +
            $"{TgDataFormatUtils.GetFormatString(UserName, 25).TrimEnd(),-25} | " +
            $"{(IsActive ? "active" : ""),-6} | " +
            $"{TgDataFormatUtils.GetFormatString(PhoneNumber, 11).TrimEnd(),-11} | " +
            $"{name,-40}";
    }

    /// <inheritdoc />
    public override string ToConsoleHeaderString() =>
        $"{nameof(Id),11} | " +
        $"{TgDataFormatUtils.GetFormatString(nameof(UserName), 25).TrimEnd(),-25} | " +
        $"Active | " +
        $"{TgDataFormatUtils.GetFormatString(nameof(PhoneNumber), 11).TrimEnd(),-11} | " +
        $"Name";

    public TgEfUserDto SetCountMessages(int messagesCount)
    {
        MessagesCount = messagesCount;
        return this;
    }

    public string GetShortStatus(string status) => status switch
    {
        nameof(TL.UserStatusLastMonth) => "LastMonth",
        "TL." + nameof(TL.UserStatusLastMonth) => "LastMonth",
        nameof(TL.UserStatusLastWeek) => "LastWeek",
        "TL." + nameof(TL.UserStatusLastWeek) => "LastWeek",
        nameof(TL.UserStatusOffline) => "Offline",
        "TL." + nameof(TL.UserStatusOffline) => "Offline",
        nameof(TL.UserStatusOnline) => "Online",
        "TL." + nameof(TL.UserStatusOnline) => "Online",
        nameof(TL.UserStatusRecently) => "Recently",
        "TL." + nameof(TL.UserStatusRecently) => "Recently",
        _ => status,
    };

    private string GetLongStatus(string status) => status switch
    {
        "LastMonth" => nameof(TL.UserStatusLastMonth),
        "LastWeek" => nameof(TL.UserStatusLastWeek),
        "Offline" => nameof(TL.UserStatusOffline),
        "Online" => nameof(TL.UserStatusOnline),
        "Recently" => nameof(TL.UserStatusRecently),
        _ => status,
    };

    #endregion
}
