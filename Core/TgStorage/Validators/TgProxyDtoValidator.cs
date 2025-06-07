// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Validators;

/// <summary> Proxy validator </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed class TgProxyDtoValidator : TgValidatorDtoBase<TgEfProxyEntity, TgEfProxyDto>
{
    #region Public and private fields, properties, constructor

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