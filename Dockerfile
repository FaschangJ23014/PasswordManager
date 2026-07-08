# 1. SDK zum Bauen holen
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Kopiere alle Projektdateien
COPY . .

# Wir bauen jetzt EXAKT das Web-Backend (großes oder kleines B wird durch den Pfad abgefangen)
RUN dotnet restore "backend/backend/backend.csproj" 2>/dev/null || dotnet restore "backend/backend.csproj"
RUN dotnet publish "backend/backend.csproj" 2>/dev/null -c Release -o /app/publish || dotnet publish "backend/backend/backend.csproj" -c Release -o /app/publish

# 2. Runtime zum Starten holen
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Port für Render öffnen
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "backend.dll"]