// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Contacts;

/// <summary> Contact DTO </summary>
public sealed partial class TgEfContactDto : TgDtoBase, ITgDto<TgEfContactEntity, TgEfContactDto>
{
	#region Public and private fields, properties, constructor

	[ObservableProperty]
	public partial DateTime DtChanged { get; set; }
	[ObservableProperty]
	public partial long Id { get; set; }
	[ObservableProperty]
	public partial long AccessHash { get; set; }
	[ObservableProperty]
	public partial bool IsContactActive { get; set; }
	[ObservableProperty]
	public partial bool IsBot { get; set; }
	[ObservableProperty]
	public partial string FirstName { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string LastName { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string UserName { get; set; } = string.Empty;
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
	public partial int StoriesMaxId { get; set; }
	[ObservableProperty]
	public partial string BotInfoVersion { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string BotInlinePlaceholder { get; set; } = string.Empty;
	[ObservableProperty]
	public partial int BotActiveUsers { get; set; }

	[ObservableProperty]
	public partial bool IsDownload { get; set; }
	public bool IsReady => Id > 0;

	#endregion

	#region Private methods

	public string DtChangedString => $"{DtChanged:yyyy-MM-dd HH:mm:ss}";

	public override string ToString() => $"{Id} | {AccessHash}";

	public TgEfContactDto Copy(TgEfContactDto dto, bool isUidCopy)
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
		StoriesMaxId = dto.StoriesMaxId;
		BotInfoVersion = dto.BotInfoVersion;
		BotInlinePlaceholder = dto.BotInlinePlaceholder;
		BotActiveUsers = dto.BotActiveUsers;
		IsDownload = dto.IsDownload;
		return this;
	}

	public TgEfContactDto Copy(TgEfContactEntity item, bool isUidCopy)
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
		StoriesMaxId = item.StoriesMaxId;
		BotInfoVersion = item.BotInfoVersion ?? string.Empty;
		BotInlinePlaceholder = item.BotInlinePlaceholder;
		BotActiveUsers = item.BotActiveUsers;
		IsDownload = false;
		return this;
	}

	public TgEfContactDto GetNewDto(TgEfContactEntity item) => new TgEfContactDto().Copy(item, isUidCopy: true);

	public TgEfContactEntity GetNewEntity(TgEfContactDto dto) => new()
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
		StoriesMaxId = dto.StoriesMaxId,
		BotInfoVersion = dto.BotInfoVersion,
		BotInlinePlaceholder = dto.BotInlinePlaceholder,
		BotActiveUsers = dto.BotActiveUsers,
	};

	public TgEfContactEntity GetNewEntity() => new()
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

	#endregion
}
