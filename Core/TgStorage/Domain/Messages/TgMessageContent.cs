namespace TgStorage.Domain.Messages;

/// <summary> Исходный текст Telegram, разметка UTF-16 и отдельное имя медиафайла </summary>
public sealed class TgMessageContent
{
    private const string _prefix = "OTR:1:";
    public string Text { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public TgMessageEntity[] Entities { get; set; } = [];

    /// <summary> Сохраняет версионный JSON; обычный текст без разметки оставляет читаемым </summary>
    public string Serialize() => FileName.Length == 0 && Entities.Length == 0 && !Text.StartsWith(_prefix, StringComparison.Ordinal)
        ? Text : _prefix + System.Text.Json.JsonSerializer.Serialize(this);

    /// <summary> Читает новый формат и исторические записи без изменения БД </summary>
    public static TgMessageContent Read(string value, TgEnumMessageType type)
    {
        if (value.StartsWith(_prefix, StringComparison.Ordinal))
        {
            try
            {
                var content = System.Text.Json.JsonSerializer.Deserialize<TgMessageContent>(value[_prefix.Length..]);
                if (content is not null && content.Text is not null && content.FileName is not null && content.Entities is not null) return content;
            }
            catch (System.Text.Json.JsonException ex) { TgLogUtils.WriteException(ex); }
        }
        if (type == TgEnumMessageType.Document) return new() { FileName = value.Trim() };
        var separator = type == TgEnumMessageType.Photo ? value.LastIndexOf('|') : -1;
        return separator < 0 ? new() { Text = value } : new() { Text = value[..separator].Trim(), FileName = value[(separator + 1)..].Trim() };
    }

    /// <summary> Разбивает текст на фрагменты с сочетаниями стилей и ссылками, сохраняя исходные символы </summary>
    public IReadOnlyList<TgMessageTextRun> GetRuns()
    {
        var entities = Entities.Where(x => x is not null && x.Offset >= 0 && x.Length > 0 && x.Offset <= Text.Length
            && x.Length <= Text.Length - x.Offset).ToArray();
        var boundaries = entities.SelectMany(x => new[] { x.Offset, x.Offset + x.Length }).Append(0).Append(Text.Length).Distinct().Order().ToArray();
        var runs = new List<TgMessageTextRun>();
        for (var i = 1; i < boundaries.Length; i++)
        {
            var start = boundaries[i - 1]; var end = boundaries[i];
            var styles = entities.Where(x => x.Offset <= start && x.Offset + x.Length >= end).ToArray();
            var link = styles.Select(x => GetLink(x)).FirstOrDefault(x => x.Length > 0) ?? string.Empty;
            runs.Add(new(Text[start..end], styles.Any(x => x.Type == "Bold"), styles.Any(x => x.Type is "Italic" or "Blockquote"),
                styles.Any(x => x.Type == "Underline"), styles.Any(x => x.Type == "Strike"), styles.Any(x => x.Type is "Code" or "Pre"), link));
        }
        return runs;
    }

    // Преобразуем только известные виды ссылок; обычный текст не интерпретируется как разметка
    private string GetLink(TgMessageEntity entity)
    {
        var text = Text.Substring(entity.Offset, entity.Length);
        var target = entity.Type switch
        {
            "TextUrl" => entity.Value ?? string.Empty,
            "Url" => text.Contains("://", StringComparison.Ordinal) ? text : "https://" + text,
            "Mention" => "https://t.me/" + text.TrimStart('@'),
            "Email" => "mailto:" + text,
            "Phone" => "tel:" + text,
            "MentionName" => "tg://user?id=" + entity.Value,
            _ => string.Empty
        };
        return Uri.TryCreate(target, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https" or "tg" or "mailto" or "tel" ? target : string.Empty;
    }
}

/// <summary> Диапазон исходной разметки Telegram; неизвестные виды сохраняются для последующего отображения </summary>
public sealed record TgMessageEntity(string Type, int Offset, int Length, string Value = "");

/// <summary> Фрагмент текста для нативного отображения без HTML и Markdown </summary>
public sealed record TgMessageTextRun(string Text, bool Bold, bool Italic, bool Underline, bool Strike, bool Monospace, string Link);
