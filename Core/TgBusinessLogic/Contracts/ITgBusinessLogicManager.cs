// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

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
