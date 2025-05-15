// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Models;

public sealed class LocalSettingsOptions
{
    public string? ApplicationDataFolder { get; set; }
    public string? LocalSettingsFile { get; set; }
    public string? AppTheme { get; set; }
	public string? AppLanguage { get; set; }
    public string? AppStorage { get; set; }
    public string? AppSession { get; set; }
}
