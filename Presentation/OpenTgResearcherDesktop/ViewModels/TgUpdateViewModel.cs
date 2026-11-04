namespace OpenTgResearcherDesktop.ViewModels;

/// <summary> Проверка и скачивание обновлений Desktop </summary>
public partial class TgUpdateViewModel : TgPageViewModelBase
{
    [ObservableProperty]
    public partial string UpdateLog { get; set; } = string.Empty;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UpdateReleaseCommand), nameof(UpdatePreviewCommand), nameof(CancelUpdateCommand))]
    public partial bool IsUpdating { get; private set; }
    [ObservableProperty]
    public partial bool IsDownloading { get; private set; }
    [ObservableProperty]
    public partial int DownloadProgress { get; private set; }

    public IAsyncRelayCommand UpdateReleaseCommand { get; }
    public IAsyncRelayCommand UpdatePreviewCommand { get; }
    public IRelayCommand CancelUpdateCommand { get; }

    /// <summary> Создаёт команды обновления с общим состоянием занятости </summary>
    public TgUpdateViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService,
        ILogger<TgUpdateViewModel> logger) : base(loadStateService, settingsService, navigationService, logger, nameof(TgUpdateViewModel))
    {
        UpdateReleaseCommand = new AsyncRelayCommand(UpdateReleaseAsync, () => !IsUpdating);
        UpdatePreviewCommand = new AsyncRelayCommand(UpdatePreviewAsync, () => !IsUpdating);
        CancelUpdateCommand = new RelayCommand(CancelUpdate, () => IsUpdating);
    }

    /// <summary> Открывает страницу без обращения к Telegram и хранилищу </summary>
    public override Task OnNavigatedToAsync(NavigationEventArgs? e) => Task.CompletedTask;

    /// <summary> Сохраняет журнал и текущую загрузку при обновлении страницы </summary>
    public override Task ReloadUiAsync() => Task.CompletedTask;

    // Проверяем стабильную версию
    private Task UpdateReleaseAsync(CancellationToken ct = default) => ConfirmUpdateAsync(isPreview: false, ct);

    // Проверяем предварительную версию
    private Task UpdatePreviewAsync(CancellationToken ct = default) => ConfirmUpdateAsync(isPreview: true, ct);

    // Ожидаем закрытия подтверждения и всей загрузки в рамках одной команды
    private async Task ConfirmUpdateAsync(bool isPreview, CancellationToken ct = default)
    {
        if (XamlRootVm is null || IsUpdating) return;
        IsUpdating = true;
        try
        {
            var dialog = new ContentDialog
            {
                XamlRoot = XamlRootVm,
                Title = isPreview ? TgResourceExtensions.AskUpdatePreviewApp() : TgResourceExtensions.AskUpdateReleaseApp(),
                PrimaryButtonText = TgResourceExtensions.GetYesButton(),
                CloseButtonText = TgResourceExtensions.GetCancelButton(),
                DefaultButton = ContentDialogButton.Close,
            };
            if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
            ct.ThrowIfCancellationRequested();
            UpdateLog = string.Empty;
            DownloadProgress = 0;
            await VelopackUpdateAsync(isPreview, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            AppendUpdateLog("UpdateCancelled");
        }
        catch (Exception ex)
        {
            AppendUpdateLog("UpdateFailed", ex.Message);
            LogError(ex);
        }
        finally
        {
            IsDownloading = false;
            IsUpdating = false;
        }
    }

    // Проверяем установку Velopack до сети и скачиваем пакет с прогрессом и отменой
    private async Task VelopackUpdateAsync(bool isPreview, CancellationToken ct = default)
    {
        var manager = new UpdateManager(new GithubSource(TgConstants.LinkGitHub, string.Empty, prerelease: isPreview));
        if (!manager.IsInstalled)
        {
            AppendUpdateLog("UpdateNotInstalled");
            return;
        }
        AppendUpdateLog("UpdateStarted", manager.CurrentVersion);
        AppendUpdateLog(isPreview ? "UpdateCheckingPreview" : "UpdateCheckingRelease");
        // Velopack не принимает токен для проверки; отмена останавливает дальнейшую загрузку после её завершения
        var update = await manager.CheckForUpdatesAsync();
        ct.ThrowIfCancellationRequested();
        if (update is null)
        {
            AppendUpdateLog("UpdateAlreadyLatest");
            return;
        }
        AppendUpdateLog("UpdateDownloading", update.TargetFullRelease.Version);
        IsDownloading = true;
        await manager.DownloadUpdatesAsync(update, progress => SetDownloadProgress(progress, ct), cancelToken: ct);
        ct.ThrowIfCancellationRequested();
        DownloadProgress = 100;
        // Ранний вызов Velopack в Main применит подготовленный пакет при следующем запуске
        AppendUpdateLog("UpdateDownloaded", update.TargetFullRelease.Version);
    }

    // Переносим уведомления загрузчика в поток интерфейса
    private void SetDownloadProgress(int progress, CancellationToken ct = default) => TgDesktopUtils.InvokeOnUIThread(() =>
    {
        if (!ct.IsCancellationRequested && !App.IsClosing) DownloadProgress = Math.Clamp(progress, 0, 100);
    });

    // Добавляем сообщение на выбранном языке, сохраняя техническую причину ошибки
    private void AppendUpdateLog(string resource, params object?[] args) =>
        UpdateLog += string.Format(CultureInfo.CurrentCulture, resource.GetLocalizedResource(), args) + Environment.NewLine;

    // Передаём отмену активной команде обновления
    private void CancelUpdate()
    {
        UpdateReleaseCommand.Cancel();
        UpdatePreviewCommand.Cancel();
    }
}
