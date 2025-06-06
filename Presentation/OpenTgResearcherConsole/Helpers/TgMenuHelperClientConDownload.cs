// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
    #region Public and private methods

    private TgEnumMenuClientConDownload SetMenuClientConDownload()
    {
        var selectionPrompt = new SelectionPrompt<string>()
            .Title($"  {TgLocale.MenuSwitchNumber}")
            .PageSize(Console.WindowHeight - 17)
            .MoreChoicesText(TgLocale.MoveUpDown);
        selectionPrompt.AddChoices(
            TgLocale.MenuReturn,
            TgLocale.MenuDownloadSetSource,
            TgLocale.MenuDownloadSetFolder,
            TgLocale.MenuDownloadSetSourceFirstIdAuto,
            TgLocale.MenuDownloadSetSourceFirstIdManual,
            TgLocale.MenuDownloadSetIsRewriteFiles,
            TgLocale.MenuDownloadSetIsSaveMessages,
            TgLocale.MenuDownloadSetIsRewriteMessages,
            TgLocale.MenuDownloadSetIsAddMessageId,
            TgLocale.MenuDownloadSetIsAutoUpdate,
            TgLocale.MenuDownloadSetIsCreatingSubdirectories,
            TgLocale.MenuDownloadSetIsFileNamingByMessage
        );
        // Check paid license
        if (BusinessLogicManager.LicenseService.CurrentLicense.CheckPaidLicense())
            selectionPrompt.AddChoice(TgLocale.MenuDownloadSetCountThreadsByPaidLicense);
        else
            selectionPrompt.AddChoice(TgLocale.MenuDownloadSetCountThreadsByFreeLicense);
        selectionPrompt.AddChoices(
            TgLocale.MenuSaveSettings,
            TgLocale.MenuManualDownload);

        var prompt = AnsiConsole.Prompt(selectionPrompt);
        if (prompt.Equals(TgLocale.MenuDownloadSetSource))
            return TgEnumMenuClientConDownload.DownloadSetSource;
        if (prompt.Equals(TgLocale.MenuDownloadSetFolder))
            return TgEnumMenuClientConDownload.DownloadSetDestDirectory;
        if (prompt.Equals(TgLocale.MenuDownloadSetSourceFirstIdAuto))
            return TgEnumMenuClientConDownload.DownloadSetSourceFirstIdAuto;
        if (prompt.Equals(TgLocale.MenuDownloadSetSourceFirstIdManual))
            return TgEnumMenuClientConDownload.DownloadSetSourceFirstIdManual;
        if (prompt.Equals(TgLocale.MenuDownloadSetIsSaveMessages))
            return TgEnumMenuClientConDownload.DownloadSetIsSaveMessages;
        if (prompt.Equals(TgLocale.MenuDownloadSetIsRewriteFiles))
            return TgEnumMenuClientConDownload.DownloadSetIsRewriteFiles;
        if (prompt.Equals(TgLocale.MenuDownloadSetIsRewriteMessages))
            return TgEnumMenuClientConDownload.DownloadSetIsRewriteMessages;
        if (prompt.Equals(TgLocale.MenuDownloadSetIsAddMessageId))
            return TgEnumMenuClientConDownload.DownloadSetIsAddMessageId;
        if (prompt.Equals(TgLocale.MenuDownloadSetIsAutoUpdate))
            return TgEnumMenuClientConDownload.DownloadSetIsAutoUpdate;
        if (prompt.Equals(TgLocale.MenuDownloadSetIsCreatingSubdirectories))
            return TgEnumMenuClientConDownload.DownloadSetIsCreatingSubdirectories;
        if (prompt.Equals(TgLocale.MenuDownloadSetIsFileNamingByMessage))
            return TgEnumMenuClientConDownload.DownloadSetIsFileNamingByMessage;
        if (prompt.Equals(TgLocale.MenuDownloadSetCountThreadsByFreeLicense))
            return TgEnumMenuClientConDownload.DownloadSetCountThreads;
        if (prompt.Equals(TgLocale.MenuDownloadSetCountThreadsByPaidLicense))
            return TgEnumMenuClientConDownload.DownloadSetCountThreads;
        if (prompt.Equals(TgLocale.MenuSaveSettings))
            return TgEnumMenuClientConDownload.DownloadSettingsSave;
        if (prompt.Equals(TgLocale.MenuManualDownload))
            return TgEnumMenuClientConDownload.DownloadManual;

        return TgEnumMenuClientConDownload.Return;
    }

    public async Task SetupClientConDownloadAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        TgEnumMenuClientConDownload menu;
        do
        {
            await ShowTableClientConDownloadAsync(tgDownloadSettings);

            menu = SetMenuClientConDownload();
            switch (menu)
            {
                case TgEnumMenuClientConDownload.DownloadSetSource:
                    tgDownloadSettings = await DownloadSetSourceAsync();
                    break;
                case TgEnumMenuClientConDownload.DownloadSetSourceFirstIdAuto:
                    await RunTaskStatusAsync(tgDownloadSettings, DownloadSetSourceFirstIdAutoAsync, isSkipCheckTgSettings: true,
                        isScanCount: false, isWaitComplete: true);
                    break;
                case TgEnumMenuClientConDownload.DownloadSetSourceFirstIdManual:
                    await DownloadSetSourceFirstIdManualAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuClientConDownload.DownloadSetDestDirectory:
                    SetupDownloadDestDirectory(tgDownloadSettings);
                    if (!tgDownloadSettings.SourceVm.Dto.IsAutoUpdate)
                        DownloadSetDestDirectory(tgDownloadSettings);
                    break;
                case TgEnumMenuClientConDownload.DownloadSetIsSaveMessages:
                    DownloadSetIsSaveMessages(tgDownloadSettings);
                    break;
                case TgEnumMenuClientConDownload.DownloadSetIsRewriteFiles:
                    DownloadSetIsRewriteFiles(tgDownloadSettings);
                    break;
                case TgEnumMenuClientConDownload.DownloadSetIsRewriteMessages:
                    DownloadSetIsRewriteMessages(tgDownloadSettings);
                    break;
                case TgEnumMenuClientConDownload.DownloadSetIsAddMessageId:
                    DownloadSetIsAddMessageId(tgDownloadSettings);
                    break;
                case TgEnumMenuClientConDownload.DownloadSetIsAutoUpdate:
                    DownloadSetDestDirectory(tgDownloadSettings);
                    break;
                case TgEnumMenuClientConDownload.DownloadSetIsCreatingSubdirectories:
                    DownloadSetIsCreatingSubdirectories(tgDownloadSettings);
                    break;
                case TgEnumMenuClientConDownload.DownloadSetIsFileNamingByMessage:
                    DownloadSetIsFileNamingByMessage(tgDownloadSettings);
                    break;
                case TgEnumMenuClientConDownload.DownloadSetCountThreads:
                    await DownloadSetCountThreadsAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuClientConDownload.DownloadSettingsSave:
                    await RunTaskStatusAsync(tgDownloadSettings, DownloadSettingsSaveAsync, isSkipCheckTgSettings: true, isScanCount: false, isWaitComplete: false);
                    break;
                case TgEnumMenuClientConDownload.DownloadManual:
                    await RunTaskProgressAsync(tgDownloadSettings, DownloadManualAsync, isSkipCheckTgSettings: false, isScanCount: false);
                    break;
                case TgEnumMenuClientConDownload.Return:
                    break;
            }
        } while (menu is not TgEnumMenuClientConDownload.Return);
    }

    private async Task<TgDownloadSettingsViewModel> SetupDownloadSourceByIdAsync(long id)
    {
        var tgDownloadSettings = SetupDownloadSourceByIdCore(id);
        await LoadTgClientSettingsByIdAsync(tgDownloadSettings);
        await BusinessLogicManager.ConnectClient.CreateChatAsync(tgDownloadSettings, isSilent: true);
        return tgDownloadSettings;
    }

    private async Task<TgDownloadSettingsViewModel> DownloadSetSourceAsync()
    {
        var tgDownloadSettings = SetupDownloadSourceCore();
        if (string.IsNullOrEmpty(tgDownloadSettings.SourceVm.Dto.UserName))
            await LoadTgClientSettingsByIdAsync(tgDownloadSettings);
        else
            await LoadTgClientSettingsByNameAsync(tgDownloadSettings);
        await BusinessLogicManager.ConnectClient.CreateChatAsync(tgDownloadSettings, isSilent: true);
        return tgDownloadSettings;
    }

    private TgDownloadSettingsViewModel SetupDownloadSourceByIdCore(long id)
    {
        TgDownloadSettingsViewModel tgDownloadSettings = new();
        tgDownloadSettings.SourceVm.Dto.Id = id;
        return tgDownloadSettings;
    }

    private TgDownloadSettingsViewModel SetupDownloadSourceCore()
    {
        TgDownloadSettingsViewModel tgDownloadSettings = new();
        var input = AnsiConsole.Ask<string>(TgLog.GetLineStampInfo($"{TgLocale.MenuDownloadSetSource}:"));
        if (!string.IsNullOrEmpty(input))
        {
            if (long.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var sourceId))
                return SetupDownloadSourceByIdCore(sourceId);
            input = TgDataUtils.ClearTgPeer(input);
            tgDownloadSettings.SourceVm.Dto.UserName = input;
            if (!string.IsNullOrEmpty(tgDownloadSettings.SourceVm.Dto.UserName))
                return tgDownloadSettings;
        }
        return tgDownloadSettings;
    }

    private async Task DownloadSetSourceFirstIdAutoAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        await LoadTgClientSettingsByIdAsync(tgDownloadSettings);
        await BusinessLogicManager.ConnectClient.CreateChatAsync(tgDownloadSettings, isSilent: true);
        await BusinessLogicManager.ConnectClient.SetChannelMessageIdFirstAsync(tgDownloadSettings);
    }

    private async Task DownloadSetSourceFirstIdManualAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        await LoadTgClientSettingsByIdAsync(tgDownloadSettings);
        do
        {
            tgDownloadSettings.SourceVm.Dto.FirstId = AnsiConsole.Ask<int>(TgLog.GetLineStampInfo($"{TgLocale.TypeTgSourceFirstId}:"));
        } while (!tgDownloadSettings.SourceVm.Dto.IsReadySourceFirstId);
    }

    private void SetupDownloadDestDirectory(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        do
        {
            tgDownloadSettings.SourceVm.Dto.Directory = AnsiConsole.Ask<string>(TgLog.GetLineStampInfo($"{TgLocale.DirectoryDestType}:"));
            if (!Directory.Exists(tgDownloadSettings.SourceVm.Dto.Directory))
            {
                TgLog.MarkupInfo(TgLocale.DirectoryIsNotExists(tgDownloadSettings.SourceVm.Dto.Directory));
                if (AskQuestionTrueFalseReturnPositive(TgLocale.DirectoryCreate, true))
                {
                    try
                    {
                        Directory.CreateDirectory(tgDownloadSettings.SourceVm.Dto.Directory);
                    }
                    catch (Exception ex)
                    {
                        CatchException(ex, TgLocale.DirectoryCreateIsException(ex));
                    }
                }
            }
        } while (!Directory.Exists(tgDownloadSettings.SourceVm.Dto.Directory));
    }

    private void DownloadSetIsSaveMessages(TgDownloadSettingsViewModel tgDownloadSettings) =>
        tgDownloadSettings.IsSaveMessages = AskQuestionTrueFalseReturnPositive(TgLocale.MenuDownloadSetIsSaveMessages, true);

    private void DownloadSetIsRewriteFiles(TgDownloadSettingsViewModel tgDownloadSettings) =>
        tgDownloadSettings.IsRewriteFiles = AskQuestionTrueFalseReturnPositive(TgLocale.MenuDownloadSetIsRewriteFiles, true);

    private void DownloadSetIsRewriteMessages(TgDownloadSettingsViewModel tgDownloadSettings) =>
        tgDownloadSettings.IsRewriteMessages = AskQuestionTrueFalseReturnPositive(TgLocale.MenuDownloadSetIsRewriteMessages, true);

    private void DownloadSetIsAddMessageId(TgDownloadSettingsViewModel tgDownloadSettings) =>
        tgDownloadSettings.IsJoinFileNameWithMessageId = AskQuestionTrueFalseReturnPositive(TgLocale.MenuDownloadSetIsAddMessageId, true);

    private void DownloadSetDestDirectory(TgDownloadSettingsViewModel tgDownloadSettings) =>
        tgDownloadSettings.SourceVm.Dto.IsAutoUpdate = AskQuestionTrueFalseReturnPositive(TgLocale.MenuDownloadSetIsAutoUpdate, true);

    private void DownloadSetIsCreatingSubdirectories(TgDownloadSettingsViewModel tgDownloadSettings) =>
        tgDownloadSettings.SourceVm.Dto.IsCreatingSubdirectories = AskQuestionTrueFalseReturnPositive(TgLocale.MenuDownloadSetIsCreatingSubdirectories, true);

    private void DownloadSetIsFileNamingByMessage(TgDownloadSettingsViewModel tgDownloadSettings) =>
        tgDownloadSettings.SourceVm.Dto.IsFileNamingByMessage = AskQuestionTrueFalseReturnPositive(TgLocale.MenuDownloadSetIsFileNamingByMessage, true);

    private async Task DownloadSetCountThreadsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        await LoadTgClientSettingsByIdAsync(tgDownloadSettings);
        tgDownloadSettings.CountThreads = AnsiConsole.Ask<int>(TgLog.GetLineStampInfo($"{TgLocale.MenuDownloadSetCountThreadsByFreeLicense}:"));
        if (tgDownloadSettings.CountThreads < 1)
            tgDownloadSettings.CountThreads = 1;
        else
        {
            switch (BusinessLogicManager.LicenseService.CurrentLicense.LicenseType)
            {
                case TgEnumLicenseType.Test:
                case TgEnumLicenseType.Paid:
                case TgEnumLicenseType.Premium:
                    if (tgDownloadSettings.CountThreads > TgGlobalTools.DownloadCountThreadsLimitPaid)
                        tgDownloadSettings.CountThreads = TgGlobalTools.DownloadCountThreadsLimitPaid;
                    break;
                default:
                    if (tgDownloadSettings.CountThreads > TgGlobalTools.DownloadCountThreadsLimitFree)
                        tgDownloadSettings.CountThreads = TgGlobalTools.DownloadCountThreadsLimitFree;
                    break;
            }
        }
    }

    private async Task DownloadSettingsSaveAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        await tgDownloadSettings.UpdateSourceWithSettingsAsync();
        await BusinessLogicManager.ConnectClient.UpdateStateSourceAsync(tgDownloadSettings.SourceVm.Dto.Id, tgDownloadSettings.SourceVm.Dto.FirstId, tgDownloadSettings.SourceVm.Dto.Count,
            TgLocale.SettingsChat);
    }

    private async Task LoadTgClientSettingsByIdAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        // Save manual settings
        var directory = tgDownloadSettings.SourceVm.Dto.Directory;
        var firstId = tgDownloadSettings.SourceVm.Dto.FirstId;
        // Find by ID
        var storageResult = await BusinessLogicManager.StorageManager.SourceRepository.GetAsync(new() { Id = tgDownloadSettings.SourceVm.Dto.Id });
        if (storageResult.IsExists)
        {
            if (string.IsNullOrEmpty(directory))
                directory = storageResult.Item.Directory;
            tgDownloadSettings.SourceVm.Dto = new TgEfSourceDto().Copy(storageResult.Item, isUidCopy: true);
        }
        // Restore manual settings
        if (!string.IsNullOrEmpty(directory))
            tgDownloadSettings.SourceVm.Dto.Directory = directory;
        if (firstId > 1)
            tgDownloadSettings.SourceVm.Dto.FirstId = firstId;
    }

    private async Task LoadTgClientSettingsByNameAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        var directory = tgDownloadSettings.SourceVm.Dto.Directory;
        // Find by UserName
        var storageResult = await BusinessLogicManager.StorageManager.SourceRepository.GetAsync(new() { UserName = tgDownloadSettings.SourceVm.Dto.UserName });
        if (storageResult.IsExists)
        {
            if (string.IsNullOrEmpty(directory))
                directory = storageResult.Item.Directory;
            tgDownloadSettings.SourceVm.Dto = new TgEfSourceDto().Copy(storageResult.Item, isUidCopy: true);
        }
        // Restore directory
        if (!string.IsNullOrEmpty(directory) && string.IsNullOrEmpty(tgDownloadSettings.SourceVm.Dto.Directory))
            tgDownloadSettings.SourceVm.Dto.Directory = directory;
    }

    private async Task DownloadManualAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        await ShowTableClientConDownloadAsync(tgDownloadSettings);
        await DownloadSettingsSaveAsync(tgDownloadSettings);
        try
        {
            await BusinessLogicManager.ConnectClient.DownloadAllDataAsync(tgDownloadSettings);
        }
        catch (Exception ex)
        {
            CatchException(ex);
            var floodWait = BusinessLogicManager.ConnectClient.Client?.FloodRetryThreshold ?? 60;
            TgLog.MarkupWarning($"Flood control: waiting {floodWait} seconds");
            await Task.Delay(floodWait * 1_000);
            // Repeat request after waiting
            await DownloadManualAsync(tgDownloadSettings);
        }
        await DownloadSettingsSaveAsync(tgDownloadSettings);
    }

    private async Task MarkHistoryReadCoreAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        await ShowTableMarkHistoryReadProgressAsync(tgDownloadSettings);
        await BusinessLogicManager.ConnectClient.MarkHistoryReadAsync();
        await ShowTableMarkHistoryReadCompleteAsync(tgDownloadSettings);
    }

    #endregion
}
