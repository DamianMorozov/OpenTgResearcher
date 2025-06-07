// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Common;

/// <summary> Validator base </summary>
public class TgValidatorDtoBase<TEfEntity, TDto> : AbstractValidator<TDto>, ITgDebug
    where TEfEntity : class, ITgEfEntity<TEfEntity>, new()
    where TDto : class, ITgDto<TEfEntity, TDto>, new()
{
    #region Public and private fields, properties, constructor

    public TgValidatorDtoBase()
	{
		RuleFor(item => item.Uid)
			.NotNull();
	}

    #endregion

    #region Public and private methods

    public string ToDebugString() => string.Empty;

    #endregion
}