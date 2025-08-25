// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
	#region Methods

	private static TgEnumMenuStorageFilters SetMenuStorageFilter()
	{
        var selectionPrompt = new SelectionPrompt<string>()
            .Title($"  {TgLocale.MenuSwitchNumber}")
            .PageSize(Console.WindowHeight - 17)
            .MoreChoicesText(TgLocale.MoveUpDown);
        selectionPrompt.AddChoices(
            TgLocale.MenuReturn,
            TgLocale.MenuFiltersAdd,
            TgLocale.MenuStorageFiltersClear,
            TgLocale.MenuFiltersEdit,
            TgLocale.MenuFiltersRemove,
            TgLocale.MenuFiltersView
        );

        var prompt = AnsiConsole.Prompt(selectionPrompt);
        if (prompt.Equals(TgLocale.MenuFiltersAdd))
            return TgEnumMenuStorageFilters.FiltersAdd;
        if (prompt.Equals(TgLocale.MenuStorageFiltersClear))
            return TgEnumMenuStorageFilters.FiltersClear;
        if (prompt.Equals(TgLocale.MenuFiltersEdit))
            return TgEnumMenuStorageFilters.FiltersEdit;
        if (prompt.Equals(TgLocale.MenuFiltersRemove))
            return TgEnumMenuStorageFilters.FiltersRemove;
        if (prompt.Equals(TgLocale.MenuFiltersView))
            return TgEnumMenuStorageFilters.FiltersView;

        return TgEnumMenuStorageFilters.Return;
	}

	public async Task SetupStorageFilterAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		TgEnumMenuStorageFilters menu;
		do
		{
			await ShowTableStorageFiltersAsync(tgDownloadSettings);
			menu = SetMenuStorageFilter();
			switch (menu)
            {
                case TgEnumMenuStorageFilters.FiltersAdd:
                    await FiltersAddAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuStorageFilters.FiltersClear:
                    await FiltersClearAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuStorageFilters.FiltersEdit:
                    await FiltersEditAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuStorageFilters.FiltersRemove:
                    await FiltersRemoveAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuStorageFilters.FiltersView:
                    await FiltersViewAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuStorageFilters.Return:
					break;
			}
		} while (menu is not TgEnumMenuStorageFilters.Return);
	}

    private async Task FiltersAddAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        var filter = new TgEfFilterEntity();
        var type = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title($"  {TgLocale.MenuFiltersSetType}")
            .PageSize(Console.WindowHeight - 17)
            .AddChoices(TgLocale.MenuReturn, TgLocale.MenuFiltersSetSingleName, TgLocale.MenuFiltersSetSingleExtension,
                TgLocale.MenuFiltersSetMultiName, TgLocale.MenuFiltersSetMultiExtension,
                TgLocale.MenuFiltersSetMinSize, TgLocale.MenuFiltersSetMaxSize));
        if (Equals(type, TgLocale.MenuReturn))
            return;

        filter.IsEnabled = true;
        filter.Name = AnsiConsole.Ask<string>(TgLog.GetMarkupString($"{TgLocale.MenuFiltersSetName}:"));
        switch (type)
        {
            case "Single name":
                filter.FilterType = TgEnumFilterType.SingleName;
                break;
            case "Single extension":
                filter.FilterType = TgEnumFilterType.SingleExtension;
                break;
            case "Multi name":
                filter.FilterType = TgEnumFilterType.MultiName;
                break;
            case "Multi extension":
                filter.FilterType = TgEnumFilterType.MultiExtension;
                break;
            case "File minimum size":
                filter.FilterType = TgEnumFilterType.MinSize;
                break;
            case "File maximum size":
                filter.FilterType = TgEnumFilterType.MaxSize;
                break;
        }
        switch (filter.FilterType)
        {
            case TgEnumFilterType.SingleName:
            case TgEnumFilterType.SingleExtension:
            case TgEnumFilterType.MultiName:
            case TgEnumFilterType.MultiExtension:
                filter.Mask = AnsiConsole.Ask<string>(TgLog.GetMarkupString($"{TgLocale.MenuFiltersSetMask}:"));
                break;
            case TgEnumFilterType.MinSize:
                SetFilterSize(filter, TgLocale.MenuFiltersSetMinSize);
                break;
            case TgEnumFilterType.MaxSize:
                SetFilterSize(filter, TgLocale.MenuFiltersSetMaxSize);
                break;
        }

        await BusinessLogicManager.StorageManager.FilterRepository.SaveAsync(filter);
        await FiltersViewAsync(tgDownloadSettings);
    }

    /// <summary> Edit filter </summary>
    private async Task FiltersEditAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        var dtos = await BusinessLogicManager.StorageManager.FilterRepository.GetListDtosAsync();
        dtos = [.. dtos.OrderBy(x => x.IsEnabled).ThenBy(x => x.Name)];
        var dto = await GetDtoFromEnumerableAsync(TgLocale.MenuStorageViewFilters, dtos, BusinessLogicManager.StorageManager.FilterRepository);
        if (dto.Uid != Guid.Empty)
        {
            var filter = await BusinessLogicManager.StorageManager.FilterRepository.GetItemWhereAsync(x => x.Uid == dto.Uid);
            filter.IsEnabled = AskQuestionTrueFalseReturnPositive(TgLocale.MenuFiltersSetIsEnabled, true);
            await BusinessLogicManager.StorageManager.FilterRepository.SaveAsync(filter);
            await FiltersViewAsync(tgDownloadSettings);
        }
    }

    /// <summary> Set filter size </summary>
    private void SetFilterSize(TgEfFilterEntity filter, string question)
    {
        filter.SizeType = AnsiConsole.Prompt(new SelectionPrompt<TgEnumFileSizeType>()
            .Title($"  {TgLocale.MenuFiltersSetSizeType}")
            .PageSize(Console.WindowHeight - 17)
            .AddChoices(TgEnumFileSizeType.Bytes, TgEnumFileSizeType.KBytes, TgEnumFileSizeType.MBytes, TgEnumFileSizeType.GBytes, TgEnumFileSizeType.TBytes));
        filter.Size = AnsiConsole.Ask<uint>(TgLog.GetMarkupString($"{question}:"));
    }

    /// <summary> Remove filter </summary>
    private async Task FiltersRemoveAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        var dtos = await BusinessLogicManager.StorageManager.FilterRepository.GetListDtosAsync();
        dtos = [.. dtos.OrderBy(x => x.IsEnabled).ThenBy(x => x.Name)];
        var dto = await GetDtoFromEnumerableAsync(TgLocale.MenuStorageViewFilters, dtos, BusinessLogicManager.StorageManager.FilterRepository);
        if (dto.Uid != Guid.Empty)
        {
            var filter = await BusinessLogicManager.StorageManager.FilterRepository.GetItemWhereAsync(x => x.Uid == dto.Uid);
            filter.IsEnabled = AskQuestionTrueFalseReturnPositive(TgLocale.MenuFiltersSetIsEnabled, true);
            await BusinessLogicManager.StorageManager.FilterRepository.DeleteAsync(filter);
            await FiltersViewAsync(tgDownloadSettings);
        }
    }

    #endregion
}