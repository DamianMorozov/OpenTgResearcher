# See https://aka.ms/customizecontainer to learn how to customize your debug container and 
# how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Docker base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

# Image setup
WORKDIR /app
EXPOSE 7681/tcp

# Install any additional dependencies if necessary
RUN apt-get update && apt-get install -y wget
RUN wget -q http://launchpadlibrarian.net/572052648/ttyd_1.6.3+20210924-1build1_amd64.deb
RUN apt install -y ./ttyd_1.6.3+20210924-1build1_amd64.deb

# App
COPY "/app" "/app"
CMD ["ttyd", "/bin/sh"]
