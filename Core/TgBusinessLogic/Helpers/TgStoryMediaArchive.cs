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
