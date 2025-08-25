// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
#pragma warning disable CA1416

namespace OpenTgResearcherDesktop.Services;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgSettingsService : ObservableRecipient, ITgSettingsService
{
    #region Fields, properties, constructor

    private const string SettingsKeyAppTheme = nameof(LocalSettingsOptions.AppTheme);
    private const string SettingsKeyAppLanguage = nameof(LocalSettingsOptions.AppLanguage);
    private const string SettingsKeyAppStorage = nameof(LocalSettingsOptions.AppStorage);
    private const string SettingsKeyAppSession = nameof(LocalSettingsOptions.AppSession);
    private const string SettingsKeyWindowWidth = nameof(LocalSettingsOptions.WindowWidth);
    private const string SettingsKeyWindowHeight = nameof(LocalSettingsOptions.WindowHeight);
    private const string SettingsKeyWindowX = nameof(LocalSettingsOptions.WindowX);
    private const string SettingsKeyWindowY = nameof(LocalSettingsOptions.WindowY);
    [ObservableProperty]
    public partial ObservableCollection<TgEnumTheme> AppThemes { get; set; } = null!;
    [ObservableProperty]
    public partial TgEnumTheme AppTheme { get; set; } = TgEnumTheme.Default;
    [ObservableProperty]
    public partial ObservableCollection<TgEnumLanguage> AppLanguages { get; set; } = null!;
    [ObservableProperty]
    public partial TgEnumLanguage AppLanguage { get; set; } = TgEnumLanguage.Default;
    [ObservableProperty]
    public partial string UserDirectory { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string ApplicationDirectory { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string SettingFile { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string AppStorage { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string AppSession { get; set; } = string.Empty;
    [ObservableProperty]
    public partial bool IsExistsAppStorage { get; set; }
    [ObservableProperty]
    public partial bool IsExistsAppSession { get; set; }
    [ObservableProperty]
    public partial int WindowWidth { get; set; }
    [ObservableProperty]
    public partial int WindowHeight { get; set; }
    [ObservableProperty]
    public partial int WindowX { get; set; }
    [ObservableProperty]
    public partial int WindowY { get; set; }

    private const string DefaultLocalSettingsFile = "TgLocalSettings.json";
    private readonly IFileService _fileService;
    private readonly string _applicationDataFolder = string.Empty;
    private readonly string _localSettingsFile = string.Empty;
    private IDictionary<string, object> _settings;
    private bool _isInitialized;
    private readonly LocalSettingsOptions _options;

    public TgSettingsService(IFileService fileService, IOptions<LocalSettingsOptions> options)
    {
        _fileService = fileService;
        _settings = new Dictionary<string, object>();
        _options = options.Value;

        try
        {
            AppThemes = [TgEnumTheme.Default, TgEnumTheme.Light, TgEnumTheme.Dark];
            AppLanguages = [TgEnumLanguage.Default, TgEnumLanguage.English, TgEnumLanguage.Russian];
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }

        try
        {
            _applicationDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), _options.ApplicationDataFolder ?? TgConstants.OpenTgResearcherDesktop);
            _localSettingsFile = _options.LocalSettingsFile ?? DefaultLocalSettingsFile;
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }

        try
        {
            Default();
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }
    }

    #endregion

    #region Methods

    public string ToDebugString() => TgObjectUtils.ToDebugString(this);

    // TODO: Fix here
    public void SetAppLanguage()
    {
        try
        {
            if (TgRuntimeHelper.IsMSIX)
            {
                var languageCode = TgEnumUtils.GetLanguageAsString(AppLanguage);
                // The PrimaryLanguageOverride setting should be done in the UI thread
                if (App.MainWindow?.DispatcherQueue != null)
                {
                    var tcs = new TaskCompletionSource<bool>();
                    App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                    {
                        try
                        {
                            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = languageCode;
                            tcs.SetResult(true);
                        }
                        catch (Exception ex)
                        {
                            TgLogUtils.WriteException(ex);
                            tcs.SetException(ex);
                        }
                    });
                }
            }
            else
            {
                var context = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForViewIndependentUse();
                context.Languages = AppLanguage switch
                {
                    TgEnumLanguage.Russian => new List<string> { TgConstants.LocaleRuRu },
                    _ => new List<string> { TgConstants.LocaleEnUs },
                };
            }

            // TODO: update UI resources
            //if (App.MainWindow?.Content is FrameworkElement rootElement)
            //{
            //    rootElement.Resources.MergedDictionaries.Clear();
            //    rootElement.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri($"ms-appx:///Resources/Strings.{languageCode}.xaml") });
            //}

            // TODO: add logic for reloading the current page
            //var frame = (App.MainWindow?.Content as Frame);
            //if (frame != null)
            //{
            //    var currentPageType = frame.CurrentSourcePageType;
            //    frame.Navigate(currentPageType);
            //}
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteExceptionWithMessage(ex, "Error occurred while setting app language.");
        }
    }

    private TgEnumTheme LoadAppTheme()
    {
        var themeName = ReadSetting<string>(SettingsKeyAppTheme);
        return Enum.TryParse(themeName, out TgEnumTheme cacheTheme) ? cacheTheme : TgEnumTheme.Default;
    }

    /// <inheritdoc />
    public void SetupAppStorage()
    {
        if (!string.IsNullOrEmpty(AppStorage))
            return;

        // Attempt to read the path from settings
        var savedPath = ReadSetting<string>(SettingsKeyAppStorage);

        if (!string.IsNullOrEmpty(savedPath) && Directory.Exists(Path.GetDirectoryName(savedPath)))
        {
            AppStorage = savedPath;
            TgGlobalTools.AppStorage = AppStorage;
        }
        else
        {
            // If the path is not found or is incorrect, use the default path.
            var defaultFileName = TgGlobalTools.AppStorage;
            var defaultPath = Path.Combine(ApplicationDirectory, defaultFileName);

            AppStorage = defaultPath;

            // Save the path to settings so that it does not need to be recalculated
            SaveSetting(SettingsKeyAppStorage, AppStorage);
        }

        IsExistsAppStorage = File.Exists(AppStorage);
    }

    private string LoadAppSession() => ReadSetting<string>(SettingsKeyAppSession) ?? TgFileUtils.FileTgSession;

    private TgEnumLanguage LoadAppLanguage()
    {
        var appLanguage = ReadSetting<string>(SettingsKeyAppLanguage) ?? nameof(TgEnumLanguage.Default);
        return TgEnumUtils.GetLanguageAsEnum(appLanguage);
    }

    private int LoadWindowWidth()
    {
        var str = ReadSettingAsInt<string>(SettingsKeyWindowWidth) ?? string.Empty;
        return string.IsNullOrEmpty(str) ? 0 : int.TryParse(str, out var width) ? width : 0;
    }

    private int LoadWindowHeight()
    {
        var str = ReadSettingAsInt<string>(SettingsKeyWindowHeight) ?? string.Empty;
        return string.IsNullOrEmpty(str) ? 0 : int.TryParse(str, out var width) ? width : 0;
    }

    private int LoadWindowX()
    {
        var str = ReadSettingAsInt<string>(SettingsKeyWindowX) ?? string.Empty;
        return string.IsNullOrEmpty(str) ? 0 : int.TryParse(str, out var width) ? width : 0;
    }

    private int LoadWindowY()
    {
        var str = ReadSettingAsInt<string>(SettingsKeyWindowY) ?? string.Empty;
        return string.IsNullOrEmpty(str) ? 0 : int.TryParse(str, out var width) ? width : 0;
    }

    public void SetTheme(TgEnumTheme appTheme)
    {
        if (App.MainWindow.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = TgThemeUtils.GetElementTheme(AppTheme);
            TgTitleBarHelper.UpdateTitleBar(rootElement.RequestedTheme);
        }
    }

    private void SaveWindowWidth(int value) => SaveSetting(SettingsKeyWindowWidth, value);

    private void SaveWindowHeight(int value) => SaveSetting(SettingsKeyWindowHeight, value);

    private void SaveWindowX(int value) => SaveSetting(SettingsKeyWindowX, value);

    private void SaveWindowY(int value) => SaveSetting(SettingsKeyWindowY, value);

    /// <summary> Set user directory path </summary>
    private void SetUserDirectory() => UserDirectory = TgLogUtils.GetAppDirectory(TgEnumAppType.Desktop);

    /// <summary> Set application directory path </summary>
	private void SetApplicationDirectory()
    {
        try
        {
            //ApplicationDirectory = ApplicationData.Current?.LocalFolder?.Path ?? string.Empty;
            if (string.IsNullOrEmpty(ApplicationDirectory) || !Directory.Exists(ApplicationDirectory))
                ApplicationDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (string.IsNullOrEmpty(ApplicationDirectory) || !Directory.Exists(ApplicationDirectory))
                ApplicationDirectory = Path.GetDirectoryName(Environment.ProcessPath) ?? string.Empty;
        }
        catch (Exception ex)
        {
            try
            {
                TgLogUtils.WriteException(ex);
            }
            catch (Exception)
            {
                //
            }
        }

        try
        {
            if (!Directory.Exists(ApplicationDirectory))
                ApplicationDirectory = AppDomain.CurrentDomain.BaseDirectory;
        }
        catch (Exception ex)
        {
            try
            {
                TgLogUtils.WriteException(ex);
            }
            catch (Exception)
            {
                //
            }
        }

        try
        {
            if (!Directory.Exists(ApplicationDirectory))
                ApplicationDirectory = Path.GetDirectoryName(Environment.ProcessPath) ?? string.Empty;
        }
        catch (Exception ex)
        {
            try
            {
                TgLogUtils.WriteException(ex);
            }
            catch (Exception)
            {
                //
            }
        }
    }

    /// <summary> Set application directory path </summary>
    private void SetSettingFile()
    {
        Initialize();
        SettingFile = Path.Combine(_applicationDataFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), _localSettingsFile);
    }

