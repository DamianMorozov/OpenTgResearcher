namespace TgBusinessLogic.Services;

public sealed class TgBusinessLogicManager : TgWebDisposable, ITgBusinessLogicManager
{
    #region Fields, properties, constructor

    private ILifetimeScope Scope { get; } = default!;
    public ITgStorageService StorageManager { get; private set; } = default!;
    public ITgLicenseService LicenseService { get; private set; } = default!;
    public ITgConnectClient ConnectClient { get; private set; } = default!;
    public ITgFloodControlService FloodControlService { get; private set; } = default!;
    public ILoadStateService LoadStateService { get; private set; } = default!;
    public IFusionCache Cache { get; private set; } = default!;

    public TgBusinessLogicManager() : base()
    {
        Scope = TgGlobalTools.Container.BeginLifetimeScope();

        StorageManager = Scope.Resolve<ITgStorageService>();
        LicenseService = Scope.Resolve<ITgLicenseService>(new TypedParameter(typeof(ITgStorageService), StorageManager));
        FloodControlService = Scope.Resolve<ITgFloodControlService>();
        LoadStateService = Scope.Resolve<ILoadStateService>();
        Cache = Scope.Resolve<IFusionCache>();
        ConnectClient = TgGlobalTools.AppType switch
        {
            TgEnumAppType.Console => Scope.Resolve<ITgConnectClientConsole>(
                new TypedParameter(typeof(ITgStorageService), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService),
                new TypedParameter(typeof(IFusionCache), Cache), new TypedParameter(typeof(ILoadStateService), LoadStateService)),
            TgEnumAppType.Desktop => Scope.Resolve<ITgConnectClientDesktop>(
                new TypedParameter(typeof(ITgStorageService), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService),
                new TypedParameter(typeof(IFusionCache), Cache), new TypedParameter(typeof(ILoadStateService), LoadStateService)),
            TgEnumAppType.Blazor => Scope.Resolve<ITgConnectClientBlazor>(
                new TypedParameter(typeof(ITgStorageService), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService),
                new TypedParameter(typeof(IFusionCache), Cache), new TypedParameter(typeof(ILoadStateService), LoadStateService)),
            TgEnumAppType.Test => Scope.Resolve<ITgConnectClientTest>(
                new TypedParameter(typeof(ITgStorageService), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService),
                new TypedParameter(typeof(IFusionCache), Cache), new TypedParameter(typeof(ILoadStateService), LoadStateService)),
            TgEnumAppType.Memory or _ => Scope.Resolve<ITgConnectClient>(
                new TypedParameter(typeof(ITgStorageService), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService),
                new TypedParameter(typeof(IFusionCache), Cache), new TypedParameter(typeof(ILoadStateService), LoadStateService)),
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
                new TypedParameter(typeof(ITgStorageService), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService), new TypedParameter(typeof(ILoadStateService), LoadStateService)),
            TgEnumAppType.Desktop => Scope.Resolve<ITgConnectClientDesktop>(
                new TypedParameter(typeof(ITgStorageService), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService), new TypedParameter(typeof(ILoadStateService), LoadStateService)),
            TgEnumAppType.Blazor => Scope.Resolve<ITgConnectClientBlazor>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment),
                new TypedParameter(typeof(ITgStorageService), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService), new TypedParameter(typeof(ILoadStateService), LoadStateService)),
            TgEnumAppType.Test => Scope.Resolve<ITgConnectClientTest>(
                new TypedParameter(typeof(ITgStorageService), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService), new TypedParameter(typeof(ILoadStateService), LoadStateService)),
            TgEnumAppType.Memory or _ => Scope.Resolve<ITgConnectClient>(
                new TypedParameter(typeof(ITgStorageService), StorageManager), new TypedParameter(typeof(ITgFloodControlService), FloodControlService), new TypedParameter(typeof(ILoadStateService), LoadStateService)),
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
