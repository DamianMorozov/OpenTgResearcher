namespace OpenTgResearcherDesktop;

public partial class App
{
    /// <summary> Применяет профиль после остановки операций и освобождения сессии, затем перезапускает Desktop </summary>
    internal static Task RestartForProfileAsync(Action saveProfile, Action restoreProfile) => ((App)Current).CloseForUpdateAsync(() =>
    {
        try
        {
            saveProfile();
            var result = AppInstance.Restart(string.Empty);
            if (result != Windows.ApplicationModel.Core.AppRestartFailureReason.RestartPending)
                throw new InvalidOperationException($"Не удалось перезапустить приложение для смены профиля: {result}");
        }
        catch
        {
            restoreProfile();
            throw;
        }
    });
}
