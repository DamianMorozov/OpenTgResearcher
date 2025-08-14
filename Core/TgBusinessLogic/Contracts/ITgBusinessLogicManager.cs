// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Contracts;

public interface ITgBusinessLogicManager : IDisposable
{
    /// <summary> Storage manager for Telegram data </summary>
    public ITgStorageManager StorageManager { get; }
    /// <summary> License service for managing licenses </summary>
    public ITgLicenseService LicenseService { get; }
    /// <summary> Connect client for managing connections to Telegram </summary>
    public ITgConnectClient ConnectClient { get; }
    /// <summary> Backup storage </summary>
    public (bool IsSuccess, string FileName) BackupDb(string storagePath = "");
    /// <summary> Create and update storage </summary>
    public Task CreateAndUpdateDbAsync();
    /// <summary> Shrink storage </summary>
    public Task ShrinkDbAsync();
    /// <summary> Load storage table dtos </summary>
    public Task<ObservableCollection<TgStorageTableDto>> LoadStorageTableDtosAsync(string appsName, string chatsName, string contactsName, 
        string documentsName, string filtersName, string messagesName, string proxiesName, string storiesName, string versionsName);
    /// <summary> Load storage backup dtos </summary>
    public ObservableCollection<TgStorageBackupDto> LoadStorageBackupDtos(string storagePath = "");
    /// <summary> Remove duplicate messages from the database </summary>
    public Task RemoveDuplicateMessagesAsync();
    /// <summary> Remove duplicate messages from the database </summary>
    public Task RemoveDuplicateMessagesByDirectSqlAsync();
}
