// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Contracts;

public interface ITgStorageManager : IDisposable
{
    /// <summary> EF context and repositories for Telegram data storage </summary>
    public ITgEfContext EfContext { get; }
    /// <summary> Storage file path </summary>
    public string StoragePath { get; }
    /// <summary> Application repository for managing application settings and configurations </summary>
    public ITgEfAppRepository AppRepository { get; }
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
}
