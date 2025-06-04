// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Contracts;

public interface ITgBusinessLogicManager : IDisposable
{
    public ITgStorageManager StorageManager { get; }
    public ITgLicenseService LicenseService { get; }
    public ITgConnectClient ConnectClient { get; }

    (bool IsSuccess, string FileName) BackupDbAsync();
    Task CreateAndUpdateDbAsync();
    Task ShrinkDbAsync();
}