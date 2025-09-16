namespace OpenTgResearcherDesktop.Models;

public sealed class LocalSettingsOptions
{
    public string? ApplicationDataFolder { get; set; }
    public string? LocalSettingsFile { get; set; }
    public string? AppTheme { get; set; }
	public string? AppLanguage { get; set; }
    public string? AppStorage { get; set; }
    public string? AppSession { get; set; }
    public string? WindowWidth { get; set; }
    public string? WindowHeight { get; set; }
    public string? WindowX { get; set; }
    public string? WindowY { get; set; }
}
