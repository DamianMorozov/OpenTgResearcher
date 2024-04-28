﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Filters;

public sealed class TgEfFilterRepository(TgEfContext efContext) : TgEfRepositoryBase<TgEfFilterEntity>(efContext)
{
	#region Public and private methods

	public override TgEfOperResult<TgEfFilterEntity> Get(TgEfFilterEntity item, bool isNoTracking)
	{
		TgEfOperResult<TgEfFilterEntity> operResult = base.Get(item, isNoTracking);
		if (operResult.IsExist)
			return operResult;
		TgEfFilterEntity? itemFind = isNoTracking
			? EfContext.Filters.AsNoTracking()
				.SingleOrDefault(x => x.FilterType == item.FilterType && x.Name == item.Name)
			: EfContext.Filters
				.SingleOrDefault(x => x.FilterType == item.FilterType && x.Name == item.Name);
		return itemFind is not null
			? new TgEfOperResult<TgEfFilterEntity>(TgEnumEntityState.IsExist, itemFind)
			: new TgEfOperResult<TgEfFilterEntity>(TgEnumEntityState.NotExist, item);
	}

	public override async Task<TgEfOperResult<TgEfFilterEntity>> GetAsync(TgEfFilterEntity item, bool isNoTracking)
	{
		TgEfOperResult<TgEfFilterEntity> operResult = await base.GetAsync(item, isNoTracking).ConfigureAwait(false);
		if (operResult.IsExist)
			return operResult;
		TgEfFilterEntity? itemFind = isNoTracking
			? await EfContext.Filters.AsNoTracking()
				.SingleOrDefaultAsync(x => x.FilterType == item.FilterType && x.Name == item.Name)
				.ConfigureAwait(false)
			: await EfContext.Filters
				.SingleOrDefaultAsync(x => x.FilterType == item.FilterType && x.Name == item.Name)
				.ConfigureAwait(false);
		return itemFind is not null
			? new TgEfOperResult<TgEfFilterEntity>(TgEnumEntityState.IsExist, itemFind)
			: new TgEfOperResult<TgEfFilterEntity>(TgEnumEntityState.NotExist, item);
	}

	public override TgEfOperResult<TgEfFilterEntity> GetFirst(bool isNoTracking)
	{
		TgEfFilterEntity? item = isNoTracking
			? EfContext.Filters.AsTracking().FirstOrDefault()
			: EfContext.Filters.FirstOrDefault();
		return item is null
			? new TgEfOperResult<TgEfFilterEntity>(TgEnumEntityState.NotExist)
			: new TgEfOperResult<TgEfFilterEntity>(TgEnumEntityState.IsExist, item);
	}

	public override async Task<TgEfOperResult<TgEfFilterEntity>> GetFirstAsync(bool isNoTracking)
	{
		TgEfFilterEntity? item = isNoTracking
			? await EfContext.Filters.AsTracking().FirstOrDefaultAsync().ConfigureAwait(false)
			: await EfContext.Filters.FirstOrDefaultAsync().ConfigureAwait(false);
		return item is null
			? new TgEfOperResult<TgEfFilterEntity>(TgEnumEntityState.NotExist)
			: new TgEfOperResult<TgEfFilterEntity>(TgEnumEntityState.IsExist, item);
	}

	public override TgEfOperResult<TgEfFilterEntity> GetEnumerable(TgEnumTableTopRecords topRecords, bool isNoTracking) =>
		topRecords switch
		{
			TgEnumTableTopRecords.Top1 => GetEnumerable(1, isNoTracking),
			TgEnumTableTopRecords.Top20 => GetEnumerable(20, isNoTracking),
			TgEnumTableTopRecords.Top100 => GetEnumerable(200, isNoTracking),
			TgEnumTableTopRecords.Top1000 => GetEnumerable(1_000, isNoTracking),
			TgEnumTableTopRecords.Top10000 => GetEnumerable(10_000, isNoTracking),
			TgEnumTableTopRecords.Top100000 => GetEnumerable(100_000, isNoTracking),
			TgEnumTableTopRecords.Top1000000 => GetEnumerable(1_000_000, isNoTracking),
			_ => GetEnumerable(0, isNoTracking),
		};

