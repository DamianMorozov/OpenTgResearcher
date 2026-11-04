namespace TgStorageTests.Common;

[TestFixture]
[NonParallelizable]
internal sealed class TgStorageTestsBaseTests
{
    /// <summary> Проверяет создание схемы и запись данных в изолированной SQLite </summary>
    [Test]
    public async Task DefaultStorageIsInMemoryAndMigratedAsync()
    {
        using var fixture = new StorageFixture();
        var context = fixture.Storage.EfContext;
        var connection = new SqliteConnectionStringBuilder(context.Database.GetDbConnection().ConnectionString);
        Assert.That(connection.DataSource, Is.EqualTo(":memory:"));
        Assert.That(await context.Database.GetPendingMigrationsAsync(), Is.Empty);
        var proxy = new TgEfProxyDto { Uid = Guid.NewGuid(), HostName = "storage.example.test", Port = 1080, Type = TgEnumProxyType.Socks };
        await fixture.Storage.ProxyRepository.SaveAsync(proxy);
        var saved = await fixture.Storage.ProxyRepository.GetDtoAsync(x => x.Uid == proxy.Uid);
        Assert.That(saved.HostName, Is.EqualTo(proxy.HostName));
    }

    /// <summary> Проверяет отсутствие данных предыдущего экземпляра тестовой базы </summary>
    [Test]
    public async Task SeparateFixturesDoNotShareRecordsAsync()
    {
        var uid = Guid.NewGuid();
        using (var first = new StorageFixture())
        {
            await first.Storage.ProxyRepository.SaveAsync(new TgEfProxyDto { Uid = uid, HostName = "first.example.test", Port = 1080, Type = TgEnumProxyType.Socks });
            Assert.That(await first.Storage.ProxyRepository.GetCountAsync(x => x.Uid == uid), Is.EqualTo(1));
        }
        using var second = new StorageFixture();
        Assert.That(await second.Storage.ProxyRepository.GetCountAsync(x => x.Uid == uid), Is.Zero);
    }

    /// <summary> Проверяет освобождение соединения и контейнера после завершения тестов </summary>
    [Test]
    public void DisposeReleasesDatabaseAndScope()
    {
        var fixture = new StorageFixture();
        var connection = fixture.Storage.EfContext.Database.GetDbConnection();
        var scope = fixture.LifetimeScope;
        try
        {
            Assert.That(connection.State, Is.EqualTo(System.Data.ConnectionState.Open));
        }
        finally { fixture.Dispose(); }
        Assert.That(connection.State, Is.EqualTo(System.Data.ConnectionState.Closed));
        Assert.Throws<ObjectDisposedException>(() => scope.Resolve<ITgEfContext>());
    }

    private sealed class StorageFixture : TgStorageTestsBase
    {
        /// <summary> Предоставляет настоящее хранилище тестового окружения </summary>
        public ITgStorageService Storage => StorageManager;
        /// <summary> Предоставляет область зависимостей для проверки освобождения </summary>
        public ILifetimeScope LifetimeScope => Scope;
    }
}
