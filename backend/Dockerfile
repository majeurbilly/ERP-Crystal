FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Crystal.API/Crystal.API.csproj", "Crystal.API/"]
COPY ["Crystal.Core/Crystal.Core.csproj", "Crystal.Core/"]
COPY ["Crystal.Infrastructure/Crystal.Infrastructure.csproj", "Crystal.Infrastructure/"]

# Restaurer les dépendances
RUN dotnet restore Crystal.API/Crystal.API.csproj

# Copier tout le reste
COPY . .

# Publier l'application
WORKDIR /src/Crystal.API
RUN dotnet publish Crystal.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

USER app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Crystal.API.dll"]