#!/bin/bash

USERNAME="majeurbilly"
VERSION_FILE=".version"

NOVERSION=$(cat "$VERSION_FILE") 

VERSION="v1.0.$NOVERSION"

echo "========================================"
echo "🚀 DÉMARRAGE DU PUSH - VERSION $VERSION"
echo "========================================"

echo "☁️ 1/2 : Envoi du Backend vers le nuage..."
docker push $USERNAME/crystal-backend:$VERSION
docker push $USERNAME/crystal-backend:latest

echo "☁️ 2/2 : Envoi du Frontend vers le nuage..."
docker push $USERNAME/crystal-frontend:$VERSION
docker push $USERNAME/crystal-frontend:latest

echo "========================================"
echo "✅ PUSH TERMINÉ ! La version $VERSION est en ligne."
echo "========================================"