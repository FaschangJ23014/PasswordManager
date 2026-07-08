# 1. SDK zum Bauen holen
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Kopiere alle Projektdateien
COPY . .

# Restore und Publish direkt ausführen
RUN dotnet restore "backend/backend.csproj"
RUN dotnet publish "backend/backend.csproj" -c Release -o /app/publish

# 2. Runtime zum Starten holen
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Port für Railway öffnen
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "backend.dll"]