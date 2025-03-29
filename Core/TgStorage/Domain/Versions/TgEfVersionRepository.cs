// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Versions;

/// <summary> Version repository </summary>
public sealed class TgEfVersionRepository : TgEfRepositoryBase<TgEfVersionEntity, TgEfVersionDto>, ITgEfVersionRepository
{
	#region Public and private methods

	public override string ToDebugString() => $"{nameof(TgEfVersionRepository)}";

	public override IQueryable<TgEfVersionEntity> GetQuery(bool isReadOnly = true) => 
		isReadOnly ? EfContext.Versions.AsNoTracking() : EfContext.Versions.AsTracking();

	public override async Task<TgEfStorageResult<TgEfVersionEntity>> GetAsync(TgEfVersionEntity item, bool isReadOnly = true)
	{
		try
		{
			// Find by Uid
			var itemFind = await GetQuery(isReadOnly)
				.Where(x => x.Uid == item.Uid)
				.FirstOrDefaultAsync();
			if (itemFind is not null)
				return new(TgEnumEntityState.IsExists, itemFind);
			// Find by Version
			itemFind = await GetQuery(isReadOnly).SingleOrDefaultAsync(x => x.Version == item.Version);
			return itemFind is not null
				? new(TgEnumEntityState.IsExists, itemFind)
				: new TgEfStorageResult<TgEfVersionEntity>(TgEnumEntityState.NotExists, item);
		}
#if DEBUG
		catch (Exception ex)
		{
			Debug.WriteLine(ex, TgConstants.LogTypeStorage);
			Debug.WriteLine(ex.StackTrace);
#else
		catch (Exception)
		{
#endif
			throw;
		}
	}

	private static Expression<Func<TgEfVersionEntity, TgEfVersionDto>> SelectDto() => item => new TgEfVersionDto().GetNewDto(item);

	public async Task<TgEfVersionDto> GetDtoAsync(Expression<Func<TgEfVersionEntity, bool>> where)
	{
		var dto = await GetQuery().Where(where).Select(SelectDto()).SingleOrDefaultAsync() ?? new TgEfVersionDto();
		return dto;
	}

	public async Task<List<TgEfVersionDto>> GetListDtosAsync(int take = 0, int skip = 0)
	{
		var dtos = take > 0
			? await GetQuery(isReadOnly: true).Skip(skip).Take(take).Select(SelectDto()).ToListAsync()
			: await GetQuery(isReadOnly: true).Select(SelectDto()).ToListAsync();
		return dtos;
	}

	public async Task<List<TgEfVersionDto>> GetListDtosAsync(int take, int skip, Expression<Func<TgEfVersionEntity, bool>> where)
	{
		var dtos = take > 0
			? await GetQuery(isReadOnly: true).Where(where).Skip(skip).Take(take).Select(SelectDto()).ToListAsync()
			: await GetQuery(isReadOnly: true).Where(where).Select(SelectDto()).ToListAsync();
		return dtos;
	}

	public override async Task<TgEfStorageResult<TgEfVersionEntity>> GetListAsync(int take, int skip, bool isReadOnly = true)
	{
		IList<TgEfVersionEntity> items = take > 0 
			? await GetQuery(isReadOnly).Skip(skip).Take(take).ToListAsync() 
			: await GetQuery(isReadOnly).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<TgEfStorageResult<TgEfVersionEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfVersionEntity, bool>> where, bool isReadOnly = true)
	{
		IList<TgEfVersionEntity> items = take > 0
			? await GetQuery(isReadOnly).Where(where).Skip(skip).Take(take).ToListAsync()
			: await GetQuery(isReadOnly).Where(where).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<int> GetCountAsync()
	{
		return await EfContext.Versions.AsNoTracking().CountAsync();
	}

	public override async Task<int> GetCountAsync(Expression<Func<TgEfVersionEntity, bool>> where)
	{
		return await EfContext.Versions.AsNoTracking().Where(where).CountAsync();
	}

	#endregion

	#region Public and private methods - Delete

	public override async Task<TgEfStorageResult<TgEfVersionEntity>> DeleteAllAsync()
	{
		var storageResult =
			await GetListAsync(0, 0, isReadOnly: false);
		if (storageResult.IsExists)
		{
			foreach (var item in storageResult.Items)
			{
				await DeleteAsync(item);
			}
		}
		return new(storageResult.IsExists
			? TgEnumEntityState.IsDeleted
			: TgEnumEntityState.NotDeleted);
	}

	#endregion

	#region Public and private methods - ITgEfVersionRepository

	public short LastVersion => 36;

	public async Task<TgEfVersionEntity> GetLastVersionAsync()
	{
		TgEfVersionEntity versionLast = new();
		var defaultVersion = new TgEfVersionEntity().Version;
		var versions = (await GetListAsync(TgEnumTableTopRecords.All, 0)).Items
			.Where(x => x.Version != defaultVersion).OrderBy(x => x.Version).ToList();
		if (versions.Any())
			versionLast = versions[^1];
		return versionLast;
	}

	public async Task FillTableVersionsAsync()
	{
		await DeleteNewAsync();
		var isLast = false;
		while (!isLast)
		{
			var versionLast = await GetLastVersionAsync();
			if (Equals(versionLast.Version, new TgEfVersionEntity().Version))
				versionLast.Version = 0;
			switch (versionLast.Version)
			{
				case 0:
					await SaveAsync(new() { Version = 1, Description = "Added versions table" });
					break;
				case 1:
					await SaveAsync(new() { Version = 2, Description = "Added apps table" });
					break;
				case 2:
					await SaveAsync(new() { Version = 3, Description = "Added documents table" });
					break;
				case 3:
					await SaveAsync(new() { Version = 4, Description = "Added filters table" });
					break;
				case 4:
					await SaveAsync(new() { Version = 5, Description = "Added messages table" });
					break;
				case 5:
					await SaveAsync(new() { Version = 6, Description = "Added proxies table" });
					break;
				case 6:
					await SaveAsync(new() { Version = 7, Description = "Added sources table" });
					break;
				case 7:
					await SaveAsync(new() { Version = 8, Description = "Updated sources table" });
					break;
				case 8:
					await SaveAsync(new() { Version = 9, Description = "Updated versions table" });
					break;
				case 9:
					await SaveAsync(new() { Version = 10, Description = "Updated apps table" });
					break;
				case 10:
					await SaveAsync(new() { Version = 11, Description = "Upgraded storage to XPO framework" });
					break;
				case 11:
					await SaveAsync(new() { Version = 12, Description = "Updated apps table" });
					break;
				case 12:
					await SaveAsync(new() { Version = 13, Description = "Updated documents table" });
					break;
				case 13:
					await SaveAsync(new() { Version = 14, Description = "Updated filters table" });
					break;
				case 14:
					await SaveAsync(new() { Version = 15, Description = "Updated messages table" });
					break;
				case 15:
					await SaveAsync(new() { Version = 16, Description = "Updated proxies table" });
					break;
				case 16:
					await SaveAsync(new() { Version = 17, Description = "Updated sources table" });
					break;
				case 17:
					await SaveAsync(new() { Version = 18, Description = "Updated the UID field in the apps table" });
					break;
				case 18:
					await SaveAsync(new() { Version = 19, Description = "Updated the UID field in the documents table" });
					break;
				case 19:
					await SaveAsync(new() { Version = 20, Description = "Updated the UID field in the filters table" });
					break;
				case 20:
					await SaveAsync(new() { Version = 21, Description = "Updated the UID field in the messages table" });
					break;
				case 21:
					await SaveAsync(new() { Version = 22, Description = "Updated the UID field in the proxies table" });
					break;
				case 22:
					await SaveAsync(new() { Version = 23, Description = "Updated the UID field in the sources table" });
					break;
				case 23:
					await SaveAsync(new() { Version = 24, Description = "Updated the UID field in the versions table" });
					break;
				case 24:
					await SaveAsync(new() { Version = 25, Description = "Migrated storage to EF Core" });
					break;
				case 25:
					await SaveAsync(new() { Version = 26, Description = "Updated apps table" });
					break;
				case 26:
					await SaveAsync(new() { Version = 27, Description = "Added contacts table" });
					break;
				case 27:
					await SaveAsync(new() { Version = 28, Description = "Added stories table" });
					break;
				case 28:
					await SaveAsync(new() { Version = 29, Description = "Updated sources table" });
					break;
				case 29:
					await SaveAsync(new() { Version = 30, Description = "Added RowVersion field" });
					break;
				case 30:
					await SaveAsync(new() { Version = 31, Description = "Updated apps table" });
					break;
				case 31:
					await SaveAsync(new() { Version = 32, Description = "Updated apps table" });
					break;
				case 32:
					await SaveAsync(new() { Version = 33, Description = "Updated messages table" });
					break;
				case 33:
					await SaveAsync(new() { Version = 34, Description = "Updated documents table" });
					break;
				case 34:
					await SaveAsync(new() { Version = 35, Description = "Updated sources table" });
					break;
				case 35:
					await SaveAsync(new() { Version = 36, Description = "Updated sources table" });
					break;
			}
			if (versionLast.Version >= LastVersion)
				isLast = true;
		}
	}

	#endregion
}