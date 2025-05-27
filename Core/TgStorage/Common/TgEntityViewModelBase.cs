// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Common;

/// <summary> Base class for TgMvvmModel </summary>
[DebuggerDisplay("{ToDebugString()}")]
public abstract partial class TgEntityViewModelBase<TEfEntity, TDto> : TgViewModelBase
    where TEfEntity : class, ITgEfEntity<TEfEntity>, new()
    where TDto : class, ITgDto<TEfEntity, TDto>, new()
{
    #region Public and private fields, properties, constructor

    public virtual TgEfRepositoryBase<TEfEntity, TDto> Repository { get; } = null!;

    #endregion

    #region Public and private methods

    public override string ToDebugString() => $"{TgDataUtils.GetIsLoad(IsLoad)}";

    #endregion
}