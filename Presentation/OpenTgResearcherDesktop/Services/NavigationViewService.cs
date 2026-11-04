namespace OpenTgResearcherDesktop.Services;

public sealed class NavigationViewService : INavigationViewService
{
	private readonly INavigationService _navigationService;
	private readonly IPageService _pageService;
	private NavigationView? _navigationView;
	public IList<object>? MenuItems => _navigationView?.MenuItems;
	public object? SettingsItem => _navigationView?.SettingsItem;

	public NavigationViewService(INavigationService navigationService, IPageService pageService)
	{
		_navigationService = navigationService;
		_pageService = pageService;
	}

	[MemberNotNull(nameof(_navigationView))]
	public void Initialize(NavigationView navigationView)
	{
		_navigationView = navigationView;
		_navigationView.Loaded += OnLoaded;
		Localizer.Get().LanguageChanged += OnLanguageChanged;
		_navigationView.BackRequested += OnBackRequested;
		_navigationView.ItemInvoked += OnItemInvoked;
	}

	public void UnregisterEvents()
	{
		if (_navigationView != null)
		{
			_navigationView.Loaded -= OnLoaded;
			Localizer.Get().LanguageChanged -= OnLanguageChanged;
			_navigationView.BackRequested -= OnBackRequested;
			_navigationView.ItemInvoked -= OnItemInvoked;
		}
	}

	public NavigationViewItem? GetSelectedItem(Type pageType)
	{
		if (_navigationView != null)
		{
			return GetSelectedItem(_navigationView.MenuItems, pageType) ?? GetSelectedItem(_navigationView.FooterMenuItems, pageType);
		}

		return null;
	}

	private void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args) => _navigationService.GoBack();

    // Подключаем встроенный пункт настроек к локализатору приложения после создания шаблона NavigationView
    private void OnLoaded(object sender, RoutedEventArgs e) => LocalizeSettingsItem();

    // Обновляем встроенную кнопку при смене языка приложения
    private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e) => TgDesktopUtils.InvokeOnUIThread(LocalizeSettingsItem);

    // Видимая подпись и имя для средств доступности встроенной кнопки задаются раздельно
    private void LocalizeSettingsItem()
    {
        if (_navigationView?.SettingsItem is not NavigationViewItem settingsItem) return;
        Uids.SetUid(settingsItem, "Shell_Settings");
        var caption = "Shell_Settings".GetLocalizedResource();
        settingsItem.Content = caption;
        settingsItem.SetValue(Microsoft.UI.Xaml.Automation.AutomationProperties.NameProperty, caption);
    }

	private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
	{
		if (args.IsSettingsInvoked)
		{
			_navigationService.NavigateTo(typeof(TgSettingsViewModel).FullName!);
		}
		else
		{
			var selectedItem = args.InvokedItemContainer as NavigationViewItem;

			if (selectedItem?.GetValue(TgNavigationHelper.NavigateToProperty) is string pageKey)
			{
				_navigationService.NavigateTo(pageKey);
			}
		}
	}

	private NavigationViewItem? GetSelectedItem(IEnumerable<object> menuItems, Type pageType)
	{
		foreach (var item in menuItems.OfType<NavigationViewItem>())
		{
			if (IsMenuItemForPageType(item, pageType))
			{
				return item;
			}
			var selectedChild = GetSelectedItem(item.MenuItems, pageType);
			if (selectedChild != null)
			{
				return selectedChild;
			}
		}
		return null;
	}

	private bool IsMenuItemForPageType(NavigationViewItem menuItem, Type sourcePageType)
	{
		if (menuItem.GetValue(TgNavigationHelper.NavigateToProperty) is string pageKey)
		{
			return _pageService.GetPageType(pageKey) == sourcePageType;
		}
		return false;
	}
}
