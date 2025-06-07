// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using TgInfrastructure.Contracts;

using ValidationResult = FluentValidation.Results.ValidationResult;

namespace TgStorage.Utils;

/// <summary> Global tools </summary>
public static class TgGlobalTools
{
    #region Public and private fields, properties, constructor

    public static TgEnumAppType AppType { get; private set; }

    public static bool IsXmlReady => TgAppSettingsHelper.Instance.AppXml.IsExistsEfStorage;

    /// <summary> Autofac Container </summary>
	public static Autofac.IContainer Container = null!;
	/// <summary> Limit count of download threads by free license </summary>
	public static int DownloadCountThreadsLimitFree => 10;
	/// <summary> Limit count of download threads by test license </summary>
	public static int DownloadCountThreadsLimitPaid => 100;
	/// <summary> Limit batch of saving messages </summary>
	public static int BatchMessagesLimit => 100;
    public static string AppStorage = string.Empty;

    public const string FileEfStorage = "TgStorage.db";
    public const string HttpHeaderContentTypeJson = "application/json";
    public const string RouteChangeLog = "ChangeLog";
    public const string RouteController = "[controller]";
    public const string RouteCreated = "Created";
    public const string RouteGet = "Get";
    public const string RouteGetFiles = "GetFiles";
    public const string RoutePost = "Post";
    public const string RoutePromoStatistic = "PromoStatistic";
    public const string RouteRoot = "";
    public const string RouteValid = "Valid";

    #endregion

    #region Public and private methods

    public static void SetAppType(TgEnumAppType appType) => AppType = appType;

    public static bool IsPercentCountAll(TgEfSourceEntity source) => source.Count <= source.FirstId;

    /// <summary> Validate entity </summary>
    /// <typeparam name="TEfEntity"> Type of entity </typeparam>
    /// <param name="item"> Entity </param>
    public static ValidationResult GetEfValid<TEfEntity>(TEfEntity item) 
        where TEfEntity : class, ITgEfEntity<TEfEntity>, new() =>
        item switch
        {
            TgEfAppEntity app => new TgEfAppValidator().Validate(app),
            TgEfContactEntity contact => new TgEfContactValidator().Validate(contact),
            TgEfDocumentEntity document => new TgEfDocumentValidator().Validate(document),
            TgEfFilterEntity filter => new TgEfFilterValidator().Validate(filter),
            TgEfLicenseEntity license => new TgEfLicenseValidator().Validate(license),
            TgEfMessageEntity message => new TgEfMessageValidator().Validate(message),
            TgEfSourceEntity source => new TgEfSourceValidator().Validate(source),
            TgEfStoryEntity story => new TgEfStoryValidator().Validate(story),
            TgEfProxyEntity proxy => new TgEfProxyValidator().Validate(proxy),
            TgEfVersionEntity version => new TgEfVersionValidator().Validate(version),
            _ => new() { Errors = [new ValidationFailure(nameof(item), "Type error!")] }
        };

    /// <summary> Validate DTO </summary>
    /// <typeparam name="TDto"> Type of DTO </typeparam>
    /// <param name="item"> DTO </param>
    public static ValidationResult GetValidDto<TEfEntity, TDto>(TDto item)
        where TEfEntity : class, ITgEfEntity<TEfEntity>, new()
        where TDto : class, ITgDto<TEfEntity, TDto>, new() =>
        item switch
        {
            TgEfProxyDto proxy => new TgProxyDtoValidator().Validate(proxy),
            _ => new() { Errors = [new ValidationFailure(nameof(item), "Type error!")] }
        };

    /// <summary> Normilize entity </summary>
    /// <typeparam name="TEfEntity"> Type of entity </typeparam>
    /// <param name="item"> Entity </param>
    public static void Normilize<TEfEntity>(TEfEntity item) where TEfEntity : class, ITgEfEntity<TEfEntity>, new()
    {
        switch (item)
        {
            case TgEfAppEntity app:
                if (app.ProxyUid == Guid.Empty)
                    app.ProxyUid = null;
                break;
            case TgEfContactEntity contact:
                break;
            case TgEfDocumentEntity document:
                break;
            case TgEfFilterEntity filter:
                break;
            case TgEfLicenseEntity license:
                break;
            case TgEfMessageEntity message:
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

    public static Expression<Func<TEfEntity, bool>> WhereUidNotEmpty<TEfEntity>() where TEfEntity : class, ITgEfEntity<TEfEntity>, new() =>
        x => x.Uid != Guid.Empty;

    public static Expression<Func<TEfEntity, List<TEfEntity>, bool>> WhereUidNotEquals<TEfEntity>() where TEfEntity : class, ITgEfEntity<TEfEntity>, new() =>
        (itemFrom, itemsTo) => itemsTo.All(itemTo => itemTo.Uid.ToString().ToUpper() != itemFrom.Uid.ToString().ToUpper());

    #endregion
}