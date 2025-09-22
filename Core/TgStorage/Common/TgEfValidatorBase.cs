namespace TgStorage.Common;

/// <summary> Validator base </summary>
public class TgEfValidatorBase<TEfEntity> : AbstractValidator<TEfEntity>, ITgDebug
	where TEfEntity : class, ITgEfEntity, new()
{
    #region Fields, properties, constructor

    public TgEfValidatorBase() => RuleFor(item => item.Uid).NotNull();

    #endregion

    #region Methods

    public string ToDebugString() => string.Empty;

    #endregion
}
