# 1. SDK zum Bauen holen
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore *.sln 2>/dev/null || dotnet restore $(find . -name "*.csproj" | head -n 1)
RUN dotnet publish $(find . -name "*.csproj" | head -n 1) -c Release -o /app/publish

# 2. Runtime zum Starten holen
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Port für Railway/Render öffnen
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "backend.dll"]