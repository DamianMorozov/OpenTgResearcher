namespace TgStorage.Common;

/// <summary> Base class for TgMvvmModel </summary>
[DebuggerDisplay("{ToDebugString()}")]
public abstract partial class TgEntityViewModelBase<TEfEntity, TDto> : TgViewModelBase
    where TEfEntity : class, ITgEfEntity, new()
    where TDto : class, ITgDto, new()
{
    #region Fields, properties, constructor

    /// <inheritdoc />
    public virtual ITgEfRepository<TEfEntity, TDto> Repository { get; } = null!;

    #endregion

    #region Methods

    /// <inheritdoc />
    public override string ToDebugString() => $"{TgDataUtils.GetIsLoad(IsLoad)}";

    #endregion
}
