# OpenTgResearcher — руководство разработчика

[README](README.md) · [Архитектура](ARCHITECTURE.md) · [Журнал изменений](CHANGELOG.md) · [Список задач](TODO.md)

Здесь собраны команды сборки, отладка Desktop, селфтесты и диагностика. Пользовательские инструкции находятся в [README](README.md) и [руководствах](Docs/GUIDES-RUS.md).
Команды выполняются из корня решения; блоки PowerShell запускайте в PowerShell.

## Содержание

- [Сборка решения](#сборка-решения)
- [Локализация при отладке Desktop](#локализация-при-отладке-desktop)
- [Проверка прокси и подключения Desktop](#проверка-прокси-и-подключения-desktop)
- [Проверка страницы чата](#проверка-страницы-чата)
- [Проверка страницы обновлений Desktop](#проверка-страницы-обновлений-desktop)
- [Проверка после обновления пакетов](#проверка-после-обновления-пакетов)
- [Проверка запуска Velopack](#проверка-запуска-velopack)
- [Проверка закрытия Desktop](#проверка-закрытия-desktop)

## Сборка решения

Для сборки всех проектов требуется SDK .NET 10 или новее.

Проверка целевых платформ проектов, профилей публикации, CI и Docker, относительных ссылок тестовых проектов и отсутствия привязки тестов к локальным дискам, затем сборка решения:

```powershell
./Tests/SelfTests/Test-Net10.ps1
./Tests/SelfTests/Test-RelativePaths.ps1
dotnet restore OpenTgResearcher.slnx -p:Platform=x64
dotnet build OpenTgResearcher.slnx --no-restore -c Debug -p:Platform=x64 -p:AppxPackageSigningEnabled=false
```

### Тесты хранилища

`TgStorageTests` использует отдельную SQLite в памяти для каждого набора тестов, применяет настоящие миграции и заполняет таблицу версий. Соединение и контейнер зависимостей освобождаются после набора; пользовательская БД не требуется. Регрессионные тесты проверяют запись и чтение, изоляцию данных и освобождение ресурсов.

```powershell
dotnet test Tests/TgStorageTests/TgStorageTests.csproj -c Debug -p:Platform=x64
```

### Тесты бизнес-логики

`TgBusinessLogicTests` запускается через NUnit и VSTest, как тесты хранилища. Используется общее тестовое окружение с мигрированной SQLite в памяти. Проверяются доступ к данным через бизнес-логику, события настоящего мониторинга ресурсов, остановка событий, независимые экземпляры сервиса и сохранность общего кеша после освобождения мониторинга. Telegram и пользовательская БД не используются.

```powershell
dotnet test Tests/TgBusinessLogicTests/TgBusinessLogicTests.csproj -c Debug -p:Platform=x64
```

## Локализация при отладке Desktop

При запуске профиля `OpenTgResearcherDesktop (Package)` строки локализации читаются из `%LOCALAPPDATA%\Packages\<PackageFamilyName>\LocalState\Strings`.
До построения локализатора приложение автоматически создаёт папки `en-US` и `ru-RU` и копирует отсутствующие `Resources.resw` из ресурсов пакета.
При повторном запуске существующий файл заменяется, если его размер отличается от файла в пакете; файлы того же размера не перезаписываются.
`<PackageFamilyName>` зависит от идентификатора и издателя пакета.
Ошибки подготовки ресурсов записываются в журнал приложения.
При запуске профиля `OpenTgResearcherDesktop (Unpackaged)` используется каталог `Strings` рядом с исполняемым файлом.

## Проверка прокси и подключения Desktop

### Формат настроек Telegram Desktop

Порядок полей и шифрование сверены с исходниками Telegram Desktop:
[контейнер настроек](https://github.com/telegramdesktop/tdesktop/blob/5ff615713d2178b8458e201778d23d7abe78db9b/Telegram/SourceFiles/storage/details/storage_file_utilities.cpp),
[настройки приложения](https://github.com/telegramdesktop/tdesktop/blob/5ff615713d2178b8458e201778d23d7abe78db9b/Telegram/SourceFiles/core/core_settings.cpp),
[список прокси](https://github.com/telegramdesktop/tdesktop/blob/5ff615713d2178b8458e201778d23d7abe78db9b/Telegram/SourceFiles/core/core_settings_proxy.cpp).

### Селфтесты без Telegram

Селфтест проверяет исходные модели страниц, SQLite в памяти, импорт синтетических зашифрованных настроек, HTTP CONNECT и SOCKS5 на локальном сервере; подключения к Telegram не выполняются:

```powershell
dotnet run --project Tests/SelfTests/Proxies/Proxies.SelfTest.csproj
```

### Проверка настоящего окна

Открытие страницы и локализацию заголовков проверяет отдельный селфтест настоящего окна Desktop. Перед запуском закройте Desktop и укажите путь к опубликованному unpackaged-приложению.
Тест создаёт отдельные настройки и БД, открывает страницу «Прокси», проверяет шесть заголовков и отсутствие необработанных исключений в журнале, затем закрывает приложение с проверкой кода выхода. Подключение к Telegram не выполняется.

```powershell
./Tests/SelfTests/Test-DesktopProxies.ps1 -ExecutablePath '.../OpenTgResearcher/Releases/Desktop-x64/OpenTgResearcherDesktop.exe' -Language Russian
./Tests/SelfTests/Test-DesktopProxies.ps1 -ExecutablePath '.../OpenTgResearcher/Releases/Desktop-x64/OpenTgResearcherDesktop.exe' -Language English
```

### Проверка реального подключения

Для явной проверки реального подключения укажите БД и сохранённую сессию. Режим `--live` последовательно проверяет прямое соединение и первую запись с типом, отличным от `None`, в порядке списка страницы (сервер, тип, порт).
Проверка вызывает настоящий `TgConnectClientDesktop`, восстанавливает авторизацию, запрашивает текущего пользователя у Telegram и завершает клиент с проверкой сохранности и освобождения сессии. БД открывается только для чтения; для каждого подключения создаётся отдельная временная копия сессии, которая удаляется после проверки. Если нужен код или пароль, проверка останавливается.

```powershell
dotnet run --project Tests/SelfTests/Proxies/Proxies.SelfTest.csproj -- --live "...TgStorage.db" "$env:LOCALAPPDATA/OpenTgResearcherDesktop/current/OpenTgResearcher.session"
```

### Проверка чтения файла сессии

Расшифровку сессии можно проверить отдельно без сети: БД открывается только для чтения, временная копия проверяется штатным файловым хранилищем WTelegram, реквизиты не выводятся.
Успех этой проверки подтверждает чтение файла; действительность авторизации проверяется отдельным режимом `--live`.

```powershell
dotnet run --project Tests/SelfTests/ClientConnection/ClientConnection.SelfTest.csproj -p:Platform=x64 -- --inspect-session "...TgStorage.db" "...OpenTgResearcher.session"
```

## Проверка страницы чата

Селфтест проверяет исходные модели всех восьми страниц чата, обработчик выбора вкладок, разметку, сохранение и пагинацию на настоящих репозиториях с SQLite в памяти. Telegram заменён тестовым клиентом:

```powershell
dotnet run --project Tests/SelfTests/Chat/Chat.SelfTest.csproj -c Debug -p:Platform=x64
```

Для проверки настоящего окна закройте Desktop и укажите опубликованное unpackaged-приложение. Скрипт создаёт отдельные настройки, БД и путь сессии, открывает чат, последовательно посещает семь вкладок и проверяет журнал и код завершения:

```powershell
$desktop = '.../OpenTgResearcher/Releases/Desktop-x64/OpenTgResearcherDesktop.exe'
$chatTest = 'Tests/SelfTests/Chat/bin/Debug_x64/net10.0/Chat.SelfTest.dll'
./Tests/SelfTests/Test-DesktopChat.ps1 -ExecutablePath $desktop -SelfTestAssembly $chatTest -Language Russian
./Tests/SelfTests/Test-DesktopChat.ps1 -ExecutablePath $desktop -SelfTestAssembly $chatTest -Language English
```

Без дополнительных параметров создаётся синтетический чат. Параметры `-SourceDatabase`, `-ChatId` и `-ChatDirectory` позволяют проверить выбранный чат из существующей БД:
источник открывается только для чтения, в тестовую БД копируются выбранный чат, до 100 последних сообщений и связанные пользователи. Реквизиты подключения и сессии не копируются.
Результаты сохраняются в `Tests/SelfTests/bin/ChatPage`. Скрипт проверяет открытие и навигацию без Telegram, сохранение обоих состояний автообновления через диалог и перечитывание SQLite.
При выборе существующей БД также проверяется получение авторов сообщений в таблицу участников; изменяется только тестовая выборка. Настройки должны показывать выбранный чат уже при первом открытии.
Параметр `-CheckMediaPreview` проверяет синтетический чат с настоящей JPEG-обложкой: находит видимый элемент `Image`, проверяет отрисованный цвет и сохраняет снимок области обложки. Этот режим запускается без `-SourceDatabase`.
Он также проверяет скрытие обложки переключателем конфиденциальных данных и отображение подсказки пустого списка после подтверждённой очистки тестовой выборки.
Параметр `-WindowWidth 1280` задаёт ширину тестового окна. Проверяются девять локализованных заголовков, доступная ширина фильтра, границы кнопок и выбор раздела «Чаты»; снимки страниц сохраняются рядом с журналом.
Селфтесты чата также проверяют медиафайлы в отдельных временных каталогах: настоящий обработчик выбирает файл и открывает поток, подменяется только передача байтов из Telegram.

Для ограниченной сетевой проверки готовых документов используется отдельная команда:

```powershell
dotnet $chatTest --live-media '.../TgStorage.db' '.../OpenTgResearcher.session' ID '.../dir'
```

Проверка восстанавливает существующую авторизацию из временной копии сессии, читает метаданные двадцати документов и дважды вызывает настоящий обработчик пропуска готовых файлов.
Исходная БД открывается только для чтения, готовые файлы блокируются от записи, скачивание запрещено. Новые коды и пароли не запрашиваются; временная сессия удаляется после отключения.
Этот режим проверяет пропуск существующих документов; полный парсинг архива и восстановление отсутствующих файлов он не запускает.

Для ограниченной проверки парсинга и настоящей передачи файлов используется отдельный режим:

```powershell
dotnet $chatTest --live-chat '.../TgStorage.db' '.../OpenTgResearcher.session' ID
```

Он читает каталог выбранного чата из БД, восстанавливает авторизацию из копии сессии и разбирает последние несколько сообщений в отдельную SQLite в памяти.
Затем закрывает TCP только тестового клиента, проверяет восстановление авторизованного запроса и повторный разбор без дубликатов.
Из последних ста сообщений выбираются до двух документов размером до 1 МиБ, скачиваются в созданный тестом подкаталог каталога чата и проверяются по размеру.
После отключения тест удаляет только этот подкаталог и копию сессии. Исходные БД, сессия и готовые файлы не изменяются; полный архив не обрабатывается.
Если авторизация недействительна, каталог недоступен или подходящих документов нет, проверка завершается с ошибкой и сохраняет журнал.

## Проверка страницы обновлений Desktop

Пользовательское описание верхней панели и обновлений: [README](README.md#верхняя-панель-и-обновление-desktop).

Селфтесты используют исходные VM и службу навигации, настоящие ресурсы обоих языков и подменённый источник пакетов:

```powershell
dotnet run --project Tests/SelfTests/Updates/Updates.SelfTest.csproj -c Debug -p:Platform=x64
```

Проверка опубликованного окна использует отдельные настройки и тестовую БД, проверяет подпись настроек, переход на страницу, подтверждения и доступность журнала:

```powershell
./Tests/SelfTests/Test-DesktopUpdates.ps1 -ExecutablePath '.../OpenTgResearcherDesktop.exe' -Language Russian
./Tests/SelfTests/Test-DesktopUpdates.ps1 -ExecutablePath '.../OpenTgResearcherDesktop.exe' -Language English
```

Параметр `-Toolbar` дополнительно проверяет элементы верхней панели настоящими нажатиями мыши, переключение языка и темы, сохранение выбора и геометрию при ширине окна 800 и 1280. Для конфиденциальных данных проверяются оба состояния, скрытие поля пути тестовой БД в настройках и сохранение состояния при переходах между страницами без подключения Telegram.
Проверка сетевого сканирования использует отдельную БД и TCP-сервер loopback с заполненной очередью подключений. Она сравнивает кадры верхнего логотипа до, во время и после проверки прокси, проверяет значок настоящего окна и штатное завершение. Перед запуском соберите `Tests/SelfTests/Proxies/Proxies.SelfTest.csproj` в Debug x64 и закройте Desktop:

```powershell
./Tests/SelfTests/Test-DesktopProxies.ps1 -ExecutablePath '.../OpenTgResearcherDesktop.exe' -Language Russian -NetworkLogo -Theme Dark
./Tests/SelfTests/Test-DesktopProxies.ps1 -ExecutablePath '.../OpenTgResearcherDesktop.exe' -Language English -NetworkLogo -Theme Light
```

Параметр `-Splash` проверяет последовательность эффектов и кадры настоящего окна, движение точки со шлейфом снаружи кнопки «Продолжить», обычное нажатие кнопки, общий знак OTR в верхней панели при ширине 800 и 1280 и штатное закрытие. Параметр `-Theme Light` или `-Theme Dark` задаёт тему тестового запуска. Мягкое появление и орбита выполняются параллельно пять секунд, затем сканирование делает один проход. Инициализация идёт параллельно; после неё и вводных пяти секунд становится доступна кнопка «Продолжить». Мягкая точка с коротким затухающим шлейфом обходит кнопку по часовой стрелке до ухода со заставки. При отключении анимаций Windows содержимое видно сразу и дополнительного ожидания нет.

```powershell
./Tests/SelfTests/Test-DesktopUpdates.ps1 -ExecutablePath '.../OpenTgResearcherDesktop.exe' -Language Russian -Splash -Theme Dark
./Tests/SelfTests/Test-DesktopUpdates.ps1 -ExecutablePath '.../OpenTgResearcherDesktop.exe' -Language English -Splash -Theme Light
```

Эти проверки не устанавливают новую версию и не подтверждают обновление установленного приложения через GitHub; полный переход между выпусками проверяется отдельно в пользовательском контуре.

## Проверка после обновления пакетов

Общая версия всех проектов задаётся свойством `Version` в [Directory.Build.props](Directory.Build.props). Версии сборок и файлов вычисляются SDK; наследование в Debug и Release проверяет команда `./Tests/SelfTests/Test-ProjectVersion.ps1`.
Совместимость с обновлёнными API Telegram проверяется без сетевого подключения, на SQLite в памяти. Отдельный селфтест проверяет сохранность результатов Rebuild при повторном запросе очистки:

```powershell
dotnet run --project Tests/SelfTests/TelegramCompatibility/TelegramCompatibility.SelfTest.csproj -p:Platform=x64
./Tests/SelfTests/Test-RebuildOutputs.ps1
```

## Проверка запуска Velopack

Desktop и Console вызывают `VelopackApp.Run()` в начале синхронного `Main`, до создания интерфейса, сервисов и хранилища.
Приветствие после первой установки выводится после подготовки приложения. Селфтест читает IL готовых сборок без их запуска: проверяет порядок вызовов, отсутствие повторного запуска Velopack, STA и запуск WinUI в Desktop.
После сборки решения `Debug | x64` выполните из корня решения:

```powershell
$desktopAssembly = 'Presentation/OpenTgResearcherDesktop/bin/Debug_x64/net10.0-windows10.0.19041.0/win-x64/OpenTgResearcherDesktop.dll'
$consoleAssembly = 'Presentation/OpenTgResearcherConsole/bin/Debug_x64/net10.0/OpenTgResearcherConsole.dll'
dotnet run --project Tests/SelfTests/DesktopStartup/DesktopStartup.SelfTest.csproj -p:Platform=x64 --no-build -- --velopack $desktopAssembly $consoleAssembly
```

При упаковке `vpk` должен подтвердить `Verified VelopackApp.Run()` в `Program::Main()` для каждого приложения. Проверка IL и упаковка не заменяют проверку установки и обновления пользователем.

Для проверки опубликованного Desktop без MSIX выполните в PowerShell из корня решения:

```powershell
./Tests/SelfTests/Test-DesktopReleaseStartup.ps1 -ExecutablePath '.../OpenTgResearcher/Releases/Desktop-x64/OpenTgResearcherDesktop.exe' -Language Russian
./Tests/SelfTests/Test-DesktopReleaseStartup.ps1 -ExecutablePath '.../OpenTgResearcher/Releases/Desktop-x64/OpenTgResearcherDesktop.exe' -Language English
```

Перед проверкой закройте Desktop. Каждый запуск использует отдельные настройки, БД и путь сессии в `Tests/SelfTests/bin/ReleaseStartup`; подключения к Telegram нет.
Проверка требует сохранения языка после загрузки заставки, продолжения работы процесса и штатного закрытия с кодом 0. Она воспроизводит вылет UWP `ResourceContext` в Release без MSIX, который не обнаруживается одной сборкой и упаковкой.

## Проверка закрытия Desktop

Порядок завершения приложения: [README](README.md#закрытие-desktop).

Для воспроизведения сценария «главная страница → закрытие» в открытой Visual Studio выберите это решение, конфигурацию `Debug | x64` и профиль `OpenTgResearcherDesktop (Package)`. Остановите текущую отладку, затем выполните в PowerShell 7:

```powershell
./Tests/SelfTests/Test-DesktopShutdownVs.ps1 -CloseDelaySeconds 3
```

Селфтест запускает выбранный профиль, нажимает «Продолжить», подтверждает загрузку главной страницы по новым строкам журнала и закрывает окно. Успех требует завершения процесса с кодом 0. При остановке отладчика состояние сохраняется в `Tests/SelfTests/bin/DesktopShutdownVs/break.txt`; неудачный запуск остаётся доступен для диагностики. SHA256 загруженной WinUI выводится в консоль; параметр `-ExpectedXamlSha256` позволяет проверить конкретную DLL. Команда с `-StopFailedRun` завершает активную отладку этого решения.
Отдельная проверка под CDB из Windows SDK не включает инструменты Live Visual Tree. Каталог Windows SDK определяется через системный каталог Program Files (x86); другой путь можно передать параметром `-DebuggerPath`, в том числе относительно текущего каталога консоли:

```powershell
./Tests/SelfTests/Test-DesktopShutdown.ps1 -PackageFullName '<PackageFullName>' -ReadyPattern 'Notification loading|Загрузка уведомлений' -OpenMainPage -CaptureManagedExceptions
```

В дампе одного из сбоев под VS обнаружен стек `RuntimeObject.GetXamlRoot → RemoveRootObjectFromLVT → XamlIsland.Close`; аналогичный сбой описан в [отчёте Microsoft](https://github.com/microsoft/microsoft-ui-xaml/issues/10587). Прохождение CDB не подтверждает устранение этой ошибки под VS. Повторная проверка под VS пока нестабильна; подтверждённого исправления нет.
