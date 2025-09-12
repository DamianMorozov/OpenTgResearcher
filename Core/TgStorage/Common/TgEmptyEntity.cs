namespace TgStorage.Common;

/// <summary> Empty entity </summary>
public sealed class TgEmptyEntity : ITgEfEntity<TgEmptyEntity>
{
	#region Fields, properties, constructor

	[DefaultValue("00000000-0000-0000-0000-000000000000")]
	public Guid Uid { get; set; }

	/// <summary> Default constructor </summary>
	public TgEmptyEntity()
	{
		Default();
	}

	#endregion

	#region Methods

	public string ToDebugString() => $"{Uid}";

	private void Default()
	{
		Uid = this.GetDefaultPropertyGuid(nameof(Uid));
	}

	public TgEmptyEntity Copy(TgEmptyEntity item, bool isUidCopy)
	{
		if (isUidCopy)
			Uid = item.Uid;
		return this;
	}

	#endregion
}
