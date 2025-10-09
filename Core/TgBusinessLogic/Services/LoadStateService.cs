namespace TgBusinessLogic.Services;

/// <summary> Load state service to track loading states in the application </summary>
public sealed partial class LoadStateService : ObservableObject, ILoadStateService, IDisposable
{
    #region Fields, properties, constructor

    /// <inheritdoc />
    [ObservableProperty]
    public partial bool IsMemoryProcessing { get; private set; }
    /// <inheritdoc />
    [ObservableProperty]
    public partial bool IsStorageProcessing { get; private set; }
    /// <inheritdoc />
    [ObservableProperty]
    public partial bool IsOnlineProcessing { get; private set; }
    /// <inheritdoc />
    [ObservableProperty]
    public partial bool IsLocatorProcessing { get; private set; }
    /// <inheritdoc />
    [ObservableProperty]
    public partial bool IsDownloadProcessing { get; private set; }
    /// <inheritdoc />
    [ObservableProperty]
    public partial bool IsFloodLogProcessing { get; private set; }
    /// <inheritdoc />
    [ObservableProperty]
    public partial bool IsDisplaySensitiveData { get; private set; }
    /// <inheritdoc />
    [ObservableProperty]
    public partial bool IsOnlineReady { get; private set; }
    /// <inheritdoc />
    public string SensitiveData => "**********";

    /// <inheritdoc />
    public CancellationTokenSource? MemoryCts { get; private set; }
    /// <inheritdoc />
    public CancellationToken MemoryToken { get; private set; } = CancellationToken.None;
    /// <inheritdoc />
    public CancellationTokenSource? StorageCts { get; private set; }
    /// <inheritdoc />
    public CancellationToken StorageToken { get; private set; } = CancellationToken.None;
    /// <inheritdoc />
    public CancellationTokenSource? OnlineCts { get; private set; }
    /// <inheritdoc />
    public CancellationToken OnlineToken { get; private set; } = CancellationToken.None;
    /// <inheritdoc />
    public CancellationTokenSource? LocatorCts { get; private set; }
    /// <inheritdoc />
    public CancellationToken LocatorToken { get; private set; } = CancellationToken.None;
    /// <inheritdoc />
    public CancellationTokenSource? DownloadCts { get; private set; }
    /// <inheritdoc />
    public CancellationToken DownloadToken { get; private set; } = CancellationToken.None;
    /// <inheritdoc />
    public CancellationTokenSource? FloodLogCts { get; private set; }
    /// <inheritdoc />
    public CancellationToken FloodLogToken { get; private set; } = CancellationToken.None;


    private readonly HashSet<Guid> _memoryUids = [];
    private readonly HashSet<Guid> _storageUids = [];
    private readonly HashSet<Guid> _onlineUids = [];
    private readonly HashSet<Guid> _locatorUids = [];
    private readonly HashSet<Guid> _downloadUids = [];
    private readonly HashSet<Guid> _floodLogUids = [];
    private static readonly Lock _locker = new();

    #endregion

    #region IDisposable

    /// <summary> To detect redundant calls </summary>
    private bool _disposed;

    /// <summary> Finalizer </summary>
	~LoadStateService() => Dispose(false);

    /// <summary> Throw exception if disposed </summary>
    public void CheckIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    /// <summary> Release managed resources </summary>
    public void ReleaseManagedResources()
    {
        CheckIfDisposed();
        StopHardAllProcessing();
    }

