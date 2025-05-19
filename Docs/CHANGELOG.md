# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.6.240] - 2025-05-19
### Changed
- Updated the menu in OpenTgResearcherConsole
- Updated license view
- Updated NuGet packages
- Updated Velopack installer
- Updated splash screen and added animation in OpenTgResearcherDesktop
- Fix loading and saving settings in OpenTgResearcherDesktop
- Changed installer loading in OpenTgResearcherDesktop
- Enhance logging and window state management in OpenTgResearcherDesktop
- Updated Docker version of OpenTgResearcherConsole
### Added
- Added methods to memorize window size and position between sessions in OpenTgResearcherDesktop
- Added license clear
- Added test license request
- Increased the limit on the number of download threads for a user with a paid or test license
- Added sensitive field handling in OpenTgResearcherDesktop

## [0.6.140] - 2025-04-26
### Fixed
- Fixed progress display for chats in OpenTgResearcherDesktop
- Fixed connection to the server
### Changed
- Updated search in OpenTgResearcherDesktop chats
### Added
- Added option to create subdirectories for chats in TgDownloaderConsole
- Added option to create subdirectories for chats in OpenTgResearcherDesktop
- Added option to naming media files by message for chats in TgDownloaderConsole
- Added option to naming media files by message for chats in OpenTgResearcherDesktop
- Added connecting and disconnecting a client from the OpenTgResearcherDesktop header
- Added links to TgDownloaderConsole

## [0.6.080] - 2025-03-09
### Fixed
- Fixed page views in OpenTgResearcherDesktop
- Fixed loading of portable version of OpenTgResearcherDesktop
### Changed
- Updated load pages in OpenTgResearcherDesktop
- Updated checking for updates in TgDownloaderConsole
### Added
- Added logs pages in OpenTgResearcherDesktop
- Added OpenTgResearcherDesktop application loading progress

## [0.6.040] - 2025-03-02
### Fixed
- Fixed references in the documentation
- Fixed bugs in TgStorage
- Fixed bugs in TgDownloaderConsole
### Changed
- Updated GitHub workflows actions
- Updated apps table for TgStorage
- Updated messages table for TgStorage
- Updated documents table for TgStorage
- Updated sources table for TgStorage
### Added
- Added option to enable/disable saving messages for chat in TgDownloaderConsole
- Added reading user access to chat via search in TgDownloaderConsole
- Added license page for TgDownloaderDesktop
- Added connection state for TgDownloaderDesktop application title

## [0.5.450] - 2025-01-31
### Changed
- Updated NuGet packages
- Fixed some exceptions in TgDownloaderConsole

## [0.5.440] - 2025-01-25
### Fixed
- Fixed file checks for TgDownloaderDesktop
### Changed
- Updated connection page in TgDownloaderDesktop
- Updated TgDownloaderConsole Docker image
### Added
- Added Velopack installer for TgDownloaderDesktop
- Added chat filter to TgDownloaderDesktop
- Added page of chat details to TgDownloaderDesktop
- Added launching download chat on the details page to TgDownloaderDesktop
- Added chat download stop on the details page to TgDownloaderDesktop

## [0.5.430] - 2025-01-23
### Fixed
- Fixed bugs for TgStorage
- Fixed bugs for TgDownloaderConsole
### Changed
- Updated apps table for TgStorage
- Updated NuGet packages
- Updated .NET version from 8.0 to 9.0

## [0.5.410] - 2025-01-18
### Fixed
- Fixed chat connection check to TgDownloaderConsole
### Changed
- Updated apps table for TgStorage
### Added
- Added Velopack installer for TgDownloaderConsole
- Added update menu for TgDownloaderConsole

## [0.5.330] - 2025-01-05
### Fixed
- Fixed connection page to TgDownloaderDesktop
### Added
- Added contact edit page to TgDownloaderDesktop

## [0.5.320] - 2025-01-04
### Fixed
- Fixed `Sequence contains more than one element` error to TgDownloaderConsole
- Fixed `Access to the path ...\TgDownloader.xml is denied` error when connecting to the server to TgDownloaderDesktop
### Changed
- Updated contacts page to TgDownloaderDesktop
- Updated filters page to TgDownloaderDesktop
- Updated sources page to TgDownloaderDesktop
- Updated stories page to TgDownloaderDesktop
### Added
- Source search by UserName field to TgDownloaderConsole
- Added proxies page to TgDownloaderDesktop

