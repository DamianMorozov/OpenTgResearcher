namespace TgBusinessLogicTests.Services;

internal sealed class TgHardwareResourceMonitoringServiceTests : BusinessLogicTestsBase
{
    /// <summary> Подготавливает зависимости настоящего сервиса мониторинга </summary>
    public TgHardwareResourceMonitoringServiceTests() : base(RegisterHardwareResourceMonitoringTypes)
    {
        //
    }

    // Создаём новый сервис для каждой проверки без повторного использования освобождённого экземпляра
    private static void RegisterHardwareResourceMonitoringTypes(ContainerBuilder cb)
    {
        cb.RegisterType<TgHardwareResourceMonitoringService>().As<ITgHardwareResourceMonitoringService>().InstancePerDependency();
    }

    /// <summary> Проверяет получение нового сервиса вместо освобождённого предыдущей проверкой </summary>
    [Test]
    public void SeparateResolutionsUseDifferentMonitoringServices()
    {
        using var first = Scope.Resolve<ITgHardwareResourceMonitoringService>();
        using var second = Scope.Resolve<ITgHardwareResourceMonitoringService>();
        Assert.That(second, Is.Not.SameAs(first));
    }

    /// <summary> Проверяет сохранность общего кеша после освобождения сервиса мониторинга </summary>
    [Test]
    public void DisposeDoesNotDisposeSharedCache()
    {
        var cache = Scope.Resolve<IFusionCache>();
        var key = $"monitoring-test-{Guid.NewGuid():N}";
        cache.Set(key, 42);
        Scope.Resolve<ITgHardwareResourceMonitoringService>().Dispose();
        Assert.That(cache.GetOrDefault<int>(key), Is.EqualTo(42));
        cache.Set(key, 43);
        Assert.That(cache.GetOrDefault<int>(key), Is.EqualTo(43));
    }

    /// <summary> Проверяет запрет запуска освобождённого сервиса </summary>
    [Test]
    public void Dispose_ShouldPreventFurtherStart()
    {
        using var service = Scope.Resolve<ITgHardwareResourceMonitoringService>();
        service.Dispose();

        var act = () => service.StartMonitoring();

        Assert.Throws<ObjectDisposedException>(act);
    }

    /// <summary> Проверяет получение метрик и допустимые значения процентов </summary>
    [Test]
    public async Task StartMonitoring_ShouldRaiseMetricsUpdated()
    {
        using var service = Scope.Resolve<ITgHardwareResourceMonitoringService>();
        var tcs = new TaskCompletionSource<TgHardwareMetrics>(TaskCreationOptions.RunContinuationsAsynchronously);

        service.MetricsUpdated += (_, metrics) => tcs.TrySetResult(metrics);

        service.StartMonitoring(TimeSpan.FromMilliseconds(200));

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var completed = await Task.WhenAny(tcs.Task, Task.Delay(-1, cts.Token));

        Assert.That(completed, Is.SameAs(tcs.Task));

        var metrics = await tcs.Task;
        Assert.That(metrics.CpuAppPercent, Is.InRange(0, 100));
        Assert.That(metrics.CpuTotalPercent, Is.InRange(0, 100));
        Assert.That(metrics.MemoryTotalPercent, Is.InRange(0, 100));
        Assert.That(metrics.MemoryAppPercent, Is.InRange(0, 100));

        await service.StopMonitoringAsync(isClose: false);
    }

    /// <summary> Проверяет продолжение мониторинга при повторном запуске </summary>
    [Test]
    public async Task StartMonitoring_Twice_ShouldNotThrow_AndNotRestart()
    {
        using var service = Scope.Resolve<ITgHardwareResourceMonitoringService>();
        service.StartMonitoring(TimeSpan.FromMilliseconds(200));
        service.StartMonitoring(TimeSpan.FromMilliseconds(200));

        var tcs = new TaskCompletionSource<bool>();
        service.MetricsUpdated += (_, __) => tcs.TrySetResult(true);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        var completed = await Task.WhenAny(tcs.Task, Task.Delay(-1, cts.Token));

        Assert.That(completed, Is.SameAs(tcs.Task));

        await service.StopMonitoringAsync(isClose: false);
    }

    /// <summary> Проверяет прекращение событий после остановки и повторную остановку </summary>
    [Test]
    public async Task StopMonitoring_ShouldStopWithoutExceptions()
    {
        using var service = Scope.Resolve<ITgHardwareResourceMonitoringService>();
        var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var count = 0;
        service.MetricsUpdated += (_, _) =>
        {
            Interlocked.Increment(ref count);
            started.TrySetResult(true);
        };
        service.StartMonitoring(TimeSpan.FromMilliseconds(50));
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await service.StopMonitoringAsync(isClose: false);
        var stoppedCount = Volatile.Read(ref count);
        await service.StopMonitoringAsync(isClose: false);
        await Task.Delay(TimeSpan.FromMilliseconds(200));
        Assert.That(Volatile.Read(ref count), Is.EqualTo(stoppedCount));
    }
}
