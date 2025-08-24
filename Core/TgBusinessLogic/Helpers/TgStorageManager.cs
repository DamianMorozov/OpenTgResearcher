// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using TL;

namespace TgBusinessLogic.Helpers;

public sealed class TgStorageManager : TgWebDisposable, ITgStorageManager
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

    public TgStorageManager() : base()
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

    public TgStorageManager(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment)
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
    public async Task<TgEfUserEntity> CreateOrGetUserAsync(TL.User user, bool isContact)
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

    #endregion
}
