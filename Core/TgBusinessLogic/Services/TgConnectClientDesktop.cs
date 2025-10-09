namespace TgBusinessLogic.Services;

/// <summary> Desktop connection client </summary>
public sealed partial class TgConnectClientDesktop(ITgStorageService storageManager, ITgFloodControlService floodControlService, IFusionCache cache, ILoadStateService loadStateService) : 
    TgConnectClientBase(storageManager, floodControlService, cache, loadStateService), ITgConnectClientDesktop
{
    public override async Task LoginUserAsync(bool isProxyUpdate)
	{
		ClientException = new();
        var appDto = await StorageManager.AppRepository.GetCurrentDtoAsync();
        if (appDto.UseClient)
        {
            if (Client is null)
                return;
            try
            {
                Me = await Client.LoginUserIfNeeded();
            }
            catch (Exception ex)
            {
                await SetClientExceptionAsync(ex);
                Me = null;
            }
            finally
            {
                await CheckClientConnectionReadyAsync();
                await AfterClientConnectAsync();
            }
        }
        else if (appDto.UseBot)
        {
            //
        }
	}
}
