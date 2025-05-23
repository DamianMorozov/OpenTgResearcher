// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Contracts;

public interface ITgStorageManager : IDisposable
{
    public ITgEfContext EfContext { get; }
    public ITgEfAppRepository AppRepository { get; }
    public ITgEfContactRepository ContactRepository { get; }
    public ITgEfDocumentRepository DocumentRepository { get; }
    public ITgEfFilterRepository FilterRepository { get; }
    public ITgEfLicenseRepository LicenseRepository { get; }
    public ITgEfMessageRepository MessageRepository { get; }
    public ITgEfProxyRepository ProxyRepository { get; }
    public ITgEfSourceRepository SourceRepository { get; }
    public ITgEfStoryRepository StoryRepository { get; }
    public ITgEfVersionRepository VersionRepository { get; }
}