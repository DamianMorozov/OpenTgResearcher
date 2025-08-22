// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Helpers;

/// <summary> A stream-safe buffer of entities for subsequent batch storage </summary>
/// <typeparam name="TEfEntity">EF entity type</typeparam>
public sealed class TgBufferCacheHelper<TEfEntity>(IFusionCache cache, string cachePrefix, TimeSpan? cacheDuration = null) : ITgDisposable
    where TEfEntity : class, ITgEfEntity<TEfEntity>, new()
{
    #region Public and private fields, properties, constructor

    private readonly IFusionCache Cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly List<TEfEntity> _buffer = [];
    private readonly Lock _bufferLock = new();
    private readonly string _cachePrefix = cachePrefix ?? typeof(TEfEntity).Name;
    private readonly TimeSpan _cacheDuration = cacheDuration ?? TimeSpan.FromMinutes(5);

    #endregion

    #region IDisposable

    /// <summary> To detect redundant calls </summary>
    private bool _disposed;

    /// <summary> Finalizer </summary>
	~TgBufferCacheHelper() => Dispose(false);

    /// <summary> Throw exception if disposed </summary>
    public void CheckIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException($"{nameof(TgDisposable)}: {TgConstants.ObjectHasBeenDisposedOff}!");
    }

    /// <summary> Release managed resources </summary>
    public void ReleaseManagedResources()
    {
        Clear();
    }

    /// <summary> Release unmanaged resources </summary>
    public void ReleaseUnmanagedResources()
    {
        //
    }

    /// <summary> Dispose pattern </summary>
    public void Dispose()
    {
        // Dispose of unmanaged resources
        Dispose(true);
        // Suppress finalization
        GC.SuppressFinalize(this);
    }

    /// <summary> Dispose pattern </summary>
    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        using (_bufferLock.EnterScope())
        {
            // Release managed resources
            if (disposing)
                ReleaseManagedResources();
            // Release unmanaged resources
            ReleaseUnmanagedResources();
            // Flag
            _disposed = true;
        }
    }

    #endregion

    #region Public and private methods

    public int Count
    {
        get
        {
            CheckIfDisposed();

            using (_bufferLock.EnterScope())
                return _buffer.Count;
        }
    }

    /// <summary> Add a single entity to the buffer </summary>
    public void Add(TEfEntity entity)
    {
        CheckIfDisposed();
        
        ArgumentNullException.ThrowIfNull(entity);
        using (_bufferLock.EnterScope())
            _buffer.Add(entity);

        // Put it in the cache by entity key
        var key = GetCacheKey(entity);
        Cache.Set(key, entity, _cacheDuration);
    }

    /// <summary> Add a collection of entities to the buffer </summary>
    public void AddRange(IList<TEfEntity> entities)
    {
        CheckIfDisposed();

        ArgumentNullException.ThrowIfNull(entities);
        using (_bufferLock.EnterScope())
        {
            _buffer.AddRange(entities);
            foreach (var entity in entities)
                Cache.Set(GetCacheKey(entity), entity, _cacheDuration);
        }
    }

    /// <summary> Clear the buffer without returning entities </summary>
    public void Clear()
    {
        CheckIfDisposed();

        using (_bufferLock.EnterScope())
        {
            RemoveFromCache(_buffer);

            _buffer.Clear();
        }
    }

    private void RemoveFromCache(IEnumerable<TEfEntity> entities)
    {
        foreach (var entity in entities)
        {
            var key = GetCacheKey(entity);
            Cache.Remove(key);
        }
    }

    private string GetCacheKey(TEfEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        // For entities implementing ITgEfIdEntity: use its Id property
        if (entity is ITgEfIdEntity<TEfEntity> idEntity)
            return $"{_cachePrefix}:{idEntity.Id}";

        // For TgEfMessageEntity: combine SourceId and Id
        if (entity is TgEfMessageEntity messageEntity)
            return $"{_cachePrefix}:{messageEntity.SourceId}:{messageEntity.Id}";

        // For TgEfMessageRelationEntity: combine all parent and child identifiers
        if (entity is TgEfMessageRelationEntity relationEntity)
            return $"{_cachePrefix}:{relationEntity.ParentSourceId}:{relationEntity.ParentMessageId}:{relationEntity.ChildSourceId}:{relationEntity.ChildMessageId}";

        // Optional: handle unknown entity types explicitly
        throw new NotSupportedException(
            $"Entity type '{entity.GetType().Name}' is not supported for cache key generation.");
    }

    /// <summary> Flush the buffer and return a copy of the entities </summary>
    public List<TEfEntity> Flush()
    {
        CheckIfDisposed();

        using (_bufferLock.EnterScope())
        {
            var copy = new List<TEfEntity>(_buffer);
            _buffer.Clear();

            // Invalidate cache for all items
            RemoveFromCache(copy);

            return copy;
        }
    }

    /// <summary> Flush the buffer if the predicate returns true or if forced </summary>
    public async Task FlushAsync(bool isSaveMessages, bool isForce, Func<IList<TEfEntity>, Task> saveAction, Func<IList<TEfEntity>, Task>? postSaveAction = null)
    {
        if (!isSaveMessages) return;

        await TgCacheUtils.SaveLock.WaitAsync();
        try
        {
            if (Count <= TgGlobalTools.BatchMessagesLimit && !isForce) return;

            var toSave = Flush();
            if (toSave.Count == 0) return;

            await saveAction(toSave);

            if (postSaveAction is not null)
            {
                await postSaveAction(toSave);
            }
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }
        finally
        {
            TgCacheUtils.SaveLock.Release();
        }
    }

    /// <summary> Returns the first element in the buffer that matches the optional predicate or default/null if none is found </summary>
    public TEfEntity? FirstOrDefault(Func<TEfEntity, bool>? predicate = null)
    {
        CheckIfDisposed();

        using (_bufferLock.EnterScope())
        {
            if (_buffer.Count == 0)
                return null;

            if (predicate is null)
                return _buffer.FirstOrDefault();

            return _buffer.FirstOrDefault(predicate);
        }
    }

    #endregion
}
