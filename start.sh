#!/bin/bash

set -e

echo "🚀 Démarrage du protocole de réinitialisation NUCLÉAIRE de Crystal ERP..."

echo "🔍 Vérification des prérequis locaux..."

if ! command -v npm &> /dev/null; then
    echo "❌ ERREUR: npm n'est pas installé. Veuillez installer Node.js."
    exit 1
fi

if ! command -v pnpm &> /dev/null; then
    echo "⚠️ pnpm introuvable. Installation globale via npm en cours (10.22.0)..."
    npm install -g pnpm@10.22.0
fi

if ! command -v dotnet &> /dev/null; then
    echo "❌ ERREUR: Le SDK .NET n'est pas installé (Requis pour le Backend)."
    exit 1
fi

echo "🔍 Vérification de dotnet-ef..."
if [ -f backend/.config/dotnet-tools.json ]; then
    echo "📦 Restauration des outils dotnet locaux..."
    (cd backend && dotnet tool restore)
elif ! dotnet ef --version &>/dev/null; then
    echo "⚠️ dotnet ef introuvable — installation globale..."
    dotnet tool install --global dotnet-ef
fi

echo "🧹 Purge de l'environnement Docker du projet..."
docker-compose down -v --remove-orphans

echo "☢️ Destruction des anciennes migrations C#..."
rm -rf backend/Crystal.Infrastructure/Migrations
rm -rf backend/Crystal.API/Migrations

echo "☢️ Création d'une migration initiale propre..."
cd backend
dotnet restore Crystal.sln


dotnet ef migrations add InitialCreate -p Crystal.Infrastructure/Crystal.Infrastructure.csproj -s Crystal.API/Crystal.API.csproj
cd ..

echo "📦 Installation des paquets Frontend (pnpm)..."
cd frontend
pnpm install --config.confirmModulesPurge=false
cd ..

echo "🐳 Construction des images Docker sans cache (Tout à neuf)..."
docker-compose build --no-cache

echo "⚡ Démarrage de l'infrastructure..."
docker-compose up -d --force-recreate

echo "⏳ Attente de PostgreSQL..."
for i in $(seq 1 30); do
    if docker exec crystal-db pg_isready -U stringempty -d bd-erp-crystal >/dev/null 2>&1; then
        echo "  OK — PostgreSQL est prêt."
        break
    fi
    if [ "$i" -eq 30 ]; then
        echo "  ⚠️ PostgreSQL met du temps à démarrer — vérifiez avec: docker-compose logs db"
    fi
    sleep 2
done

echo "========================================================"
echo "✅ SUCCÈS ! Crystal ERP tourne sur une base 100% vierge avec de nouvelles migrations."
echo "========================================================"
docker-compose logs -f