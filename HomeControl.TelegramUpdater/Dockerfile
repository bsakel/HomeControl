#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["HomeControl.TelegramUpdater/HomeControl.TelegramUpdater.csproj", "HomeControl.TelegramUpdater/"]
RUN dotnet restore "./HomeControl.TelegramUpdater/./HomeControl.TelegramUpdater.csproj"
COPY . .
WORKDIR "/src/HomeControl.TelegramUpdater"
RUN dotnet build "./HomeControl.TelegramUpdater.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./HomeControl.TelegramUpdater.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HomeControl.TelegramUpdater.dll"]