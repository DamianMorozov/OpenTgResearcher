// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Contacts;

/// <summary> Contact repository </summary>
public sealed class TgEfContactRepository : TgEfRepositoryBase<TgEfContactEntity, TgEfContactDto>, ITgEfContactRepository
{
	#region Public and private fields, properties, constructor

	public TgEfContactRepository() : base() { }

	public TgEfContactRepository(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment) { }

	#endregion

	#region Public and private methods

	public override string ToDebugString() => $"{nameof(TgEfContactRepository)}";

	public override IQueryable<TgEfContactEntity> GetQuery(bool isReadOnly = true) => 
		isReadOnly ? EfContext.Contacts.AsNoTracking() : EfContext.Contacts.AsTracking();

	public override async Task<TgEfStorageResult<TgEfContactEntity>> GetAsync(TgEfContactEntity item, bool isReadOnly = true)
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
				: new TgEfStorageResult<TgEfContactEntity>(TgEnumEntityState.NotExists, item);
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

	private static Expression<Func<TgEfContactEntity, TgEfContactDto>> SelectDto() => item => new TgEfContactDto().GetNewDto(item);

	public async Task<TgEfContactDto> GetDtoAsync(Expression<Func<TgEfContactEntity, bool>> where)
	{
		var dto = await GetQuery().Where(where).Select(SelectDto()).SingleOrDefaultAsync() ?? new TgEfContactDto();
		return dto;
	}

	public async Task<List<TgEfContactDto>> GetListDtosAsync(int take = 0, int skip = 0)
	{
		var dtos = take > 0
			? await GetQuery(isReadOnly: true).Skip(skip).Take(take).Select(SelectDto()).ToListAsync()
			: await GetQuery(isReadOnly: true).Select(SelectDto()).ToListAsync();
		return dtos;
	}

	public async Task<List<TgEfContactDto>> GetListDtosAsync(int take, int skip, Expression<Func<TgEfContactEntity, bool>> where)
	{
		var dtos = take > 0
			? await GetQuery(isReadOnly: true).Where(where).Skip(skip).Take(take).Select(SelectDto()).ToListAsync()
			: await GetQuery(isReadOnly: true).Where(where).Select(SelectDto()).ToListAsync();
		return dtos;
	}

	public override async Task<TgEfStorageResult<TgEfContactEntity>> GetListAsync(int take, int skip, bool isReadOnly = true)
	{
		IList<TgEfContactEntity> items = take > 0 
			? await GetQuery(isReadOnly).Skip(skip).Take(take).ToListAsync() 
			: await GetQuery(isReadOnly).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<TgEfStorageResult<TgEfContactEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfContactEntity, bool>> where, bool isReadOnly = true)
	{
		IList<TgEfContactEntity> items = take > 0
			? await GetQuery(isReadOnly).Where(where).Skip(skip).Take(take).ToListAsync()
			: await GetQuery(isReadOnly).Where(where).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<int> GetCountAsync()
	{
		return await EfContext.Contacts.AsNoTracking().CountAsync();
	}

	public override async Task<int> GetCountAsync(Expression<Func<TgEfContactEntity, bool>> where)
	{
		return await EfContext.Contacts.AsNoTracking().Where(where).CountAsync();
	}

	#endregion

	#region Public and private methods - Delete

	public override async Task<TgEfStorageResult<TgEfContactEntity>> DeleteAllAsync()
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