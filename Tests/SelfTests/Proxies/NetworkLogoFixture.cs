namespace OpenTgResearcherDesktop.ProxySelfTests;

internal static partial class Program
{
    // Добавляем только локальный тестовый сервер в отдельную БД настоящего GUI-селфтеста
    private static async Task<int> SeedNetworkProxyAsync(string path, ushort port)
    {
        try
        {
            var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../bin")) + Path.DirectorySeparatorChar;
            var target = Path.GetFullPath(path);
            var allowed = new[] { "ProxiesPage", "NetworkPage" }.Any(name => target.StartsWith(Path.Combine(root, name) + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase));
            if (!allowed) throw new InvalidOperationException("Разрешена только БД GUI-селфтеста прокси или сетевой активности");
            await using var context = new TestContext(new DbContextOptionsBuilder().UseSqlite($"Data Source={target}").Options);
            await context.Database.EnsureCreatedAsync();
            context.Proxies.Add(new TgEfProxyEntity { Uid = Guid.NewGuid(), Type = TgEnumProxyType.Socks, HostName = "127.0.0.1", Port = port });
            await context.SaveChangesAsync();
            Console.WriteLine("PASS: добавлен тестовый прокси loopback");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
}
