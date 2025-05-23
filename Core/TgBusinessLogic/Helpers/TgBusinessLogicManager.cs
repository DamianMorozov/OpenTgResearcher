// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Helpers;

public sealed class TgBusinessLogicManager : TgWebDisposable, ITgBusinessLogicManager
{
    #region Public and private fields, properties, constructor

    public ITgStorageManager StorageManager { get; private set; } = default!;
    public ITgLicenseService LicenseService { get; private set; } = default!;
    public ITgConnectClient ConnectClient { get; private set; } = default!;

    private ILifetimeScope Scope { get; }

    public TgBusinessLogicManager() : base()
    {
        Scope = TgGlobalTools.Container.BeginLifetimeScope();

        StorageManager = Scope.Resolve<ITgStorageManager>();
        LicenseService = Scope.Resolve<ITgLicenseService>(new TypedParameter(typeof(TgStorageManager), StorageManager));
        ConnectClient = TgGlobalTools.AppType switch
        {
            TgEnumAppType.Console => Scope.Resolve<ITgConnectClientConsole>(new TypedParameter(typeof(TgStorageManager), StorageManager)),
            TgEnumAppType.Desktop => Scope.Resolve<ITgConnectClientDesktop>(new TypedParameter(typeof(TgStorageManager), StorageManager)),
            TgEnumAppType.Blazor => Scope.Resolve<ITgConnectClientBlazor>(new TypedParameter(typeof(TgStorageManager), StorageManager)),
            TgEnumAppType.Test => Scope.Resolve<ITgConnectClientTest>(new TypedParameter(typeof(TgStorageManager), StorageManager)),
            TgEnumAppType.Memory or _ => Scope.Resolve<ITgConnectClient>(new TypedParameter(typeof(TgStorageManager), StorageManager)),
        };
    }

    public TgBusinessLogicManager(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment)
    {
        Scope = TgGlobalTools.Container.BeginLifetimeScope();

        StorageManager = Scope.Resolve<ITgStorageManager>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
        LicenseService = Scope.Resolve<ITgLicenseService>(new TypedParameter(typeof(TgStorageManager), StorageManager));
        ConnectClient = TgGlobalTools.AppType switch
        {
            TgEnumAppType.Console => Scope.Resolve<ITgConnectClientConsole>(new TypedParameter(typeof(TgStorageManager), StorageManager)),
            TgEnumAppType.Desktop => Scope.Resolve<ITgConnectClientDesktop>(new TypedParameter(typeof(TgStorageManager), StorageManager)),
            TgEnumAppType.Blazor => Scope.Resolve<ITgConnectClientBlazor>(new TypedParameter(typeof(TgStorageManager), StorageManager)),
            TgEnumAppType.Test => Scope.Resolve<ITgConnectClientTest>(new TypedParameter(typeof(TgStorageManager), StorageManager)),
            TgEnumAppType.Memory or _ => Scope.Resolve<ITgConnectClient>(new TypedParameter(typeof(TgStorageManager), StorageManager)),
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

        Scope.Dispose();
    }

    /// <summary> Release unmanaged resources </summary>
    public override void ReleaseUnmanagedResources()
    {
        //
    }

    #endregion

    #region Public and private methods

    public void VersionsView()
    {
        var storageResult = StorageManager.VersionRepository.GetList(TgEnumTableTopRecords.All, 0);
        if (storageResult.IsExists)
        {
            foreach (var version in storageResult.Items)
            {
                TgLogHelper.Instance.WriteLine($" {version.Version:00} | {version.Description}");
            }
        }
    }

    /// <summary> Create and update storage </summary>
    public async Task CreateAndUpdateDbAsync()
    {
        await StorageManager.EfContext.MigrateDbAsync();
        await StorageManager.VersionRepository.FillTableVersionsAsync();
        await StorageManager.EfContext.ShrinkDbAsync();
    }

    /// <summary> Shrink storage </summary>
    public async Task ShrinkDbAsync()
    {
        await StorageManager.EfContext.ShrinkDbAsync();
    }

    /// <summary> Backup storage </summary>
    public (bool IsSuccess, string FileName) BackupDbAsync()
    {
        return StorageManager.EfContext.BackupDb();
    }

    #endregion
}
