// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Repositories;

/// <summary> User repository </summary>
public sealed class TgEfUserRepository : TgEfRepositoryBase<TgEfUserEntity, TgEfUserDto>, ITgEfUserRepository
{
	#region Public and private fields, properties, constructor

	public TgEfUserRepository() : base() { }

	public TgEfUserRepository(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment) { }

	#endregion

	#region Public and private methods

	public override async Task<TgEfStorageResult<TgEfUserEntity>> GetAsync(TgEfUserEntity item, bool isReadOnly = true)
	{
		try
		{
			// Find by Uid
			var itemFind = await GetQuery(isReadOnly)
				.Where(x => x.Uid == item.Uid)
				.FirstOrDefaultAsync();
			if (itemFind is not null)
				return new(TgEnumEntityState.IsExists, itemFind);
			// Find by ID
			itemFind = await GetQuery(isReadOnly).SingleOrDefaultAsync(x => x.Id == item.Id);
			return itemFind is not null
				? new(TgEnumEntityState.IsExists, itemFind)
				: new TgEfStorageResult<TgEfUserEntity>(TgEnumEntityState.NotExists, item);
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

	public override async Task<TgEfStorageResult<TgEfUserEntity>> GetListAsync(int take, int skip, bool isReadOnly = true)
	{
		IList<TgEfUserEntity> items = take > 0 
			? await GetQuery(isReadOnly).Skip(skip).Take(take).ToListAsync() 
			: await GetQuery(isReadOnly).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<TgEfStorageResult<TgEfUserEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfUserEntity, bool>> where, bool isReadOnly = true)
	{
		IList<TgEfUserEntity> items = take > 0
			? await GetQuery(isReadOnly).Where(where).Skip(skip).Take(take).ToListAsync()
			: await GetQuery(isReadOnly).Where(where).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    public override async Task<int> GetCountAsync() => await EfContext.Users.AsNoTracking().CountAsync();

    public override async Task<int> GetCountAsync(Expression<Func<TgEfUserEntity, bool>> where) => await EfContext.Users.AsNoTracking().Where(where).CountAsync();

    #endregion

    #region Public and private methods - Delete

    public override async Task<TgEfStorageResult<TgEfUserEntity>> DeleteAllAsync()
	{
		var storageResult = await GetListAsync(0, 0, isReadOnly: false);
		if (storageResult.IsExists)
		{
			foreach (var item in storageResult.Items)
			{
				await DeleteAsync(item);
			}
		}
		return new(storageResult.IsExists ? TgEnumEntityState.IsDeleted : TgEnumEntityState.NotDeleted);
	}

	#endregion
}