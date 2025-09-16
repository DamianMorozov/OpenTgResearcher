namespace TgStorage.Common;

/// <summary> Base class for TgMvvmModel </summary>
[DebuggerDisplay("{ToDebugString()}")]
public abstract partial class TgEntityViewModelBase<TEfEntity, TDto> : TgViewModelBase
    where TEfEntity : class, ITgEfEntity<TEfEntity>, new()
    where TDto : class, ITgDto<TEfEntity, TDto>, new()
{
    #region Fields, properties, constructor

    public virtual ITgEfRepository<TEfEntity, TDto> Repository { get; } = null!;

    #endregion

    #region Methods

    public override string ToDebugString() => $"{TgDataUtils.GetIsLoad(IsLoad)}";

    #endregion
}
