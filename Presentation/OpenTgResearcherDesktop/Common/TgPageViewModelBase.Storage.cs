namespace OpenTgResearcherDesktop.Common;

public abstract partial class TgPageViewModelBase
{
    private int _pageStorageLoads;

    [ObservableProperty]
    public partial bool IsPageStorageProcessing { get; private set; }

    // Учитываем только загрузки этой страницы, сохраняя занятость до последнего обработчика
    private void BeginPageStorageLoading() => TgDesktopUtils.InvokeOnUIThread(() =>
    {
        _pageStorageLoads++;
        IsPageStorageProcessing = true;
    });

    // Завершение вложенного обработчика не освобождает ещё загружающуюся страницу
    private void EndPageStorageLoading() => TgDesktopUtils.InvokeOnUIThread(() =>
    {
        _pageStorageLoads--;
        IsPageStorageProcessing = _pageStorageLoads > 0;
    });

    /// <summary> Загружает данные страницы асинхронно </summary>
    public async Task LoadStorageDataAsync(Func<Task> task)
    {
        var uid = Guid.NewGuid();
        try
        {
            BeginPageStorageLoading();
            await LoadStateService.PrepareStorageTokenAsync(uid);

            await TgDesktopUtils.InvokeOnUIThreadAsync(task, LoadStateService.StorageToken);

            // При выполнении в пуле потоков изменения интерфейса должны явно использовать InvokeOnUIThreadAsync
            //await Task.Run(() => task(), StorageToken);

            //await task().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogError(ex, $"Не удалось загрузить данные страницы: {nameof(LoadStorageDataAsync)}");
        }
        finally
        {
            EndPageStorageLoading();
            LoadStateService.StopSoftStorageProcessing(uid);
            RefreshLicenseInfo();
        }
    }

    /// <summary> Выполняет действие с данными страницы </summary>
    public async Task LoadStorageDataAsync(Action action)
    {
        var uid = Guid.NewGuid();
        try
        {
            BeginPageStorageLoading();
            await LoadStateService.PrepareStorageTokenAsync(uid);

            TgDesktopUtils.InvokeOnUIThread(action);
        }
        catch (Exception ex)
        {
            LogError(ex, $"Не удалось загрузить данные страницы: {nameof(LoadStorageDataAsync)}");
        }
        finally
        {
            EndPageStorageLoading();
            LoadStateService.StopSoftStorageProcessing(uid);
            RefreshLicenseInfo();
        }
    }
}
