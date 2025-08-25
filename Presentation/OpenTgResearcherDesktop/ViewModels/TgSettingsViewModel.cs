// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public partial class TgSettingsViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial ObservableCollection<TgEnumTheme> AppThemes { get; set; } = [];
    [ObservableProperty]
    public partial ObservableCollection<TgEnumLanguage> AppLanguages { get; set; } = [];
    [ObservableProperty]
    public partial TgEnumTheme AppTheme { get; set; }
    [ObservableProperty]
    public partial TgEnumLanguage AppLanguage { get; set; }
    [ObservableProperty]
    public partial string AppStorage { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string AppSession { get; set; } = string.Empty;
    [ObservableProperty]
    public partial bool IsExistsAppStorage { get; set; }
    [ObservableProperty]
    public partial bool IsExistsAppSession { get; set; }
    [ObservableProperty]
    public partial string UserDirectory { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string ApplicationDirectory { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string SettingFile { get; set; } = string.Empty;

    public IRelayCommand SettingsLoadCommand { get; }
    public IRelayCommand SettingsDefaultCommand { get; }
    public IRelayCommand SettingsSaveCommand { get; }

    public TgSettingsViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgSettingsViewModel> logger)
        : base(settingsService, navigationService, logger, nameof(TgSettingsViewModel))
    {
        // Commands
        SettingsLoadCommand = new AsyncRelayCommand(SettingsLoadAsync);
        SettingsDefaultCommand = new AsyncRelayCommand(SettingsDefaultAsync);
        SettingsSaveCommand = new AsyncRelayCommand(SettingsSaveAsync);
        SetDisplaySensitiveCommand = new AsyncRelayCommand(SetDisplaySensitiveAsync);

        //
        PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(AppStorage))
                SettingsService.AppStorage = AppStorage;
            if (args.PropertyName == nameof(AppSession))
                SettingsService.AppSession = AppSession;
            if (args.PropertyName == nameof(AppTheme))
                SettingsService.AppTheme = AppTheme;
            if (args.PropertyName == nameof(AppLanguage))
                SettingsService.AppLanguage = AppLanguage;
        };
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e)
    {
        SettingsLoadCore();
        await Task.CompletedTask;
    }

    private async Task SetDisplaySensitiveAsync()
    {
        _ = IsDisplaySensitiveData;

        await Task.CompletedTask;
    }

    private async Task SettingsLoadAsync() => await ContentDialogAsync(SettingsLoadCore, TgResourceExtensions.AskSettingsLoad());

    private void SettingsLoadCore()
    {
        SettingsService.Load();
        LoadSettingsFromService();
    }

    private async Task SettingsDefaultAsync() => await ContentDialogAsync(SettingsDefaultCoreAsync, TgResourceExtensions.AskSettingsDefault());

    private async Task SettingsDefaultCoreAsync()
    {
        SettingsService.Default();
        LoadSettingsFromService();
        await Task.CompletedTask;
    }

    private async Task SettingsSaveAsync()
    {
        await ContentDialogAsync(SettingsSaveCore, TgResourceExtensions.AskSettingsSave());
    }

    private void SettingsSaveCore()
    {
        SettingsService.Save();
        LoadSettingsFromService();
    }

    private void LoadSettingsFromService()
    {
        AppStorage = SettingsService.AppStorage;
        IsExistsAppStorage = SettingsService.IsExistsAppStorage;

        AppSession = SettingsService.AppSession;
        // Fix windows path
        if (AppSession.Equals(TgFileUtils.FileTgSession))
        {
            AppSession = Path.Combine(SettingsService.ApplicationDirectory, TgFileUtils.FileTgSession);
        }
        IsExistsAppSession = SettingsService.IsExistsAppSession;

        if (!AppThemes.Any())
        {
            AppThemes = SettingsService.AppThemes;
        }

        if (!AppLanguages.Any())
        {
            AppLanguages = SettingsService.AppLanguages;
        }

        AppTheme = SettingsService.AppTheme;
        AppLanguage = SettingsService.AppLanguage;

        UserDirectory = SettingsService.UserDirectory;
        
        ApplicationDirectory = SettingsService.ApplicationDirectory;
        
        SettingFile = SettingsService.SettingFile;
    }

    #endregion
}
