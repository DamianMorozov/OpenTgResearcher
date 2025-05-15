// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Activation;

public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
	private readonly INavigationService _navigationService;

	public DefaultActivationHandler(INavigationService navigationService)
	{
		_navigationService = navigationService;
	}

	protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
	{
		// None of the ActivationHandlers has handled the activation.
		return _navigationService.Frame?.Content == null;
	}

	protected override async Task HandleInternalAsync(LaunchActivatedEventArgs args)
	{
		_navigationService.NavigateTo(typeof(TgMainViewModel).FullName!, args.Arguments);

		await Task.CompletedTask;
	}
}
