namespace OpenTgResearcherDesktop.ConnectionSelfTests;

// Проверяем загрузку настроек и прокси исходной страницей при занятом контексте фоновой операции
internal static partial class Program
{
    private sealed class PageContext(DbContextOptions options) : TgEfContextBase(options)
    {
        // Используем исключительно отдельную SQLite в памяти
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }
    }

    private sealed class PageQueryGate : DbCommandInterceptor
    {
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public bool HoldNext { get; set; }

        /// <summary> Удерживает один настоящий запрос, не блокируя контексты читателей </summary>
        public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData,
            InterceptionResult<DbDataReader> result, CancellationToken ct = default)
        {
            if (HoldNext) { HoldNext = false; await Release.Task.WaitAsync(ct); }
            return result;
        }
    }

    // Открытие подключения не должно переподключать клиент, менять профиль или ждать завершения фонового запроса
    private static async Task ConnectionReadsWhileParserBusyAsync()
    {
        var gate = new PageQueryGate();
        var options = new DbContextOptionsBuilder().UseSqlite($"Data Source=connection-{Guid.NewGuid():N};Mode=Memory;Cache=Shared")
            .AddInterceptors(gate).Options;
        await using var context = new PageContext(options);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();
        var proxyUid = Guid.NewGuid(); var appUid = Guid.NewGuid();
        context.Proxies.Add(new() { Uid = proxyUid, Type = TgEnumProxyType.Socks, UserName = "test-proxy" });
        context.Apps.Add(new() { Uid = appUid, ApiId = 123, ApiHash = Guid.NewGuid(), ProxyUid = proxyUid });
        await context.SaveChangesAsync();
        var builder = new ContainerBuilder();
        builder.Register(_ => new PageContext(options)).As<ITgEfContext>().InstancePerLifetimeScope();
        using var container = builder.Build();
        TgStorage.Utils.TgGlobalTools.Container = container;
        using var root = new TgEfAppRepository();
        App.BusinessLogicManager.StorageManager.AppRepository.ReadCurrent = ct => root.GetCurrentDtoAsync(ct);
        var state = new TestLoadStateService { IsOnlineProcessing = true };
        var vm = new TgClientConnectionViewModel(state, new TestSettingsService(), null!, null!);
        gate.HoldNext = true;
        var pending = root.GetCurrentDtoAsync();
        try
        {
            await vm.OnNavigatedToAsync(null).WaitAsync(TimeSpan.FromSeconds(3));
            Assert(vm.Errors.Count == 0 && vm.AppDto?.Uid == appUid && vm.AppDto.ApiId == 123 && vm.ProxyVm?.Dto.Uid == proxyUid,
                "Страница не прочитала сохранённые настройки и прокси при занятом контексте парсера");
            Assert(state.IsOnlineProcessing && App.BusinessLogicManager.ConnectClient.ConnectCalls == 0
                && App.BusinessLogicManager.ConnectClient.DisconnectCalls == 0 && App.BusinessLogicManager.StorageManager.AppRepository.SaveCalls == 0,
                "Просмотр настроек вмешался в подключение или перезаписал профиль");
        }
        finally { gate.Release.TrySetResult(); await pending; }
    }
}
