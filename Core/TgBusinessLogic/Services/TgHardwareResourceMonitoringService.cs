namespace TgBusinessLogic.Services;

/// <summary> Hardware resource monitoring service </summary>
public sealed class TgHardwareResourceMonitoringService : ITgHardwareResourceMonitoringService
{
    #region Fields, properties, constructor

    public static readonly SemaphoreSlim _locker = new(initialCount: 1, maxCount: 1);
    private readonly Computer _computer = new() { IsCpuEnabled = true, IsMemoryEnabled = true };
    private readonly IVisitor _visitor = new TgUpdateVisitor();
    private CancellationTokenSource? _cts;
    private Task? _worker;
    private TimeSpan _interval;
    // To calculate the CPU of the current process without unnecessary delays
    private readonly Process _process = Process.GetCurrentProcess();
    private TimeSpan _lastProcCpu;
    private DateTime _lastWall;
    private TgHardwareMetrics _lastMetrics = new();
    private ILifetimeScope _scope = default!;
    private IFusionCache _cache = default!;
    private ISensor? _cpuSensor;
    private ISensor? _memUsedSensor;
    private ISensor? _memAvailableSensor;

    public TgHardwareResourceMonitoringService()
    {
        _computer.Open();
        _scope = TgGlobalTools.Container.BeginLifetimeScope();
        _cache = _scope.Resolve<IFusionCache>();
    }

    #endregion

    #region IDisposable

    /// <summary> To detect redundant calls </summary>
    private bool _disposed;

    /// <summary> Finalizer </summary>
	~TgHardwareResourceMonitoringService() => Dispose(false);

    /// <summary> Throw exception if disposed </summary>
    public void CheckIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    /// <summary> Release managed resources </summary>
    public void ReleaseManagedResources()
    {
        CheckIfDisposed();

        Task.WhenAll(StopMonitoringAsync(isClose: true));
        _cache.Dispose();
        _scope.Dispose();
        _locker.Dispose();
    }

    /// <summary> Release unmanaged resources </summary>
    public void ReleaseUnmanagedResources()
    {
        CheckIfDisposed();

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
    public event EventHandler<TgHardwareMetrics>? MetricsUpdated;

    /// <inheritdoc />
    public void StartMonitoring(TimeSpan? interval = null, CancellationToken ct = default)
    {
        CheckIfDisposed();
        if (_cts != null && _worker != null && !_worker.IsCompleted) return;

        try
        {
            _locker.Wait();
            _interval = interval ?? TimeSpan.FromSeconds(1);
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            // Initialization of the base point for calculating the CPU process
            _lastProcCpu = _process.TotalProcessorTime;
            _lastWall = DateTime.UtcNow;
            CacheSensors();
            _worker = Task.Run(() => RunAsync(_cts.Token), _cts.Token);
        }
        finally
        {
            _locker.Release();
        }
    }

    /// <inheritdoc />
    public async Task StopMonitoringAsync(bool isClose)
    {
        CheckIfDisposed();
        if (_cts == null) return;

        try
        {
            _locker.Wait();

            _cts.Cancel();
            if (_worker != null)
            {
                try { await _worker; }
                catch (OperationCanceledException) { /* ignore */ }
            }

            if (isClose)
                _computer.Close();
            }
        finally
        {
            _cts?.Dispose();
            _cts = null;
            _worker?.Dispose();
            _worker = null;
            _locker.Release();
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                _computer.Accept(_visitor);

                var cpuTotal = GetCpuAndMemoryUsage();

                // Calculation of CPU of the current process by increments
                _lastMetrics.TimestampUtc = DateTime.UtcNow;
                var nowCpu = _process.TotalProcessorTime;

                var deltaCpuMs = (nowCpu - _lastProcCpu).TotalMilliseconds;
                var deltaWallMs = (_lastMetrics.TimestampUtc - _lastWall).TotalMilliseconds;
                _lastMetrics.CpuAppPercent = Math.Clamp(deltaWallMs > 0 ? deltaCpuMs / (deltaWallMs * Environment.ProcessorCount) * 100.0 : 0.0, 0, 100);

                _lastProcCpu = nowCpu;
                _lastWall = _lastMetrics.TimestampUtc;

                _lastMetrics.CpuTotalPercent = Math.Clamp(cpuTotal, 0, 100);
                _lastMetrics.MemoryTotalPercent = (_lastMetrics.MemoryTotalGb > 0) ? Math.Clamp(_lastMetrics.MemoryUsedGb / _lastMetrics.MemoryTotalGb * 100.0, 0, 100) : 0;

                var workingSetBytes = _process.WorkingSet64;
                _lastMetrics.MemoryAppGb = workingSetBytes / (1024f * 1024f * 1024f);
                _lastMetrics.MemoryAppPercent = _lastMetrics.MemoryTotalGb > 0f
                    ? Math.Clamp(_lastMetrics.MemoryAppGb / _lastMetrics.MemoryTotalGb * 100f, 0f, 100f)
                    : 0f;

                MetricsUpdated?.Invoke(this, new TgHardwareMetrics(_lastMetrics.TimestampUtc, _lastMetrics.CpuAppPercent, _lastMetrics.CpuTotalPercent,
                    _lastMetrics.MemoryAppPercent, _lastMetrics.MemoryTotalPercent, _lastMetrics.MemoryAppGb, _lastMetrics.MemoryUsedGb, _lastMetrics.MemoryTotalGb));
            }
            catch (OperationCanceledException ocex)
            {
                Debug.WriteLine(ocex);
                break;
            }
#if DEBUG
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
#else
            catch (Exception)
            {
                //
            }
#endif

            try
            {
                //await Task.Delay(_interval, token).ConfigureAwait(false);
                await Task.Delay(_interval, ct);
            }
            catch (OperationCanceledException) { break; }
        }
    }

    private double GetCpuAndMemoryUsage()
    {
        var key = TgCacheUtils.GetCacheKeyCpuTotal();

        try
        {
            // Try to retrieve from cache in a strictly typed manner
            var maybe = _cache.TryGet<double>(key, TgCacheUtils.CacheOptionsProcessMessage);
            if (maybe.HasValue)
                return maybe.Value;

            // Cache miss: retrieve from server and save
            return GetCpuAndMemoryUsageCore();
        }
        catch (OperationCanceledException)
        {
            // Quietly exit without logging in
            return default;
        }
        catch (InvalidCastException)
        {
            // There is another type in the cache: clear and reload
            _cache.Remove(key);
            return GetCpuAndMemoryUsageCore();
        }
    }

    private double GetCpuAndMemoryUsageCore()
    {
        double cpuTotal = 0;
        foreach (var hw in _computer.Hardware)
        {
            switch (hw.HardwareType)
            {
                case HardwareType.Cpu:
                    cpuTotal = _cpuSensor?.Value ?? 0;
                    break;
                case HardwareType.Memory:
                    _lastMetrics.MemoryUsedGb = _memUsedSensor?.Value ?? 0;
                    _lastMetrics.MemoryTotalGb = _lastMetrics.MemoryUsedGb + (_memAvailableSensor?.Value ?? 0);
                    break;
            }
        }
        return cpuTotal;
    }

    private void CacheSensors()
    {
        foreach (var hw in _computer.Hardware)
        {
            if (hw.HardwareType == HardwareType.Cpu)
                _cpuSensor = hw.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name == "CPU Total");

            if (hw.HardwareType == HardwareType.Memory)
            {
                _memUsedSensor = hw.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Data && s.Name.Contains("Memory Used"));
                _memAvailableSensor = hw.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Data && s.Name.Contains("Memory Available"));
            }
        }
    }

    #endregion
}
