# OpenTgResearcher - инструмент анализа Telegram-чатов и скачивания их содержимого

## Назначение продукта
- Открытый исследователь и поисковик Телеграм
- Скачивай медиа файлы из чатов на свой диск
- Сохраняй сообщения чатов в локальное хранилище
- Просматривай расширенную информацию об участниках чатов
- Просматривай статистику чатов
- Используй как OSINT инструмент
- Используй как парсер инструмент

## Документация
### <div align="center"><b><a href="README.md">Прочти меня</a> | <a href="CHANGELOG.md">Журнал изменений</a> | <a href="RELEASES.md">Статистика скачиваний</a></b></div>
### <div align="center"><b><a href="ARCHITECTURE.md">Архитектура приложения</a> | <a href="TODO.md">Список задач</a></b></div>
### <div align="center"><b><a href="SCREENSHOTS.md">Скриншоты</a> | <a href="Docs/GUIDES-RUS.md">Руководства</a> | <a href="LICENSE.md">Лицензия</a></b></div>

## Скачать
- [Релизы](https://github.com/DamianMorozov/OpenTgResearcher/releases)

## ГитХаб сообщество
- [ГитХаб репозиторий](https://github.com/DamianMorozov/OpenTgResearcher)
- [Сообщить о баге](https://github.com/DamianMorozov/OpenTgResearcher/issues)
- [Обсуждения](https://github.com/DamianMorozov/OpenTgResearcher/discussions)

## Поддержка пользователей
- [OpenTgResearcher группа в ТГ](https://t.me/OpenTgResearcher)

## Докер
- [Докер образ](https://hub.docker.com/repository/docker/damianmorozov/opentgresearcher-console)

## Требования к установке
- [Зарегистрируйте Telegram приложение](https://my.telegram.org/apps)
- Сохраните значения полей секции `App configuration`: `App api_id` / `App api_hash`

## Локализация при отладке Десктоп
При запуске профиля `OpenTgResearcherDesktop (Package)` строки локализации читаются из `%LOCALAPPDATA%\Packages\<PackageFamilyName>\LocalState\Strings`.
До построения локализатора приложение автоматически создаёт папки `en-US` и `ru-RU` и копирует отсутствующие `Resources.resw` из ресурсов пакета.
При повторном запуске существующий файл заменяется, если его размер отличается от файла в пакете; файлы того же размера не перезаписываются.
`<PackageFamilyName>` зависит от идентификатора и издателя пакета.
Ошибки подготовки ресурсов записываются в журнал приложения.
При запуске профиля `OpenTgResearcherDesktop (Unpackaged)` используется каталог `Strings` рядом с исполняемым файлом.

## Ключевые слова
ОпенТгРесечер, ОпенТгРесёчер, Опен-Тг-Ресечер, Опен-Тг-Ресёчер, Телеграм, скачать, тгскачать, тг-скачать, ресёчер, ресечер, тгресечер, тг-ресечер, тгресёчер, тг-ресёчер

---

## Анимации и скриншоты

### Заставка OpenTgResearcherDesktop
<p align="center"><img src="Assets/Animations/OpenTgResearcherDesktop_SplashScreen.gif"></p>

### Заставка OpenTgResearcherConsole
<p align="center"><img src="Assets/Animations/OpenTgResearcherConsole_SplashScreen.gif"></p>

### OpenTgResearcherConsole - Мониторинг чатов
<p align="center"><img src="Assets/Animations/OpenTgResearcherConsole_Start_monitoring_chats.gif"></p>

### OpenTgResearcherConsole - Поиск ключевых слов в чатах
<p align="center"><img src="Assets/Animations/OpenTgResearcherConsole_Start_search_for_keywords_in_chats.gif"></p>
