# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./DiscordBot/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY ./DiscordBot/*.cs ./
COPY ./DiscordBot/Source ./
COPY ./Assets/DecentM/Scripts/SubtitleParser ../Assets/DecentM/Scripts/SubtitleParser

RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "DiscordBot.dll"]
