namespace OpenTgResearcherDesktop.ViewModels;

public sealed partial class TgChatsViewModel
{
    private CancellationTokenSource? _publicProfileCancellation;
    private CancellationTokenSource? _publicPhotosCancellation;
    private CancellationTokenSource? _publicMediaCancellation;
    internal Task PublicProfileTask { get; private set; } = Task.CompletedTask;
    internal Task PublicPhotosTask { get; private set; } = Task.CompletedTask;
    [ObservableProperty]
    public partial bool IsPublicProfileOpen { get; set; }

    /// <summary> Раскрывает сведения выбранного результата, включая повторное нажатие по той же строке </summary>
    public void OpenPublicProfile(OpenTgResearcherDesktop.Models.TgPublicChatSearchRow row)
    {
        var isSameRow = ReferenceEquals(SelectedPublicChat, row);
        SelectedPublicChat = row;
        IsPublicProfileOpen = true;
        if (isSameRow && !row.IsProfileLoaded) PublicProfileTask = LoadPublicProfileAsync(row);
    }

    // Закрываем только сведения выбранного источника, сохраняя выдачу и фотографии
    [RelayCommand]
    private void ClosePublicProfile()
    {
        IsPublicProfileOpen = false;
        _publicProfileCancellation?.Cancel();
        _publicMediaCancellation?.Cancel();
    }

    // Повторяем запрос подробностей после сетевого сбоя
    [RelayCommand]
    private async Task ReloadPublicProfileAsync()
    {
        if (SelectedPublicChat is not { } row || row.IsProfileBusy) return;
        row.IsProfileLoaded = false;
        await LoadPublicProfileAsync(row);
    }

    // Запрашиваем описание только выбранного источника, отменяя предыдущий выбор
    private async Task LoadPublicProfileAsync(OpenTgResearcherDesktop.Models.TgPublicChatSearchRow? row)
    {
        _publicProfileCancellation?.Cancel();
        _publicMediaCancellation?.Cancel();
        if (row is null || row.IsProfileLoaded || !IsTelegramSearch) return;
        using var cancellation = new CancellationTokenSource();
        _publicProfileCancellation = cancellation;
        var ct = cancellation.Token;
        row.IsProfileBusy = true;
        row.ProfileError = string.Empty;
        try
        {
            await LoadOnlineDataAsync(async () =>
            {
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, LoadStateService.OnlineToken);
                var profile = await App.BusinessLogicManager.ConnectClient.GetPublicChatProfileAsync(row.Id, linked.Token);
                linked.Token.ThrowIfCancellationRequested();
                row.About = profile.About;
                row.ParticipantsCount = profile.ParticipantsCount ?? row.ParticipantsCount;
                row.UserNames = [.. profile.UserNames.Select(x => "https://t.me/" + x)];
                row.DiscussionLink = profile.DiscussionLink is { } link ? new Uri(link) : null;
                row.IsProfileLoaded = true;
            });
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested || LoadStateService.OnlineToken.IsCancellationRequested) { }
        catch (Exception ex) { row.ProfileError = ex.Message; LogError(ex); }
        finally
        {
            if (!ReferenceEquals(SelectedPublicChat, row) || ReferenceEquals(_publicProfileCancellation, cancellation)) row.IsProfileBusy = false;
            if (ReferenceEquals(_publicProfileCancellation, cancellation)) _publicProfileCancellation = null;
        }
    }

    // Подгружаем фотографии после выдачи списка, не удерживая кнопку поиска
    private async Task LoadPublicPhotosAsync(OpenTgResearcherDesktop.Models.TgPublicChatSearchRow[] rows)
    {
        _publicPhotosCancellation?.Cancel();
        using var cancellation = new CancellationTokenSource();
        _publicPhotosCancellation = cancellation;
        var ct = cancellation.Token;
        try
        {
            await LoadOnlineDataAsync(async () =>
            {
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, LoadStateService.OnlineToken);
                foreach (var row in rows)
                {
                    linked.Token.ThrowIfCancellationRequested();
                    try
                    {
                        var photo = await App.BusinessLogicManager.ConnectClient.GetPublicChatPhotoAsync(row.Id, linked.Token);
                        linked.Token.ThrowIfCancellationRequested();
                        row.Photo = photo.Length > 0 ? photo : null;
                    }
                    catch (OperationCanceledException) when (linked.IsCancellationRequested) { throw; }
                    catch (Exception ex) { row.PhotoError = ex.Message; LogError(ex); }
                }
            });
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested || LoadStateService.OnlineToken.IsCancellationRequested) { }
        catch (Exception ex) { LogError(ex); }
        finally { if (ReferenceEquals(_publicPhotosCancellation, cancellation)) _publicPhotosCancellation = null; }
    }

    // Получаем счётчики при раскрытии блока медиа, сохраняя успешный ответ для повторного открытия
    [RelayCommand]
    private async Task LoadPublicMediaAsync()
    {
        if (SelectedPublicChat is not { } row || row.IsMediaLoaded || row.IsMediaBusy) return;
        _publicMediaCancellation?.Cancel();
        using var cancellation = new CancellationTokenSource();
        _publicMediaCancellation = cancellation;
        var ct = cancellation.Token;
        row.MediaError = string.Empty;
        row.IsMediaBusy = true;
        try
        {
            await LoadOnlineDataAsync(async () =>
            {
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, LoadStateService.OnlineToken);
                var counts = await App.BusinessLogicManager.ConnectClient.GetPublicChatMediaCountsAsync(row.Id, linked.Token);
                linked.Token.ThrowIfCancellationRequested();
                row.Media = [.. counts.Select(x => new OpenTgResearcherDesktop.Models.TgPublicChatMediaRow(x))];
                row.IsMediaLoaded = true;
            });
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested || LoadStateService.OnlineToken.IsCancellationRequested) { }
        catch (Exception ex) { row.MediaError = ex.Message; LogError(ex); }
        finally
        {
            row.IsMediaBusy = false;
            if (ReferenceEquals(_publicMediaCancellation, cancellation)) _publicMediaCancellation = null;
        }
    }

    // Явно перечитываем счётчики по запросу пользователя
    [RelayCommand]
    private async Task ReloadPublicMediaAsync()
    {
        if (SelectedPublicChat is not { } row || row.IsMediaBusy) return;
        row.IsMediaLoaded = false;
        await LoadPublicMediaAsync();
    }
}
