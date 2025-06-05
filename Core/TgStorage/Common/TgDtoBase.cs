// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Common;

/// <summary> Base DTO </summary>
[DebuggerDisplay("{ToDebugString()}")]
public partial class TgDtoBase : ObservableRecipient
{
	#region Public and private fields, properties, constructor

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

    #region Public and private methods

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