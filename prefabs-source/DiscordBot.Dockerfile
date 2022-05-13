# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./DiscordBot/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY ./DiscordBot/src/* ./
COPY ./Assets/DecentM/Prefabs/VideoPlayer/Scripts/Editor/Subtitles/* ./
COPY ./Assets/DecentM/Editor/TextProcessing.cs ./Assets/DecentM/Editor/ArabicLigaturesPreprocessor.cs ./

ARG version
RUN dotnet publish -c Release -o out /p:AssemblyVersion=${version}

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "DiscordBot.dll"]
