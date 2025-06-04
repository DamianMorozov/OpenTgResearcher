// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Enums;

public enum TgEnumMenuStorage
{
	Return,
	DbClear,
	DbBackup,
	TablesCompact,
    ClearChats,
    ResetAutoDownload,
    // View
    ViewChats,
    ViewContacts,
    ViewStories,
    ViewVersions,
    // Filters
    FiltersClear,
    FiltersView,
    FiltersAdd,
    FiltersEdit,
    FiltersRemove,
}