    public void Default()
    {
        AppTheme = AppThemes.First(x => x == TgEnumTheme.Default);
        AppLanguage = AppLanguages.First(x => x == TgEnumLanguage.Default);
        SetUserDirectory();
        SetApplicationDirectory();
        SetSettingFile();
        if (Directory.Exists(ApplicationDirectory))
        {
            AppSession = Directory.Exists(ApplicationDirectory) ? Path.Combine(ApplicationDirectory, TgFileUtils.FileTgSession) : string.Empty;
            IsExistsAppSession = File.Exists(AppSession);
        }
        else
        {
            AppSession = string.Empty;
            IsExistsAppSession = false;
        }

        SetupAppStorage();
    }

    public void Load()
    {
        var appTheme = LoadAppTheme();
        AppTheme = AppThemes.First(x => x == appTheme);
        var appLanguage = LoadAppLanguage();
        AppLanguage = AppLanguages.First(x => x == appLanguage);

        SetupAppStorage();
        // Register TgEfContext as the DbContext for EF Core
        TgGlobalTools.AppStorage = App.GetService<ITgSettingsService>().AppStorage;

        AppSession = LoadAppSession();
        IsExistsAppSession = File.Exists(AppSession);
    }

    public void LoadWindow()
    {
        WindowWidth = LoadWindowWidth();
        WindowHeight = LoadWindowHeight();
        WindowX = LoadWindowX();
        WindowY = LoadWindowY();
    }