## [0.5.300] - 2025-01-03
### Fixed
- Connecting to Telegram server to TgDownloaderDesktop
### Changed
- Updated contacts page to TgDownloaderConsole
- Updated home page to TgDownloaderDesktop
- Updated sources page to TgDownloaderConsole
- Updated table of sources to the storage
- Updated storage tables for asynchronous write cases
- Updated stories page to TgDownloaderConsole
### Added
- Added contacts page to TgDownloaderDesktop
- Added filters page to TgDownloaderDesktop
- Added stories page to TgDownloaderDesktop
- Added reading of contacts on the contact page to TgDownloaderDesktop

## [0.5.250] - 2024-12-08
### Changed
- Updated settings page to TgDownloaderDesktop
### Added
- Added table of contacts to the storage
- Added table of stories to the storage
- Added searching and viewing of contacts to TgDownloaderConsole
- Added searching and viewing of stories to TgDownloaderConsole
- Added sources page to TgDownloaderDesktop
- Added PowerShell script for installing TgDownloaderDesktop

## [0.5.230] - 2024-12-01
### Fixed
- Fixed display of special characters in download directory name to TgDownloaderConsole
- Fixed default display of ID = 1 to TgDownloaderConsole
- Fixed scan sources to TgDownloaderConsole
### Changed
- Default number of download threads (from 1 to 5) to TgDownloaderConsole
- Updated NuGet packages
- Improved stability of work to TgDownloaderConsole
- Improved stability of work to TgDownloaderDesktop
### Added
- Added language localization to TgDownloaderDesktop
- Connection page to TgDownloaderDesktop
- Free license to TgDownloaderDesktop

## [0.5.180] - 2024-11-10
### Fixed
- Created EF Core entities
- Fixed test execution
- GitHub workflows actions
- Fixed saving file names with unsupported characters on Windows / Linux / MacOS
- Fixed moving existing files in the current directory
- Fixed creating TgStorage
## Changed
- Using EF Core instead of DevExpress XPO
## Added
- Added multithreading to TgDownloaderConsole
- Added TgDownloaderDesktop app (WinUI)
- Added TgDownloaderDesktop.Core app (WinUI)
- Added TgDownloaderDesktop.Tests.MSTest app (WinUI)
- Added settings page to TgDownloaderDesktop

## [0.4.150] - 2024-03-27
## Added projects
- Added TgDownloaderBlazor app (web app)
- Added TgEfCore library (EF Core storage access library)
- Added TgEfCoreTests (EF Core storage access tests)
- Added Docker-container TgDownloaderConsole
### Added into TgEfCore
- Application table support
- Document table support
- Filter table support
- Message table support
- Proxy table support
- Source table support
- Version table support
### Added into TgDownloaderConsole
- Docker support
### Added into TgDownloaderBlazor
- Docker support
- Home page
- Header component
- Client page
- Filter page
- Proxy page
- Source page
- Version page
- RadzenDataGrid for tables

## [0.3.110] - 2024-02-17
### Added into TgDownloaderConsole
- Progressbar for downloading current file
- Mark all messages as read
### Added into TgDownloaderWinDesktop
- Progressbar for downloading current file
- Mark all messages as read
### Changed into TgDownloaderWinDesktop
- UI

## [0.3.20] - 2023-12-17
### Added
- Guide to setup the docker version

## [0.3.10] - 2023-12-09
### Changed
- NET 8 version updated
- Updated version of NuGet packages
### Added into TgDownloaderWinDesktop
- Copying fields in the source

## [0.2.580] - 2023-11-10
### Added
- [Issue template](ISSUE.md)

## [0.2.570] - 2023-11-09
### Added into TgDownloaderWinDesktop
- Auto update progress when downloading to the source page
### Fixed in TgDownloaderConsole
- Downloading a source that is not yet in the source table

## [0.2.550] - 2023-11-05
### Fixed in TgDownloaderConsole
- Reading the number of the last message
### Fixed in TgDownloaderWinDesktop
- Clearing the application table
- Message when client connects to Telegram server
- Correct loading of empty sources on first download
- Reading sources from Telegram
- Channel/dialogue scanning
### Added
- Guide to setting up the desktop version
- Channel/dialogue scanning

