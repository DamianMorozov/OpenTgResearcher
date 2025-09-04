// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Helpers;

/// <summary> A stream-safe buffer of entities for subsequent batch storage </summary>
/// <typeparam name="TEntity">EF entity type</typeparam>
public sealed class TgBufferCacheHelper<TEntity>(IFusionCache cache, string cachePrefix, Func<TEntity, string> cacheKeySelector, TimeSpan? cacheDuration = null) 
    : ITgDisposable where TEntity : class
{
    #region Fields, properties, constructor

    private readonly IFusionCache Cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly List<TEntity> _buffer = [];
    private readonly Lock _bufferLock = new();
    private readonly string _cachePrefix = cachePrefix ?? throw new ArgumentNullException(nameof(cachePrefix));
    private readonly TimeSpan _cacheDuration = cacheDuration ?? TimeSpan.FromMinutes(5);
    private readonly Func<TEntity, string> _cacheKeySelector = cacheKeySelector ?? throw new ArgumentNullException(nameof(cacheKeySelector));

    #endregion

    #region IDisposable

    /// <summary> To detect redundant calls </summary>
    private bool _disposed;

    /// <summary> Finalizer </summary>
	~TgBufferCacheHelper() => Dispose(false);

    /// <summary> Throw exception if disposed </summary>
    public void CheckIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

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

    #region Methods

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
    public void Add(TEntity entity)
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
    public void AddRange(IList<TEntity> entities)
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

    private void RemoveFromCache(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            Cache.Remove(GetCacheKey(entity));
        }
    }

    private string GetCacheKey(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var key = _cacheKeySelector(entity);
        return $"{_cachePrefix}:{key}";
    }

    /// <summary> Flush the buffer and return a copy of the entities </summary>
    private List<TEntity> Flush()
    {
        CheckIfDisposed();

        using (_bufferLock.EnterScope())
        {
            var copy = new List<TEntity>(_buffer);
            _buffer.Clear();

            // Invalidate cache for all items
            RemoveFromCache(copy);

            return copy;
        }
    }

    /// <summary> Flush the buffer if the predicate returns true or if forced </summary>
    public async Task FlushAsync(bool isSaveMessages, bool isForce, Func<IList<TEntity>, Task> saveAction, Func<IList<TEntity>, Task>? postSaveAction = null)
    {
        CheckIfDisposed();

        if (!isSaveMessages) return;

        await TgCacheUtils.SaveLock.WaitAsync();
        try
        {
            if (Count <= TgGlobalTools.BatchMessagesLimit && !isForce) return;

            var toSave = Flush();
            if (toSave.Count > 0)
            {
                await saveAction(toSave);
            }
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
    public TEntity? FirstOrDefault(Func<TEntity, bool>? predicate = null)
    {
        CheckIfDisposed();

        using (_bufferLock.EnterScope())
        {
            return _buffer.Count == 0 ? null : predicate is null ? _buffer.FirstOrDefault() : _buffer.FirstOrDefault(predicate);
        }
    }

    /// <summary> Returns a copy of the list of entities in the buffer that match the optional predicate or all if none is provided </summary>
    public List<TEntity> GetList(Func<TEntity, bool>? predicate = null)
    {
        CheckIfDisposed();

        using (_bufferLock.EnterScope())
        {
            return predicate is null ? [.. _buffer] : [.. _buffer.Where(predicate)];
        }
    }

    #endregion
}
