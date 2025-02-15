// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using ValidationResult = FluentValidation.Results.ValidationResult;

namespace TgStorage.Utils;

public static class TgEfUtils
{
	#region Public and private fields, properties, constructor

	public static TgLogHelper TgLog => TgLogHelper.Instance;
	public static TgLocaleHelper TgLocale => TgLocaleHelper.Instance;
	public static string FileEfStorage => "TgStorage.db";
	public static string AppStorage = string.Empty;

	#endregion

	#region Public and private methods

	public static IEnumerable<ITgDbEntity> GetTableModels()
	{
		yield return new TgEfAppEntity();
		yield return new TgEfDocumentEntity();
		yield return new TgEfFilterEntity();
		yield return new TgEfProxyEntity();
		yield return new TgEfSourceEntity();
		yield return new TgEfVersionEntity();
	}

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
	/// <typeparam name="TEntity"> Type of entity </typeparam>
	/// <param name="item"> Entity </param>
	public static ValidationResult GetEfValid<TEntity>(TEntity item) where TEntity : ITgDbFillEntity<TEntity>, new() =>
		item switch
		{
			TgEfAppEntity app => new TgEfAppValidator().Validate(app),
			TgEfContactEntity contact => new TgEfContactValidator().Validate(contact),
			TgEfDocumentEntity document => new TgEfDocumentValidator().Validate(document),
			TgEfFilterEntity filter => new TgEfFilterValidator().Validate(filter),
			TgEfMessageEntity message => new TgEfMessageValidator().Validate(message),
			TgEfSourceEntity source => new TgEfSourceValidator().Validate(source),
			TgEfStoryEntity story => new TgEfStoryValidator().Validate(story),
			TgEfProxyEntity proxy => new TgEfProxyValidator().Validate(proxy),
			TgEfVersionEntity version => new TgEfVersionValidator().Validate(version),
			_ => new() { Errors = [new ValidationFailure(nameof(item), "Type error!")] }
		};

	/// <summary> Normilize entity </summary>
	/// <typeparam name="TEntity"> Type of entity </typeparam>
	/// <param name="item"> Entity </param>
	public static void Normilize<TEntity>(TEntity item) where TEntity : ITgDbFillEntity<TEntity>, new()
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
				if (document.SourceId == 0)
					document.SourceId = null;
				break;
			case TgEfFilterEntity filter:
				break;
			case TgEfMessageEntity message:
				if (message.SourceId == 0)
					message.SourceId = null;
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
		using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
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

	public static void FiltersView()
	{
		var filterRepository = new TgEfFilterRepository();
		var storageResult = filterRepository.GetList(TgEnumTableTopRecords.All, 0);
		if (storageResult.IsExists)
		{
			foreach (var filter in storageResult.Items)
			{
				TgLog.WriteLine(filter.ToConsoleString());
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

	///// <summary> Data transfer between storages </summary>
	//public static async Task DataTransferBetweenStoragesAsync(TgEfContext efContextFrom, TgEfContext efContextTo, Action<string> logWrite)
	//{
	//	// Transferring apps
	//	await DataTransferCoreAsync(new TgEfAppRepository(efContextFrom), new TgEfAppRepository(efContextTo), logWrite, TgEfConstants.TableApps);
	//	// Transferring filters
	//	await DataTransferCoreAsync(new TgEfFilterRepository(efContextFrom), new TgEfFilterRepository(efContextTo), logWrite, TgEfConstants.TableFilters);
	//	// Transferring proxies
	//	await DataTransferCoreAsync(new TgEfProxyRepository(efContextFrom), new TgEfProxyRepository(efContextTo), logWrite, TgEfConstants.TableProxies);
	//	// Transferring sources
	//	await DataTransferCoreAsync(new TgEfSourceRepository(efContextFrom), new TgEfSourceRepository(efContextTo), logWrite, TgEfConstants.TableSources);
	//}

//	private static async Task DataTransferCoreAsync<TEntity>(ITgEfRepository<TEntity> repoFrom, ITgEfRepository<TEntity> repoTo, 
//		Action<string> logWrite, string tableName) where TEntity : ITgDbFillEntity<TEntity>, new()
//	{
//		logWrite($"Transferring table {tableName}: ...");
//		var batchSizeFrom = 100;
//		//const int batchSizeTo = 100;
//		var countFrom = await repoFrom.GetCountAsync(WhereUidNotEmpty<TEntity>());
//		//int countTo = await repoTo.GetCountAsync(WhereUidNotEmpty<TEntity>());

//		for (var i = 0; i < countFrom; i += batchSizeFrom)
//		{
//			try
//			{
//				var storageResultFrom = await repoFrom.GetListAsync(
//					batchSizeFrom, i, WhereUidNotEmpty<TEntity>(), isReadOnly: false);
//				if (storageResultFrom.IsExists)
//				{
//					//List<TEntity> itemsTo = storageResultTo.Items.ToList();
//					//List<TEntity> itemsFrom = storageResultFrom.Items.Where(itemFrom => itemsTo.All(itemTo => itemTo.Uid != itemFrom.Uid)).ToList();
//					//List<TEntity> itemsFrom = storageResultFrom.Items.ToList();
//					var countErrors = 0;
//					//await using var transaction = await repoTo.BeginTransactionAsync();
//					var tasks = new List<Task>();
//					if (storageResultFrom.Items.Any())
//					{
//						foreach (var itemFrom in storageResultFrom.Items)
//						{
//							tasks.Add(Task.Run(async () =>
//							{
//								try
//								{
//									await repoTo.SaveAsync(itemFrom);
//								}
//#if DEBUG
//								catch (Exception ex)
//#else
//								catch (Exception)
//#endif
//								{
//									Interlocked.Increment(ref countErrors);
//#if DEBUG
//									logWrite(ex.Message);
//									if (ex.InnerException is not null)
//										logWrite(ex.InnerException.Message);
//#endif
//								}
//							}));
//						}
//						// Ожидаем завершения всех задач
//						await Task.WhenAll(tasks);
//					}
//					var countSuccess = storageResultFrom.Items.Count() - countErrors;
//					if (countSuccess > 0)
//						logWrite($"Transferring table {tableName}: copied {countSuccess} records");
//					if (countErrors > 0)
//						logWrite($"Transferring table {tableName}: errors in {countErrors} records");
//				}
//			}
//#if DEBUG
//			catch (Exception ex)
//#else
//			catch (Exception)
//#endif
//			{
//#if DEBUG
//				logWrite(ex.Message);
//#endif
//				throw;
//			}
//		}
//		logWrite($"Transferring table {tableName}: completed");
//	}

	public static Expression<Func<TEntity, bool>> WhereUidNotEmpty<TEntity>() where TEntity : ITgDbFillEntity<TEntity>, new() => x => x.Uid != Guid.Empty;
	
	public static Expression<Func<TEntity, List<TEntity>, bool>> WhereUidNotEquals<TEntity>() where TEntity : ITgDbFillEntity<TEntity>, new() =>
		(itemFrom, itemsTo) => itemsTo.All(itemTo => itemTo.Uid.ToString().ToUpper() != itemFrom.Uid.ToString().ToUpper());

	#endregion
}