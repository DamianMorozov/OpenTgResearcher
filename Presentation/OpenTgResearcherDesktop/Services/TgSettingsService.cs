// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
#pragma warning disable CA1416

namespace OpenTgResearcherDesktop.Services;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgSettingsService : ObservableRecipient, ITgSettingsService
{
    #region Public and private fields, properties, constructor

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
    private readonly INavigationService _navigationService;

    public TgSettingsService(IFileService fileService, IOptions<LocalSettingsOptions> options, INavigationService navigationService)
    {
        _fileService = fileService;
        _settings = new Dictionary<string, object>();
        _options = options.Value;
        _navigationService = navigationService;

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

    #region Public and private methods

    public string ToDebugString() => TgObjectUtils.ToDebugString(this);

    public async Task SetAppLanguageAsync()
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
                    await tcs.Task;
                }
            }
            else
            {
                var context = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForViewIndependentUse();
                switch (AppLanguage)
                {
                    case TgEnumLanguage.Russian:
                        context.Languages = new List<string> { TgConstants.LocaleRuRu };
                        break;
                    default:
                        context.Languages = new List<string> { TgConstants.LocaleEnUs };
                        break;
                }
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

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteExceptionWithMessage(ex, "Error occurred while setting app language.");
        }
    }

    private async Task<TgEnumTheme> LoadAppThemeAsync()
    {
        var themeName = await ReadSettingAsync<string>(SettingsKeyAppTheme);
        return Enum.TryParse(themeName, out TgEnumTheme cacheTheme) ? cacheTheme : TgEnumTheme.Default;
    }

    private async Task<string> LoadAppStorageAsync() => await ReadSettingAsync<string>(SettingsKeyAppStorage) ?? TgGlobalTools.AppStorage;

    private async Task<string> LoadAppSessionAsync() => await ReadSettingAsync<string>(SettingsKeyAppSession) ?? TgFileUtils.FileTgSession;

    private async Task<TgEnumLanguage> LoadAppLanguageAsync()
    {
        var appLanguage = await ReadSettingAsync<string>(SettingsKeyAppLanguage) ?? nameof(TgEnumLanguage.Default);
        return TgEnumUtils.GetLanguageAsEnum(appLanguage);
    }

    private async Task<int> LoadWindowWidthAsync()
    {
        var str = await ReadSettingAsIntAsync<string>(SettingsKeyWindowWidth) ?? string.Empty;
        return string.IsNullOrEmpty(str) ? 0 : int.TryParse(str, out var width) ? width : 0;
    }

    private async Task<int> LoadWindowHeightAsync()
    {
        var str = await ReadSettingAsIntAsync<string>(SettingsKeyWindowHeight) ?? string.Empty;
        return string.IsNullOrEmpty(str) ? 0 : int.TryParse(str, out var width) ? width : 0;
    }

    private async Task<int> LoadWindowXAsync()
    {
        var str = await ReadSettingAsIntAsync<string>(SettingsKeyWindowX) ?? string.Empty;
        return string.IsNullOrEmpty(str) ? 0 : int.TryParse(str, out var width) ? width : 0;
    }

    private async Task<int> LoadWindowYAsync()
    {
        var str = await ReadSettingAsIntAsync<string>(SettingsKeyWindowY) ?? string.Empty;
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

    private async Task SaveWindowWidthAsync(int value) => await SaveSettingAsync(SettingsKeyWindowWidth, value);

    private async Task SaveWindowHeightAsync(int value) => await SaveSettingAsync(SettingsKeyWindowHeight, value);

    private async Task SaveWindowXAsync(int value) => await SaveSettingAsync(SettingsKeyWindowX, value);

    private async Task SaveWindowYAsync(int value) => await SaveSettingAsync(SettingsKeyWindowY, value);

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
    private async void SetSettingFile()
    {
        await InitializeAsync();
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
            AppStorage = Directory.Exists(ApplicationDirectory) ? Path.Combine(ApplicationDirectory, TgGlobalTools.AppStorage) : string.Empty;
            IsExistsAppStorage = File.Exists(AppStorage);
            AppSession = Directory.Exists(ApplicationDirectory) ? Path.Combine(ApplicationDirectory, TgFileUtils.FileTgSession) : string.Empty;
            IsExistsAppSession = File.Exists(AppSession);
        }
        else
        {
            AppStorage = string.Empty;
            IsExistsAppStorage = false;
            AppSession = string.Empty;
            IsExistsAppSession = false;
        }
    }

    public async Task LoadAsync()
    {
        var appTheme = await LoadAppThemeAsync();
        AppTheme = AppThemes.First(x => x == appTheme);
        var appLanguage = await LoadAppLanguageAsync();
        AppLanguage = AppLanguages.First(x => x == appLanguage);

        AppStorage = await LoadAppStorageAsync();
        IsExistsAppStorage = File.Exists(AppStorage);
        // Register TgEfContext as the DbContext for EF Core
        TgGlobalTools.AppStorage = App.GetService<ITgSettingsService>().AppStorage;

        AppSession = await LoadAppSessionAsync();
        IsExistsAppSession = File.Exists(AppSession);
    }

    public async Task LoadWindowAsync()
    {
        WindowWidth = await LoadWindowWidthAsync();
        WindowHeight = await LoadWindowHeightAsync();
        WindowX = await LoadWindowXAsync();
        WindowY = await LoadWindowYAsync();
    }

    public async Task SaveAsync()
    {
        await SaveSettingAsync(SettingsKeyAppStorage, AppStorage);
        await SaveSettingAsync(SettingsKeyAppSession, AppSession);
        await SaveSettingAsync(SettingsKeyAppTheme, AppTheme.ToString());
        await SaveSettingAsync(SettingsKeyAppLanguage, AppLanguage.ToString());

        SetTheme(AppTheme);

        await SetAppLanguageAsync();
    }

    public async Task SaveWindowAsync(int width, int height, int x, int y)
    {
        await SaveWindowWidthAsync(WindowWidth = width);
        await SaveWindowHeightAsync(WindowHeight = height);
        await SaveWindowXAsync(WindowX = x);
        await SaveWindowYAsync(WindowY = y);
    }

    private async Task InitializeAsync()
    {
        if (!_isInitialized)
        {
            _settings = await Task.Run(() => _fileService.Read<IDictionary<string, object>>(_applicationDataFolder, _localSettingsFile)) ?? new Dictionary<string, object>();
            _isInitialized = true;
        }
    }

    public async Task<T?> ReadSettingAsync<T>(string key)
    {
        try
        {
            await InitializeAsync();

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

    public async Task<T?> ReadSettingAsIntAsync<T>(string key)
    {
        try
        {
            string value = string.Empty;
            await InitializeAsync();
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

    public async Task SaveSettingAsync<T>(string key, T value)
    {
        try
        {
            await InitializeAsync();
            if (value is string strValue)
            {
                _settings[key] = strValue;
            }
            else
            {
                _settings[key] = await Json.StringifyAsync(value);
            }
            await Task.Run(() => _fileService.Save(_applicationDataFolder, _localSettingsFile, _settings));
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }
    }

    #endregion
}
