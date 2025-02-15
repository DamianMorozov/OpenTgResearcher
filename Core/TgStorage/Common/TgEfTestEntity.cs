//// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
//// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

//namespace TgStorage.Common;

///// <summary> Common class to tests </summary>
//public sealed class TgEfTestEntity : ITgDbEntity, ITgDbFillEntity<TgEfTestEntity>
//{
//	#region Public and private fields, properties, constructor

//	[DefaultValue("00000000-0000-0000-0000-000000000000")]
//	[Key]
//	[Required]
//	[Column(TgEfConstants.ColumnUid, TypeName = "CHAR(36)")]
//	[SQLite.Collation("NOCASE")]
//	public Guid Uid { get; set; }

//	[Timestamp]
//	[Column(TgEfConstants.ColumnRowVersion)]
//	public byte[]? RowVersion { get; set; }
	
//	[DefaultValue(true)]
//	public bool BoolValue { get; set; }

//	[DefaultValue(TgEnumCommon.Some1)]
//	public TgEnumCommon EnumValue { get; set; }

//	[DefaultValue("Some string")]
//	[MaxLength(128)]
//	public string StringValue { get; set; } = null!;

//	[DefaultValue(-101)]
//	public short ShortValue { get; set; }

//	[DefaultValue(101)]
//	public ushort UshortValue { get; set; }

//	[DefaultValue(-201)]
//	public int IntValue { get; set; }

//	[DefaultValue(201)]
//	public uint UintValue { get; set; }

//	[DefaultValue(-301)]
//	public long LongValue { get; set; }

//	[DefaultValue(301)]
//	public ulong UlongValue { get; set; }

//	[DefaultValue("2001-02-03 11:22:33")]
//	public DateTime DtValue { get; set; }

//	/// <summary> Default constructor </summary>
//	public TgEfTestEntity()
//	{
//		Default();
//	}

//	#endregion

//	#region Public and private methods

//	public string ToDebugString() => $"{Uid}";

//	private void Default()
//	{
//		Uid = this.GetDefaultPropertyGuid(nameof(Uid));
//		BoolValue = this.GetDefaultPropertyBool(nameof(BoolValue));
//		EnumValue = this.GetDefaultPropertyGeneric<TgEnumCommon>(nameof(EnumValue));
//		StringValue = this.GetDefaultPropertyString(nameof(StringValue));
//		ShortValue = this.GetDefaultPropertyShort(nameof(ShortValue));
//		UshortValue = this.GetDefaultPropertyUshort(nameof(UshortValue));
//		IntValue = this.GetDefaultPropertyInt(nameof(IntValue));
//		UintValue = this.GetDefaultPropertyUint(nameof(UintValue));
//		LongValue = this.GetDefaultPropertyLong(nameof(LongValue));
//		UlongValue = this.GetDefaultPropertyUlong(nameof(UlongValue));
//		DtValue = this.GetDefaultPropertyDateTime(nameof(DtValue));
//	}

//	public TgEfTestEntity Fill(TgEfTestEntity item, bool isUidCopy)
//	{
//		if (isUidCopy)
//			Uid = item.Uid;
//		BoolValue = item.BoolValue;
//		EnumValue = item.EnumValue;
//		StringValue = item.StringValue;
//		ShortValue = item.ShortValue;
//		UshortValue = item.UshortValue;
//		IntValue = item.IntValue;
//		UintValue = item.UintValue;
//		LongValue = item.LongValue;
//		UlongValue = item.UlongValue;
//		DtValue = item.DtValue;
//		return this;
//	}

//	#endregion
//}