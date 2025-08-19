// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Services;

/// <summary> Blazor connection client </summary>
public sealed partial class TgConnectClientBlazor : TgConnectClientBase, ITgConnectClientBlazor
{
    public TgConnectClientBlazor(ITgStorageManager storageManager, ITgFloodControlService floodControlService, IFusionCache cache) : 
        base(storageManager, floodControlService, cache) { }

    public TgConnectClientBlazor(IWebHostEnvironment webHostEnvironment, ITgStorageManager storageManager, ITgFloodControlService floodControlService,
        IFusionCache cache) : 
        base(webHostEnvironment, storageManager, floodControlService, cache)
    {
        //
    }
}
