namespace TgBusinessLogicTests.Services;

internal sealed class TgHardwareResourceMonitoringServiceTests : BusinessLogicTestsBase
{
    public TgHardwareResourceMonitoringServiceTests() : base(RegisterHardwareResourceMonitoringTypes)
    {
        //
    }

    private static void RegisterHardwareResourceMonitoringTypes(ContainerBuilder cb)
    {
        cb.RegisterType<TgHardwareResourceMonitoringService>().As<ITgHardwareResourceMonitoringService>().InstancePerLifetimeScope();
    }

    [Test]
    public void Dispose_ShouldPreventFurtherStart()
    {
        using var service = Scope.Resolve<ITgHardwareResourceMonitoringService>();
        service.Dispose();

        var act = () => service.StartMonitoring();
        
        Assert.Throws<ObjectDisposedException>(act);
    }

    [Test]
    public async Task StartMonitoring_ShouldRaiseMetricsUpdated()
    {
        using var service = Scope.Resolve<ITgHardwareResourceMonitoringService>();
        var tcs = new TaskCompletionSource<TgHardwareMetrics>(TaskCreationOptions.RunContinuationsAsynchronously);

        service.MetricsUpdated += async (s, metrics) =>
        {
            try
            {
                await Assert.That(metrics.CpuAppPercent).IsGreaterThanOrEqualTo(0);
                await Assert.That(metrics.CpuAppPercent).IsLessThanOrEqualTo(100);
                await Assert.That(metrics.CpuAppPercent).IsGreaterThanOrEqualTo(0);
                await Assert.That(metrics.CpuAppPercent).IsLessThanOrEqualTo(100);
                await Assert.That(metrics.CpuTotalPercent).IsGreaterThanOrEqualTo(0);
                await Assert.That(metrics.CpuTotalPercent).IsLessThanOrEqualTo(100);
                await Assert.That(metrics.MemoryTotalPercent).IsGreaterThanOrEqualTo(0);
                await Assert.That(metrics.MemoryTotalPercent).IsLessThanOrEqualTo(100);
                await Assert.That(metrics.MemoryAppPercent).IsGreaterThanOrEqualTo(0);
                await Assert.That(metrics.MemoryAppPercent).IsLessThanOrEqualTo(100);
                tcs.TrySetResult(metrics);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        };

        service.StartMonitoring(TimeSpan.FromMilliseconds(200));

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var completed = await Task.WhenAny(tcs.Task, Task.Delay(-1, cts.Token));

        await Assert.That<Task>(completed).IsSameReferenceAs(tcs.Task);

        await service.StopMonitoringAsync(isClose: false);
    }

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

        await Assert.That<Task>(completed).IsSameReferenceAs(tcs.Task);

        await service.StopMonitoringAsync(isClose: false);
    }

    [Test]
    public async Task StopMonitoring_ShouldStopWithoutExceptions()
    {
        using var service = Scope.Resolve<ITgHardwareResourceMonitoringService>();
        service.StartMonitoring(TimeSpan.FromMilliseconds(200));
        await service.StopMonitoringAsync(isClose: false);

        await service.StopMonitoringAsync(isClose: false);
    }
}
