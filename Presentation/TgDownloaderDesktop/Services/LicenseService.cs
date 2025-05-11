// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.Services;

public sealed class LicenseService() : ILicenseService
{
	#region Public and private fields, properties, constructor

	public TgLicenseManagerHelper TgLicenseManager { get; set; } = TgLicenseManagerHelper.Instance;
	private ITgEfLicenseRepository LicenseRepository { get; } = new TgEfLicenseRepository();

	#endregion

	#region Public and private methods

	public async Task LicenseActivateAsync()
	{
		var licenseDtos = await LicenseRepository.GetListDtosAsync();
		var currentLicenseDto = licenseDtos.FirstOrDefault(x => x.IsConfirmed && x.ValidTo >= DateTime.UtcNow);
		if (currentLicenseDto is not null)
			TgLicenseManager.ActivateLicense(currentLicenseDto.IsConfirmed, currentLicenseDto.LicenseKey,
				currentLicenseDto.LicenseType, currentLicenseDto.UserId, currentLicenseDto.ValidTo);
		else
			TgLicenseManager.ActivateDefaultLicense();
	}

	#endregion
}
