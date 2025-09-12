namespace TgStorage.Validators;

/// <summary> Proxy validator </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed class TgProxyDtoValidator : TgValidatorDtoBase<TgEfProxyEntity, TgEfProxyDto>
{
    #region Fields, properties, constructor

    public TgProxyDtoValidator()
    {
        RuleFor(item => item.Type)
            .NotNull();
        RuleFor(item => item.HostName)
            .NotEmpty()
            .NotNull();
        RuleFor(item => item.Port)
            .NotNull()
            .GreaterThan(ushort.MinValue)
            .LessThan(ushort.MaxValue);
    }

    #endregion
}
