// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Contracts.Services;

public interface INavigationViewService
{
	IList<object>? MenuItems { get; }
	object? SettingsItem { get; }
	void Initialize(NavigationView navigationView);
	void UnregisterEvents();
	NavigationViewItem? GetSelectedItem(Type pageType);
}
