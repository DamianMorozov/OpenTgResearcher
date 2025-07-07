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
	public partial string AppFolder { get; set; } = string.Empty;
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

    private const string DefaultApplicationDataFolder = "OpenTgResearcherDesktop/ApplicationData";
	private const string DefaultLocalSettingsFile = "TgLocalSettings.json";
	private readonly IFileService _fileService;
	private readonly string _localApplicationData = string.Empty;
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
			Default();
		}
		catch (Exception ex)
		{
			TgLogUtils.WriteException(ex);
		}

		try
		{
			_localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			_applicationDataFolder = Path.Combine(_localApplicationData, _options.ApplicationDataFolder ?? DefaultApplicationDataFolder);
			_localSettingsFile = _options.LocalSettingsFile ?? DefaultLocalSettingsFile;
		}
		catch (Exception ex)
		{
			TgLogUtils.WriteException(ex);
		}
	}

	#endregion

	#region Public and private methods

	public string ToDebugString() => TgObjectUtils.ToDebugString(this);

	public async Task SetAppThemeAsync()
	{
		await Task.CompletedTask;
	}

	public async Task SetAppLanguageAsync()
	{
		Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = TgEnumUtils.GetLanguageAsString(AppLanguage);
		//// Update UI and resources
		//var resourceLoader = new ResourceLoader();
		//if (App.Current is App app)
		//{
		//	if (App.MainWindow.Content is FrameworkElement rootElement)
		//	{
		//		rootElement.Resources.ApplyResources(fe, fe.ResourceManager, languageCode);
		//	}
		//}
		await Task.CompletedTask;
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

    public void ApplyTheme(TgEnumTheme appTheme)
	{
		if (App.MainWindow.Content is FrameworkElement rootElement)
		{
			rootElement.RequestedTheme = TgThemeUtils.GetElementTheme(AppTheme);
			TgTitleBarHelper.UpdateTitleBar(rootElement.RequestedTheme);
		}
	}

	private async Task SaveAppThemeAsync(TgEnumTheme appTheme)
	{
		ApplyTheme(appTheme);
		await SaveSettingAsync(SettingsKeyAppTheme, appTheme.ToString());
	}

    private async Task SaveAppLanguageAsync(TgEnumLanguage appLanguage) => await SaveSettingAsync(SettingsKeyAppLanguage, appLanguage.ToString());

    private async Task SaveAppStorageAsync(string appStorage) => await SaveSettingAsync(SettingsKeyAppStorage, appStorage);

	private async Task SaveAppSessionAsync(string appSession) => await SaveSettingAsync(SettingsKeyAppSession, appSession);

	private async Task SaveWindowWidthAsync(int value) => await SaveSettingAsync(SettingsKeyWindowWidth, value);

	private async Task SaveWindowHeightAsync(int value) => await SaveSettingAsync(SettingsKeyWindowHeight, value);

    private async Task SaveWindowXAsync(int value) => await SaveSettingAsync(SettingsKeyWindowX, value);

	private async Task SaveWindowYAsync(int value) => await SaveSettingAsync(SettingsKeyWindowY, value);

    /// <summary> Set application folder path </summary>
	private void SetAppFolder()
	{
		try
		{
			AppFolder = ApplicationData.Current.LocalFolder.Path;
			if (!Directory.Exists(AppFolder))
				AppFolder = AppDomain.CurrentDomain.BaseDirectory;
			if (!Directory.Exists(AppFolder))
				AppFolder = Path.GetDirectoryName(Environment.ProcessPath) ?? string.Empty;
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
			if (!Directory.Exists(AppFolder))
				AppFolder = AppDomain.CurrentDomain.BaseDirectory;
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
			if (!Directory.Exists(AppFolder))
				AppFolder = Path.GetDirectoryName(Environment.ProcessPath) ?? string.Empty;
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

	public void Default()
	{
		AppTheme = AppThemes.First(x => x == TgEnumTheme.Default);
		AppLanguage = AppLanguages.First(x => x == TgEnumLanguage.Default);
		SetAppFolder();
		if (Directory.Exists(AppFolder))
		{
			AppStorage = Directory.Exists(AppFolder) ? Path.Combine(AppFolder, TgGlobalTools.AppStorage) : string.Empty;
			IsExistsAppStorage = File.Exists(AppStorage);
			AppSession = Directory.Exists(AppFolder) ? Path.Combine(AppFolder, TgFileUtils.FileTgSession) : string.Empty;
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
		await SaveAppThemeAsync(AppTheme);
		await SaveAppLanguageAsync(AppLanguage);
		await SaveAppStorageAsync(AppStorage);
		await SaveAppSessionAsync(AppSession);
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
			if (TgRuntimeHelper.IsMSIX)
			{
				if (ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out var obj))
				{
					return await Json.ToObjectAsync<T>((string)obj);
				}
			}
			else
			{
				await InitializeAsync();
				if (_settings != null && _settings.TryGetValue(key, out var obj))
				{
					return await Json.ToObjectAsync<T>((string)obj);
				}
			}
		}
		catch (Exception ex)
		{
			TgLogUtils.WriteException(ex);
		}
		return default;
	}

	public async Task<T?> ReadSettingAsIntAsync<T>(string key)
	{
        try
        {
            string value = string.Empty;
            if (TgRuntimeHelper.IsMSIX)
            {
                if (ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out var obj))
                {
                    value = obj.ToString() ?? string.Empty;
                }
            }
            else
            {
                await InitializeAsync();
                if (_settings != null && _settings.TryGetValue(key, out var obj))
                {
                    value = obj.ToString() ?? string.Empty;
                }
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
			if (TgRuntimeHelper.IsMSIX)
			{
				ApplicationData.Current.LocalSettings.Values[key] = await Json.StringifyAsync(value);
			}
			else
			{
				await InitializeAsync();
				_settings[key] = await Json.StringifyAsync(value);
				await Task.Run(() => _fileService.Save(_applicationDataFolder, _localSettingsFile, _settings));
			}
		}
		catch (Exception ex)
		{
			TgLogUtils.WriteException(ex);
		}
	}

	#endregion
}
