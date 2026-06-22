# ERP Crystal — Guide pour le correcteur

Ce document explique **étape par étape** comment récupérer le projet sur votre machine et le faire tourner. La méthode recommandée utilise **Docker** : tout le nécessaire (base de données, API, interface web) démarre avec une seule commande.

---

## Ce dont vous avez besoin (une seule fois)

1. **Git** — pour cloner le dépôt.  
   - Vérifiez dans un terminal : `git --version`  
   - Si la commande n’existe pas : installez Git pour Windows depuis [https://git-scm.com](https://git-scm.com) (laissez les options par défaut, c’est suffisant).

2. **Docker Desktop** — pour lancer les conteneurs.  
   - Téléchargez et installez depuis [https://www.docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop).  
   - Au premier lancement, acceptez les conditions et attendez que Docker indique qu’il est **prêt** (icône stable dans la barre des tâches).  
   - Vérifiez : `docker --version` et `docker compose version`

> **Sans Docker**, il est possible de lancer le backend avec le SDK .NET 9 et le frontend avec Node, mais il faut alors installer **PostgreSQL** localement et aligner les ports avec le fichier `frontend/src/api/apiBaseUrl.ts`. Pour une évaluation rapide, **Docker reste la voie la plus simple**.

---

## Étape 1 — Cloner le dépôt

1. Ouvrez **PowerShell**, **Invite de commandes** ou **Terminal** (au choix).
2. Placez-vous dans le dossier où vous voulez le projet, par exemple le Bureau :
   ```bash
   cd %USERPROFILE%\Desktop
   ```
3. Clonez (remplacez l’URL par celle que l’étudiant vous aura fournie — GitHub, Azure DevOps, etc.) :
   ```bash
   git clone <URL_DU_DEPOT>
   ```
4. **Renommer le dossier cloné (important).**  
   Selon l’hébergeur, Git peut créer un dossier avec des caractères encodés dans le nom, par exemple `ERP%20simplifi%C3%A9` (équivalent de *ERP simplifié*). **Renommez ce dossier en `ERP`** (ou un nom court sans espaces ni `%`) pour éviter des problèmes de chemins et pour suivre les commandes ci‑dessous.  
   - Dans l’Explorateur de fichiers : clic droit sur le dossier → **Renommer** → tapez `ERP`.  
   - Ou en PowerShell, depuis le dossier parent (ex. le Bureau) :
     ```powershell
     Rename-Item -LiteralPath "ERP%20simplifi%C3%A9" -NewName "ERP"
     ```
     *(Ajustez le nom source si Windows affiche plutôt `ERP simplifié`.)*
5. Entrez dans le dossier du projet :
   ```bash
   cd ERP
   ```
6. Vérifiez que vous voyez bien les dossiers `frontend`, `backend`, et les fichiers `docker-compose.yaml` et `docker-compose.override.yaml` à la racine.

---

## Étape 2 — Démarrer tout l’environnement avec Docker

1. **Toujours à la racine du projet** (là où se trouve `docker-compose.yaml`), exécutez :
   ```bash
   docker compose up -d --build
   ```
   - La première fois, le téléchargement des images et la compilation peuvent prendre **plusieurs minutes** : c’est normal.
   - L’option `-d` lance les services en arrière-plan pour libérer le terminal.

2. Vérifiez que les conteneurs tournent :
   ```bash
   docker compose ps
   ```
   Vous devriez voir des services du type `db`, `backend`, `frontend`, et souvent `pgadmin` (fichier `docker-compose.override.yaml`).

---

## Étape 3 — Où ouvrir l’application dans le navigateur

Une fois les conteneurs démarrés :

| Ressource | Adresse dans le navigateur |
|-----------|----------------------------|
| **Application web (React)** | [http://localhost:3000](http://localhost:3000) |
| **API (Swagger)** | [http://localhost:8080/swagger](http://localhost:8080/swagger) |
| **pgAdmin** (optionnel, administration PostgreSQL) | [http://localhost:5050](http://localhost:5050) |

**Identifiants pgAdmin** (tels que configurés dans le projet) :

- Courriel : `admin@crystal.com`  
- Mot de passe : `Pizzapizza123`  

Pour ajouter le serveur PostgreSQL dans pgAdmin, utilisez l’hôte **`db`** si vous configurez depuis un autre conteneur ; depuis votre PC, utilisez **`localhost`** et le port **5432** (utilisateur / base / mot de passe : voir `docker-compose.yaml`).

---

## Étape 4 — Arrêter l’environnement

À la **racine du projet** :

```bash
docker compose down
```

Les données PostgreSQL sont en général conservées grâce au volume Docker défini dans la composition ; un prochain `docker compose up -d` retrouve la base.

---

## En cas de problème

- **« Docker n’est pas reconnu »** : Docker Desktop n’est pas installé ou pas démarré. Lancez Docker Desktop et attendez qu’il soit prêt, puis rouvrez le terminal.
- **Port déjà utilisé** (3000, 8080, 5432 ou 5050) : une autre application utilise ce port. Fermez l’autre programme ou modifiez temporairement le mappage de ports dans `docker-compose.yaml` (réservé aux utilisateurs à l’aise avec Docker).
- **Le site s’affiche mais la connexion échoue** : vérifiez que le conteneur `backend` est bien « Up » (`docker compose ps`) et ouvrez Swagger sur le port 8080 pour confirmer que l’API répond.

---

## Fichiers utiles pour comprendre la stack

- `docker-compose.yaml` — services principaux (PostgreSQL, API, frontend).  
- `docker-compose.override.yaml` — pgAdmin et variables d’environnement de développement pour le backend.  
- `frontend/src/api/apiBaseUrl.ts` — URL de base de l’API telle que le navigateur l’appelle (**localhost:8080** en configuration Docker actuelle).  
- `backend/Crystal.API/appsettings.json` — chaîne de connexion par défaut quand on lance l’API **sans** Docker (PostgreSQL sur `localhost:5432`).

---

*Document préparé pour faciliter l’évaluation du projet ERP Crystal.*
