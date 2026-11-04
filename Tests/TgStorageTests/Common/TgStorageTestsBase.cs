#pragma warning disable NUnit1033

namespace TgStorageTests.Common;

[NonParallelizable]
public abstract class TgStorageTestsBase : TgDisposable
{
    #region Fields, properties, constructor

    protected ILifetimeScope Scope { get; set; }
    protected ITgStorageService StorageManager { get; }
    private readonly IContainer _container;

    protected TgStorageTestsBase(Action<ContainerBuilder>? registerTypes = null, bool isRegisterEfContext = true)
    {
        // Устанавливаем тестовый тип приложения
        TgGlobalTools.SetAppType(TgEnumAppType.Test);

        // Регистрируем зависимости тестового окружения
        var cb = new ContainerBuilder();

        // Регистрируем FusionCache
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
        // Регистрируем репозитории
        cb.RegisterType<TgEfAppRepository>().As<ITgEfAppRepository>();
        cb.RegisterType<TgEfUserRepository>().As<ITgEfUserRepository>();
        cb.RegisterType<TgEfChatUserRepository>().As<ITgEfChatUserRepository>();
        cb.RegisterType<TgEfDocumentRepository>().As<ITgEfDocumentRepository>();
        cb.RegisterType<TgEfFilterRepository>().As<ITgEfFilterRepository>();
        cb.RegisterType<TgEfLicenseRepository>().As<ITgEfLicenseRepository>();
        cb.RegisterType<TgEfMessageRepository>().As<ITgEfMessageRepository>();
        cb.RegisterType<TgEfMessageRelationRepository>().As<ITgEfMessageRelationRepository>();
        cb.RegisterType<TgEfProxyRepository>().As<ITgEfProxyRepository>();
        cb.RegisterType<TgEfSourceRepository>().As<ITgEfSourceRepository>();
        cb.RegisterType<TgEfStoryRepository>().As<ITgEfStoryRepository>();
        cb.RegisterType<TgEfVersionRepository>().As<ITgEfVersionRepository>();
        // Сохраняем SQLite в памяти на время жизни тестового окружения
        if (isRegisterEfContext)
        {
            cb.Register(_ =>
            {
                var context = new TgEfTestContext();
                context.Database.SetConnectionString("Data Source=:memory:");
                context.Database.OpenConnection();
                return context;
            }).As<ITgEfContext>().SingleInstance();
        }
        // Регистрируем службы
        cb.RegisterType<TgStorageService>().As<ITgStorageService>().SingleInstance();
        cb.RegisterType<TgFloodControlService>().As<ITgFloodControlService>().SingleInstance();
        cb.RegisterType<TgConnectClientTest>().As<ITgConnectClientTest>().SingleInstance();
        cb.RegisterType<TgLicenseService>().As<ITgLicenseService>().SingleInstance();
        cb.RegisterType<TgHardwareResourceMonitoringService>().As<ITgHardwareResourceMonitoringService>().SingleInstance();
        cb.RegisterType<LoadStateService>().As<ILoadStateService>().SingleInstance();
        cb.RegisterType<TgBusinessLogicManager>().As<ITgBusinessLogicManager>().SingleInstance();
        // Регистрируем дополнительные типы наследников
        registerTypes?.Invoke(cb);
        // Создаём контейнер тестового окружения
        _container = cb.Build();
        TgGlobalTools.Container = _container;

        // Открываем область зависимостей
        Scope = _container.BeginLifetimeScope();
        StorageManager = Scope.Resolve<ITgStorageService>();

        // Очищаем FusionCache перед проверками
        var fusionCache = Scope.Resolve<IFusionCache>();
        fusionCache.ClearAll();

        // Применяем настоящие миграции и заполняем таблицу версий
        StorageManager.CreateAndUpdateDbAsync().GetAwaiter().GetResult();
    }

    #endregion

    /// <summary> Освобождает тестовое окружение после завершения набора проверок </summary>
    [OneTimeTearDown]
    public void TearDownStorage() => Dispose();

    /// <summary> Освобождает службы, контекст SQLite и контейнер зависимостей </summary>
    public override void ReleaseManagedResources()
    {
        Scope.Dispose();
        _container.Dispose();
    }
}
