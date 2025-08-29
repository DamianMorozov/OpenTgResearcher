// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using TL;

namespace TgBusinessLogic.Services;

public sealed class TgStorageService : TgWebDisposable, ITgStorageService
{
    #region Fields, properties, constructor

    /// <summary> Autofac lifetime scope </summary>
    private ILifetimeScope Scope { get; }
    /// <inheritdoc />
    public ITgEfContext EfContext { get; }
    /// <inheritdoc />
    public string StoragePath { get; }
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

    public TgStorageService() : base()
    {
        Scope = TgGlobalTools.Container.BeginLifetimeScope();
        
        EfContext = Scope.Resolve<ITgEfContext>();
        var connectionString = EfContext.Database.GetDbConnection()?.ConnectionString ?? string.Empty;
        var builder = new SqliteConnectionStringBuilder(connectionString);
        StoragePath = builder.DataSource;

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

    public TgStorageService(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment)
    {
        Scope = TgGlobalTools.Container.BeginLifetimeScope();
        
        EfContext = Scope.Resolve<ITgEfContext>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
        var connectionString = EfContext.Database.GetDbConnection()?.ConnectionString ?? string.Empty;
        var builder = new SqliteConnectionStringBuilder(connectionString);
        StoragePath = builder.DataSource;

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

    #region Methods

    /// <inheritdoc />
    public async Task<TgEfStoryEntity> CreateOrGetStoryAsync(long peerId, StoryItem story)
    {
        var storageResult = await StoryRepository.GetByDtoAsync(new() { FromId = peerId, Id = story.id });
        var storyEntity = storageResult.IsExists && storageResult.Item is not null ? storageResult.Item : new();
        storyEntity.DtChanged = DateTime.UtcNow;
        storyEntity.Id = story.id;
        storyEntity.FromId = story.from_id?.ID ?? peerId;
        storyEntity.Date = story.date;
        storyEntity.ExpireDate = story.expire_date;
        storyEntity.Caption = story.caption;
        return storyEntity;
    }

    /// <inheritdoc />
    public async Task<TgEfUserEntity> CreateOrGetUserAsync(User user, bool isContact)
    {
        var storageResult = await UserRepository.GetByDtoAsync(new() { Id = user.id });
        var userEntity = storageResult.IsExists && storageResult.Item is not null ? storageResult.Item : new();
        userEntity.DtChanged = DateTime.UtcNow;
        userEntity.Id = user.id;
        userEntity.AccessHash = user.access_hash;
        userEntity.IsActive = user.IsActive;
        userEntity.IsBot = user.IsBot;
        userEntity.FirstName = user.first_name;
        userEntity.LastName = user.last_name;
        userEntity.UserName = user.username;
        userEntity.UserNames = user.usernames is null ? string.Empty : string.Join("|", user.usernames.ToList());
        userEntity.PhoneNumber = user.phone;
        userEntity.Status = user.status is null ? string.Empty : user.status.ToString();
        userEntity.RestrictionReason = user.restriction_reason is null ? string.Empty : string.Join("|", user.restriction_reason.ToList());
        userEntity.LangCode = user.lang_code;
        userEntity.StoriesMaxId = user.stories_max_id;
        userEntity.BotInfoVersion = user.bot_info_version.ToString();
        userEntity.BotInlinePlaceholder = user.bot_inline_placeholder is null ? string.Empty : user.bot_inline_placeholder.ToString();
        userEntity.BotActiveUsers = user.bot_active_users;
        userEntity.IsContact = isContact;
        return userEntity;
    }

    /// <inheritdoc />
    public async Task<bool> CheckTableExistsAsync(string tableName = "")
    {
        if (string.IsNullOrWhiteSpace(tableName))
            tableName = "__EFMigrationsHistory";

        var connection = EfContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        // For SQLite
        if (connection.GetType().Name.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName";
            var param = cmd.CreateParameter();
            param.ParameterName = "@tableName";
            param.Value = tableName;
            cmd.Parameters.Add(param);

            var result = await cmd.ExecuteScalarAsync();
            return result != null;
        }

        // For other DBMSs via INFORMATION_SCHEMA
        using var cmd2 = connection.CreateCommand();
        cmd2.CommandText = @"
        SELECT 1 
        FROM INFORMATION_SCHEMA.TABLES 
        WHERE TABLE_NAME = @tableName";
        var param2 = cmd2.CreateParameter();
        param2.ParameterName = "@tableName";
        param2.Value = tableName;
        cmd2.Parameters.Add(param2);

        var exists = await cmd2.ExecuteScalarAsync();
        return exists != null;
    }

    /// <inheritdoc />
    public async Task RemoveDuplicateMessagesAsync()
    {
        // Find duplicate messages by SourceId and Id
        var allMessages = await EfContext.Messages.ToListAsync();

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
            EfContext.Messages.RemoveRange(duplicates);
            await EfContext.SaveChangesAsync();
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

        await EfContext.Database.ExecuteSqlRawAsync(sql);
    }

    /// <inheritdoc />
    public async Task CreateAndUpdateDbAsync()
    {
        if (await CheckTableExistsAsync())
        {
            // Remove duplicate messages from the database
            await RemoveDuplicateMessagesByDirectSqlAsync();
        }
        // Apply migration
        await EfContext.MigrateDbAsync();
        // Fill version table
        await VersionRepository.FillTableVersionsAsync();
    }

    /// <inheritdoc />
    public async Task ShrinkDbAsync() => await EfContext.ShrinkDbAsync();

    /// <inheritdoc />
    public (bool IsSuccess, string FileName) BackupDb(string storagePath = "") => EfContext.BackupDb(storagePath);

    /// <inheritdoc />
    public async Task<ObservableCollection<TgStorageTableDto>> LoadStorageTableDtosAsync(string appsName, string chatsName, string contactsName,
        string documentsName, string filtersName, string messagesName, string proxiesName, string storiesName, string versionsName)
    {
        var appDtos = new TgStorageTableDto(appsName, await AppRepository.GetListCountAsync());
        var chatsDtos = new TgStorageTableDto(chatsName, await SourceRepository.GetListCountAsync());
        var usersDtos = new TgStorageTableDto(contactsName, await UserRepository.GetListCountAsync());
        var documentsDtos = new TgStorageTableDto(documentsName, await DocumentRepository.GetListCountAsync());
        var filtersDtos = new TgStorageTableDto(filtersName, await FilterRepository.GetListCountAsync());
        var messagesDtos = new TgStorageTableDto(messagesName, await MessageRepository.GetListCountAsync());
        var proxiesDtos = new TgStorageTableDto(proxiesName, await ProxyRepository.GetListCountAsync());
        var storiesDtos = new TgStorageTableDto(storiesName, await StoryRepository.GetListCountAsync());
        var versionsDtos = new TgStorageTableDto(versionsName, await VersionRepository.GetListCountAsync());

        // Order
        ObservableCollection<TgStorageTableDto> dtos = [appDtos, chatsDtos, usersDtos, documentsDtos, filtersDtos, messagesDtos, proxiesDtos, storiesDtos, versionsDtos];
        return [.. dtos.OrderBy(x => x.Name)];
    }

    /// <inheritdoc />
    public ObservableCollection<TgStorageBackupDto> LoadStorageBackupDtos(string storagePath = "") =>
        EfContext.LoadStorageBackupDtos(storagePath);

    #endregion
}
