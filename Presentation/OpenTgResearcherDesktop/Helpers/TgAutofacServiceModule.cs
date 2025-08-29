// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Helpers;

/// <summary> Autofac module for services </summary>
internal sealed class TgAutofacServiceModule : Autofac.Module
{
    protected override void Load(ContainerBuilder containerBuilder)
    {
        containerBuilder.RegisterType<TgStorageManager>().As<ITgStorageManager>().SingleInstance();
        containerBuilder.RegisterType<TgFloodControlService>().As<ITgFloodControlService>().SingleInstance();
        containerBuilder.RegisterType<TgConnectClientDesktop>().As<ITgConnectClientDesktop>().SingleInstance();
        containerBuilder.RegisterType<TgLicenseService>().As<ITgLicenseService>().SingleInstance();
        containerBuilder.RegisterType<TgBusinessLogicManager>().As<ITgBusinessLogicManager>().SingleInstance();
        containerBuilder.RegisterType<TgHardwareResourceMonitoringService>().As<ITgHardwareResourceMonitoringService>().SingleInstance();
    }
}
