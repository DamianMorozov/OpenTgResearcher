namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgChatSettingsViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial Guid Uid { get; set; } = Guid.Empty!;
    [ObservableProperty]
    public partial TgEfSourceDto Dto { get; set; } = default!;
    [ObservableProperty]
    public partial TgEfSourceDto DiscussionDto { get; set; } = default!;
    [ObservableProperty]
    public partial bool IsDiscussionDtoExists { get; set; }

    public IAsyncRelayCommand OpenDiscussionChatCommand { get; }

    public TgChatSettingsViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILoadStateService loadStateService, 
        ILogger<TgChatSettingsViewModel> logger)
        : base(settingsService, navigationService, loadStateService, logger, nameof(TgChatSettingsViewModel))
    {
        // Commands
        OpenDiscussionChatCommand = new AsyncRelayCommand(OpenDiscussionChatAsync);
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(() =>
    {
        Uid = e?.Parameter is Guid uid ? uid : Guid.Empty;
    });

    private async Task OpenDiscussionChatAsync()
    {
        // If discussion chat exists
        if (IsDiscussionDtoExists)
        {
            NavigationService.NavigateTo(typeof(TgChatViewModel).FullName!, DiscussionDto.Uid);
        }
        // If discussion chat not exists
        else
        {
            await ContentDialogAsync(TgResourceExtensions.Navigation(), TgResourceExtensions.DiscussionChatNotFound());
        }
    }

    #endregion
}
