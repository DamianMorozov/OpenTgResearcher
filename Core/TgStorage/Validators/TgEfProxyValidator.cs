namespace TgStorage.Validators;

/// <summary> Проверка настроек прокси перед сохранением </summary>
public sealed class TgEfProxyValidator : TgEfValidatorBase<TgEfProxyEntity>
{
    public TgEfProxyValidator()
    {
        RuleFor(x => x.Type).IsInEnum().WithMessage("Выберите поддерживаемый тип прокси");
        RuleFor(x => x.HostName).NotNull().MaximumLength(128).WithMessage("Адрес прокси не должен превышать 128 символов");
        RuleFor(x => x.HostName).NotEmpty().Must(x => Uri.CheckHostName(x) != UriHostNameType.Unknown)
            .WithMessage("Введите имя сервера или IP-адрес без схемы и пути").When(x => x.Type != TgEnumProxyType.None);
        RuleFor(x => x.Port).GreaterThan((ushort)0).WithMessage("Порт должен быть от 1 до 65535").When(x => x.Type != TgEnumProxyType.None);
        RuleFor(x => x.UserName).NotNull().MaximumLength(128).WithMessage("Имя пользователя не должно превышать 128 символов");
        RuleFor(x => x.Password).NotNull().MaximumLength(128).WithMessage("Пароль не должен превышать 128 символов");
        RuleFor(x => x.Secret).NotNull().MaximumLength(128).WithMessage("Секрет не должен превышать 128 символов");
        RuleFor(x => x.Secret).NotEmpty().WithMessage("Для MTProto необходимо указать секрет").When(x => x.Type == TgEnumProxyType.MtProto);
    }
}
