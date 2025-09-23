namespace OpenTgResearcherDesktop.Helpers;

/// <summary> Autofac module for repositories </summary>
internal sealed class TgAutofacRepositoryModule : Autofac.Module
{
    protected override void Load(ContainerBuilder cb)
    {
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
    }
}
