namespace TgStorage.Validators;

/// <summary> Document validator </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed class TgEfChatUserValidator : TgEfValidatorBase<TgEfChatUserEntity>
{
    #region Fields, properties, constructor

    public TgEfChatUserValidator()
    {
        RuleFor(item => item.ChatId)
            .NotNull()
            .GreaterThanOrEqualTo(0);
        RuleFor(item => item.UserId)
            .NotNull()
            .GreaterThanOrEqualTo(0);
        RuleFor(item => item.Role)
            .NotNull();
        RuleFor(item => item.JoinedAt)
            .NotNull();
    }

    #endregion
}
