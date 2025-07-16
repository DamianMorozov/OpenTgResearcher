// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Users;

/// <summary> User DTO </summary>
public sealed partial class TgEfUserDto : TgDtoBase, ITgDto<TgEfUserEntity, TgEfUserDto>
{
    #region Public and private fields, properties, constructor

    [ObservableProperty]
    public partial string SensitiveData { get; set; } = "**********";
    [ObservableProperty]
	public partial bool IsDisplaySensitiveData { get; set; }
	[ObservableProperty]
	public partial DateTime DtChanged { get; set; }
    public string DtChangedString => $"{DtChanged:yyyy-MM-dd HH:mm:ss}";
    [ObservableProperty]
	public partial long Id { get; set; }
	[ObservableProperty]
	public partial bool IsContactActive { get; set; }
	[ObservableProperty]
	public partial bool IsBot { get; set; }
	[ObservableProperty]
	public partial TimeSpan LastSeenAgo { get; set; }
    public string LastSeenAgoAsString => TgDtUtils.FormatLastSeenAgo(LastSeenAgo);
    [ObservableProperty]
    public partial string UserName { get; set; } = string.Empty;
    public string MainUserName => string.IsNullOrEmpty(UserName) ? string.Empty : $"@{UserName}";
    [ObservableProperty]
    public partial long AccessHash { get; set; }
    [ObservableProperty]
	public partial string FirstName { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string LastName { get; set; } = string.Empty;
	public string FirstLastName => $"{FirstName}{LastName}";
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
	public partial bool IsDeleted { get; set; }

	public TgEfUserDto() : base()
	{
		DtChanged = DateTime.MinValue;
		Id = 0;
		IsContactActive = false;
		IsBot = false;
        LastSeenAgo = TimeSpan.Zero;
		UserName = string.Empty;
        AccessHash = 0;
		FirstName = string.Empty;
		LastName = string.Empty;
		UserNames = string.Empty;
        PhoneNumber = string.Empty;
		Status = string.Empty;
		RestrictionReason = string.Empty;
		LangCode = string.Empty;
		IsDeleted = false;
    }

	#endregion

	#region Private methods

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
		IsDeleted = dto.IsDeleted;
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
		IsDeleted = false;
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

	#endregion
}
