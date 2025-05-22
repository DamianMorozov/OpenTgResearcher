// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Helpers;

public sealed class TgStorageManager : TgWebDisposable, ITgStorageManager
{
    #region Public and private fields, properties, constructor

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

    public TgStorageManager() : base()
    {
        var scope = TgGlobalTools.Container.BeginLifetimeScope();

        AppRepository = scope.Resolve<ITgEfAppRepository>();
        ContactRepository = scope.Resolve<ITgEfContactRepository>();
        DocumentRepository = scope.Resolve<ITgEfDocumentRepository>();
        FilterRepository = scope.Resolve<ITgEfFilterRepository>();
        LicenseRepository = scope.Resolve<ITgEfLicenseRepository>();
        MessageRepository = scope.Resolve<ITgEfMessageRepository>();
        ProxyRepository = scope.Resolve<ITgEfProxyRepository>();
        SourceRepository = scope.Resolve<ITgEfSourceRepository>();
        StoryRepository = scope.Resolve<ITgEfStoryRepository>();
        VersionRepository = scope.Resolve<ITgEfVersionRepository>();
    }

    public TgStorageManager(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment)
    {
        var scope = TgGlobalTools.Container.BeginLifetimeScope();

        AppRepository = scope.Resolve<ITgEfAppRepository>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
        ContactRepository = scope.Resolve<ITgEfContactRepository>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
        DocumentRepository = scope.Resolve<ITgEfDocumentRepository>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
        FilterRepository = scope.Resolve<ITgEfFilterRepository>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
        LicenseRepository = scope.Resolve<ITgEfLicenseRepository>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
        MessageRepository = scope.Resolve<ITgEfMessageRepository>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
        ProxyRepository = scope.Resolve<ITgEfProxyRepository>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
        SourceRepository = scope.Resolve<ITgEfSourceRepository>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
        StoryRepository = scope.Resolve<ITgEfStoryRepository>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
        VersionRepository = scope.Resolve<ITgEfVersionRepository>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
    }

    #endregion

    #region IDisposable

    /// <summary> Release managed resources </summary>
    public override void ReleaseManagedResources()
    {
        //
    }

    /// <summary> Release unmanaged resources </summary>
    public override void ReleaseUnmanagedResources()
    {
        AppRepository.Dispose();
        ContactRepository.Dispose();
        DocumentRepository.Dispose();
        FilterRepository.Dispose();
        LicenseRepository.Dispose();
        MessageRepository.Dispose();
        ProxyRepository.Dispose();
        SourceRepository.Dispose();
        StoryRepository.Dispose();
        VersionRepository.Dispose();
    }

    #endregion
}
