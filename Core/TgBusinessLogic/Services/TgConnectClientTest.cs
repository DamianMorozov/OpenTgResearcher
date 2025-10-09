namespace TgBusinessLogic.Services;

/// <summary> Test connection client </summary>
public sealed partial class TgConnectClientTest(ITgStorageService storageManager, ITgFloodControlService floodControlService, IFusionCache cache, ILoadStateService loadStateService) : 
    TgConnectClientBase(storageManager, floodControlService, cache, loadStateService), ITgConnectClientTest
{
    //
}
