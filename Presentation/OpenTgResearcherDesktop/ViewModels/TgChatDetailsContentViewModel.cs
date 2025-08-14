// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgChatDetailsContentViewModel : TgPageViewModelBase
{
    #region Public and private fields, properties, constructor

    [ObservableProperty]
    public partial IAppNotificationService AppNotificationService { get; private set; }

    [ObservableProperty]
    public partial Guid Uid { get; set; } = Guid.Empty!;
    [ObservableProperty]
    public partial TgEfSourceDto Dto { get; set; } = null!;
    [ObservableProperty]
    public partial TgChatDetailsDto ChatDetailsDto { get; set; } = new();
    [ObservableProperty]
    public partial ObservableCollection<TgEfMessageDto> MessageDtos { get; set; } = new();
    [ObservableProperty]
    public partial ObservableCollection<TgEfUserDto> UserDtos { get; set; } = new();
    [ObservableProperty]
    public partial Action ScrollRequested { get; set; } = () => { };
    [ObservableProperty]
    public partial bool IsImageViewerVisible { get; set; }
    [ObservableProperty]
    public partial TgEfContentStatisticsDto ContentStatisticsDto { get; set; } = new();

    public IRelayCommand CalcContentStatisticsCommand { get; }

    public TgChatDetailsContentViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgChatDetailsContentViewModel> logger,
        IAppNotificationService appNotificationService)
        : base(settingsService, navigationService, logger, nameof(TgChatDetailsContentViewModel))
    {
        AppNotificationService = appNotificationService;
        // Commands
        SetDisplaySensitiveCommand = new AsyncRelayCommand(SetDisplaySensitiveAsync);
        CalcContentStatisticsCommand = new AsyncRelayCommand(CalcContentStatisticsAsync);
        // Updates
        App.BusinessLogicManager.ConnectClient.SetupUpdateStateSource(UpdateStateSource);
    }

    #endregion

    #region Public and private methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadDataAsync(async () =>
        {
            Uid = e?.Parameter is Guid uid ? uid : Guid.Empty;
            await LoadDataStorageCoreAsync();
        });

    private async Task SetDisplaySensitiveAsync()
    {
        foreach (var userDto in UserDtos)
        {
            userDto.IsDisplaySensitiveData = IsDisplaySensitiveData;
        }

        foreach (var messageDto in MessageDtos)
        {
            messageDto.IsDisplaySensitiveData = IsDisplaySensitiveData;
        }

        await Task.CompletedTask;
    }

    private async Task LoadDataStorageCoreAsync()
    {
        try
        {
            if (!SettingsService.IsExistsAppStorage) return;

            Dto = await App.BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Uid == Uid);

            MessageDtos = [.. await App.BusinessLogicManager.StorageManager.MessageRepository.GetListDtosWithoutRelationsAsync(
                take: 1000, skip: 0, where: x => x.SourceId == Dto.Id, order: x => x.Id)];
            foreach (var messageDto in MessageDtos)
            {
                messageDto.Directory = Dto.Directory;
            }
        }
        finally
        {
            IsEmptyData = !MessageDtos.Any();
            ScrollRequested?.Invoke();

            await ReloadUiAsync();
            await SetDisplaySensitiveAsync();
        }
    }

    private async Task CalcContentStatisticsAsync() => await ContentDialogAsync(CalcContentStatisticsCoreAsync, TgResourceExtensions.AskCalcContentStatistics());
    
    private async Task CalcContentStatisticsCoreAsync()
    {
        ContentStatisticsDto.DefaultValues();

        if (Directory.Exists(Dto.Directory))
        {
            var files = Directory.GetFiles(Dto.Directory);
            if (files.Length != 0)
            {
                var imageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp" };
                var audioExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    { ".mp3", ".wav", ".flac", ".aac", ".ogg", ".m4a", ".wma", ".alac", ".aiff", ".ape", ".opus" };
                var videoExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv", ".webm" };

                ContentStatisticsDto.ImagesCount = files.Count(file => imageExtensions.Contains(Path.GetExtension(file)));
                ContentStatisticsDto.AudiosCount = files.Count(file => audioExtensions.Contains(Path.GetExtension(file)));
                ContentStatisticsDto.VideosCount = files.Count(file => videoExtensions.Contains(Path.GetExtension(file)));
            }
        }

        await Task.CompletedTask;
    }

    /// <summary> Click on user </summary>
    internal void OnUserClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        var tag = button.Tag.ToString();
        if (string.IsNullOrEmpty(tag)) return;

        Tuple<long, long> tuple = new(long.Parse(tag), Dto.Id);
        NavigationService.NavigateTo(typeof(TgUserDetailsViewModel).FullName!, tuple);
    }

    #endregion
}
