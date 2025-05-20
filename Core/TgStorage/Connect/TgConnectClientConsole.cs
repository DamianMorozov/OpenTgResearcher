// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Connect;

/// <summary> Console connection client </summary>
public partial class TgConnectClientConsole : TgConnectClientBase, ITgConnectClientConsole
{
	public override async Task LoginUserAsync(bool isProxyUpdate)
	{
		ClientException = new();
		if (Client is null)
			return;

		try
		{
			if (Me is null || !Me.IsActive)
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
			//		//if (await ProxyRepository.GetCurrentProxyUidAsync(await AppRepository.GetCurrentAppAsync()) != app.ProxyUid)
			//		//{
			//		//	app.ProxyUid = await ProxyRepository.GetCurrentProxyUidAsync(await AppRepository.GetCurrentAppAsync());
			//		//	await AppRepository.SaveAsync(app);
			//		//}
			//		appResult.Item.ProxyUid = await ProxyRepository.GetCurrentProxyUidAsync(appResult);
			//		await AppRepository.SaveAsync(appResult.Item);
			//	}
			//}
			await AfterClientConnectAsync();
		}
	}
}
