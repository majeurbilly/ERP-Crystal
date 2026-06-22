#!/bin/bash

# Arrêter l'exécution immédiatement si une commande échoue
set -e

echo "🚀 Démarrage du protocole de réinitialisation NUCLÉAIRE de Crystal ERP..."

# 1. Vérification et installation des outils de base
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

# Outil EF Core : manifeste local (backend/.config/dotnet-tools.json) ou global / Nix
echo "🔍 Vérification de dotnet-ef..."
if [ -f backend/.config/dotnet-tools.json ]; then
    echo "📦 Restauration des outils dotnet locaux..."
    (cd backend && dotnet tool restore)
elif ! dotnet ef --version &>/dev/null; then
    echo "⚠️ dotnet ef introuvable — installation globale..."
    dotnet tool install --global dotnet-ef
fi

# 2. Nettoyage nucléaire de Docker (Remise à zéro)
echo "🧹 Purge de l'environnement Docker du projet..."
# Arrête les conteneurs, supprime les réseaux, supprime les volumes (Base de données à neuf !) et les orphelins
docker-compose down -v --remove-orphans

# 3. Éradication et reconstruction des Migrations EF Core (Le fameux "Gunné")
echo "☢️ Destruction des anciennes migrations C#..."
# Ajuste le chemin ici si ton dossier Migrations est dans Crystal.API au lieu de Crystal.Infrastructure
rm -rf backend/Crystal.Infrastructure/Migrations
rm -rf backend/Crystal.API/Migrations

echo "☢️ Création d'une migration initiale propre..."
cd backend
dotnet restore Crystal.sln

# Génération de la nouvelle migration.
# MODIFIE CES NOMS SI TES PROJETS S'APPELLENT AUTREMENT :
# -p = Projet contenant le DbContext (souvent Infrastructure)
# -s = Projet de démarrage (souvent API)
dotnet ef migrations add InitialCreate -p Crystal.Infrastructure/Crystal.Infrastructure.csproj -s Crystal.API/Crystal.API.csproj
cd ..

# 4. Installation des dépendances Frontend
echo "📦 Installation des paquets Frontend (pnpm)..."
cd frontend
# Sans TTY (Git Bash, CI, Cursor), pnpm refuse de purger node_modules sans confirmation
pnpm install --config.confirmModulesPurge=false
cd ..

# 5. Reconstruction et lancement
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
# Affiche les logs du backend et de la db pour vérifier que tout monte bien
docker-compose logs -f