namespace OpenTgResearcherDesktop.Models;

/// <summary> Редактируемая пара хранилища и сессии отдельного аккаунта Telegram </summary>
public sealed partial class TgAccountProfile : ObservableObject
{
    public Guid Id { get; init; } = Guid.NewGuid();
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StoragePath { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string SessionPath { get; set; } = string.Empty;
}

/// <summary> Каталог профилей в общих настройках Desktop, независимый от выбранной базы </summary>
public sealed class TgAccountProfiles
{
    public const string SettingsKey = "AccountProfiles";
    private readonly ITgSettingsService _settings;
    public ObservableCollection<TgAccountProfile> Profiles { get; } = [];
    public Guid ActiveProfileId { get; private set; }

    /// <summary> Загружает каталог или переносит прежние пути в первый профиль </summary>
    public TgAccountProfiles(ITgSettingsService settings, string defaultName)
    {
        _settings = settings;
        Reload(defaultName);
    }

    /// <summary> Перечитывает сохранённый каталог, отменяя несохранённое редактирование </summary>
    public void Reload(string defaultName)
    {
        var saved = _settings.ReadSetting<State>(SettingsKey);
        Profiles.Clear();
        if (saved is not null && saved.Profiles.Count > 0)
        {
            foreach (var item in saved.Profiles)
                Profiles.Add(new() { Id = item.Id, Name = item.Name, StoragePath = item.StoragePath, SessionPath = item.SessionPath });
            ActiveProfileId = Profiles.Any(x => x.Id == saved.ActiveProfileId) ? saved.ActiveProfileId : Profiles[0].Id;
        }
        else
        {
            var initial = Create(defaultName, _settings.AppStorage, _settings.AppSession);
            Save(initial.Id);
        }
    }

    /// <summary> Возвращает сохранённую пару путей при запуске приложения </summary>
    public static Values? ReadActive(ITgSettingsService settings)
    {
        var state = settings.ReadSetting<State>(SettingsKey);
        return state?.Profiles.FirstOrDefault(x => x.Id == state.ActiveProfileId) ?? state?.Profiles.FirstOrDefault();
    }

    /// <summary> Создаёт черновик профиля без создания базы и сессии </summary>
    public TgAccountProfile Create(string name, string storagePath, string sessionPath)
    {
        var profile = new TgAccountProfile { Name = name, StoragePath = storagePath, SessionPath = sessionPath };
        Profiles.Add(profile);
        return profile;
    }

    /// <summary> Убирает только запись из каталога, сохраняя файлы базы и сессии </summary>
    public void Delete(Guid id)
    {
        if (Profiles.Count <= 1) throw new InvalidOperationException("Нельзя удалить единственный профиль");
        Profiles.Remove(GetProfile(id));
    }

    /// <summary> Проверяет название и пути перед сохранением или переключением аккаунта </summary>
    public void Validate(Guid selectedId)
    {
        _ = GetProfile(selectedId);
        foreach (var profile in Profiles)
        {
            if (string.IsNullOrWhiteSpace(profile.Name)) throw new InvalidOperationException("Укажите название профиля");
            if (Profiles.Count(x => string.Equals(x.Name.Trim(), profile.Name.Trim(), StringComparison.OrdinalIgnoreCase)) > 1)
                throw new InvalidOperationException("Названия профилей должны различаться");
            var storage = ValidatePath(profile.StoragePath, "хранилища");
            var session = ValidatePath(profile.SessionPath, "сессии");
            if (string.Equals(storage, session, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Для хранилища и сессии укажите разные файлы");
        }
    }

    /// <summary> Сохраняет снимок каталога и выбранный профиль без изменения открытых файлов </summary>
    public void Save(Guid selectedId)
    {
        Validate(selectedId);
        _settings.SaveProfileState(new State(selectedId,
            Profiles.Select(x => new Values(x.Id, x.Name.Trim(), Path.GetFullPath(x.StoragePath), Path.GetFullPath(x.SessionPath))).ToList()));
        ActiveProfileId = selectedId;
    }

    // Находим выбранную запись без неявного переключения на другой аккаунт
    private TgAccountProfile GetProfile(Guid id) => Profiles.FirstOrDefault(x => x.Id == id)
        ?? throw new InvalidOperationException("Выберите существующий профиль");

    // Проверяем путь к файлу, разрешая ещё не созданные файлы и каталоги
    private static string ValidatePath(string path, string title)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new InvalidOperationException($"Укажите файл {title} в профиле");
        var fullPath = Path.GetFullPath(path);
        if (string.IsNullOrEmpty(Path.GetFileName(fullPath)) || Directory.Exists(fullPath))
            throw new InvalidOperationException($"Для {title} укажите путь к файлу, а не к каталогу");
        return fullPath;
    }

    /// <summary> Сохранённые поля одного профиля </summary>
    public sealed record Values(Guid Id, string Name, string StoragePath, string SessionPath);
    /// <summary> Снимок каталога с идентификатором применённого профиля </summary>
    public sealed record State(Guid ActiveProfileId, List<Values> Profiles);
}
