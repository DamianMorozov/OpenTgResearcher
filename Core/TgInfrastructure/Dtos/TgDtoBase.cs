namespace TgInfrastructure.Dtos;

/// <summary> Base DTO </summary>
[DebuggerDisplay("{ToDebugString()}")]
public partial class TgDtoBase : ObservableRecipient
{
	#region Fields, properties, constructor

	[ObservableProperty]
	public partial bool IsLoad { get; set; }
	[ObservableProperty]
	public partial Guid Uid { get; set; }
	[ObservableProperty]
	public partial bool IsExistsAtStorage { get; set; }

	public TgDtoBase()
	{
		IsLoad = false;
		Uid = Guid.Empty;
		IsExistsAtStorage = false;
	}

    #endregion

    #region Methods

    public virtual string ToDebugString() => TgObjectUtils.ToDebugString(this);

    public override string ToString() => ToDebugString();

    public virtual string ToConsoleString() => ToDebugString();
    
    public virtual string ToConsoleHeaderString() => string.Empty;

    protected TgDtoBase Copy(TgDtoBase dto, bool isUidCopy)
	{
		IsLoad = dto.IsLoad;
		if (isUidCopy)
			Uid = dto.Uid;
		IsExistsAtStorage = dto.IsExistsAtStorage;
		return this;
	}

	#endregion
}
