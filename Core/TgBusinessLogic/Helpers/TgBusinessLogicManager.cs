// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Helpers;

public sealed class TgBusinessLogicManager : TgWebDisposable, ITgBusinessLogicManager
{
    #region Public and private fields, properties, constructor

    private ILifetimeScope Scope { get; } = default!;
    public ITgStorageManager StorageManager { get; private set; } = default!;
    public ITgLicenseService LicenseService { get; private set; } = default!;
    public ITgConnectClient ConnectClient { get; private set; } = default!;

    public TgBusinessLogicManager() : base()
    {
        Scope = TgGlobalTools.Container.BeginLifetimeScope();

        StorageManager = Scope.Resolve<ITgStorageManager>();
        LicenseService = Scope.Resolve<ITgLicenseService>(new TypedParameter(typeof(ITgStorageManager), StorageManager));
        ConnectClient = TgGlobalTools.AppType switch
        {
            TgEnumAppType.Console => Scope.Resolve<ITgConnectClientConsole>(new TypedParameter(typeof(ITgStorageManager), StorageManager)),
            TgEnumAppType.Desktop => Scope.Resolve<ITgConnectClientDesktop>(new TypedParameter(typeof(ITgStorageManager), StorageManager)),
            TgEnumAppType.Blazor => Scope.Resolve<ITgConnectClientBlazor>(new TypedParameter(typeof(ITgStorageManager), StorageManager)),
            TgEnumAppType.Test => Scope.Resolve<ITgConnectClientTest>(new TypedParameter(typeof(ITgStorageManager), StorageManager)),
            TgEnumAppType.Memory or _ => Scope.Resolve<ITgConnectClient>(new TypedParameter(typeof(ITgStorageManager), StorageManager)),
        };
    }

    public TgBusinessLogicManager(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment)
    {
        Scope = TgGlobalTools.Container.BeginLifetimeScope();

        StorageManager = Scope.Resolve<ITgStorageManager>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
        LicenseService = Scope.Resolve<ITgLicenseService>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment),
            new TypedParameter(typeof(ITgStorageManager), StorageManager));
        ConnectClient = TgGlobalTools.AppType switch
        {
            TgEnumAppType.Console => Scope.Resolve<ITgConnectClientConsole>(new TypedParameter(typeof(ITgStorageManager), StorageManager)),
            TgEnumAppType.Desktop => Scope.Resolve<ITgConnectClientDesktop>(new TypedParameter(typeof(ITgStorageManager), StorageManager)),
            TgEnumAppType.Blazor => Scope.Resolve<ITgConnectClientBlazor>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment),
                new TypedParameter(typeof(ITgStorageManager), StorageManager)),
            TgEnumAppType.Test => Scope.Resolve<ITgConnectClientTest>(new TypedParameter(typeof(ITgStorageManager), StorageManager)),
            TgEnumAppType.Memory or _ => Scope.Resolve<ITgConnectClient>(new TypedParameter(typeof(ITgStorageManager), StorageManager)),
        };
    }

    #endregion

    #region IDisposable

    /// <summary> Release managed resources </summary>
    public override void ReleaseManagedResources()
    {
        ConnectClient.Dispose();
        LicenseService.Dispose();
        StorageManager.Dispose();

        Scope.Dispose();
    }

    /// <summary> Release unmanaged resources </summary>
    public override void ReleaseUnmanagedResources()
    {
        //
    }

    #endregion

    #region Public and private methods

    /// <inheritdoc />
    public async Task CreateAndUpdateDbAsync()
    {
        await StorageManager.EfContext.MigrateDbAsync();
        await StorageManager.VersionRepository.FillTableVersionsAsync();
        await StorageManager.EfContext.ShrinkDbAsync();
    }

    /// <inheritdoc />
    public async Task ShrinkDbAsync() => 
        await StorageManager.EfContext.ShrinkDbAsync();

    /// <inheritdoc />
    public (bool IsSuccess, string FileName) BackupDb(string storagePath = "") => 
        StorageManager.EfContext.BackupDb(storagePath);

    /// <inheritdoc />
    public async Task<ObservableCollection<TgStorageTableDto>> LoadStorageTableDtosAsync(string appsName, string chatsName, string contactsName,
        string documentsName, string filtersName, string messagesName, string proxiesName, string storiesName, string versionsName)
    {
        var appDtos = new TgStorageTableDto(appsName, await StorageManager.AppRepository.GetListCountAsync());
        var chatsDtos = new TgStorageTableDto(chatsName, await StorageManager.SourceRepository.GetListCountAsync());
        var usersDtos = new TgStorageTableDto(contactsName, await StorageManager.UserRepository.GetListCountAsync());
        var documentsDtos = new TgStorageTableDto(documentsName, await StorageManager.DocumentRepository.GetListCountAsync());
        var filtersDtos = new TgStorageTableDto(filtersName, await StorageManager.FilterRepository.GetListCountAsync());
        var messagesDtos = new TgStorageTableDto(messagesName, await StorageManager.MessageRepository.GetListCountAsync());
        var proxiesDtos = new TgStorageTableDto(proxiesName, await StorageManager.ProxyRepository.GetListCountAsync());
        var storiesDtos = new TgStorageTableDto(storiesName, await StorageManager.StoryRepository.GetListCountAsync());
        var versionsDtos = new TgStorageTableDto(versionsName, await StorageManager.VersionRepository.GetListCountAsync());

        // Order
        ObservableCollection<TgStorageTableDto> dtos = [appDtos, chatsDtos, usersDtos, documentsDtos, filtersDtos, messagesDtos, proxiesDtos, storiesDtos, versionsDtos];
        return [.. dtos.OrderBy(x => x.Name)];
    }

    /// <inheritdoc />
    public ObservableCollection<TgStorageBackupDto> LoadStorageBackupDtos(string storagePath = "") =>
        StorageManager.EfContext.LoadStorageBackupDtos(storagePath);

    #endregion
}
