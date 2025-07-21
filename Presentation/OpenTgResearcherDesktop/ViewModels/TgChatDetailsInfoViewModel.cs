// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgChatDetailsInfoViewModel : TgPageViewModelBase
{
    #region Public and private fields, properties, constructor

    [ObservableProperty]
    public partial long Id { get; set; }
    [ObservableProperty]
    public partial TgChatDetailsDto ChatDetailsDto { get; set; } = new();

    public IRelayCommand UpdateChatSettingsCommand { get; }

    public TgChatDetailsInfoViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgChatDetailsInfoViewModel> logger)
        : base(settingsService, navigationService, logger, nameof(TgChatDetailsInfoViewModel))
    {
        // Commands
        UpdateChatSettingsCommand = new AsyncRelayCommand(UpdateChatSettingsAsync);
        SetDisplaySensitiveCommand = new AsyncRelayCommand(SetDisplaySensitiveAsync);
        // Updates
        App.BusinessLogicManager.ConnectClient.SetupUpdateStateSource(UpdateStateSource);
    }

    #endregion

    #region Public and private methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadDataAsync(async () =>
        {
            if (e?.Parameter is long id)
            {
                Id = id;
            }
            await LoadDataStorageCoreAsync();
        });

    private async Task SetDisplaySensitiveAsync()
    {
        ChatDetailsDto.IsDisplaySensitiveData = IsDisplaySensitiveData;
        await Task.CompletedTask;
    }

    private async Task LoadDataStorageCoreAsync()
    {
        try
        {
            if (!SettingsService.IsExistsAppStorage) return;
        }
        finally
        {
            await ReloadUiAsync();
        }
    }

    private async Task UpdateChatSettingsAsync() => await ContentDialogAsync(UpdateChatDetailsCoreAsync, TgResourceExtensions.AskUpdateChatDetails());

    private async Task UpdateChatDetailsCoreAsync() => await LoadDataAsync(async () =>
    {
        if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;

        ChatDetailsDto = await App.BusinessLogicManager.ConnectClient.GetChatDetailsByClientAsync(Id);
    });

    #endregion
}