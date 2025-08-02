// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Services;

public sealed class PageService : IPageService
{
	private readonly Dictionary<string, Type> _pages = [];

	public PageService()
	{
		Configure<ContentGridDetailViewModel, ContentGridDetailPage>();
		Configure<ContentGridViewModel, ContentGridPage>();
		Configure<DataGridViewModel, DataGridPage>();
		Configure<ListDetailsViewModel, ListDetailsPage>();
		Configure<TgBotConnectionViewModel, TgBotConnectionPage>();
		Configure<TgChatDetailsContentViewModel, TgChatDetailsContentPage>();
		Configure<TgChatDetailsInfoViewModel, TgChatDetailsInfoPage>();
		Configure<TgChatDetailsParticipantsViewModel, TgChatDetailsParticipantsPage>();
		Configure<TgChatDetailsStatisticsViewModel, TgChatDetailsStatisticsPage>();
		Configure<TgChatViewModel, TgChatPage>();
		Configure<TgChatsViewModel, TgChatsPage>();
		Configure<TgClientConnectionViewModel, TgClientConnectionPage>();
		Configure<TgFiltersViewModel, TgFiltersPage>();
		Configure<TgLicenseViewModel, TgLicensePage>();
		Configure<TgLoadDataViewModel, TgLoadDataPage>();
		Configure<TgLogsViewModel, TgLogsPage>();
		Configure<TgMainViewModel, TgMainPage>();
		Configure<TgProxiesViewModel, TgProxiesPage>();
		Configure<TgSettingsViewModel, TgSettingsPage>();
		Configure<TgSplashScreenViewModel, TgSplashScreenPage>();
		Configure<TgStorageViewModel, TgStoragePage>();
		Configure<TgStoriesViewModel, TgStoriesPage>();
		Configure<TgUpdateViewModel, TgUpdatePage>();
		Configure<TgUserDetailsViewModel, TgUserDetailsPage>();
		Configure<TgUsersViewModel, TgUsersPage>();
		Configure<WebViewViewModel, WebViewPage>();
	}

	public Type GetPageType(string key)
	{
		Type? pageType;
		lock (_pages)
		{
			if (!_pages.TryGetValue(key, out pageType))
			{
				throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
			}
		}
		return pageType;
	}

	private void Configure<VM, V>()
		where VM : ObservableObject
		where V : Page
	{
		lock (_pages)
		{
			var key = typeof(VM).FullName!;
			if (_pages.ContainsKey(key))
			{
				throw new ArgumentException($"The key {key} is already configured in PageService");
			}

			var type = typeof(V);
			if (_pages.ContainsValue(type))
			{
				throw new ArgumentException($"This type is already configured with key {_pages.First(p => p.Value == type).Key}");
			}

			_pages.Add(key, type);
		}
	}
}
