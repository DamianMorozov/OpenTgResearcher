namespace TgStorage.Utils;

/// <summary> Global tools </summary>
public static class TgGlobalTools
{
    #region Fields, properties, constructor

    public static TgEnumAppType AppType { get; private set; }

    public static bool IsXmlReady => TgAppSettingsHelper.Instance.AppXml.IsExistsEfStorage;

    /// <summary> Autofac Container </summary>
	public static Autofac.IContainer Container = null!;
    /// <summary> Limit count of download threads </summary>
    public static int DownloadCountThreadsLimit => 100;
    /// <summary> Limit batch of saving messages </summary>
    public static int BatchMessagesLimit => 25;
    /// <summary> EF storage </summary>
    public static string AppStorage = "TgStorage.db";
        
    public const string DeleteByType = "DeleteByType";
    public const string HttpHeaderContentTypeJson = "application/json";
    public const string RouteBase = "Base";
    public const string RouteChangeLog = "ChangeLog";
    public const string RouteController = "[controller]";
    public const string RouteCreate = "Create";
    public const string RouteCreateCommunity = "CreateCommunity";
    public const string RouteCreated = "Created";
    public const string RouteDelete = "Delete";
    public const string RouteGet = "Get";
    public const string RouteGetFile = "GetFile";
    public const string RouteGetFiles = "GetFiles";
    public const string RouteGetList = "GetList";
    public const string RouteImage = "Image";
    public const string RouteLicense = "License";
    public const string RouteLicenses = "Licenses";
    public const string RouteLicenseStatistic = "LicenseStatistic";
    public const string RouteMedias = "Medias";
    public const string RoutePost = "Post";
    public const string RouteReleases = "Releases";
    public const string RouteRoot = "";
    public const string RouteValid = "Valid";
    public const string RouteVersionHistory = "VersionHistory";

    #endregion

    #region Methods

    public static void SetAppType(TgEnumAppType appType) => AppType = appType;

    public static bool IsPercentCountAll(TgEfSourceEntity source) => source.Count <= source.FirstId;

    /// <summary> Validate entity </summary>
    /// <typeparam name="TEfEntity"> Type of entity </typeparam>
    /// <param name="item"> Entity </param>
    public static FluentValidation.Results.ValidationResult GetEfValid<TEfEntity>(TEfEntity item)
        where TEfEntity : class, ITgEfEntity, new() =>
        item switch
        {
            TgEfAppEntity app => new TgEfAppValidator().Validate(app),
            TgEfUserEntity contact => new TgEfUserValidator().Validate(contact),
            TgEfDocumentEntity document => new TgEfDocumentValidator().Validate(document),
            TgEfFilterEntity filter => new TgEfFilterValidator().Validate(filter),
            TgEfLicenseEntity license => new TgEfLicenseValidator().Validate(license),
            TgEfMessageEntity message => new TgEfMessageValidator().Validate(message),
            TgEfMessageRelationEntity messageRelation => new TgEfMessageRelationValidator().Validate(messageRelation),
            TgEfSourceEntity source => new TgEfSourceValidator().Validate(source),
            TgEfStoryEntity story => new TgEfStoryValidator().Validate(story),
            TgEfProxyEntity proxy => new TgEfProxyValidator().Validate(proxy),
            TgEfVersionEntity version => new TgEfVersionValidator().Validate(version),
            _ => new() { Errors = [new ValidationFailure(nameof(item), "Type error!")] }
        };

    /// <summary> Validate DTO </summary>
    /// <typeparam name="TDto"> Type of DTO </typeparam>
    /// <param name="item"> DTO </param>
    public static FluentValidation.Results.ValidationResult GetValidDto<TEfEntity, TDto>(TDto item)
        where TEfEntity : class, ITgEfEntity, new()
        where TDto : class, ITgDto, new() =>
        item switch
        {
            TgEfProxyDto proxy => new TgProxyDtoValidator().Validate(proxy),
            _ => new() { Errors = [new ValidationFailure(nameof(item), "Type error!")] }
        };

    /// <summary> Normalize entity </summary>
    /// <typeparam name="TEfEntity"> Type of entity </typeparam>
    /// <param name="item"> Entity </param>
    public static void Normalize<TEfEntity>(TEfEntity item) where TEfEntity : class, ITgEfEntity, new()
    {
        switch (item)
        {
            case TgEfAppEntity app:
                if (app.ProxyUid == Guid.Empty)
                    app.ProxyUid = null;
                break;
            case TgEfUserEntity contact:
                break;
            case TgEfDocumentEntity document:
                break;
            case TgEfFilterEntity filter:
                break;
            case TgEfLicenseEntity license:
                break;
            case TgEfMessageEntity message:
                break;
            case TgEfMessageRelationEntity messageRelation:
                break;
            case TgEfSourceEntity source:
                break;
            case TgEfStoryEntity story:
                break;
            case TgEfProxyEntity proxy:
                break;
            case TgEfVersionEntity version:
                break;
        }
        if (item.Uid == Guid.Empty)
            item.Uid = Guid.NewGuid();
    }

    public static Expression<Func<TEfEntity, bool>> WhereUidNotEmpty<TEfEntity>() where TEfEntity : class, ITgEfEntity, new() =>
        x => x.Uid != Guid.Empty;

    public static Expression<Func<TEfEntity, List<TEfEntity>, bool>> WhereUidNotEquals<TEfEntity>() where TEfEntity : class, ITgEfEntity, new() =>
        (itemFrom, itemsTo) => itemsTo.All(itemTo => !itemTo.Uid.ToString().Equals(itemFrom.Uid.ToString(), StringComparison.OrdinalIgnoreCase));

    #endregion
}
