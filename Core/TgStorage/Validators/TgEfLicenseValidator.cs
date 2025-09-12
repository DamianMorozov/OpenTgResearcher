namespace TgStorage.Validators;

public sealed class TgEfLicenseValidator : TgEfValidatorBase<TgEfLicenseEntity>
{
	public TgEfLicenseValidator()
	{
		RuleFor(x => x.LicenseKey)
			.NotEmpty()
			.WithMessage("License key required");

		RuleFor(x => x.UserId)
			.GreaterThan(0)
			.WithMessage("Invalid user ID");

		//RuleFor(x => x.ValidTo)
		//	.GreaterThan(DateTime.UtcNow)
		//	.WithMessage("License expiration date must be in future");
	}
}
