// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using ValidationResult = FluentValidation.Results.ValidationResult;

namespace TgStorage.Utils;

/// <summary> SQL utilities </summary>
public static class TgEfUtils
{
	#region Public and private fields, properties, constructor

	public static TgLogHelper TgLog => TgLogHelper.Instance;
	public static TgLocaleHelper TgLocale => TgLocaleHelper.Instance;
	public static string FileEfStorage => "TgStorage.db";
	public static string AppStorage = string.Empty;

	#endregion

	#region Public and private methods

	public static IEnumerable<Type> GetTableTypes()
	{
		yield return typeof(TgEfAppEntity);
		yield return typeof(TgEfDocumentEntity);
		yield return typeof(TgEfFilterEntity);
		yield return typeof(TgEfProxyEntity);
		yield return typeof(TgEfSourceEntity);
		yield return typeof(TgEfVersionEntity);
	}

	public static string GetPercentCountString(TgEfSourceEntity source)
	{
		var percent = source.Count <= source.FirstId ? 100 : source.FirstId > 1 ? (float)source.FirstId * 100 / source.Count : 0;
		if (IsPercentCountAll(source))
			return "100.00 %";
		return percent > 9 ? $" {percent:00.00} %" : $"  {percent:0.00} %";
	}

	public static bool IsPercentCountAll(TgEfSourceEntity source) => source.Count <= source.FirstId;

	/// <summary> Validate entity </summary>
	/// <typeparam name="TEfEntity"> Type of entity </typeparam>
	/// <param name="item"> Entity </param>
	public static ValidationResult GetEfValid<TEfEntity>(TEfEntity item) where TEfEntity : class, ITgEfEntity<TEfEntity>, new() =>
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

	public static void VersionsView()
	{
		var versionRepository = new TgEfVersionRepository();
		var storageResult = versionRepository.GetList(TgEnumTableTopRecords.All, 0);
		if (storageResult.IsExists)
		{
			foreach (var version in storageResult.Items)
			{
				TgLog.WriteLine($" {version.Version:00} | {version.Description}");
			}
		}
	}

	/// <summary> Create and update storage </summary>
	public static async Task CreateAndUpdateDbAsync()
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		await efContext.MigrateDbAsync();
		TgEfVersionRepository versionRepository = new();
		await versionRepository.FillTableVersionsAsync();
		await efContext.ShrinkDbAsync();
	}

	/// <summary> Shrink storage </summary>
	public static async Task ShrinkDbAsync()
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		await efContext.ShrinkDbAsync();
	}

	/// <summary> Backup storage </summary>
	public static (bool IsSuccess, string FileName) BackupDbAsync()
	{
		using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		return efContext.BackupDb();
	}

	public static Expression<Func<TEfEntity, bool>> WhereUidNotEmpty<TEfEntity>() where TEfEntity : class, ITgEfEntity<TEfEntity>, new() => 
		x => x.Uid != Guid.Empty;
	
	public static Expression<Func<TEfEntity, List<TEfEntity>, bool>> WhereUidNotEquals<TEfEntity>() where TEfEntity : class, ITgEfEntity<TEfEntity>, new() =>
		(itemFrom, itemsTo) => itemsTo.All(itemTo => itemTo.Uid.ToString().ToUpper() != itemFrom.Uid.ToString().ToUpper());

	#endregion
}
