using System.ComponentModel;

namespace OpenTgResearcherDesktop.Contracts.Services;

/// <summary> Settings service </summary>
public interface ITgSettingsService : INotifyPropertyChanged
{
    public ObservableCollection<TgEnumTheme> AppThemes { get; }
    public ObservableCollection<TgEnumLanguage> AppLanguages { get; }
    public TgEnumTheme AppTheme { get; set; }
    public TgEnumLanguage AppLanguage { get; set; }
    public string AppStorage { get; set; }
    public string AppSession { get; set; }
    public string UserDirectory { get; }
    public string ApplicationDirectory { get; }
    public string SettingFile { get; }
    public bool IsExistsAppStorage { get; }
    public bool IsExistsAppSession { get; }
    public int WindowWidth { get; }
    public int WindowHeight { get; }
    public int WindowX { get; }
    public int WindowY { get; }

    public void Default();
    public void Load();
    public void LoadWindow();
    public void Save();
    public void SaveWindow(int width, int height, int x, int y);
    public void SetTheme(TgEnumTheme appTheme);
    public T? ReadSetting<T>(string key);
	public void SaveSetting<T>(string key, T value);
    /// <summary> Setup application storage path </summary>
    public void SetupAppStorage();
}