    public void Save()
    {
        SaveSetting(SettingsKeyAppStorage, AppStorage);
        SaveSetting(SettingsKeyAppSession, AppSession);
        SaveSetting(SettingsKeyAppTheme, AppTheme.ToString());
        SaveSetting(SettingsKeyAppLanguage, AppLanguage.ToString());

        SetTheme(AppTheme);

        SetAppLanguage();
    }

    public void SaveWindow(int width, int height, int x, int y)
    {
        SaveWindowWidth(WindowWidth = width);
        SaveWindowHeight(WindowHeight = height);
        SaveWindowX(WindowX = x);
        SaveWindowY(WindowY = y);
    }

    private void Initialize()
    {
        if (!_isInitialized)
        {
            _settings = _fileService.Read<IDictionary<string, object>>(_applicationDataFolder, _localSettingsFile) ?? new Dictionary<string, object>();
            _isInitialized = true;
        }
    }

    public T? ReadSetting<T>(string key)
    {
        try
        {
            Initialize();

            if (_settings == null || !_settings.TryGetValue(key, out var obj))
                return default;

            // If T is a string or a value of the required type, we try to return it directly
            if (typeof(T) == typeof(string))
            {
                if (obj is string s)
                    return (T)(object)s;

                if (obj != null)
                {
                    var str = obj?.ToString();
                    return str == null ? default : (T)(object)str;
                }

                return default;
            }

            // If T is an enum and obj is a string, we try to parse the enum from the string “Dark”, “Default”, etc.
            if (typeof(T).IsEnum)
            {
                if (obj is string enumStr)
                {
                    if (Enum.TryParse(typeof(T), enumStr, ignoreCase: true, out var enumVal))
                        return (T)enumVal;
                    else
                        return default;
                }
                // If obj is a JsonElement with the string
                if (obj is JsonElement jeEnum && jeEnum.ValueKind == JsonValueKind.String)
                {
                    var enumStr2 = jeEnum.GetString();
                    if (enumStr2 != null && Enum.TryParse(typeof(T), enumStr2, ignoreCase: true, out var enumVal2))
                        return (T)enumVal2;
                    else
                        return default;
                }
            }

            // If obj is JsonElement and T is not string/enum - deserialize by raw text
            if (obj is JsonElement je)
            {
                var raw = je.GetRawText();
                if (string.IsNullOrWhiteSpace(raw))
                    return default;
                return JsonSerializer.Deserialize<T>(raw, TgJsonSerializerUtils.GetJsonOptions());
            }

            // If obj is string (json), deserialize it
            if (obj is string jsonString)
            {
                if (string.IsNullOrWhiteSpace(jsonString))
                    return default;

                return JsonSerializer.Deserialize<T>(jsonString, TgJsonSerializerUtils.GetJsonOptions());
            }

            // For other cases we try to cast to string and deserialize (if possible)
            var strVal = obj?.ToString();
            if (string.IsNullOrWhiteSpace(strVal))
                return default;

            return JsonSerializer.Deserialize<T>(strVal, TgJsonSerializerUtils.GetJsonOptions());
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            return default;
        }
    }

    public T? ReadSettingAsInt<T>(string key)
    {
        try
        {
            string value = string.Empty;
            Initialize();
            if (_settings != null && _settings.TryGetValue(key, out var obj))
            {
                value = obj.ToString() ?? string.Empty;
            }

            if (typeof(T) == typeof(JsonElement))
            {
                return JsonSerializer.Deserialize<T>(value, TgJsonSerializerUtils.GetJsonOptions());
            }
            else if (typeof(T) == typeof(string))
            {
                return (T)(object)value;
            }
            else
            {
                return JsonSerializer.Deserialize<T>(value, TgJsonSerializerUtils.GetJsonOptions());
            }
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }
        return default;
    }

    public void SaveSetting<T>(string key, T value)
    {
        try
        {
            Initialize();
            if (value is string strValue)
            {
                _settings[key] = strValue;
            }
            else
            {
                _settings[key] = JsonUtils.Stringify(value);
            }
            _fileService.Save(_applicationDataFolder, _localSettingsFile, _settings);
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }
    }

    #endregion
}
