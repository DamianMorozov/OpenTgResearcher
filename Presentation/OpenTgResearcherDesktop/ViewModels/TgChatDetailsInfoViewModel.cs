// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgChatDetailsInfoViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial Guid Uid { get; set; } = Guid.Empty!;
    [ObservableProperty]
    public partial TgChatDetailsDto ChatDetailsDto { get; set; } = new();

    public IRelayCommand UpdateChatSettingsCommand { get; }

    public TgChatDetailsInfoViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgChatDetailsInfoViewModel> logger)
        : base(settingsService, navigationService, logger, nameof(TgChatDetailsInfoViewModel))
    {
        // Commands
        UpdateChatSettingsCommand = new AsyncRelayCommand(UpdateChatSettingsAsync);
        SetDisplaySensitiveCommand = new AsyncRelayCommand(SetDisplaySensitiveAsync);
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadDataAsync(async () =>
        {
            Uid = e?.Parameter is Guid uid ? uid : Guid.Empty;
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

    private async Task UpdateChatSettingsAsync() => await ContentDialogAsync(UpdateChatDetailsCoreAsync, TgResourceExtensions.AskUpdateChatDetails(), useLoadData: true);

    private async Task UpdateChatDetailsCoreAsync() => await LoadDataAsync(async () =>
    {
        if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;

        ChatDetailsDto = await App.BusinessLogicManager.ConnectClient.GetChatDetailsByClientAsync(Uid);
    });

    #endregion
}