	public override TgEfOperResult<TgEfFilterEntity> GetEnumerable(int count, bool isNoTracking)
	{
		IEnumerable<TgEfFilterEntity> items;
		if (count > 0)
		{
			items = isNoTracking
				? EfContext.Filters.AsNoTracking().Take(count).AsEnumerable()
				: EfContext.Filters.Take(count).AsEnumerable();
		}
		else
		{
			items = isNoTracking
				? EfContext.Filters.AsNoTracking().AsEnumerable()
				: EfContext.Filters.AsEnumerable();
		}
		return new TgEfOperResult<TgEfFilterEntity>(items.Any() ? TgEnumEntityState.IsExist : TgEnumEntityState.NotExist, items);
	}

	public override async Task<TgEfOperResult<TgEfFilterEntity>> GetEnumerableAsync(TgEnumTableTopRecords topRecords, bool isNoTracking) =>
		topRecords switch
		{
			TgEnumTableTopRecords.Top1 => await GetEnumerableAsync(1, isNoTracking).ConfigureAwait(false),
			TgEnumTableTopRecords.Top20 => await GetEnumerableAsync(20, isNoTracking).ConfigureAwait(false),
			TgEnumTableTopRecords.Top100 => await GetEnumerableAsync(200, isNoTracking).ConfigureAwait(false),
			TgEnumTableTopRecords.Top1000 => await GetEnumerableAsync(1_000, isNoTracking).ConfigureAwait(false),
			TgEnumTableTopRecords.Top10000 => await GetEnumerableAsync(10_000, isNoTracking).ConfigureAwait(false),
			TgEnumTableTopRecords.Top100000 => await GetEnumerableAsync(100_000, isNoTracking).ConfigureAwait(false),
			TgEnumTableTopRecords.Top1000000 => await GetEnumerableAsync(1_000_000, isNoTracking).ConfigureAwait(false),
			_ => await GetEnumerableAsync(0, isNoTracking).ConfigureAwait(false),
		};

	public override async Task<TgEfOperResult<TgEfFilterEntity>> GetEnumerableAsync(int count, bool isNoTracking)
	{
		await Task.Delay(TimeSpan.FromMilliseconds(1)).ConfigureAwait(false);
		IEnumerable<TgEfFilterEntity> items;
		if (count > 0)
		{
			items = isNoTracking
				? EfContext.Filters.AsNoTracking().Take(count).AsEnumerable()
				: EfContext.Filters.Take(count).AsEnumerable();
		}
		else
		{
			items = isNoTracking
				? EfContext.Filters.AsNoTracking().AsEnumerable()
				: EfContext.Filters.AsEnumerable();
		}
		return new TgEfOperResult<TgEfFilterEntity>(items.Any() ? TgEnumEntityState.IsExist : TgEnumEntityState.NotExist, items);
	}

	public override int GetCount() => EfContext.Filters.AsNoTracking().Count();

	public override async Task<int> GetCountAsync() => await EfContext.Filters.AsNoTracking().CountAsync().ConfigureAwait(false);

	#endregion

	#region Public and private methods - Write

	//

	#endregion

	#region Public and private methods - Delete

	public override TgEfOperResult<TgEfFilterEntity> DeleteAll()
	{
		TgEfOperResult<TgEfFilterEntity> operResult = GetEnumerable(0, isNoTracking: false);
		if (operResult.IsExist)
		{
			foreach (TgEfFilterEntity item in operResult.Items)
			{
				Delete(item, isSkipFind: true);
			}
		}
		return new TgEfOperResult<TgEfFilterEntity>(operResult.IsExist ? TgEnumEntityState.IsDeleted : TgEnumEntityState.NotDeleted);
	}

	public override async Task<TgEfOperResult<TgEfFilterEntity>> DeleteAllAsync()
	{
		TgEfOperResult<TgEfFilterEntity> operResult = await GetEnumerableAsync(0, isNoTracking: false).ConfigureAwait(false);
		if (operResult.IsExist)
		{
			foreach (TgEfFilterEntity item in operResult.Items)
			{
				await DeleteAsync(item, isSkipFind: true).ConfigureAwait(false);
			}
		}
		return new TgEfOperResult<TgEfFilterEntity>(operResult.IsExist ? TgEnumEntityState.IsDeleted : TgEnumEntityState.NotDeleted);
	}

	#endregion
}