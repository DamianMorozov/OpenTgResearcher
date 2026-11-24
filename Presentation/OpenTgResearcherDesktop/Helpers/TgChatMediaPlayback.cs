namespace OpenTgResearcherDesktop.Helpers;

/// <summary> Форматы для встроенного проигрывателя чата </summary>
public static class TgChatMediaPlayback
{
    /// <summary> Показывает кнопку воспроизведения только для подходящих файлов </summary>
    public static Visibility PlayButtonVisibility(string path) => CanPlay(path) ? Visibility.Visible : Visibility.Collapsed;

    /// <summary> Определяет, можно ли открыть файл во встроенном проигрывателе </summary>
    public static bool CanPlay(string path) => IsVideo(path)
        || Path.GetExtension(path).ToLowerInvariant() is ".mp3" or ".wav" or ".flac" or ".aac" or ".ogg" or ".m4a" or ".opus" or ".wma";

    /// <summary> Определяет, есть ли у формата видеодорожка для увеличенного просмотра </summary>
    public static bool IsVideo(string path) => TgStoryMediaArchive.IsVideo(path)
        || Path.GetExtension(path).ToLowerInvariant() is ".avi" or ".wmv" or ".m4v" or ".3gp" or ".mpeg" or ".mpg";
}