    /// <summary> Release unmanaged resources </summary>
    public void ReleaseUnmanagedResources()
    {
        CheckIfDisposed();
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
        // Release managed resources
        if (disposing)
            ReleaseManagedResources();
        // Release unmanaged resources
        ReleaseUnmanagedResources();
        // Flag
        _disposed = true;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public void SetIsOnlineReady(bool isReady) => IsOnlineReady = isReady;

    /// <inheritdoc />
    public void SetIsDisplaySensitiveData(bool isDisplaySensitiveData) => IsDisplaySensitiveData = isDisplaySensitiveData;

    /// <inheritdoc />
    public async Task PrepareMemoryTokenAsync(Guid uid)
    {
        try
        {
            IsMemoryProcessing = true;
            using (_locker.EnterScope())
            {
                _memoryUids.Add(uid);
            }

            if (MemoryCts is null || MemoryCts.IsCancellationRequested)
            {
                MemoryCts?.Cancel();
                MemoryCts?.Dispose();
                MemoryCts = new CancellationTokenSource();
                MemoryToken = MemoryCts.Token;
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        catch (Exception ex)
        {
            IsMemoryProcessing = false;
            TgLogUtils.WriteException(ex);
        }
        finally
        {
            await Task.Delay(TgConstants.TimeOutUIShortMilliseconds);
        }
    }

    /// <inheritdoc />
    public async Task PrepareStorageTokenAsync(Guid uid)
    {
        try
        {
            IsStorageProcessing = true;
            using (_locker.EnterScope())
            {
                _storageUids.Add(uid);
            }

            if (StorageCts is null || StorageCts.IsCancellationRequested)
            {
                StorageCts?.Cancel();
                StorageCts?.Dispose();
                StorageCts = new CancellationTokenSource();
                StorageToken = StorageCts.Token;
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        catch (Exception ex)
        {
            IsStorageProcessing = false;
            TgLogUtils.WriteException(ex);
        }
        finally
        {
            await Task.Delay(TgConstants.TimeOutUIShortMilliseconds);
        }
    }

    /// <inheritdoc />
    public async Task PrepareOnlineTokenAsync(Guid uid)
    {
        try
        {
            IsOnlineProcessing = true;
            using (_locker.EnterScope())
            {
                _onlineUids.Add(uid);
            }
            
            if (OnlineCts is null || OnlineCts.IsCancellationRequested)
            {
                OnlineCts?.Cancel();
                OnlineCts?.Dispose();
                OnlineCts = new CancellationTokenSource();
                OnlineToken = OnlineCts.Token;
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        catch (Exception ex)
        {
            IsOnlineProcessing = false;
            TgLogUtils.WriteException(ex);
        }
        finally
        {
            await Task.Delay(TgConstants.TimeOutUIShortMilliseconds);
        }
    }

    /// <inheritdoc />
    public async Task PrepareLocatorTokenAsync(Guid uid)
    {
        try
        {
            IsLocatorProcessing = true;
            using (_locker.EnterScope())
            {
                _locatorUids.Add(uid);
            }
            
            if (LocatorCts is null || LocatorCts.IsCancellationRequested)
            {
                LocatorCts?.Cancel();
                LocatorCts?.Dispose();
                LocatorCts = new CancellationTokenSource();
                LocatorToken = LocatorCts.Token;
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        catch (Exception ex)
        {
            IsLocatorProcessing = false;
            TgLogUtils.WriteException(ex);
        }
        finally
        {
            await Task.Delay(TgConstants.TimeOutUIShortMilliseconds);
        }
    }

    /// <inheritdoc />
    public async Task PrepareDownloadTokenAsync(Guid uid)
    {
        try
        {
            IsDownloadProcessing = true;
            using (_locker.EnterScope())
            {
                _downloadUids.Add(uid);
            }
            
            if (DownloadCts is null || DownloadCts.IsCancellationRequested)
            {
                DownloadCts?.Cancel();
                DownloadCts?.Dispose();
                DownloadCts = new CancellationTokenSource();
                DownloadToken = DownloadCts.Token;
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        catch (Exception ex)
        {
            IsDownloadProcessing = false;
            TgLogUtils.WriteException(ex);
        }
        finally
        {
            await Task.Delay(TgConstants.TimeOutUIShortMilliseconds);
        }
    }

    /// <inheritdoc />
    public async Task PrepareFloodLogTokenAsync(Guid uid)
    {
        try
        {
            IsFloodLogProcessing = true;
            using (_locker.EnterScope())
            {
                _floodLogUids.Add(uid);
            }
            
            if (FloodLogCts is null || FloodLogCts.IsCancellationRequested)
            {
                FloodLogCts?.Cancel();
                FloodLogCts?.Dispose();
                FloodLogCts = new CancellationTokenSource();
                FloodLogToken = FloodLogCts.Token;
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        catch (Exception ex)
        {
            IsFloodLogProcessing = false;
            TgLogUtils.WriteException(ex);
        }
        finally
        {
            await Task.Delay(TgConstants.TimeOutUIShortMilliseconds);
        }
    }

    /// <inheritdoc />
    public void StopHardAllProcessing()
    {
        StopHardOnlineProcessing();
        StopHardStorageProcessing();
        StopHardMemoryProcessing();
        StopHardLocatorProcessing();
        StopHardDownloadProcessing();
        StopHardFloodLogProcessing();
    }

    /// <inheritdoc />
    public void StopHardMemoryProcessing()
    {
        try
        {
            if (MemoryCts is not null && !MemoryCts.IsCancellationRequested)
            {
                MemoryCts.Cancel();
                MemoryCts.Dispose();
                MemoryCts = null;
            }
            if (MemoryToken != CancellationToken.None)
                MemoryToken = CancellationToken.None;
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex, "Error stopping memory processing");
        }
        finally
        {
            using (_locker.EnterScope())
            {
                _memoryUids.Clear();
                IsMemoryProcessing = false;
            }
        }
    }

    /// <inheritdoc />
    public void StopHardStorageProcessing()
    {
        try
        {
            if (StorageCts is not null && !StorageCts.IsCancellationRequested)
            {
                StorageCts.Cancel();
                StorageCts.Dispose();
                StorageCts = null;
            }
            if (StorageToken != CancellationToken.None)
                StorageToken = CancellationToken.None;
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex, "Error stopping storage processing");
        }
        finally
        {
            using (_locker.EnterScope())
            {
                _storageUids.Clear();
                IsStorageProcessing = false;
            }
        }
    }

    /// <inheritdoc />
    public void StopHardOnlineProcessing()
    {
        try
        {
            if (OnlineCts is not null && !OnlineCts.IsCancellationRequested)
            {
                OnlineCts.Cancel();
                OnlineCts.Dispose();
                OnlineCts = null;
            }
            if (OnlineToken != CancellationToken.None)
                OnlineToken = CancellationToken.None;
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex, "Error stopping online processing");
        }
        finally
        {
            using (_locker.EnterScope())
            {
                _onlineUids.Clear();
                IsOnlineProcessing = false;
            }
        }
    }

    /// <inheritdoc />
    public void StopHardLocatorProcessing()
    {
        try
        {
            if (LocatorCts is not null && !LocatorCts.IsCancellationRequested)
            {
                LocatorCts.Cancel();
                LocatorCts.Dispose();
                LocatorCts = null;
            }
            if (LocatorToken != CancellationToken.None)
                LocatorToken = CancellationToken.None;
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex, "Error stopping locator processing");
        }
        finally
        {
            using (_locker.EnterScope())
            {
                _locatorUids.Clear();
                IsLocatorProcessing = false;
            }
        }
    }

    /// <inheritdoc />
    public void StopHardDownloadProcessing()
    {
        try
        {
            if (DownloadCts is not null && !DownloadCts.IsCancellationRequested)
            {
                DownloadCts.Cancel();
                DownloadCts.Dispose();
                DownloadCts = null;
            }
            if (DownloadToken != CancellationToken.None)
                DownloadToken = CancellationToken.None;
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex, "Error stopping download processing");
        }
        finally
        {
            using (_locker.EnterScope())
            {
                _downloadUids.Clear();
                IsDownloadProcessing = false;
            }
        }
    }

    /// <inheritdoc />
    public void StopHardFloodLogProcessing()
    {
        try
        {
            if (FloodLogCts is not null && !FloodLogCts.IsCancellationRequested)
            {
                FloodLogCts.Cancel();
                FloodLogCts.Dispose();
                FloodLogCts = null;
            }
            if (FloodLogToken != CancellationToken.None)
                FloodLogToken = CancellationToken.None;
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex, "Error stopping log processing");
        }
        finally
        {
            using (_locker.EnterScope())
            {
                _floodLogUids.Clear();
                IsFloodLogProcessing = false;
            }
        }
    }

    /// <inheritdoc />
    public void StopSoftMemoryProcessing(Guid uid)
    {
        using (_locker.EnterScope())
        {
            _memoryUids.Remove(uid);
            if (_memoryUids.Count == 0)
            {
                IsMemoryProcessing = false;
            }
        }
    }

    /// <inheritdoc />
    public void StopSoftStorageProcessing(Guid uid)
    {
        using (_locker.EnterScope())
        {
            _storageUids.Remove(uid);
            if (_storageUids.Count == 0)
            {
                IsStorageProcessing = false;
            }
        }
    }

    /// <inheritdoc />
    public void StopSoftOnlineProcessing(Guid uid)
    {
        using (_locker.EnterScope())
        {
            _onlineUids.Remove(uid);
            if (_onlineUids.Count == 0)
            {
                IsOnlineProcessing = false;
            }
        }
    }

    /// <inheritdoc />
    public void StopSoftLocatorProcessing(Guid uid)
    {
        using (_locker.EnterScope())
        {
            _locatorUids.Remove(uid);
            if (_locatorUids.Count == 0)
            {
                IsLocatorProcessing = false;
            }
        }
    }

    /// <inheritdoc />
    public void StopSoftDownloadProcessing(Guid uid)
    {
        using (_locker.EnterScope())
        {
            _downloadUids.Remove(uid);
            if (_downloadUids.Count == 0)
            {
                IsDownloadProcessing = false;
            }
        }
    }

    /// <inheritdoc />
    public void StopSoftFloodLogProcessing(Guid uid)
    {
        using (_locker.EnterScope())
        {
            _floodLogUids.Remove(uid);
            if (_floodLogUids.Count == 0)
            {
                IsFloodLogProcessing = false;
            }
        }
    }

    #endregion
}
