namespace TgBusinessLogic.Services;

/// <summary> Blazor connection client </summary>
public sealed partial class TgConnectClientBlazor : TgConnectClientBase, ITgConnectClientBlazor
{
    public TgConnectClientBlazor(ITgStorageService storageManager, ITgFloodControlService floodControlService, IFusionCache cache) : 
        base(storageManager, floodControlService, cache) { }

    public TgConnectClientBlazor(IWebHostEnvironment webHostEnvironment, ITgStorageService storageManager, ITgFloodControlService floodControlService,
        IFusionCache cache) : 
        base(webHostEnvironment, storageManager, floodControlService, cache)
    {
        //
    }
}
