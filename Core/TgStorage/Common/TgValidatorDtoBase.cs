namespace TgStorage.Common;

/// <summary> Validator base </summary>
public class TgValidatorDtoBase<TEfEntity, TDto> : AbstractValidator<TDto>, ITgDebug
    where TEfEntity : class, ITgEfEntity<TEfEntity>, new()
    where TDto : class, ITgDto<TEfEntity, TDto>, new()
{
    #region Fields, properties, constructor

    public TgValidatorDtoBase()
	{
		RuleFor(item => item.Uid)
			.NotNull();
	}

    #endregion

    #region Methods

    public string ToDebugString() => string.Empty;

    #endregion
}
