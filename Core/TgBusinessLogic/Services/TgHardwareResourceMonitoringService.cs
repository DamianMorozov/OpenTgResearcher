// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Services;

/// <summary> Hardware resource monitoring service </summary>
public sealed class TgHardwareResourceMonitoringService : ITgHardwareResourceMonitoringService
{
    #region Fields, properties, constructor

    private readonly Lock _hardwareLocker = new();
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

    public TgHardwareResourceMonitoringService()
    {
        _computer.Open();
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
    public async Task StartMonitoringAsync(TimeSpan? interval = null, CancellationToken ct = default)
    {
        CheckIfDisposed();

        using (_hardwareLocker.EnterScope())
        {
            if (_cts != null && _worker != null && !_worker.IsCompleted)
                return;

            _interval = interval ?? TimeSpan.FromSeconds(1);
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            // Initialization of the base point for calculating the CPU process
            _lastProcCpu = _process.TotalProcessorTime;
            _lastWall = DateTime.UtcNow;

            _worker = Task.Run(() => RunAsync(_cts.Token), _cts.Token);
        }

        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopMonitoringAsync(bool isClose)
    {
        CheckIfDisposed();

        Task? workerToAwait = null;
        using (_hardwareLocker.EnterScope())
        {
            if (_cts == null) return;

            _cts.Cancel();
            workerToAwait = _worker;

            _cts.Dispose();
            _cts = null;
            _worker = null;
        }

        if (workerToAwait != null)
        {
            try { await workerToAwait.ConfigureAwait(false); }
            catch (OperationCanceledException) { /* ignore */ }
        }

        if (isClose)
            _computer.Close();
    }

    private async Task RunAsync(CancellationToken ct)
    {
        double cpuTotal = 0;
        double memUsedGb = 0;
        double memTotalGb = 0;

        while (!ct.IsCancellationRequested)
        {
            try
            {
                _computer.Accept(_visitor);

                foreach (var hw in _computer.Hardware)
                {
                    switch (hw.HardwareType)
                    {
                        case HardwareType.Cpu:
                            foreach (var sensor in hw.Sensors)
                            {
                                if (sensor.SensorType == SensorType.Load && sensor.Name == "CPU Total")
                                    cpuTotal = sensor.Value ?? 0;
                            }
                            break;
                        case HardwareType.Memory:
                            foreach (var sensor in hw.Sensors)
                            {
                                if (sensor.SensorType == SensorType.Data)
                                {
                                    if (sensor.Name.Contains("Memory Used"))
                                        memUsedGb = sensor.Value ?? 0;
                                    if (sensor.Name.Contains("Memory Available"))
                                        memTotalGb = memUsedGb + (sensor.Value ?? 0);
                                }
                            }
                            break;
                    }
                }

                // Calculation of CPU of the current process by increments
                _lastMetrics.TimestampUtc = DateTime.UtcNow;
                var nowCpu = _process.TotalProcessorTime;

                var deltaCpuMs = (nowCpu - _lastProcCpu).TotalMilliseconds;
                var deltaWallMs = (_lastMetrics.TimestampUtc - _lastWall).TotalMilliseconds;
                _lastMetrics.CpuAppPercent = Math.Clamp(deltaWallMs > 0 ? (deltaCpuMs / (deltaWallMs * Environment.ProcessorCount)) * 100.0 : 0.0, 0, 100);

                _lastProcCpu = nowCpu;
                _lastWall = _lastMetrics.TimestampUtc;

                _lastMetrics.CpuTotalPercent = Math.Clamp(cpuTotal, 0, 100);
                _lastMetrics.MemoryTotalPercent = (memTotalGb > 0) ? Math.Clamp(memUsedGb / memTotalGb * 100.0, 0, 100) : 0;

                var workingSetBytes = _process.WorkingSet64;
                var memAppGb = workingSetBytes / (1024f * 1024f * 1024f);
                _lastMetrics.MemoryAppPercent = memTotalGb > 0f
                    ? Math.Clamp((memAppGb / memTotalGb) * 100f, 0f, 100f)
                    : 0f;

                MetricsUpdated?.Invoke(this, _lastMetrics);
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

    #endregion
}
