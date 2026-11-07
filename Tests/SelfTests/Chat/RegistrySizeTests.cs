namespace OpenTgResearcherDesktop.ChatSelfTests;

// Проверяем реальный размер каталога чата, единицы и обновление первой порции после поиска
internal static partial class Program
{
    private static IEnumerable<(string Name, Func<Task> Run)> GetRegistrySizeTests() =>
    [
        ("RegistryDirectorySize", RegistryDirectorySizeAsync),
        ("RegistryDirectorySizeUnits", RegistryDirectorySizeUnitsAsync),
        ("RegistryDirectorySizeUnavailable", RegistryDirectorySizeUnavailableAsync),
        ("RegistryDirectorySizeMarkup", RegistryDirectorySizeMarkupAsync),
        ("RegistryDirectoryAccessDenied", RegistryDirectoryAccessDeniedAsync),
    ];

    // Суммируем существующие файлы, включая вложенные папки и файлы вне БД
    private static async Task RegistryDirectorySizeAsync()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"registry-size-{Guid.NewGuid():N}");
        System.IO.Directory.CreateDirectory(Path.Combine(directory, "nested"));
        try
        {
            await File.WriteAllBytesAsync(Path.Combine(directory, "first.bin"), new byte[100]);
            await File.WriteAllBytesAsync(Path.Combine(directory, "nested", "second.bin"), new byte[1948]);
            await File.WriteAllBytesAsync(Path.Combine(directory, "empty.bin"), []);
            using var sources = new TgEfSourceRepository();
            _source.Directory = directory;
            await sources.SaveAsync(_source);
            var vm = new TgChatsViewModel(_state, _settings, _navigation, null!);
            await vm.OnNavigatedToAsync(null);
            await WaitRegistrySizesAsync(vm);
            var row = vm.RegistryRows.Single();
            Assert(GetDirectoryBytes(row) == 2048 && GetDirectorySizeText(row) == "2 КБ", "Размер не отражает реальные файлы в подкаталогах");
            File.Delete(Path.Combine(directory, "nested", "second.bin"));
            await vm.RefreshRegistryCommand.ExecuteAsync(null);
            await WaitRegistrySizesAsync(vm);
            Assert(GetDirectoryBytes(vm.RegistryRows.Single()) == 100, "Обновление списка сохранило устаревший размер каталога");
        }
        finally { System.IO.Directory.Delete(directory, recursive: true); }
    }

    // Проверяем границы единиц, нулевой каталог и большие файлы без выделения их объёма в памяти
    private static async Task RegistryDirectorySizeUnitsAsync()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"registry-units-{Guid.NewGuid():N}");
        System.IO.Directory.CreateDirectory(directory);
        try
        {
            var row = new OpenTgResearcherDesktop.Models.TgChatRegistryRow(new(), directory, 0, 99, default);
            var method = row.GetType().GetMethod("UpdateDirectorySizeAsync");
            Assert(method is not null, "Нет подсчёта размера каталога");
            foreach (var (bytes, text) in new (long, string)[]
                { (0, "0 Б"), (1023, "1023 Б"), (1024, "1 КБ"), (1048576, "1 МБ"), (1073741824, "1 ГБ") })
            {
                await using (var file = File.Create(Path.Combine(directory, "data.bin"))) file.SetLength(bytes);
                await (Task)method!.Invoke(row, new object[] { CancellationToken.None })!;
                Assert(GetDirectoryBytes(row) == bytes && GetDirectorySizeText(row) == text, $"Неверная единица размера: {bytes}");
            }
        }
        finally { System.IO.Directory.Delete(directory, recursive: true); }
    }

    // Неизвестный и недоступный путь не показываем как пустой каталог
    private static async Task RegistryDirectorySizeUnavailableAsync()
    {
        foreach (var directory in new[] { "", Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")) })
        {
            var row = new OpenTgResearcherDesktop.Models.TgChatRegistryRow(new(), directory, 0, 0, default);
            var method = row.GetType().GetMethod("UpdateDirectorySizeAsync");
            Assert(method is not null, "Нет подсчёта размера каталога");
            await (Task)method!.Invoke(row, new object[] { CancellationToken.None })!;
            Assert(GetDirectoryBytes(row) is null && GetDirectorySizeText(row) == "—", "Отсутствующий каталог показан как 0 байт");
        }
    }

    // Запрещаем только чтение временного каталога и восстанавливаем исходные права в любом исходе
    private static async Task RegistryDirectoryAccessDeniedAsync()
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Проверка прав каталога требует Windows");
        var path = Path.Combine(Path.GetTempPath(), $"registry-denied-{Guid.NewGuid():N}");
        var directory = System.IO.Directory.CreateDirectory(path);
        var original = System.IO.FileSystemAclExtensions.GetAccessControl(directory).GetSecurityDescriptorBinaryForm();
        try
        {
            await File.WriteAllBytesAsync(Path.Combine(path, "file.bin"), [1, 2, 3]);
            var denied = System.IO.FileSystemAclExtensions.GetAccessControl(directory);
            var sid = System.Security.Principal.WindowsIdentity.GetCurrent().User!;
            denied.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule(sid,
                System.Security.AccessControl.FileSystemRights.ListDirectory, System.Security.AccessControl.AccessControlType.Deny));
            System.IO.FileSystemAclExtensions.SetAccessControl(directory, denied);
            var accessDenied = false;
            try { _ = directory.EnumerateFiles().ToArray(); }
            catch (UnauthorizedAccessException) { accessDenied = true; }
            Assert(accessDenied, "Стенд не запретил чтение каталога");
            var row = new OpenTgResearcherDesktop.Models.TgChatRegistryRow(new(), path, 0, 0, default);
            var read = row.GetType().GetMethod("ReadDirectorySize", BindingFlags.NonPublic | BindingFlags.Instance)!;
            Assert(read.Invoke(row, [CancellationToken.None]) is null, "Недоступный каталог показан как пустой");
            await row.UpdateDirectorySizeAsync();
            Assert(row.DirectorySizeBytes is null && row.DirectorySizeText == "—", "Недоступный каталог не обработан мягко");
        }
        finally
        {
            var restored = new System.Security.AccessControl.DirectorySecurity();
            restored.SetSecurityDescriptorBinaryForm(original, System.Security.AccessControl.AccessControlSections.Access);
            System.IO.FileSystemAclExtensions.SetAccessControl(directory, restored);
            System.IO.Directory.Delete(path, recursive: true);
        }
    }

    // Колонка должна показывать размер каталога с локализованным заголовком
    private static Task RegistryDirectorySizeMarkupAsync()
    {
        var columns = ReadMarkup("TgChatsPage.xaml").Descendants().Where(x => x.Name.LocalName.EndsWith("Column"));
        Assert(columns.Any(x => ((string?)x.Attribute("Binding"))?.Contains("DirectorySizeText") == true
            && ((string?)x.Attribute("Header"))?.Contains("ChatSizeHeader") == true), "Колонка размера не подключена к каталогу чата");
        return Task.CompletedTask;
    }

    // Получаем новые свойства через отражение для содержательного падения до реализации
    private static long? GetDirectoryBytes(object row)
    {
        var property = row.GetType().GetProperty("DirectorySizeBytes");
        Assert(property is not null, "Нет фактического размера каталога в строке реестра");
        return (long?)property!.GetValue(row);
    }

    // Читаем отформатированный размер исходной строки реестра
    private static string? GetDirectorySizeText(object row) => (string?)row.GetType().GetProperty("DirectorySizeText")?.GetValue(row);
}
