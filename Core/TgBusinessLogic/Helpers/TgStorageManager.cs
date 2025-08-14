// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Helpers;

public sealed class TgStorageManager : TgWebDisposable, ITgStorageManager
{
    #region Public and private fields, properties, constructor

    private ILifetimeScope Scope { get; }
    /// <inheritdoc />
    public ITgEfContext EfContext { get; }
    /// <inheritdoc />
    public ITgEfAppRepository AppRepository { get; }
    /// <inheritdoc />
    public ITgEfUserRepository UserRepository { get; }
    /// <inheritdoc />
    public ITgEfDocumentRepository DocumentRepository { get; }
    /// <inheritdoc />
    public ITgEfFilterRepository FilterRepository { get; }
    /// <inheritdoc />
    public ITgEfLicenseRepository LicenseRepository { get; }
    /// <inheritdoc />
    public ITgEfMessageRepository MessageRepository { get; }
    /// <inheritdoc />
    public ITgEfMessageRelationRepository MessageRelationRepository { get; }
    /// <inheritdoc />
    public ITgEfProxyRepository ProxyRepository { get; }
    /// <inheritdoc />
    public ITgEfSourceRepository SourceRepository { get; }
    /// <inheritdoc />
    public ITgEfStoryRepository StoryRepository { get; }
    /// <inheritdoc />
    public ITgEfVersionRepository VersionRepository { get; }

    public TgStorageManager() : base()
    {
        Scope = TgGlobalTools.Container.BeginLifetimeScope();
        
        EfContext = Scope.Resolve<ITgEfContext>();
        AppRepository = Scope.Resolve<ITgEfAppRepository>();
        UserRepository = Scope.Resolve<ITgEfUserRepository>();
        DocumentRepository = Scope.Resolve<ITgEfDocumentRepository>();
        FilterRepository = Scope.Resolve<ITgEfFilterRepository>();
        LicenseRepository = Scope.Resolve<ITgEfLicenseRepository>();
        MessageRepository = Scope.Resolve<ITgEfMessageRepository>();
        MessageRelationRepository = Scope.Resolve<ITgEfMessageRelationRepository>();
        ProxyRepository = Scope.Resolve<ITgEfProxyRepository>();
        SourceRepository = Scope.Resolve<ITgEfSourceRepository>();
        StoryRepository = Scope.Resolve<ITgEfStoryRepository>();
        VersionRepository = Scope.Resolve<ITgEfVersionRepository>();
    }

    public TgStorageManager(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment)
    {
        Scope = TgGlobalTools.Container.BeginLifetimeScope();
        
        EfContext = Scope.Resolve<ITgEfContext>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
        AppRepository = Scope.Resolve<ITgEfAppRepository>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
        UserRepository = Scope.Resolve<ITgEfUserRepository>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
        DocumentRepository = Scope.Resolve<ITgEfDocumentRepository>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
        FilterRepository = Scope.Resolve<ITgEfFilterRepository>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
        LicenseRepository = Scope.Resolve<ITgEfLicenseRepository>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
        MessageRepository = Scope.Resolve<ITgEfMessageRepository>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
        MessageRelationRepository = Scope.Resolve<ITgEfMessageRelationRepository>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
        ProxyRepository = Scope.Resolve<ITgEfProxyRepository>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
        SourceRepository = Scope.Resolve<ITgEfSourceRepository>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
        StoryRepository = Scope.Resolve<ITgEfStoryRepository>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
        VersionRepository = Scope.Resolve<ITgEfVersionRepository>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
    }

    #endregion

    #region IDisposable

    /// <summary> Release managed resources </summary>
    public override void ReleaseManagedResources()
    {
        AppRepository.Dispose();
        UserRepository.Dispose();
        DocumentRepository.Dispose();
        FilterRepository.Dispose();
        LicenseRepository.Dispose();
        MessageRepository.Dispose();
        MessageRelationRepository.Dispose();
        ProxyRepository.Dispose();
        SourceRepository.Dispose();
        StoryRepository.Dispose();
        VersionRepository.Dispose();

        Scope.Dispose();
        EfContext.Dispose();
    }

    /// <summary> Release unmanaged resources </summary>
    public override void ReleaseUnmanagedResources()
    {
        //
    }

    #endregion
}
