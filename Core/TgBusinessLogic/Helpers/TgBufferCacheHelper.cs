// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Helpers;

/// <summary> A stream-safe buffer of entities for subsequent batch storage </summary>
/// <typeparam name="TEfEntity">EF entity type</typeparam>
public sealed class TgBufferCacheHelper<TEfEntity>(IFusionCache fusionCache, string cachePrefix, TimeSpan? cacheDuration = null)
    : ITgDisposable
    where TEfEntity : class, ITgEfIdEntity<TEfEntity>, new()
{
    #region Public and private fields, properties, constructor

    private readonly Lock _bufferLock = new();
    private readonly List<TEfEntity> _buffer = [];
    private readonly IFusionCache _fusionCache = fusionCache ?? throw new ArgumentNullException(nameof(fusionCache));
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
        _fusionCache.Set(key, entity, _cacheDuration);
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
                _fusionCache.Set(GetCacheKey(entity), entity, _cacheDuration);
        }
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
            _fusionCache.Remove(key);
        }
    }

    public TEfEntity GetFromCache(object id)
    {
        CheckIfDisposed();
        
        return _fusionCache.TryGet<TEfEntity>($"{_cachePrefix}:{id}");
    }

    private string GetCacheKey(TEfEntity entity) => $"{_cachePrefix}:{entity.Id}";

    #endregion
}
