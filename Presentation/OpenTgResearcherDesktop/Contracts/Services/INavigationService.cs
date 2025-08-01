// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Contracts.Services;

public interface INavigationService
{
	event NavigatedEventHandler Navigated;
	bool CanGoBack { get; }
	Frame? Frame { get; set; }
    bool IsDisplaySensitiveData { get; set; }

    bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false);

	bool GoBack();

	void SetListDataItemForNextConnectedAnimation(object item);
}
