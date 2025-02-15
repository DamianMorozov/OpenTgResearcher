//// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
//// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

//namespace TgStorage.Domain.Bots;

///// <summary> Bot repository </summary>
//public sealed class TgEfBotRepository : TgEfRepositoryBase<TgEfBotEntity>, ITgEfBotRepository
//{
//	#region Public and private methods

//	public override string ToDebugString() => $"{nameof(TgEfBotRepository)}";

//	public override IQueryable<TgEfBotEntity> GetQuery(bool isReadOnly = true) =>
//		isReadOnly ? EfContext.Bots.AsNoTracking() : EfContext.Bots.AsTracking();

//	public override async Task<TgEfStorageResult<TgEfBotEntity>> GetAsync(TgEfBotEntity item, bool isReadOnly = true)
//	{
// Find by Uid
//var itemFind = await EfContext.Bots.FindAsync(item.Uid);
//if (itemFind is not null)
//	return new(TgEnumEntityState.IsExists, itemFind);
// Find by BotToken
//		itemFind = await GetQuery(isReadOnly).Where(x => x.BotToken == item.BotToken).SingleOrDefaultAsync();
//		return itemFind is not null
//			? new(TgEnumEntityState.IsExists, itemFind)
//			: new TgEfStorageResult<TgEfBotEntity>(TgEnumEntityState.NotExists, item);
//	}

//	public override async Task<TgEfStorageResult<TgEfBotEntity>> GetFirstAsync(bool isReadOnly = true)
//	{
//		TgEfBotEntity? item = await GetQuery(isReadOnly).FirstOrDefaultAsync();
//		return item is null
//			? new(TgEnumEntityState.NotExists)
//			: new TgEfStorageResult<TgEfBotEntity>(TgEnumEntityState.IsExists, item);
//	}

//	private static Expression<Func<TgEfBotEntity, TgEfBotDto>> SelectDto() => item => new TgEfBotDto().GetDto(item);

//	public async Task<TgEfBotDto> GetDtoAsync(Expression<Func<TgEfBotEntity, bool>> where)
//	{
//		var dto = await GetQuery().Where(where).Select(SelectDto()).SingleOrDefaultAsync() ?? new TgEfBotDto();
//		return dto;
//	}

//	public async Task<List<TgEfBotDto>> GetListDtosAsync(int take, int skip, bool isReadOnly = true)
//	{
//		var dtos = take > 0
//			? await GetQuery(isReadOnly).Skip(skip).Take(take).Select(SelectDto()).ToListAsync()
//			: await GetQuery(isReadOnly).Select(SelectDto()).ToListAsync();
//		return dtos;
//	}

//	public async Task<List<TgEfBotDto>> GetListDtosAsync(int take, int skip, Expression<Func<TgEfBotEntity, bool>> where, bool isReadOnly = true)
//	{
//		var dtos = take > 0
//			? await GetQuery(isReadOnly).Where(where).Skip(skip).Take(take).Select(SelectDto()).ToListAsync()
//			: await GetQuery(isReadOnly).Where(where).Select(SelectDto()).ToListAsync();
//		return dtos;
//	}

//	public override async Task<TgEfStorageResult<TgEfBotEntity>> GetListAsync(int take, int skip, bool isReadOnly = true)
//	{
//		IList<TgEfBotEntity> items = take > 0
//			? await GetQuery(isReadOnly).Skip(skip).Take(take).ToListAsync()
//			: await GetQuery(isReadOnly).ToListAsync();
//		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
//	}

//	public override async Task<TgEfStorageResult<TgEfBotEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfBotEntity, bool>> where, bool isReadOnly = true)
//	{
//		IList<TgEfBotEntity> items = take > 0
//			? await GetQuery(isReadOnly).Where(where).Skip(skip).Take(take).ToListAsync()
//			: await GetQuery(isReadOnly).Where(where).ToListAsync();
//		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
//	}

//	public override async Task<int> GetCountAsync() => await EfContext.Bots.AsNoTracking().CountAsync();

//	public override async Task<int> GetCountAsync(Expression<Func<TgEfBotEntity, bool>> where) => 
//		await EfContext.Bots.AsNoTracking().Where(where).CountAsync();

//	#endregion

//	#region Public and private methods - Delete

//	public override async Task<TgEfStorageResult<TgEfBotEntity>> DeleteAllAsync()
//	{
//		TgEfStorageResult<TgEfBotEntity> storageResult = await GetListAsync(0, 0, isReadOnly: false);
//		if (storageResult.IsExists)
//		{
//			foreach (TgEfBotEntity item in storageResult.Items)
//			{
//				await DeleteAsync(item);
//			}
//		}
//		return new(storageResult.IsExists ? TgEnumEntityState.IsDeleted : TgEnumEntityState.NotDeleted);
//	}

//	#endregion

//	#region Public and private methods - ITgEfAppRepository

//	//

//	#endregion
//}