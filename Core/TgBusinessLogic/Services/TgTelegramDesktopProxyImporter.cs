namespace TgBusinessLogic.Services;

/// <summary> Содержит поддерживаемые прокси и количество пропущенных записей Web </summary>
public sealed record TgTelegramDesktopProxyImportResult(IReadOnlyList<TgEfProxyDto> Proxies, int SkippedWebCount);

/// <summary> Читает только список прокси из локальных настроек Telegram Desktop </summary>
public static class TgTelegramDesktopProxyImporter
{
    /// <summary> Читает settingss или резервный settings0/settings1 без изменения файлов Telegram </summary>
    public static async Task<TgTelegramDesktopProxyImportResult> ReadAsync(string directory, CancellationToken ct = default)
    {
        try
        {
            ct.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(directory)) throw new InvalidDataException("Укажите каталог tdata установленного Telegram Desktop");
            directory = directory.Trim();
            if (directory.Length >= 2 && directory[0] == '"' && directory[^1] == '"') directory = directory[1..^1];
            directory = Path.GetFullPath(Environment.ExpandEnvironmentVariables(directory));
            var files = FindSettingsFiles(directory);
            if (files.Length == 0 && Directory.Exists(Path.Combine(directory, "tdata")))
            {
                directory = Path.Combine(directory, "tdata");
                files = FindSettingsFiles(directory);
            }
            if (files.Length == 0)
                throw new FileNotFoundException($"В каталоге «{directory}» нет settingss, settings0 или settings1. " +
                    "Выберите каталог tdata Telegram Desktop, содержащий файл настроек");
            Exception? lastError = null;
            var lastFile = string.Empty;
            foreach (var file in files)
            {
                try
                {
                    var bytes = await File.ReadAllBytesAsync(file, ct).ConfigureAwait(false);
                    ct.ThrowIfCancellationRequested();
                    return ReadSettings(bytes);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException
                    or System.Security.Cryptography.CryptographicException) { lastError = ex; lastFile = file; }
            }
            if (lastError is IOException or UnauthorizedAccessException)
                throw new IOException($"Не удалось открыть файл настроек Telegram Desktop «{lastFile}». " +
                    "Закройте Telegram Desktop и проверьте права на чтение файла, затем повторите импорт", lastError);
            throw new InvalidDataException($"Не удалось импортировать настройки Telegram Desktop из файла «{lastFile}». " +
                "Причина указана ниже", lastError);
        }
        catch (OperationCanceledException) { throw; }
        catch (DirectoryNotFoundException ex)
        {
            var error = new DirectoryNotFoundException($"Каталог Telegram Desktop «{directory}» не найден. Проверьте путь к tdata", ex);
            TgLogUtils.WriteExceptionWithMessage(error, "Ошибка поиска локальных настроек прокси Telegram Desktop");
            throw error;
        }
        catch (UnauthorizedAccessException ex)
        {
            var error = new IOException($"Нет доступа к каталогу Telegram Desktop «{directory}». Проверьте права на чтение", ex);
            TgLogUtils.WriteExceptionWithMessage(error, "Ошибка доступа к локальным настройкам прокси Telegram Desktop");
            throw error;
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteExceptionWithMessage(ex, "Ошибка чтения локальных настроек прокси Telegram Desktop");
            throw;
        }
    }

    // Перечисляем каталог, чтобы ошибка доступа не превращалась в ложное отсутствие файла при File.Exists
    private static string[] FindSettingsFiles(string directory)
    {
        var files = Directory.GetFiles(directory).Where(x => Path.GetFileName(x) is "settingss" or "settings0" or "settings1").ToArray();
        var modern = files.FirstOrDefault(x => Path.GetFileName(x) == "settingss");
        return modern is not null ? [modern] : files.OrderByDescending(File.GetLastWriteTimeUtc).ToArray();
    }

    // Проверяем контейнер TDF$, контрольную сумму и структуру Qt до извлечения настроек
    private static TgTelegramDesktopProxyImportResult ReadSettings(byte[] file)
    {
        if (file.Length < 24 || !file.AsSpan(0, 4).SequenceEqual("TDF$"u8)) throw InvalidFormat();
        var payload = file[8..^16];
        var signature = System.Security.Cryptography.MD5.HashData([.. payload, .. BitConverter.GetBytes(payload.Length), .. file[4..8], .. "TDF$"u8]);
        if (!signature.AsSpan().SequenceEqual(file.AsSpan(file.Length - 16))) throw InvalidFormat();
        var container = new QtReader(payload);
        var salt = container.ReadBlob();
        var encrypted = container.ReadBlob();
        if (salt.Length != 32 || !container.AtEnd) throw InvalidFormat();
        var settings = new QtReader(Decrypt(salt, encrypted));
        TgTelegramDesktopProxyImportResult? result = null;
        while (!settings.AtEnd)
        {
            switch (settings.ReadInt())
            {
                case 0x5e: result = ReadApplicationSettings(settings.ReadBlob()); break;
                case 0x06: case 0x07: case 0x09: case 0x0a: case 0x0c: case 0x0d: case 0x1d: case 0x57: case 0x58: settings.Skip(4); break;
                case 0x23: case 0x60: settings.ReadBlob(); break;
                case 0x54: settings.Skip(20); break;
                case 0x61: settings.Skip(16); break;
                case 0x55: case 0x47: case 0x4e: case 0x5a: settings.Skip(8); break;
                default: throw new InvalidDataException("Эта версия формата настроек Telegram Desktop пока не поддерживается");
            }
        }
        return result ?? throw new InvalidDataException("В файле нет поддерживаемого блока настроек приложения Telegram Desktop");
    }

    // Пропускаем поля Core::Settings в порядке QDataStream, не сканируя произвольные байты в поисках прокси
    private static TgTelegramDesktopProxyImportResult ReadApplicationSettings(byte[] data)
    {
        var reader = new QtReader(data);
        reader.ReadBlob(); // Цвета темы
        reader.Skip(5 * 4); // Интерфейс, громкость и выбор каталога загрузки
        reader.ReadBlob(); // Каталог загрузки
        reader.ReadBlob(); // Закладка каталога
        reader.Skip(9 * 4); // Уведомления и автоблокировка
        reader.ReadBlob(); // Устройство вывода звука
        reader.ReadBlob(); // Устройство ввода звука
        reader.Skip(4 * 4); // Громкость звонков и предупреждения
        var sounds = reader.ReadCount();
        for (var i = 0; i < sounds; i++) { reader.ReadBlob(); reader.ReadBlob(); }
        reader.Skip(13 * 4); // Отправка, эмодзи, проверка орфографии и скорость видео
        reader.ReadBlob(); // Геометрия проигрывателя
        var dictionaries = reader.ReadCount();
        reader.Skip(checked(dictionaries * 8));
        reader.Skip(12 * 4); // Словари, разделы интерфейса и оформление окна
        reader.ReadBlob(); // Камера
        reader.Skip(2 * 4); // Предупреждение IP и режим разговора
        reader.ReadBlob(); // Сочетание клавиш разговора
        reader.Skip(8 + 2 * 4); // Задержка и устаревшие параметры звонков
        reader.ReadBlob(); // Положение окна
        var recentEmoji = reader.ReadCount();
        for (var i = 0; i < recentEmoji; i++) { reader.ReadBlob(); reader.Skip(2); }
        var variants = reader.ReadCount();
        for (var i = 0; i < variants; i++) { reader.ReadBlob(); reader.Skip(1); }
        reader.Skip(3 * 4); // Устаревшие настройки графики, шумоподавления и режим окна
        return ReadProxies(reader.ReadBlob());
    }

    // Читаем сохранённый список независимо от того, включено ли использование прокси в Telegram
    private static TgTelegramDesktopProxyImportResult ReadProxies(byte[] data)
    {
        if (data.Length == 0) return new([], 0);
        var reader = new QtReader(data);
        reader.Skip(3 * 4); // IPv6, звонки и включение прокси
        reader.ReadBlob(); // Выбранный прокси уже присутствует в списке
        var count = reader.ReadCount();
        var result = new List<TgEfProxyDto>();
        var skippedWeb = 0;
        for (var i = 0; i < count; i++)
        {
            var item = new QtReader(reader.ReadBlob());
            var type = item.ReadInt();
            var host = item.ReadString();
            var port = item.ReadInt();
            var user = item.ReadString();
            var password = item.ReadString();
            if (!item.AtEnd) throw InvalidFormat();
            if (type == 0) continue;
            if (type == 4) { skippedWeb++; continue; }
            if (type is < 1 or > 3)
                throw new InvalidDataException($"В Telegram настроен неподдерживаемый тип прокси: {type}. " +
                    "Импорт поддерживает SOCKS5, HTTP и MTProto; записи не сохранены");
            if (port is < 1 or > 65535) throw new InvalidDataException("В настройках Telegram указан недопустимый порт прокси");
            var dto = new TgEfProxyDto
            {
                Type = type switch { 1 => TgEnumProxyType.Socks, 2 => TgEnumProxyType.Http, _ => TgEnumProxyType.MtProto },
                HostName = host.Trim(), Port = (ushort)port, UserName = user,
                Password = type == 3 ? string.Empty : password, Secret = type == 3 ? password : string.Empty,
            };
            if (!TgGlobalTools.GetValidDto<TgEfProxyEntity, TgEfProxyDto>(dto).IsValid)
                throw new InvalidDataException("Запись прокси в Telegram содержит некорректные параметры. Проверьте её в настройках Telegram Desktop");
            result.Add(dto);
        }
        return new(result, skippedWeb);
    }

    // Восстанавливаем ключ общих настроек с пустым паролем согласно CreateLegacyLocalKey Telegram Desktop
    private static byte[] Decrypt(byte[] salt, byte[] encrypted)
    {
        if (encrypted.Length <= 16 || encrypted.Length % 16 != 0) throw InvalidFormat();
        var auth = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(ReadOnlySpan<byte>.Empty, salt, 4, System.Security.Cryptography.HashAlgorithmName.SHA1, 256);
        var messageKey = encrypted[..16];
        var a = System.Security.Cryptography.SHA1.HashData([.. messageKey, .. auth[8..40]]);
        var b = System.Security.Cryptography.SHA1.HashData([.. auth[40..56], .. messageKey, .. auth[56..72]]);
        var c = System.Security.Cryptography.SHA1.HashData([.. auth[72..104], .. messageKey]);
        var d = System.Security.Cryptography.SHA1.HashData([.. messageKey, .. auth[104..136]]);
        using var aes = System.Security.Cryptography.Aes.Create();
        aes.Key = [.. a[..8], .. b[8..20], .. c[4..16]];
        byte[] previousCipher = [.. a[8..20], .. b[..4]];
        byte[] previousPlain = [.. b[4..8], .. c[16..20], .. d[..8]];
        var plain = new byte[encrypted.Length - 16];
        for (var offset = 0; offset < plain.Length; offset += 16)
        {
            var cipher = encrypted[(offset + 16)..(offset + 32)];
            var mixed = new byte[16];
            for (var i = 0; i < 16; i++) mixed[i] = (byte)(cipher[i] ^ previousPlain[i]);
            var block = aes.DecryptEcb(mixed, System.Security.Cryptography.PaddingMode.None);
            for (var i = 0; i < 16; i++) plain[offset + i] = (byte)(block[i] ^ previousCipher[i]);
            previousCipher = cipher;
            previousPlain = plain[offset..(offset + 16)];
        }
        if (!System.Security.Cryptography.SHA1.HashData(plain).AsSpan(0, 16).SequenceEqual(messageKey)) throw InvalidFormat();
        var size = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(plain);
        if (size < 4 || size > plain.Length || size <= plain.Length - 16) throw InvalidFormat();
        return plain[4..size];
    }

    // Не включаем реквизиты прокси или содержимое расшифрованного файла в сообщения ошибок
    private static InvalidDataException InvalidFormat() => new("Некорректная структура локальных настроек Telegram Desktop");

    // Читаем только необходимые типы QDataStream версии Qt 5.1
    private sealed class QtReader(byte[] data)
    {
        private int _offset;
        public bool AtEnd => _offset == data.Length;

        /// <summary> Читает целое Qt в порядке big endian </summary>
        public int ReadInt()
        {
            Ensure(4);
            var value = System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(_offset, 4));
            _offset += 4;
            return value;
        }

        /// <summary> Проверяет длину счётчика по оставшейся части файла </summary>
        public int ReadCount()
        {
            var value = ReadInt();
            if (value < 0 || value > data.Length - _offset) throw InvalidFormat();
            return value;
        }

        /// <summary> Читает QByteArray или байтовое представление QString </summary>
        public byte[] ReadBlob()
        {
            var length = ReadInt();
            if (length == -1) return [];
            Ensure(length);
            var value = data[_offset..(_offset + length)];
            _offset += length;
            return value;
        }

        /// <summary> Декодирует QString без потери кириллицы </summary>
        public string ReadString()
        {
            var bytes = ReadBlob();
            if (bytes.Length % 2 != 0) throw InvalidFormat();
            return Encoding.BigEndianUnicode.GetString(bytes);
        }

        /// <summary> Пропускает поле с проверкой границ </summary>
        public void Skip(int count) { Ensure(count); _offset += count; }

        // Отклоняем усечённые поля до выделения памяти и чтения
        private void Ensure(int count)
        {
            if (count < 0 || count > data.Length - _offset) throw InvalidFormat();
        }
    }
}
