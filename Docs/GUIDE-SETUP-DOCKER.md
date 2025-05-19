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
