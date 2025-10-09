namespace TgBusinessLogic.Services;

/// <summary> Blazor connection client </summary>
public sealed partial class TgConnectClientBlazor : TgConnectClientBase, ITgConnectClientBlazor
{
    public TgConnectClientBlazor(ITgStorageService storageManager, ITgFloodControlService floodControlService, IFusionCache cache, ILoadStateService loadStateService) : 
        base(storageManager, floodControlService, cache, loadStateService) { }

    public TgConnectClientBlazor(IWebHostEnvironment webHostEnvironment, ITgStorageService storageManager, ITgFloodControlService floodControlService,
        IFusionCache cache, LoadStateService loadStateService) : 
        base(webHostEnvironment, storageManager, floodControlService, cache, loadStateService)
    {
        //
    }
}
