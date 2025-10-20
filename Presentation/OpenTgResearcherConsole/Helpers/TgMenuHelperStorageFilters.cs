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
        var filterDto = new TgEfFilterDto();
        var type = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title($"  {TgLocale.MenuFiltersSetType}")
            .PageSize(Console.WindowHeight - 17)
            .AddChoices(TgLocale.MenuReturn, TgLocale.MenuFiltersSetSingleName, TgLocale.MenuFiltersSetSingleExtension,
                TgLocale.MenuFiltersSetMultiName, TgLocale.MenuFiltersSetMultiExtension,
                TgLocale.MenuFiltersSetMinSize, TgLocale.MenuFiltersSetMaxSize));
        if (Equals(type, TgLocale.MenuReturn))
            return;

        filterDto.IsEnabled = true;
        filterDto.Name = AnsiConsole.Ask<string>(TgLog.GetMarkupString($"{TgLocale.MenuFiltersSetName}:"));
        switch (type)
        {
            case "Single name":
                filterDto.FilterType = TgEnumFilterType.SingleName;
                break;
            case "Single extension":
                filterDto.FilterType = TgEnumFilterType.SingleExtension;
                break;
            case "Multi name":
                filterDto.FilterType = TgEnumFilterType.MultiName;
                break;
            case "Multi extension":
                filterDto.FilterType = TgEnumFilterType.MultiExtension;
                break;
            case "File minimum size":
                filterDto.FilterType = TgEnumFilterType.MinSize;
                break;
            case "File maximum size":
                filterDto.FilterType = TgEnumFilterType.MaxSize;
                break;
        }
        switch (filterDto.FilterType)
        {
            case TgEnumFilterType.SingleName:
            case TgEnumFilterType.SingleExtension:
            case TgEnumFilterType.MultiName:
            case TgEnumFilterType.MultiExtension:
                filterDto.Mask = AnsiConsole.Ask<string>(TgLog.GetMarkupString($"{TgLocale.MenuFiltersSetMask}:"));
                break;
            case TgEnumFilterType.MinSize:
                SetFilterSize(filterDto, TgLocale.MenuFiltersSetMinSize);
                break;
            case TgEnumFilterType.MaxSize:
                SetFilterSize(filterDto, TgLocale.MenuFiltersSetMaxSize);
                break;
        }

        await BusinessLogicManager.StorageManager.FilterRepository.SaveAsync(filterDto);
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
            var filterDto = await BusinessLogicManager.StorageManager.FilterRepository.GetDtoAsync(where: x => x.Uid == dto.Uid);
            filterDto.IsEnabled = AskQuestionTrueFalseReturnPositive(TgLocale.MenuFiltersSetIsEnabled, true);
            await BusinessLogicManager.StorageManager.FilterRepository.SaveAsync(filterDto);
            await FiltersViewAsync(tgDownloadSettings);
        }
    }

    /// <summary> Set filter size </summary>
    private void SetFilterSize(TgEfFilterDto filterDto, string question)
    {
        filterDto.SizeType = AnsiConsole.Prompt(new SelectionPrompt<TgEnumFileSizeType>()
            .Title($"  {TgLocale.MenuFiltersSetSizeType}")
            .PageSize(Console.WindowHeight - 17)
            .AddChoices(TgEnumFileSizeType.Bytes, TgEnumFileSizeType.KBytes, TgEnumFileSizeType.MBytes, TgEnumFileSizeType.GBytes, TgEnumFileSizeType.TBytes));
        filterDto.Size = AnsiConsole.Ask<uint>(TgLog.GetMarkupString($"{question}:"));
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
