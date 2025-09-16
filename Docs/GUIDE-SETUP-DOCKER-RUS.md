# Руководство по настройке Докер версии

## Docker compose usage
```
docker pull damianmorozov/opentgresearcher-console:latest
d: && cd d:\Dockers\opentgresearcher-console
docker compose down
docker image rm opentgresearcher-console
docker compose up --build -d
docker compose logs
```

Если вы собираетесь монтировать `TgStorage.db` и другие файлы внутрь контейнера (volumes в docker-compose.yml), убедитесь, что эти файлы созданы заранее. В противном случае Docker автоматически создаст директории с такими именами при запуске контейнера.

Например, вы можете создать пустые файлы так:

**sh:**
```sh
touch TgStorage.db OpenTgResearcher.xml OpenTgResearcher.session
```

**PowerShell:**
```powershell
New-Item -ItemType File TgStorage.db, OpenTgResearcher.xml, OpenTgResearcher.session
```



## Файл docker-compose.yml
```
services:
  opentgresearcher-console:
    image: damianmorozov/opentgresearcher-console:latest
    ports:
     - "7681:7681"
    environment:
     - TZ=Europe/Rome
    volumes:
     - .\TgStorage.db:/app/TgStorage.db:rw # optional
     - .\OpenTgResearcher.xml:/app/OpenTgResearcher.xml:rw # optional
     - .\OpenTgResearcher.session:/app/OpenTgResearcher.session:rw # optional
    container_name: opentgresearcher-console
    restart: on-failure
```

## Использование в веб-браузере
http://localhost:7681
```
dotnet OpenTgResearcherConsole.dll
```
