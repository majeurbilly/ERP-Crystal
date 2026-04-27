#!/bin/bash

USERNAME="majeurbilly"
VERSION_FILE=".version"

NOVERSION=$(cat "$VERSION_FILE")     
NOVERSION=$((NOVERSION + 1))          
echo "$NOVERSION" > "$VERSION_FILE"   

VERSION="v1.0.$NOVERSION"

echo "========================================"
echo "🔨 DÉMARRAGE DU BUILD - VERSION $VERSION"
echo "========================================"

echo "📦 1/2 : Construction de l'image Backend..."
docker build -t $USERNAME/crystal-backend:$VERSION -t $USERNAME/crystal-backend:latest ./backend

echo "📦 2/2 : Construction de l'image Frontend..."
docker build -t $USERNAME/crystal-frontend:$VERSION -t $USERNAME/crystal-frontend:latest ./frontend

echo "========================================"
echo "✅ BUILD TERMINÉ AVEC SUCCÈS !"
echo "========================================"