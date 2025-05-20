// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

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