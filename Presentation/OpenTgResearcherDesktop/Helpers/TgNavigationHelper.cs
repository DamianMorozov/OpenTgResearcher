// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Helpers;

// Helper class to set the navigation target for a NavigationViewItem.
//
// Usage in XAML:
// <NavigationViewItem x:Uid="Shell_Main" Icon="Document" helpers:TgNavigationHelper.NavigateTo="AppName.ViewModels.TgMainViewModel" />
//
// Usage in code:
// TgNavigationHelper.SetNavigateTo(navigationViewItem, typeof(TgMainViewModel).FullName);
public sealed class TgNavigationHelper
{
	public static string GetNavigateTo(NavigationViewItem item) => (string)item.GetValue(NavigateToProperty);

	public static void SetNavigateTo(NavigationViewItem item, string value) => item.SetValue(NavigateToProperty, value);

	public static readonly DependencyProperty NavigateToProperty =
		DependencyProperty.RegisterAttached("NavigateTo", typeof(string), typeof(TgNavigationHelper), new PropertyMetadata(null));
}
