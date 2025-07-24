// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Contracts.Services;

public interface ITgSettingsService
{
	ObservableCollection<TgEnumTheme> AppThemes { get; }
	ObservableCollection<TgEnumLanguage> AppLanguages { get; }
	TgEnumTheme AppTheme { get; set; }
	TgEnumLanguage AppLanguage { get; set; }
	string AppStorage { get; set; }
	string AppSession { get; set; }
	string UserDirectory { get; }
	string ApplicationDirectory { get; }
	string SettingFile { get; }
	bool IsExistsAppStorage { get; }
	bool IsExistsAppSession { get; }
	int WindowWidth { get; }
	int WindowHeight { get; }
	int WindowX { get; }
	int WindowY { get; }

	void Default();
	Task LoadAsync();
	Task LoadWindowAsync();
	Task SaveAsync();
	Task SaveWindowAsync(int width, int height, int x, int y);
    void SetTheme(TgEnumTheme appTheme);
	Task<T?> ReadSettingAsync<T>(string key);
	Task SaveSettingAsync<T>(string key, T value);
}
