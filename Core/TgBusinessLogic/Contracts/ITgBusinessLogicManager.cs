// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Contracts;

public interface ITgBusinessLogicManager : IDisposable
{
    public ITgStorageManager StorageManager { get; }
    public ITgLicenseService LicenseService { get; }
    public ITgConnectClient ConnectClient { get; }
    /// <summary> Backup storage </summary>
    (bool IsSuccess, string FileName) BackupDb(string storagePath = "");
    /// <summary> Create and update storage </summary>
    Task CreateAndUpdateDbAsync();
    /// <summary> Shrink storage </summary>
    Task ShrinkDbAsync();
    /// <summary> Load storage table dtos </summary>
    Task<ObservableCollection<TgStorageTableDto>> LoadStorageTableDtosAsync(string appsName, string chatsName, string contactsName, 
        string documentsName, string filtersName, string messagesName, string proxiesName, string storiesName, string versionsName);
    /// <summary> Load storage backup dtos </summary>
    ObservableCollection<TgStorageBackupDto> LoadStorageBackupDtos(string storagePath = "");
}