namespace OpenTgResearcherDesktop.Helpers;

/// <summary> Autofac module for services </summary>
internal sealed class TgAutofacServiceModule : Autofac.Module
{
    protected override void Load(ContainerBuilder cb)
    {
        cb.RegisterType<TgStorageService>().As<ITgStorageService>().SingleInstance();
        cb.RegisterType<TgFloodControlService>().As<ITgFloodControlService>().SingleInstance();
        cb.RegisterType<TgConnectClientDesktop>().As<ITgConnectClientDesktop>().SingleInstance();
        cb.RegisterType<TgLicenseService>().As<ITgLicenseService>().SingleInstance();
        cb.RegisterType<TgHardwareResourceMonitoringService>().As<ITgHardwareResourceMonitoringService>().SingleInstance();
        cb.RegisterType<TgBusinessLogicManager>().As<ITgBusinessLogicManager>().SingleInstance();
        cb.RegisterType<LoadStateService>().As<ILoadStateService>().SingleInstance();
    }
}
