namespace OpenTgResearcher.TelegramCompatibilitySelfTests;

internal static class Program
{
    // Проверяем адаптацию новых типов Telegram без сети и пользовательской БД
    private static async Task<int> Main()
    {
        var tests = new (string Name, Func<Task> Run)[]
        {
            ("RecentStoryIdMapped", RecentStoryIdMappedAsync),
            ("MissingRecentStoryClearsId", MissingRecentStoryClearsIdAsync),
            ("ManageTopicsAllowed", () => CheckManageTopicsAsync(true)),
            ("ManageTopicsDenied", () => CheckManageTopicsAsync(false)),
            ("ManageTopicsUnspecified", () => CheckManageTopicsAsync(null)),
        };
        var failures = 0;
        foreach (var test in tests)
        {
            try { await test.Run(); Console.WriteLine($"PASS: {test.Name}"); }
            catch (Exception ex) { failures++; Console.WriteLine($"FAIL: {test.Name}: {ex.GetBaseException().Message}"); }
        }
        Console.WriteLine($"Результат: {tests.Length - failures}/{tests.Length}; SQLite в памяти, без Telegram");
        return failures == 0 ? 0 : 1;
    }

    // Проверяем перенос идентификатора истории в DTO исходным сервисом
    private static Task RecentStoryIdMappedAsync() => CheckStoryMappingAsync(hasStory: true);

    // Проверяем очистку ранее сохранённого идентификатора при отсутствии историй
    private static Task MissingRecentStoryClearsIdAsync() => CheckStoryMappingAsync(hasStory: false);

    // Используем настоящий сервис, репозитории и изолированную SQLite
    private static async Task CheckStoryMappingAsync(bool hasStory)
    {
        await using var context = new TestContext(new DbContextOptionsBuilder().UseSqlite("Data Source=:memory:").Options);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();
        var builder = new ContainerBuilder();
        builder.RegisterInstance(context).As<ITgEfContext>().ExternallyOwned();
        builder.RegisterAssemblyTypes(typeof(TgEfUserRepository).Assembly)
            .Where(type => type.Namespace == typeof(TgEfUserRepository).Namespace).AsImplementedInterfaces().InstancePerLifetimeScope();
        using var container = builder.Build();
        TgGlobalTools.Container = container;
        using var storage = new TgStorageService();
        await storage.UserRepository.SaveAsync(new() { Id = 123, FirstName = "Test", StoriesMaxId = 99 });
        var user = new TL.User { id = 123, first_name = "Test" };
        if (hasStory) user.stories_max_id = new TL.RecentStory { flags = TL.RecentStory.Flags.has_max_id, max_id = 456 };
        var dto = await storage.CreateOrGetUserAsync(user, isContact: false, isSave: true);
        var saved = await storage.UserRepository.GetDtoAsync(where: item => item.Id == user.id);
        var expectedId = hasStory ? 456 : 0;
        if (dto.StoriesMaxId != expectedId || saved is null || saved.StoriesMaxId != expectedId)
            throw new InvalidOperationException($"Ожидался ID истории {expectedId} в DTO и БД");
    }

    // Проверяем три состояния разрешения и сохранение другого разрешения
    private static Task CheckManageTopicsAsync(bool? canManageTopics)
    {
        var details = new WTelegram.Types.ChatFullInfo
        {
            Permissions = new Telegram.Bot.Types.ChatPermissions { CanManageTopics = canManageTopics, CanSendPhotos = true },
        };
        var dto = new TgChatDetailsDto();
        var method = typeof(TgConnectClientBase).GetMethod("FillChatDetailsPermissions", BindingFlags.Static | BindingFlags.NonPublic)!;
        method.Invoke(null, [dto, details]);
        var expected = canManageTopics == true ? "CanSendPhotos, CanManageTopics" : "CanSendPhotos";
        if (dto.Permissions != expected) throw new InvalidOperationException($"Ожидалось '{expected}', получено '{dto.Permissions}'");
        return Task.CompletedTask;
    }
}

internal sealed class TestContext(DbContextOptions options) : TgEfContextBase(options)
{
    // Исключаем чтение настроек и подключение к пользовательской БД
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }
}
