namespace TgStorage.Domain.Apps;

/// <summary> EF app DTO </summary>
public sealed partial class TgEfAppDto : TgDtoBase, ITgEfAppDto
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

    /// <inheritdoc />
    public override string ToString() => $"{ApiHash} | {ApiId}";
	
    #endregion
}
