namespace OpenTgResearcherDesktop;

// Подменяем запуск внешнего обновлятора, оставляя штатное завершение приложения исходным
internal sealed class VelopackAsset;
internal sealed class UpdateManager
{
    public void WaitExitThenApplyUpdates(VelopackAsset target, bool silent, bool restart) { }
}
