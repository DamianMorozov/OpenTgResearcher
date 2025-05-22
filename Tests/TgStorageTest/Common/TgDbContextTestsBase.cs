// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
#pragma warning disable NUnit1033

namespace TgStorageTest.Common;

internal abstract class TgDbContextTestsBase : TgDisposable
{
    #region Public and private fields, properties, constructor

    protected ITgBusinessLogicManager BusinessLogicManager { get; }
    private ILifetimeScope Scope { get; }

    protected TgDbContextTestsBase()
    {
        // Set app type
        TgGlobalTools.SetAppType(TgEnumAppType.Test);

        // DI register
        var containerBuilder = new ContainerBuilder();
        // Registering repositories
        containerBuilder.RegisterType<TgEfAppRepository>().As<ITgEfAppRepository>();
        containerBuilder.RegisterType<TgEfContactRepository>().As<ITgEfContactRepository>();
        containerBuilder.RegisterType<TgEfDocumentRepository>().As<ITgEfDocumentRepository>();
        containerBuilder.RegisterType<TgEfFilterRepository>().As<ITgEfFilterRepository>();
        containerBuilder.RegisterType<TgEfLicenseRepository>().As<ITgEfLicenseRepository>();
        containerBuilder.RegisterType<TgEfMessageRepository>().As<ITgEfMessageRepository>();
        containerBuilder.RegisterType<TgEfProxyRepository>().As<ITgEfProxyRepository>();
        containerBuilder.RegisterType<TgEfSourceRepository>().As<ITgEfSourceRepository>();
        containerBuilder.RegisterType<TgEfStoryRepository>().As<ITgEfStoryRepository>();
        containerBuilder.RegisterType<TgEfVersionRepository>().As<ITgEfVersionRepository>();
        // Registering services
        containerBuilder.RegisterType<TgStorageManager>().As<ITgStorageManager>();
        containerBuilder.RegisterType<TgEfTestContext>().As<ITgEfContext>();
        containerBuilder.RegisterType<TgConnectClientTest>().As<ITgConnectClientTest>();
        containerBuilder.RegisterType<TgLicenseService>().As<ITgLicenseService>();
        containerBuilder.RegisterType<TgBusinessLogicManager>().As<ITgBusinessLogicManager>();
        // Building the container
        TgGlobalTools.Container = containerBuilder.Build();

        // TgGlobalTools
        Scope = TgGlobalTools.Container.BeginLifetimeScope();
        BusinessLogicManager = Scope.Resolve<ITgBusinessLogicManager>();
        
        // Create and update storage
        var task = BusinessLogicManager.CreateAndUpdateDbAsync();
        task.Wait();
    }

    #endregion

    #region IDisposable

    /// <summary> Release managed resources </summary>
    public override void ReleaseManagedResources()
    {
        Scope.Dispose();
    }

    /// <summary> Release unmanaged resources </summary>
    public override void ReleaseUnmanagedResources()
    {
        //
    }

    #endregion
}