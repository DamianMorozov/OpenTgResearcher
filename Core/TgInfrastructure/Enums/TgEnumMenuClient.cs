// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Enums;

public enum TgEnumMenuClient
{
	Return,
    UseClient,
    ClearClientConnectionData,
	RegisterTelegramApp,
	ClientSetProxy,
	ClientConnect,
	ClientDisconnect,
    // Advanced
    AdvancedStartAutoDownload,
    AdvancedAutoViewEvents,
    AdvancedMarkAllMessagesAsRead,
    AdvancedViewChats,
    AdvancedSearchChats,
    AdvancedSearchContacts,
    AdvancedSearchDialogs,
    AdvancedSearchStories,
    // Download
    StorageResetAutoDownload,
    DownloadManual,
    DownloadSetCountThreads,
    DownloadSetDestDirectory,
    DownloadSetIsAddMessageId,
    DownloadSetIsAutoUpdate,
    DownloadSetIsCreatingSubdirectories,
    DownloadSetIsFileNamingByMessage,
    DownloadSetIsRewriteFiles,
    DownloadSetIsRewriteMessages,
    DownloadSetIsSaveMessages,
    DownloadSetSource,
    DownloadSetSourceFirstIdAuto,
    DownloadSetSourceFirstIdManual,
    DownloadSettingsSave,
}