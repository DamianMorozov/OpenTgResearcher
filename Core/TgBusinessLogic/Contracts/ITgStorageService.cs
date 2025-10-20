using TL;

namespace TgBusinessLogic.Contracts;

public interface ITgStorageService : IDisposable
{
    /// <summary> EF context and repositories for Telegram data storage </summary>
    public ITgEfContext EfContext { get; }
    /// <summary> Storage file path </summary>
    public string StoragePath { get; }
    /// <summary> Application repository for managing application settings and configurations </summary>
    public ITgEfAppRepository AppRepository { get; }
    /// <summary> Chat user repository in the Telegram storage </summary>
    public ITgEfChatUserRepository ChatUserRepository { get; }
    /// <summary> Document repository for managing documents in the Telegram storage </summary>
    public ITgEfDocumentRepository DocumentRepository { get; }
    /// <summary> Filter repository for managing filters in the Telegram storage </summary>
    public ITgEfFilterRepository FilterRepository { get; }
    /// <summary> License repository for managing licenses in the Telegram storage </summary>
    public ITgEfLicenseRepository LicenseRepository { get; }
    /// <summary> Message repository for managing messages in the Telegram storage </summary>
    public ITgEfMessageRepository MessageRepository { get; }
    /// <summary> Message relation repository for managing message relations in the Telegram storage </summary>
    public ITgEfMessageRelationRepository MessageRelationRepository { get; }
    /// <summary> Proxy repository for managing proxies in the Telegram storage </summary>
    public ITgEfProxyRepository ProxyRepository { get; }
    /// <summary> Source repository for managing sources in the Telegram storage </summary>
    public ITgEfSourceRepository SourceRepository { get; }
    /// <summary> Story repository for managing stories in the Telegram storage </summary>
    public ITgEfStoryRepository StoryRepository { get; }
    /// <summary> User repository for managing users in the Telegram storage </summary>
    public ITgEfUserRepository UserRepository { get; }
    /// <summary> Version repository for managing version information in the Telegram storage </summary>
    public ITgEfVersionRepository VersionRepository { get; }
    /// <summary> Create or get a story entity based on peer ID and story item </summary>
    public Task<TgEfStoryDto> CreateOrGetStoryAsync(long peerId, StoryItem story);
    /// <summary> Create or get a user entity based on the Telegram user and contact status </summary>
    public Task<TgEfUserDto> CreateOrGetUserAsync(User user, bool isContact, bool isSave, CancellationToken ct = default);
    /// <summary> Check if table exists in the database </summary>
    Task<bool> CheckTableExistsAsync(string tableName = "");
    /// <summary> Remove duplicate messages from the database </summary>
    public Task RemoveDuplicateMessagesAsync();
    /// <summary> Remove duplicate messages from the database </summary>
    public Task RemoveDuplicateMessagesByDirectSqlAsync();
    /// <summary> Backup storage </summary>
    public (bool IsSuccess, string FileName) BackupDb(string storagePath = "");
    /// <summary> Create and update storage </summary>
    public Task CreateAndUpdateDbAsync();
    /// <summary> Shrink storage </summary>
    public Task ShrinkDbAsync();
    /// <summary> Load storage table dtos </summary>
    public Task<ObservableCollection<TgStorageTableDto>> LoadStorageTableDtosAsync(string appsName, string chatsName, string contactsName, string chatUsersName,
        string documentsName, string filtersName, string messagesName, string proxiesName, string storiesName, string versionsName);
    /// <summary> Load storage backup dtos </summary>
    public ObservableCollection<TgStorageBackupDto> LoadStorageBackupDtos(string storagePath = "");
}
