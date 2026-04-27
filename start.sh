#!/bin/bash

# Arrêter l'exécution immédiatement si une commande échoue (Exécution sans faille)
set -e

echo "🚀 Démarrage du protocole de réinitialisation de Crystal ERP..."

# 1. Vérification et installation des outils de base
echo "🔍 Vérification des prérequis locaux..."

if ! command -v npm &> /dev/null; then
    echo "❌ ERREUR: npm n'est pas installé. Veuillez installer Node.js."
    exit 1
fi

if ! command -v pnpm &> /dev/null; then
    echo "⚠️ pnpm introuvable. Installation globale via npm en cours..."
    npm install -g pnpm
fi

if ! command -v dotnet &> /dev/null; then
    echo "❌ ERREUR: Le SDK .NET n'est pas installé (Requis pour le Backend)."
    exit 1
fi

# 2. Nettoyage nucléaire de Docker (Remise à zéro)
echo "🧹 Purge de l'environnement Docker du projet..."
# Arrête les conteneurs, supprime les réseaux, supprime les volumes (Base de données à neuf !) et les orphelins
docker-compose down -v --remove-orphans

# 3. Installation des dépendances locales (Pour ton IDE / Cursor)
echo "📦 Restauration des packages Backend (C#)..."
cd backend
# Remplace Crystal.sln par le nom exact de ta solution si différent
dotnet restore Crystal.sln 
cd ..

echo "📦 Installation des paquets Frontend (pnpm)..."
cd frontend
pnpm install
cd ..

# 4. Reconstruction et lancement
echo "🐳 Construction des images Docker sans cache (Tout à neuf)..."
docker-compose build --no-cache

echo "⚡ Démarrage de l'infrastructure..."
docker-compose up -d --force-recreate

echo "========================================================"
echo "✅ SUCCÈS ! Crystal ERP tourne sur une base 100% vierge."
echo "========================================================"
# Affiche les logs du backend et de la db pour vérifier que tout monte bien
docker-compose logs -f