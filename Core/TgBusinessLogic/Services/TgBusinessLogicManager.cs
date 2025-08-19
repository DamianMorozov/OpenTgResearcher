// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Services;

public sealed class TgBusinessLogicManager : TgWebDisposable, ITgBusinessLogicManager
{
    #region Public and private fields, properties, constructor

    private ILifetimeScope Scope { get; } = default!;
    public ITgStorageManager StorageManager { get; private set; } = default!;
    public ITgLicenseService LicenseService { get; private set; } = default!;
    public ITgConnectClient ConnectClient { get; private set; } = default!;
    public ITgFloodControlService FloodControlService { get; private set; } = default!;
    public IFusionCache Cache { get; private set; } = default!;

    public TgBusinessLogicManager() : base()
    {
        Scope = TgGlobalTools.Container.BeginLifetimeScope();

        StorageManager = Scope.Resolve<ITgStorageManager>();
        LicenseService = Scope.Resolve<ITgLicenseService>(new TypedParameter(typeof(ITgStorageManager), StorageManager));
        FloodControlService = Scope.Resolve<ITgFloodControlService>();
        Cache = Scope.Resolve<IFusionCache>();
        ConnectClient = TgGlobalTools.AppType switch
        {
            TgEnumAppType.Console => Scope.Resolve<ITgConnectClientConsole>(
                new TypedParameter(typeof(ITgStorageManager), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService),
                new TypedParameter(typeof(IFusionCache), Cache)),
            TgEnumAppType.Desktop => Scope.Resolve<ITgConnectClientDesktop>(
                new TypedParameter(typeof(ITgStorageManager), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService),
                new TypedParameter(typeof(IFusionCache), Cache)),
            TgEnumAppType.Blazor => Scope.Resolve<ITgConnectClientBlazor>(
                new TypedParameter(typeof(ITgStorageManager), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService),
                new TypedParameter(typeof(IFusionCache), Cache)),
            TgEnumAppType.Test => Scope.Resolve<ITgConnectClientTest>(
                new TypedParameter(typeof(ITgStorageManager), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService),
                new TypedParameter(typeof(IFusionCache), Cache)),
            TgEnumAppType.Memory or _ => Scope.Resolve<ITgConnectClient>(
                new TypedParameter(typeof(ITgStorageManager), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService),
                new TypedParameter(typeof(IFusionCache), Cache)),
        };
    }

    public TgBusinessLogicManager(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment)
    {
        Scope = TgGlobalTools.Container.BeginLifetimeScope();

        StorageManager = Scope.Resolve<ITgStorageManager>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
        LicenseService = Scope.Resolve<ITgLicenseService>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment),
            new TypedParameter(typeof(ITgStorageManager), StorageManager));
        FloodControlService = Scope.Resolve<ITgFloodControlService>();
        ConnectClient = TgGlobalTools.AppType switch
        {
            TgEnumAppType.Console => Scope.Resolve<ITgConnectClientConsole>(
                new TypedParameter(typeof(ITgStorageManager), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService)),
            TgEnumAppType.Desktop => Scope.Resolve<ITgConnectClientDesktop>(
                new TypedParameter(typeof(ITgStorageManager), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService)),
            TgEnumAppType.Blazor => Scope.Resolve<ITgConnectClientBlazor>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment),
                new TypedParameter(typeof(ITgStorageManager), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService)),
            TgEnumAppType.Test => Scope.Resolve<ITgConnectClientTest>(
                new TypedParameter(typeof(ITgStorageManager), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService)),
            TgEnumAppType.Memory or _ => Scope.Resolve<ITgConnectClient>(
                new TypedParameter(typeof(ITgStorageManager), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService)),
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
        FloodControlService.Dispose();

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
        // Remove duplicate messages from the database
        await RemoveDuplicateMessagesByDirectSqlAsync();
        // Apply migration
        await StorageManager.EfContext.MigrateDbAsync();
        // Fill version table
        await StorageManager.VersionRepository.FillTableVersionsAsync();
        // Shrink database
        await StorageManager.EfContext.ShrinkDbAsync();
    }

    /// <inheritdoc />
    public async Task RemoveDuplicateMessagesAsync()
    {
        // Find duplicate messages by SourceId and Id
        var allMessages = await StorageManager.EfContext.Messages.ToListAsync();

        // Group by SourceId and Id, find duplicates
        var duplicates = allMessages
            .GroupBy(x => new { x.SourceId, x.Id })
            .Where(g => g.Count() > 1)
            .SelectMany(g => g.OrderBy(x => x.Uid).Skip(1)) // Keep only one
            .ToList();

        // Log and remove duplicates
        foreach (var msg in duplicates)
        {
            TgLogUtils.WriteLog($"Removed duplicate message: SourceId={msg.SourceId}, Id={msg.Id}, Uid={msg.Uid}");
        }

        if (duplicates.Count != 0)
        {
            StorageManager.EfContext.Messages.RemoveRange(duplicates);
            await StorageManager.EfContext.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task RemoveDuplicateMessagesByDirectSqlAsync()
    {
        var sql = @"
DELETE 
FROM MESSAGES
WHERE UID IN (
    SELECT M1.UID
    FROM MESSAGES M1
    JOIN (
        SELECT SOURCE_ID, ID, MIN(UID) AS MIN_UID
        FROM MESSAGES
        GROUP BY SOURCE_ID, ID
        HAVING COUNT(*) > 1
    ) AS DUPLICATES
    ON M1.SOURCE_ID = DUPLICATES.SOURCE_ID AND m1.ID = DUPLICATES.ID
    WHERE M1.UID != DUPLICATES.MIN_UID
);"
            .TrimStart('\r', ' ', '\n', '\t').TrimEnd('\r', ' ', '\n', '\t').Replace(Environment.NewLine, " ");

        await StorageManager.EfContext.Database.ExecuteSqlRawAsync(sql);
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
