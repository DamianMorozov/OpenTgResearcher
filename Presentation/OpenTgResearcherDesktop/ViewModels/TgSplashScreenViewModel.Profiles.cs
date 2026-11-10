namespace OpenTgResearcherDesktop.ViewModels;

public partial class TgSplashScreenViewModel
{
    private TgAccountProfiles _splashProfiles = null!;
    public ObservableCollection<TgAccountProfile> Profiles => _splashProfiles.Profiles;
    public bool CanChooseProfile => IsLoadComplete && !IsProfileChanging;
    [ObservableProperty]
    public partial TgAccountProfile SelectedProfile { get; set; } = null!;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanChooseProfile))]
    [NotifyCanExecuteChangedFor(nameof(ContinueCommand))]
    public partial bool IsProfileChanging { get; private set; }
    [ObservableProperty]
    public partial string ProfileMessage { get; private set; } = string.Empty;

    // Прежние пути становятся профилем по умолчанию, если каталог ещё не создан
    private void InitializeSplashProfiles()
    {
        _splashProfiles = new(SettingsService, "ProfileDefaultName".GetLocalizedResource());
        SelectedProfile = Profiles.First(x => x.Id == _splashProfiles.ActiveProfileId);
    }

    // Продолжаем с загруженным аккаунтом либо штатно перезапускаем приложение с другим профилем
    private async Task ContinueAsync(CancellationToken ct = default)
    {
        if (!CanChooseProfile || SelectedProfile is null) return;
        IsProfileChanging = true;
        ProfileMessage = string.Empty;
        try
        {
            if (SelectedProfile.Id != _splashProfiles.ActiveProfileId)
            {
                if (XamlRootVm is null) return;
                var dialog = new ContentDialog
                {
                    XamlRoot = XamlRootVm,
                    Title = "ProfileApply".GetLocalizedResource(),
                    Content = "ProfileRestartHint".GetLocalizedResource(),
                    PrimaryButtonText = TgResourceExtensions.GetYesButton(),
                    CloseButtonText = TgResourceExtensions.GetCancelButton(),
                    DefaultButton = ContentDialogButton.Close,
                };
                if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
                ct.ThrowIfCancellationRequested();
                _splashProfiles.Validate(SelectedProfile.Id);
                var previous = SettingsService.ReadSetting<TgAccountProfiles.State>(TgAccountProfiles.SettingsKey)!;
                await App.RestartForProfileAsync(() => _splashProfiles.Save(SelectedProfile.Id),
                    () => SettingsService.SaveProfileState(previous));
                return;
            }
            BackToMainWindow.Invoke();
            await LoadingSettingsAsync();
        }
        catch (Exception ex)
        {
            ProfileMessage = ex.Message;
            LogError(ex);
        }
        finally { IsProfileChanging = false; }
    }
}
