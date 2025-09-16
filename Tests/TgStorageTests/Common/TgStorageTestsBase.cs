#pragma warning disable NUnit1033

using Autofac.Extensions.DependencyInjection;

namespace TgStorageTests.Common;

public class TgStorageTestsBase : TgDisposable
{
    #region Fields, properties, constructor

    protected ILifetimeScope Scope { get; set; }
    protected ITgStorageService StorageManager { get; }

    protected TgStorageTestsBase(Action<ContainerBuilder>? registerTypes = null, bool isRegisterEfContext = true)
    {
        // Set app type
        TgGlobalTools.SetAppType(TgEnumAppType.Test);

        var services = new ServiceCollection();
        if (isRegisterEfContext)
        {
            // Get storage path
            var context = new TgEfTestContext();
            var storagePath = context.GetDataSource();
            // Register the DbContext with SQLite
            services.AddDbContextPool<TgEfConsoleContext>(options =>
            {
                options.UseSqlite(storagePath);
                // Copy by TgEfConsoleContext.OnConfiguring
                options
#if DEBUG
                    .LogTo(message => Debug.WriteLine($"{TgGlobalTools.AppType}: {message}", TgConstants.LogTypeStorage), LogLevel.Debug)
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging()
#endif
                    .EnableThreadSafetyChecks()
                    .UseLoggerFactory(new LoggerFactory());

            }, poolSize: 128);

            services.AddScoped<ITgEfContext>(sp => sp.GetRequiredService<TgEfTestContext>());
        }

        // DI register
        var cb = new ContainerBuilder();
        cb.Populate(services);

        // Register FusionCache
        cb.RegisterInstance<IFusionCache>(new FusionCache(
            new FusionCacheOptions
            {
                DefaultEntryOptions =
                {
                    Duration = TimeSpan.FromSeconds(30),
                    JitterMaxDuration = TimeSpan.FromSeconds(3),
                    IsFailSafeEnabled = true,
                    FailSafeMaxDuration = TimeSpan.FromMinutes(1),
                    EagerRefreshThreshold = 0.8f
                }
            }))
            .SingleInstance();
        // Registering repositories
        cb.RegisterType<TgEfAppRepository>().As<ITgEfAppRepository>();
        cb.RegisterType<TgEfUserRepository>().As<ITgEfUserRepository>();
        cb.RegisterType<TgEfDocumentRepository>().As<ITgEfDocumentRepository>();
        cb.RegisterType<TgEfFilterRepository>().As<ITgEfFilterRepository>();
        cb.RegisterType<TgEfLicenseRepository>().As<ITgEfLicenseRepository>();
        cb.RegisterType<TgEfMessageRepository>().As<ITgEfMessageRepository>();
        cb.RegisterType<TgEfMessageRelationRepository>().As<ITgEfMessageRelationRepository>();
        cb.RegisterType<TgEfProxyRepository>().As<ITgEfProxyRepository>();
        cb.RegisterType<TgEfSourceRepository>().As<ITgEfSourceRepository>();
        cb.RegisterType<TgEfStoryRepository>().As<ITgEfStoryRepository>();
        cb.RegisterType<TgEfVersionRepository>().As<ITgEfVersionRepository>();
        // Registering the EF context
        if (isRegisterEfContext)
            cb.RegisterType<TgEfTestContext>().As<ITgEfContext>().SingleInstance();
        // Registering services
        cb.RegisterType<TgStorageService>().As<ITgStorageService>().SingleInstance();
        cb.RegisterType<TgFloodControlService>().As<ITgFloodControlService>().SingleInstance();
        cb.RegisterType<TgConnectClientTest>().As<ITgConnectClientTest>().SingleInstance();
        cb.RegisterType<TgLicenseService>().As<ITgLicenseService>().SingleInstance();
        cb.RegisterType<TgHardwareResourceMonitoringService>().As<ITgHardwareResourceMonitoringService>().SingleInstance();
        cb.RegisterType<TgBusinessLogicManager>().As<ITgBusinessLogicManager>().SingleInstance();
        // Registering outside types
        registerTypes?.Invoke(cb);
        // Building the container
        var container = cb.Build();
        TgGlobalTools.Container = container;

        // TgGlobalTools
        Scope = TgGlobalTools.Container.BeginLifetimeScope();
        StorageManager = Scope.Resolve<ITgStorageService>();

        // Clear FusionCache on startup
        var fusionCache = Scope.Resolve<IFusionCache>();
        fusionCache.ClearAll();

        // Create and update storage
        var task = StorageManager.CreateAndUpdateDbAsync();
        task.Wait();
    }

    #endregion
}
