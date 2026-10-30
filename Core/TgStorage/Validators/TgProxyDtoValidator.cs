namespace TgStorage.Validators;

/// <summary> Проверка DTO по тем же правилам, что и при сохранении прокси </summary>
public sealed class TgProxyDtoValidator : TgValidatorDtoBase<TgEfProxyEntity, TgEfProxyDto>
{
    public TgProxyDtoValidator()
    {
        RuleFor(x => x).Custom((dto, context) =>
        {
            var result = new TgEfProxyValidator().Validate(TgEfDomainUtils.CreateNewEntity(dto, isUidCopy: true));
            foreach (var error in result.Errors) context.AddFailure(error);
        });
    }
}
