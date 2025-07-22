// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Contracts;

public interface ITgPageViewModel
{
	public string Name { get; }

	public Task OnNavigatedToAsync(NavigationEventArgs? e);
	public void OnLoaded(object parameter);
    public Task ReloadUiAsync();
}