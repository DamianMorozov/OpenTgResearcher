// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Common;

/// <summary> Base class for TgMvvmModel </summary>
[DebuggerDisplay("{ToDebugString()}")]
public abstract partial class TgEntityViewModelBase<TEntity> : TgViewModelBase where TEntity : class, ITgDbFillEntity<TEntity>, new()
{
	#region Public and private fields, properties, constructor

	public virtual TgEfRepositoryBase<TEntity> Repository { get; } = null!;

	#endregion

	#region Public and private methods

	public override string ToDebugString() => $"{TgCommonUtils.GetIsLoad(IsLoad)}";

    #endregion
}