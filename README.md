# OpenTgResearcher - инструмент анализа Telegram-чатов и скачивания их содержимого

## Назначение продукта
- Открытый исследователь и поисковик Телеграм
- Скачивай медиа файлы из чатов на свой диск
- Сохраняй сообщения чатов в локальное хранилище
- Просматривай расширенную информацию об участниках чатов
- Просматривай статистику чатов
- Используй как OSINT инструмент
- Используй как парсер инструмент

## Официальный сайт
### <div align="center"><b><a href="https://opentgresearcher.online">opentgresearcher.online</a></b></div>

## Русская документация
### <div align="center"><b><a href="README.md">Прочти меня</a> | <a href="CHANGELOG.md">История версий</a> | <a href="Docs/RELEASES.md">Статистика</a></b></div>
### <div align="center"><b><a href="Docs/SCREENSHOTS.md">Скриншоты</a> | <a href="Docs/GUIDES-RUS.md">Руководства</a> | <a href="LICENSE.md">Лицензия</a></b></div>

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

## Программа поощрения за обнаружение ошибок
- [Программа поощрения за обнаружение ошибок](https://opentgresearcher.online/bug-bounty)

## Локализация при отладке Desktop
При запуске профиля `OpenTgResearcherDesktop (Package)` строки локализации читаются из `%LOCALAPPDATA%\Packages\<PackageFamilyName>\LocalState\Strings`.
Если подписи интерфейса пустые, закройте приложение и скопируйте папки `en-US` и `ru-RU` с файлами `Resources.resw` из [Strings проекта](Presentation/OpenTgResearcherDesktop/Strings) в этот каталог, затем повторите запуск.
Для текущего пакета `<PackageFamilyName>`; при изменении идентификатора или издателя пакета имя может отличаться.
В текущей реализации отсутствующие файлы автоматически не копируются: проверяется только обновление существующих файлов по размеру.
При запуске профиля `OpenTgResearcherDesktop (Unpackaged)` используется каталог `Strings` рядом с исполняемым файлом.

## Ключевые слова
ОпенТгРесечер, ОпенТгРесёчер, Опен-Тг-Ресечер, Опен-Тг-Ресёчер, Телеграм, скачать, тгскачать, тг-скачать, ресёчер, ресечер, тгресечер, тг-ресечер, тгресёчер, тг-ресёчер

---

## Анимации и скриншоты

### Заставка OpenTgResearcherDesktop
<p align="center"><img src="Docs/Assets/Animations/OpenTgResearcherDesktop_SplashScreen.gif"></p>

### Заставка OpenTgResearcherConsole
<p align="center"><img src="Docs/Assets/Animations/OpenTgResearcherConsole_SplashScreen.gif"></p>

### OpenTgResearcherConsole - Мониторинг чатов
<p align="center"><img src="Docs/Assets/Animations/OpenTgResearcherConsole_Start_monitoring_chats.gif"></p>

### OpenTgResearcherConsole - Поиск ключевых слов в чатах
<p align="center"><img src="Docs/Assets/Animations/OpenTgResearcherConsole_Start_search_for_keywords_in_chats.gif"></p>
