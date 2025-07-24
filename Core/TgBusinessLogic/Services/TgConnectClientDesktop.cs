// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Services;

/// <summary> Desktop connection client </summary>
public sealed partial class TgConnectClientDesktop(ITgStorageManager storageManager) : TgConnectClientBase(storageManager), ITgConnectClientDesktop
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
                await UpdateStateSourceAsync(0, 0, 0, string.Empty);
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
