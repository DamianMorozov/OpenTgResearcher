namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
	#region Methods

	private TgEnumMenuClientCon SetMenuClientCon()
	{
		var selectionPrompt = new SelectionPrompt<string>()
			.Title($"  {TgLocale.MenuSwitchNumber}")
			.PageSize(Console.WindowHeight - 17)
			.MoreChoicesText(TgLocale.MoveUpDown);
		selectionPrompt.AddChoices(
			TgLocale.MenuReturn,
			TgLocale.MenuClientSetup,
			TgLocale.MenuClientAdvanced,
			TgLocale.MenuClientDownload,
            TgLocale.MenuClientSearch
        );

        var prompt = AnsiConsole.Prompt(selectionPrompt);
        if (prompt.Equals(TgLocale.MenuClientSetup))
            return TgEnumMenuClientCon.ClientSetup;
        if (prompt.Equals(TgLocale.MenuClientAdvanced))
            return TgEnumMenuClientCon.ClientAdvanced;
        if (prompt.Equals(TgLocale.MenuClientDownload))
            return TgEnumMenuClientCon.ClientDownload;
        if (prompt.Equals(TgLocale.MenuClientSearch))
            return TgEnumMenuClientCon.ClientSearch;

        return TgEnumMenuClientCon.Return;
    }

    public async Task SetupClientConAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		TgEnumMenuClientCon menu;
		do
		{
			await ShowTableClientConAsync(tgDownloadSettings);

			menu = SetMenuClientCon();
			switch (menu)
			{
                case TgEnumMenuClientCon.ClientSetup:
                    await SetupClientConnectionSetupAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuClientCon.ClientAdvanced:
                    await SetupClientConAdvancedAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuClientCon.ClientDownload:
                    await SetupClientConDownloadAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuClientCon.ClientSearch:
                    await SetupClientConSearchAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuClientCon.Return:
					break;
			}
		} while (menu is not TgEnumMenuClientCon.Return);
	}

    #endregion
}
