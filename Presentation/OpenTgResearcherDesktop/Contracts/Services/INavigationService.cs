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
