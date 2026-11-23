namespace TgBusinessLogic.Helpers;

public static class TgStoryMediaArchive
{
    public static string ResolvePath(string directory, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(directory) || string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath))
            return string.Empty;
        var root = Path.GetFullPath(directory);
        var path = Path.GetFullPath(relativePath, root);
        var relative = Path.GetRelativePath(root, path);
        return relative == "." || relative == ".." || relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
            || Path.IsPathRooted(relative) ? string.Empty : path;
    }

    public static bool IsPhoto(string path) => Path.GetExtension(path).ToLowerInvariant() is ".jpg" or ".png" or ".webp" or ".gif";
    public static bool IsVideo(string path) => Path.GetExtension(path).ToLowerInvariant() is ".mp4" or ".webm" or ".mov" or ".mkv";

    /// <summary> Удаляет зарегистрированные медиафайлы сторис и их отметки удаления </summary>
    public static void DeleteFiles(string directory, IEnumerable<string> relativePaths, CancellationToken ct = default)
    {
        var files = new HashSet<string>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        foreach (var relative in relativePaths)
        {
            ct.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(relative)) continue;

            var path = ResolvePath(directory, relative);
            if (path.Length == 0 || relative.Contains(':'))
                throw new IOException("Недопустимый путь к медиафайлу сторис.");

            if (IsArchiveFile(path)) files.Add(path);
            var marker = path + ".deleted";
            if (IsArchiveFile(marker)) files.Add(marker);
        }

        foreach (var file in files)
        {
            ct.ThrowIfCancellationRequested();
            File.Delete(file);
        }
    }

    // Проверяем, что файл и его родительские каталоги не перенаправлены через ссылки
    private static bool IsArchiveFile(string path)
    {
        try
        {
            var fileAttributes = File.GetAttributes(path);
            if (fileAttributes.HasFlag(FileAttributes.Directory))
                throw new IOException($"На месте медиафайла сторис находится каталог: {path}");

            for (string? current = path; current is not null; current = Path.GetDirectoryName(current))
                if (File.GetAttributes(current).HasFlag(FileAttributes.ReparsePoint))
                    throw new IOException($"Путь к медиафайлу сторис проходит через ссылку: {current}");
            return true;
        }
        catch (FileNotFoundException) { return false; }
        catch (DirectoryNotFoundException) { return false; }
    }

    public static async Task<bool> SaveAsync(string path, long size, Func<Stream, Task> download, CancellationToken ct)
    {
        if (IsComplete(path, size)) return false;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + $".{Guid.NewGuid():N}.part";
        try
        {
            await using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
                await download(stream);
            ct.ThrowIfCancellationRequested();
            if (!IsComplete(temporary, size))
                throw new IOException("Не удалось полностью скачать медиа сторис.");
            File.Move(temporary, path, overwrite: true);
            return true;
        }
        finally
        {
            if (File.Exists(temporary))
                File.Delete(temporary);
        }
    }

    private static bool IsComplete(string path, long size) =>
        File.Exists(path) && new FileInfo(path).Length is > 0 and var length && (size <= 0 || size == length);
}
