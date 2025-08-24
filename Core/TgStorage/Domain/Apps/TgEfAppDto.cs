// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Apps;

/// <summary> App DTO </summary>
public sealed partial class TgEfAppDto : TgDtoBase, ITgDto<TgEfAppEntity, TgEfAppDto>
{
	#region Fields, properties, constructor

	[ObservableProperty]
	public partial Guid ApiHash { get; set; }
	[ObservableProperty]
	public partial int ApiId { get; set; }
	[ObservableProperty]
	public partial string PhoneNumber { get; set; }
	[ObservableProperty]
	public partial Guid ProxyUid { get; set; }
	[ObservableProperty]
	public partial string FirstName { get; set; }
	[ObservableProperty]
	public partial string LastName { get; set; }
    [ObservableProperty]
    public partial bool UseBot { get; set; }
    [ObservableProperty]
    public partial bool UseClient { get; set; }
	[ObservableProperty]
	public partial string BotTokenKey { get; set; }

	public string ApiIdString
	{
		get => ApiId.ToString();
		set => ApiId = int.TryParse(value, out var apiId) ? apiId : 0;
	}

	public string ApiHashString
	{
		get => ApiHash.ToString();
		set => ApiHash = Guid.TryParse(value, out var apiHash) ? apiHash : Guid.Empty;
	}

	public TgEfAppDto() : base()
	{
		ApiHash = Guid.Empty;
		ApiId = 0;
		PhoneNumber = string.Empty;
		ProxyUid = Guid.Empty;
		FirstName = string.Empty;
		LastName = string.Empty;
		UseBot = false;
		BotTokenKey = string.Empty;
        UseClient = false;
    }

	#endregion

	#region Methods

	public override string ToString() => $"{ApiHash} | {ApiId}";
	
	public TgEfAppDto Copy(TgEfAppDto dto, bool isUidCopy)
	{
		base.Copy(dto, isUidCopy);
		ApiHash = dto.ApiHash;
		ApiId = dto.ApiId;
		FirstName = dto.FirstName;
		LastName = dto.LastName;
		PhoneNumber = dto.PhoneNumber;
		ProxyUid = dto.ProxyUid;
		UseBot = dto.UseBot;
		BotTokenKey = dto.BotTokenKey;
        UseClient = dto.UseClient;
        return this;
	}

	public TgEfAppDto Copy(TgEfAppEntity item, bool isUidCopy)
	{
		if (isUidCopy)
			Uid = item.Uid;
		ApiHash = item.ApiHash;
		ApiId = item.ApiId;
		FirstName = item.FirstName;
		LastName = item.LastName;
		PhoneNumber = item.PhoneNumber;
		ProxyUid = item.ProxyUid ?? Guid.Empty;
		UseBot = item.UseBot;
		BotTokenKey = item.BotTokenKey;
        UseClient = item.UseClient;
		return this;
	}

	public TgEfAppDto GetNewDto(TgEfAppEntity item) => new TgEfAppDto().Copy(item, isUidCopy: true);

    public TgEfAppEntity GetNewEntity(TgEfAppDto dto)
    {
        var item = new TgEfAppEntity()
        {
            Uid = dto.Uid,
            ApiHash = dto.ApiHash,
            ApiId = dto.ApiId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            ProxyUid = dto.ProxyUid,
            BotTokenKey = dto.BotTokenKey,
        };
        if (dto.UseBot)
            item.SetUseBot(dto.UseBot);
        else if (dto.UseClient)
            item.SetUseClient(dto.UseClient);
        return item;
    }

    public TgEfAppEntity GetNewEntity()
    {
        var item = new TgEfAppEntity()
        {
            Uid = Uid,
            ApiHash = ApiHash,
            ApiId = ApiId,
            FirstName = FirstName,
            LastName = LastName,
            PhoneNumber = PhoneNumber,
            ProxyUid = ProxyUid,
            BotTokenKey = BotTokenKey,
        };
        if (UseBot)
            item.SetUseBot(UseBot);
        else if (UseClient)
            item.SetUseClient(UseClient);
        return item;
    }

    #endregion
}