## [0.2.490] - 2023-11-02
### Fixed
- Creating a new storage

## [0.2.480] - 2023-11-01
### Added into TgDownloaderWinDesktop
- Add new proxy
- Edit proxy
- Return to proxies section
- Return to sources section
### Fixed in TgDownloaderWinDesktop
- Delete proxy

## [0.2.460] - 2023-10-29
### Fixed in TgDownloaderConsole
- Refactoring and tests
### Added into TgDownloaderConsole
- The progress of the download in the console title
### Fixed in TgDownloaderWinDesktop
- Refactoring and tests
### Added into TgDownloaderWinDesktop
- Connecting a client via proxy
- Disconnecting a client via proxy
- Saving settings
- Navigation to the source item page
### Fixed in TgStorage
- Fixed methods in Repositories

## [0.2.300] - 2023-06-27
### Fixed in TgDownloaderConsole
- Fixed errors with file TgDownloader.session
### Fixed in Tests
- Fixed errors in tests
### Added into TgDownloaderWinDesktop
- Edit app settings
- View/edit client settings
- Client
    - Connect/Disconnect
    - Hide password
    - State and exception view
- View proxies
- View sources and download
    - Check client ready
    - Load from Storage
    - Load from Telegram
    - Clear view
    - State and exception view
    - Download media

## [0.2.230] - 2023-06-13
### Fixed in TgDownloaderConsole
- Restore ApiId when session was deleted
- Auto-update after configuring the download directory
### Added
- TgDownloaderWinDesktop project (WPF UI - Fluent Navigation (MVVM | DI))
### Added into TgDownloaderConsole
- Menu Advanced -> Auto view events
- Auto-update last message ID at Advanced -> Auto view events

## [0.2.160] - 2023-04-27
### Changed
- Storage version 18
- Viewing sources in the storage
- Scan my chats / Scan my dialogs
- Projects structure and properties
### Added
- Date time field for source table
- GitHub actions

## [0.2.130] - 2023-04-20
### Changed
- New ORM-framework for SQLite storage (DevExpress XPO)
- Storage version 17
### Added
- Scanning channels/dialogs with the ability to save as sources
- Viewing sources in the storage with the ability to go to the download menu
- Store messages
### Deprecated
- Software v0.2.xxx has a new storage format, save the previous file, it will be overwritten

## [0.1.730] - 2023-03-12
### Added
- Filters settings
- Creating backup storage

## [0.1.630] - 2023-02-24
### Fixed
- Proxy for downloads
- Overwrite zero size files
### Added
- App setting for the session file
- App setting for the storage file
- App setting for the usage proxy
- Automatic directory creation for manual download
- Automatic directory creation for auto download
- Storage versions table

## [0.1.500] - 2023-01-31
### Added
- Proxy for downloads
- Client and proxy exception messages

## [0.1.430] - 2023-01-17
### Added
- Auto download

## [0.1.390] - 2023-01-10
### Added
- Set file date and time
- Scanning subdirectories for downloaded files to move them to the root directory
### Changed
- Combining source ID and user name settings

## [0.1.360] - 2023-01-06
### Fixed
- Entering a source ID
- Autosave and autoload settings to download a channel/group

## [0.1.350] - 2023-01-05
### Added
- Auto calculation of the start message identifier
- Manual set start message identifier
- Auto renaming downloaded files if the option to add an identifier to the file name is enabled
- Autosave and autoload the directory to download a channel/group
### Changed
- Switch method for choice boolean answer
### Fixed
- Rewriting messages

## [0.1.310] - 2023-01-02
### Added
- Message identifier in the download settings
- Saving application settings to an xml file
### Changed
- Setup downloads by channel/group identifier

## [0.1.250] - 2022-12-21
### Added
- Storage settings
- Skip downloaded files
- Autosave connection info at local storage file
### Changed
- Client settings
- Download settings

## [0.1.180] - 2022-12-13
### Added
- Info sub menu
- Download progress
### Changed
- Client menu
- Download menu
- Collect info
- Try catch exceptions

## [0.1.150] - 2022-12-10
### Added
- First release
- Menu
- Log
- Client
- Download

## [0.1.100] - 2022-12-08
### Added
- English localisation
- Tests

## [0.1.020] - 2022-12-07
### Added
- Git base files
- TgDownloaderConsole project
