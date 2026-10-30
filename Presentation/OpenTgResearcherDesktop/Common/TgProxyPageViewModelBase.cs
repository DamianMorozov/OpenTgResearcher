namespace OpenTgResearcherDesktop.Common;

/// <summary> Общие операции страниц прокси с подтверждением до обращения к хранилищу </summary>
public abstract class TgProxyPageViewModelBase(ILoadStateService loadStateService, ITgSettingsService settingsService,
    INavigationService navigationService, ILogger<TgPageViewModelBase> logger, string name)
    : TgPageViewModelBase(loadStateService, settingsService, navigationService, logger, name)
{
    // Показываем подтверждение и ожидаем действие после закрытия диалога
    protected async Task ConfirmStorageActionAsync(Func<Task> action, string title, CancellationToken ct = default)
    {
        if (XamlRootVm is null) return;
        try
        {
            var dialog = new ContentDialog
            {
                XamlRoot = XamlRootVm,
                Title = title,
                PrimaryButtonText = TgResourceExtensions.GetYesButton(),
                CloseButtonText = TgResourceExtensions.GetCancelButton(),
                DefaultButton = ContentDialogButton.Close,
            };
            if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
            ct.ThrowIfCancellationRequested();
            await RunStorageActionAsync(action);
        }
        catch (Exception ex)
        {
            LogError(ex);
            Exception.Set(ex);
        }
    }

    // Показываем и журналируем ошибки до возврата управления базовой загрузке
    protected async Task RunStorageActionAsync(Func<Task> action) => await LoadStorageDataAsync(async () =>
    {
        try
        {
            Exception.Default();
            await action();
        }
        catch (Exception ex)
        {
            LogError(ex);
            Exception.Set(ex);
        }
    });
}
