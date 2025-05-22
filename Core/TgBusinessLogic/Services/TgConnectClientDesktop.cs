// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com


namespace TgBusinessLogic.Services;

/// <summary> Desktop connection client </summary>
public sealed partial class TgConnectClientDesktop : TgConnectClientBase, ITgConnectClientDesktop
{
    public TgConnectClientDesktop(TgStorageManager storageManager) : base(storageManager)
    {
		//
    }

    public override async Task LoginUserAsync(bool isProxyUpdate)
	{
		ClientException = new();
		if (Client is null)
			return;

		try
		{
			Me = await Client.LoginUserIfNeeded();
			await UpdateStateSourceAsync(0, 0, 0, string.Empty);
		}
		catch (Exception ex)
		{
			await SetClientExceptionAsync(ex);
			Me = null;
		}
		finally
		{
			await CheckClientIsReadyAsync();
			//if (isProxyUpdate && IsReady)
			//{
			//	var appResult = await AppRepository.GetCurrentAppAsync();
			//	if (appResult.IsExists)
			//	{
			//		appResult.Item.ProxyUid = await ProxyRepository.GetCurrentProxyUidAsync(await AppRepository.GetCurrentAppAsync());
			//		await AppRepository.SaveAsync(appResult.Item);
			//	}
			//}
			await AfterClientConnectAsync();
		}
	}
}
