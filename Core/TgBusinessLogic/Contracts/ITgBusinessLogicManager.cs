namespace TgBusinessLogic.Contracts;

public interface ITgBusinessLogicManager : IDisposable
{
    /// <summary> Storage manager for Telegram data </summary>
    public ITgStorageService StorageManager { get; }
    /// <summary> License service for managing licenses </summary>
    public ITgLicenseService LicenseService { get; }
    /// <summary> Connect client for managing connections to Telegram </summary>
    public ITgConnectClient ConnectClient { get; }
    /// <summary> Flood control service for managing flood control </summary>
    public ITgFloodControlService FloodControlService { get; }
    /// <summary> FusionCache </summary>
    public IFusionCache Cache { get; }
}
