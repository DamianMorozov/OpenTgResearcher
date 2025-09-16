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

    public static object GetNavigationParameter(NavigationViewItem item) => item.GetValue(NavigationParameterProperty);

    public static void SetNavigationParameter(NavigationViewItem item, object value) => item.SetValue(NavigationParameterProperty, value);

    public static readonly DependencyProperty NavigationParameterProperty =
        DependencyProperty.RegisterAttached("NavigationParameter", typeof(object), typeof(TgNavigationHelper), new PropertyMetadata(null));

}
