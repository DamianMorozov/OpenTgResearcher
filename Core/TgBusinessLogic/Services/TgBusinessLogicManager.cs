// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Services;

public sealed class TgBusinessLogicManager : TgWebDisposable, ITgBusinessLogicManager
{
    #region Fields, properties, constructor

    private ILifetimeScope Scope { get; } = default!;
    public ITgStorageService StorageManager { get; private set; } = default!;
    public ITgLicenseService LicenseService { get; private set; } = default!;
    public ITgConnectClient ConnectClient { get; private set; } = default!;
    public ITgFloodControlService FloodControlService { get; private set; } = default!;
    public IFusionCache Cache { get; private set; } = default!;

    public TgBusinessLogicManager() : base()
    {
        Scope = TgGlobalTools.Container.BeginLifetimeScope();

        StorageManager = Scope.Resolve<ITgStorageService>();
        LicenseService = Scope.Resolve<ITgLicenseService>(new TypedParameter(typeof(ITgStorageService), StorageManager));
        FloodControlService = Scope.Resolve<ITgFloodControlService>();
        Cache = Scope.Resolve<IFusionCache>();
        ConnectClient = TgGlobalTools.AppType switch
        {
            TgEnumAppType.Console => Scope.Resolve<ITgConnectClientConsole>(
                new TypedParameter(typeof(ITgStorageService), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService),
                new TypedParameter(typeof(IFusionCache), Cache)),
            TgEnumAppType.Desktop => Scope.Resolve<ITgConnectClientDesktop>(
                new TypedParameter(typeof(ITgStorageService), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService),
                new TypedParameter(typeof(IFusionCache), Cache)),
            TgEnumAppType.Blazor => Scope.Resolve<ITgConnectClientBlazor>(
                new TypedParameter(typeof(ITgStorageService), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService),
                new TypedParameter(typeof(IFusionCache), Cache)),
            TgEnumAppType.Test => Scope.Resolve<ITgConnectClientTest>(
                new TypedParameter(typeof(ITgStorageService), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService),
                new TypedParameter(typeof(IFusionCache), Cache)),
            TgEnumAppType.Memory or _ => Scope.Resolve<ITgConnectClient>(
                new TypedParameter(typeof(ITgStorageService), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService),
                new TypedParameter(typeof(IFusionCache), Cache)),
        };
    }

    public TgBusinessLogicManager(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment)
    {
        Scope = TgGlobalTools.Container.BeginLifetimeScope();

        StorageManager = Scope.Resolve<ITgStorageService>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
        LicenseService = Scope.Resolve<ITgLicenseService>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment), new TypedParameter(typeof(ITgStorageService), StorageManager));
        FloodControlService = Scope.Resolve<ITgFloodControlService>();
        Cache = Scope.Resolve<IFusionCache>();
        ConnectClient = TgGlobalTools.AppType switch
        {
            TgEnumAppType.Console => Scope.Resolve<ITgConnectClientConsole>(
                new TypedParameter(typeof(ITgStorageService), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService)),
            TgEnumAppType.Desktop => Scope.Resolve<ITgConnectClientDesktop>(
                new TypedParameter(typeof(ITgStorageService), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService)),
            TgEnumAppType.Blazor => Scope.Resolve<ITgConnectClientBlazor>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment),
                new TypedParameter(typeof(ITgStorageService), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService)),
            TgEnumAppType.Test => Scope.Resolve<ITgConnectClientTest>(
                new TypedParameter(typeof(ITgStorageService), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService)),
            TgEnumAppType.Memory or _ => Scope.Resolve<ITgConnectClient>(
                new TypedParameter(typeof(ITgStorageService), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService)),
        };
    }

    #endregion

    #region IDisposable

    /// <summary> Release managed resources </summary>
    public override void ReleaseManagedResources()
    {
        ConnectClient.Dispose();
        LicenseService.Dispose();
        StorageManager.Dispose();
        FloodControlService.Dispose();
        Cache.Dispose();

        Scope.Dispose();
        TgGlobalTools.Container.Dispose();
    }

    /// <summary> Release unmanaged resources </summary>
    public override void ReleaseUnmanagedResources()
    {
        //
    }

    #endregion

    #region Methods

    #endregion
}
