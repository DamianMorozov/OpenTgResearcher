// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
	#region Public and private methods

	private static TgEnumMenuFilter SetMenuFilters()
	{
		var prompt = AnsiConsole.Prompt(
			new SelectionPrompt<string>()
				.Title($"  {TgLocale.MenuSwitchNumber}")
				.PageSize(Console.WindowHeight - 17)
				.MoreChoicesText(TgLocale.MoveUpDown)
				.AddChoices(TgLocale.MenuMainReturn,
					TgLocale.MenuFiltersClear,
					TgLocale.MenuFiltersAdd,
					TgLocale.MenuFiltersEdit,
					TgLocale.MenuFiltersRemove,
					TgLocale.MenuFiltersView
				));
		if (prompt.Equals(TgLocale.MenuFiltersClear))
			return TgEnumMenuFilter.FiltersClear;
		if (prompt.Equals(TgLocale.MenuFiltersAdd))
			return TgEnumMenuFilter.FiltersAdd;
		if (prompt.Equals(TgLocale.MenuFiltersEdit))
			return TgEnumMenuFilter.FiltersEdit;
		if (prompt.Equals(TgLocale.MenuFiltersRemove))
			return TgEnumMenuFilter.FiltersRemove;
		if (prompt.Equals(TgLocale.MenuFiltersView))
			return TgEnumMenuFilter.FiltersView;
		return TgEnumMenuFilter.Return;
	}

	public async Task SetupFiltersAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		TgEnumMenuFilter menu;
		do
		{
			await ShowTableFiltersAsync(tgDownloadSettings);
			menu = SetMenuFilters();
			switch (menu)
			{
				case TgEnumMenuFilter.FiltersClear:
					await ClearFiltersAsync();
					break;
				case TgEnumMenuFilter.FiltersAdd:
					await SetFiltersAddAsync();
					break;
				case TgEnumMenuFilter.FiltersEdit:
					await SetFiltersEditAsync();
					break;
				case TgEnumMenuFilter.FiltersRemove:
					await SetFiltersRemoveAsync();
					break;
				case TgEnumMenuFilter.FiltersView:
					await FiltersViewAsync();
					break;
				case TgEnumMenuFilter.Return:
					break;
			}
		} while (menu is not TgEnumMenuFilter.Return);
	}

	/// <summary> View filters </summary>
	private async Task FiltersViewAsync()
	{
		var storageResult = await BusinessLogicManager.StorageManager.FilterRepository.GetListAsync(TgEnumTableTopRecords.All, 0);
		if (storageResult.IsExists)
		{
			foreach (var filter in storageResult.Items)
			{
				TgLog.WriteLine(filter.ToConsoleString());
			}
		}
		TgLog.TypeAnyKeyForReturn();
	}

	private async Task SetFiltersAddAsync()
	{
		var filter = new TgEfFilterEntity();
		var type = AnsiConsole.Prompt(new SelectionPrompt<string>()
			.Title(TgLocale.MenuFiltersSetType)
			.PageSize(Console.WindowHeight - 17)
			.AddChoices(TgLocale.MenuMainReturn, TgLocale.MenuFiltersSetSingleName, TgLocale.MenuFiltersSetSingleExtension,
				TgLocale.MenuFiltersSetMultiName, TgLocale.MenuFiltersSetMultiExtension,
				TgLocale.MenuFiltersSetMinSize, TgLocale.MenuFiltersSetMaxSize));
		if (Equals(type, TgLocale.MenuMainReturn))
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
		await FiltersViewAsync();
	}

	/// <summary> Edit filter </summary>
	private async Task SetFiltersEditAsync()
	{
		var storageResult = await BusinessLogicManager.StorageManager.FilterRepository.GetListAsync(TgEnumTableTopRecords.All, 0);
		var filter = await GetFilterFromEnumerableAsync(TgLocale.MenuViewFilters, storageResult.Items);
		filter.IsEnabled = AskQuestionTrueFalseReturnPositive(TgLocale.MenuFiltersSetIsEnabled, true);
		await BusinessLogicManager.StorageManager.FilterRepository.SaveAsync(filter);
		await FiltersViewAsync();
	}

	/// <summary> Set filter size </summary>
	private void SetFilterSize(TgEfFilterEntity filter, string question)
	{
		filter.SizeType = AnsiConsole.Prompt(new SelectionPrompt<TgEnumFileSizeType>()
			.Title(TgLocale.MenuFiltersSetSizeType)
			.PageSize(Console.WindowHeight - 17)
			.AddChoices(TgEnumFileSizeType.Bytes, TgEnumFileSizeType.KBytes, TgEnumFileSizeType.MBytes, TgEnumFileSizeType.GBytes, TgEnumFileSizeType.TBytes));
		filter.Size = AnsiConsole.Ask<uint>(TgLog.GetMarkupString($"{question}:"));
	}

	/// <summary> Remove filter </summary>
	private async Task SetFiltersRemoveAsync()
	{
		var storageResult = await BusinessLogicManager.StorageManager.FilterRepository.GetListAsync(TgEnumTableTopRecords.All, 0);
		var filter = await GetFilterFromEnumerableAsync(TgLocale.MenuViewFilters, storageResult.Items);
		await BusinessLogicManager.StorageManager.FilterRepository.DeleteAsync(filter);
		await FiltersViewAsync();
	}

	/// <summary> Clear filters </summary>
	private async Task ClearFiltersAsync()
	{
		if (AskQuestionYesNoReturnNegative(TgLocale.MenuFiltersClear)) return;

		await BusinessLogicManager.StorageManager.FilterRepository.DeleteAllAsync();
		await FiltersViewAsync();
	}

	#endregion
}