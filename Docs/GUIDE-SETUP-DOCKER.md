# Guide to setup the Docker version

## Docker compose usage
```
docker pull damianmorozov/opentgresearcher-console:latest
d: && cd d:\Dockers\opentgresearcher-console
docker compose down
docker image rm opentgresearcher-console
docker compose up --build -d
docker compose logs
```

If you are going to mount `TgStorage.db` and other files inside the container (volumes in docker-compose.yml), make sure that these files are created in advance. Otherwise, Docker will automatically create directories with these names when starting the container.

For example, you can create empty files like this:

**sh:**
```sh
touch TgStorage.db OpenTgResearcher.xml OpenTgResearcher.session
```

**PowerShell:**
```powershell
New-Item -ItemType File TgStorage.db, OpenTgResearcher.xml, OpenTgResearcher.session
```



## File docker-compose.yml
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

## Using in a web browser
http://localhost:7681
```
dotnet OpenTgResearcherConsole.dll
```
