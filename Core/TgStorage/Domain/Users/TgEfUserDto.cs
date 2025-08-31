// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Users;

/// <summary> User DTO </summary>
public sealed partial class TgEfUserDto : TgSensitiveDto, ITgDto<TgEfUserEntity, TgEfUserDto>
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
    public partial bool IsContactActive { get; set; }
    [ObservableProperty]
    public partial bool IsBot { get; set; }
    [ObservableProperty]
    public partial TimeSpan LastSeenAgo { get; set; }
    public string LastSeenAgoAsString => TgDtUtils.FormatLastSeenAgo(LastSeenAgo);
    [ObservableProperty]
    public partial string FirstName { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string LastName { get; set; } = string.Empty;
    public string FirstLastName => $"{FirstName}{LastName}";
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

    public TgEfUserDto() : base()
    {
        DtChanged = DateTime.MinValue;
        Id = 0;
        AccessHash = 0;
        IsContactActive = false;
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
    }

    #endregion

    #region Private methods

    public override string ToString() => $"{Id} | {AccessHash}";

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

    public override string ToConsoleHeaderString() =>
        $"{nameof(Id),11} | " +
        $"{TgDataFormatUtils.GetFormatString(nameof(UserName), 25).TrimEnd(),-25} | " +
        $"Active | " +
        $"{TgDataFormatUtils.GetFormatString(nameof(PhoneNumber), 11).TrimEnd(),-11} | " +
        $"Name";

    public TgEfUserDto Copy(TgEfUserDto dto, bool isUidCopy)
    {
        base.Copy(dto, isUidCopy);
        DtChanged = dto.DtChanged;
        Id = dto.Id;
        AccessHash = dto.AccessHash;
        IsContactActive = dto.IsContactActive;
        IsBot = dto.IsBot;
        FirstName = dto.FirstName;
        LastName = dto.LastName;
        UserName = dto.UserName;
        UserNames = dto.UserNames;
        PhoneNumber = dto.PhoneNumber;
        Status = dto.Status;
        RestrictionReason = dto.RestrictionReason;
        LangCode = dto.LangCode;
        IsContact = dto.IsContact;
        IsDeleted = dto.IsDeleted;
        StoriesMaxId = dto.StoriesMaxId;
        BotInfoVersion = dto.BotInfoVersion;
        BotInlinePlaceholder = dto.BotInlinePlaceholder;
        BotActiveUsers = dto.BotActiveUsers;
        IsDownload = dto.IsDownload;
        return this;
    }

    public TgEfUserDto Copy(TgEfUserEntity item, bool isUidCopy)
    {
        if (isUidCopy)
            Uid = item.Uid;
        DtChanged = item.DtChanged;
        Id = item.Id;
        AccessHash = item.AccessHash;
        IsContactActive = item.IsActive;
        IsBot = item.IsBot;
        FirstName = item.FirstName ?? string.Empty;
        LastName = item.LastName ?? string.Empty;
        UserName = item.UserName ?? string.Empty;
        UserNames = item.UserNames ?? string.Empty;
        PhoneNumber = item.PhoneNumber ?? string.Empty;
        Status = GetShortStatus(item.Status ?? string.Empty);
        RestrictionReason = item.RestrictionReason ?? string.Empty;
        LangCode = item.LangCode ?? string.Empty;
        IsContact = item.IsContact;
        IsDeleted = item.IsDeleted;
        StoriesMaxId = item.StoriesMaxId;
        BotInfoVersion = item.BotInfoVersion ?? string.Empty;
        BotInlinePlaceholder = item.BotInlinePlaceholder ?? string.Empty;
        BotActiveUsers = item.BotActiveUsers;
        IsDownload = false;
        return this;
    }

    public TgEfUserDto GetNewDto(TgEfUserEntity item) => new TgEfUserDto().Copy(item, isUidCopy: true);

    public TgEfUserEntity GetNewEntity(TgEfUserDto dto) => new()
    {
        Uid = dto.Uid,
        DtChanged = dto.DtChanged,
        Id = dto.Id,
        AccessHash = dto.AccessHash,
        IsActive = dto.IsContactActive,
        IsBot = dto.IsBot,
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        UserName = dto.UserName,
        UserNames = dto.UserNames,
        PhoneNumber = dto.PhoneNumber,
        Status = GetShortStatus(dto.Status),
        RestrictionReason = dto.RestrictionReason,
        LangCode = dto.LangCode,
        IsContact = dto.IsContact,
        IsDeleted = dto.IsDeleted,
        StoriesMaxId = dto.StoriesMaxId,
        BotInfoVersion = dto.BotInfoVersion,
        BotInlinePlaceholder = dto.BotInlinePlaceholder,
        BotActiveUsers = dto.BotActiveUsers,
    };

    public TgEfUserEntity GetNewEntity() => new()
    {
        Uid = Uid,
        DtChanged = DtChanged,
        Id = Id,
        AccessHash = AccessHash,
        IsActive = IsContactActive,
        IsBot = IsBot,
        FirstName = FirstName,
        LastName = LastName,
        UserName = UserName,
        UserNames = UserNames,
        PhoneNumber = PhoneNumber,
        Status = GetShortStatus(Status),
        RestrictionReason = RestrictionReason,
        LangCode = LangCode,
        IsContact = IsContact,
        IsDeleted = IsDeleted,
        StoriesMaxId = StoriesMaxId,
        BotInfoVersion = BotInfoVersion,
        BotInlinePlaceholder = BotInlinePlaceholder,
        BotActiveUsers = BotActiveUsers,
    };

    private string GetShortStatus(string status) => status switch
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

    public string GetDisplayName() =>
        !string.IsNullOrEmpty(FirstLastName) ? FirstLastName : UserName;

    #endregion
}
