// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Helpers;

public sealed class TgBusinessLogicManager : TgWebDisposable, ITgBusinessLogicManager
{
    #region Public and private fields, properties, constructor

    public ITgStorageManager StorageManager { get; private set; } = default!;
    public ITgLicenseService LicenseService { get; private set; } = default!;
    public ITgConnectClient ConnectClient { get; private set; } = default!;

    public TgBusinessLogicManager() : base()
    {
        var scope = TgGlobalTools.Container.BeginLifetimeScope();
        StorageManager = scope.Resolve<ITgStorageManager>();
        LicenseService = scope.Resolve<ITgLicenseService>(new TypedParameter(typeof(TgStorageManager), StorageManager));
        ConnectClient = TgGlobalTools.AppType switch
        {
            TgEnumAppType.Console => scope.Resolve<ITgConnectClientConsole>(new TypedParameter(typeof(TgStorageManager), StorageManager)),
            TgEnumAppType.Desktop => scope.Resolve<ITgConnectClientDesktop>(new TypedParameter(typeof(TgStorageManager), StorageManager)),
            TgEnumAppType.Blazor => scope.Resolve<ITgConnectClientBlazor>(new TypedParameter(typeof(TgStorageManager), StorageManager)),
            TgEnumAppType.Test => scope.Resolve<ITgConnectClientTest>(new TypedParameter(typeof(TgStorageManager), StorageManager)),
            TgEnumAppType.Memory or _ => scope.Resolve<ITgConnectClient>(new TypedParameter(typeof(TgStorageManager), StorageManager)),
        };
    }

    public TgBusinessLogicManager(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment)
    {
        var scope = TgGlobalTools.Container.BeginLifetimeScope();
        StorageManager = scope.Resolve<ITgStorageManager>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
        LicenseService = scope.Resolve<ITgLicenseService>(new TypedParameter(typeof(TgStorageManager), StorageManager));
        ConnectClient = TgGlobalTools.AppType switch
        {
            TgEnumAppType.Console => scope.Resolve<ITgConnectClientConsole>(new TypedParameter(typeof(TgStorageManager), StorageManager)),
            TgEnumAppType.Desktop => scope.Resolve<ITgConnectClientDesktop>(new TypedParameter(typeof(TgStorageManager), StorageManager)),
            TgEnumAppType.Blazor => scope.Resolve<ITgConnectClientBlazor>(new TypedParameter(typeof(TgStorageManager), StorageManager)),
            TgEnumAppType.Test => scope.Resolve<ITgConnectClientTest>(new TypedParameter(typeof(TgStorageManager), StorageManager)),
            TgEnumAppType.Memory or _ => scope.Resolve<ITgConnectClient>(new TypedParameter(typeof(TgStorageManager), StorageManager)),
        };
    }

    #endregion

    #region IDisposable

    /// <summary> Release managed resources </summary>
    public override void ReleaseManagedResources()
    {
        //
    }

    /// <summary> Release unmanaged resources </summary>
    public override void ReleaseUnmanagedResources()
    {
        ConnectClient.Dispose();
        LicenseService.Dispose();
        StorageManager.Dispose();
    }

    #endregion
}
