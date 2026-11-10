namespace OpenTgResearcherDesktop.Models;

/// <summary> Недавние чаты текущего хранилища, сохраняемые между запусками Desktop </summary>
public static class TgRecentChats
{
    public const string SettingsKey = "HomeRecentChats";
    public const int Limit = 3;

    /// <summary> Возвращает последние открытые чаты только из выбранного хранилища </summary>
    public static IReadOnlyList<Entry> Read(ITgSettingsService settings) =>
        (settings.ReadSetting<List<Entry>>(SettingsKey) ?? []).Where(x => SameStorage(x, settings.AppStorage))
            .Where(x => x.Uid != Guid.Empty && x.Id > 0).Take(Limit).ToArray();

    /// <summary> Запоминает успешно открытый чат, сохраняя историю остальных хранилищ </summary>
    public static void Remember(ITgSettingsService settings, Guid uid, long id, string title)
    {
        if (uid == Guid.Empty || id <= 0 || string.IsNullOrWhiteSpace(settings.AppStorage)) return;
        var saved = settings.ReadSetting<List<Entry>>(SettingsKey) ?? [];
        var current = new[] { new Entry(settings.AppStorage, uid, id, title) }
            .Concat(saved.Where(x => SameStorage(x, settings.AppStorage) && x.Uid != uid)).Take(Limit);
        settings.SaveSetting(SettingsKey, current.Concat(saved.Where(x => !SameStorage(x, settings.AppStorage))).ToList());
    }

    // История одного аккаунта не появляется при выборе другого хранилища
    private static bool SameStorage(Entry entry, string path) => string.Equals(entry.StoragePath, path, StringComparison.OrdinalIgnoreCase);

    /// <summary> Сохранённая ссылка на чат без содержимого сообщений и данных авторизации </summary>
    public sealed record Entry(string StoragePath, Guid Uid, long Id, string Title);
}

/// <summary> Строка главной страницы с учётом локализации и скрытия конфиденциальных данных </summary>
public sealed record TgHomeRecentChat(Guid Uid, string Title, string Detail, IRelayCommand<TgHomeRecentChat> OpenCommand);
