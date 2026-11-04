namespace TgBusinessLogicTests.Common;

[TestFixture]
internal sealed class BusinessLogicTestsBaseTests : BusinessLogicTestsBase
{
    /// <summary> Проверяет общий изолированный контекст и доступность мигрированной схемы для бизнес-логики </summary>
    [Test]
    public async Task BusinessLogicUsesSharedMigratedMemoryStorageAsync()
    {
        var storage = BusinessLogicManager.StorageManager;
        Assert.That(storage.StoragePath, Is.EqualTo(":memory:"));
        Assert.That(Scope.Resolve<ITgEfContext>(), Is.SameAs(storage.EfContext));
        Assert.That(await storage.VersionRepository.GetCountAsync(), Is.GreaterThan(0));
        var proxy = new TgEfProxyDto
        {
            Uid = Guid.NewGuid(), Type = TgEnumProxyType.Socks, HostName = "business.example.test", Port = 1080,
        };
        await storage.ProxyRepository.SaveAsync(proxy);
        var repository = Scope.Resolve<ITgEfProxyRepository>();
        var saved = await repository.GetDtoAsync(x => x.Uid == proxy.Uid);
        Assert.That(saved.HostName, Is.EqualTo(proxy.HostName));
    }
}
