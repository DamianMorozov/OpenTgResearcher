// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Common;

/// <summary> Empty entity </summary>
public sealed class TgEmptyEntity : ITgDbEntity, ITgDbFillEntity<TgEmptyEntity>
{
	#region Public and private fields, properties, constructor

	[DefaultValue("00000000-0000-0000-0000-000000000000")]
	public Guid Uid { get; set; }

	[Timestamp]
	[Column(TgEfConstants.ColumnRowVersion)]
	public byte[]? RowVersion { get; set; }
	
	/// <summary> Default constructor </summary>
	public TgEmptyEntity()
	{
		Default();
	}

	#endregion

	#region Public and private methods

	public string ToDebugString() => $"{Uid}";

	private void Default()
	{
		Uid = this.GetDefaultPropertyGuid(nameof(Uid));
	}

	public TgEmptyEntity Fill(TgEmptyEntity item, bool isUidCopy)
	{
		if (isUidCopy)
			Uid = item.Uid;
		return this;
	}

	#endregion
}