// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Common;

/// <summary> EF storage result </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed class TgEfStorageResult<TEfEntity> where TEfEntity : class, ITgEfEntity<TEfEntity>, new ()
{
	#region Public and private fields, properties, constructor

	public TgEnumEntityState State { get; set; }

	public TEfEntity? Item { get; set; }

	public IEnumerable<TEfEntity> Items { get; set; }

	public bool IsExists => State is TgEnumEntityState.IsExists or TgEnumEntityState.IsSaved;

	public TgEfStorageResult()
	{
		State = TgEnumEntityState.Unknown;
		Items = [];
	}

	public TgEfStorageResult(TgEnumEntityState state)
	{
		State = state;
		Items = [];
	}

	public TgEfStorageResult(TgEnumEntityState state, TEfEntity? item)
	{
		State = state;
		Item = item ?? new();
		Items = [];
	}

	public TgEfStorageResult(TgEnumEntityState state, IEnumerable<TEfEntity>? items)
	{
		State = state;
		Items = items ?? [];
	}

	#endregion

	#region Public and private methods

	public string ToDebugString() => Item is not null ? $"{State} | {Item.Uid} | {Items.Count()}" : $"{State} | {Items.Count()}";

	#endregion
}