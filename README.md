# ERP Crystal

Application web de gestion pour la **Librairie Crystal** : inventaire multi-succursales et ressources humaines (horaires, congés, feuilles de temps, paie). Projet réalisé par l'équipe **String.Empty**.

La méthode recommandée pour lancer le projet est **Docker** : base de données, API et interface démarrent ensemble avec une seule commande.

---

## Fonctionnalités

### Inventaire

- Catalogue d'articles (livres et produits)
- Catégories et auteurs
- Succursales et stocks par emplacement
- Rapport d'inventaire (IR)

### Ressources humaines

- Tableau de bord RH et métriques
- Utilisateurs et rôles dynamiques (permissions granulaires)
- Profils employés et postes
- Contrats d'emploi
- Demandes de congé
- Planification des quarts de travail
- Saisie des heures et feuilles de temps
- Paie
- Espace personnel (« Mon espace ») pour les employés

### Sécurité

- Authentification JWT
- Contrôle d'accès par rôle (Admin, Gérant, Assistant, Employé) avec permissions CASL côté frontend et backend

---

## Stack technique


| Couche              | Technologies                                                         |
| ------------------- | -------------------------------------------------------------------- |
| **Frontend**        | React 19, TypeScript, Vite, MUI, React Query, CASL, Vitest           |
| **Backend**         | ASP.NET Core 9, Entity Framework Core, PostgreSQL, Identity, Swagger |
| **Infrastructure**  | Docker, Docker Compose, Nginx (reverse proxy), Azure Pipelines       |
| **Base de données** | PostgreSQL 18                                                        |


---

## Structure du projet

```
ERP-r/
├── frontend/          # Interface React (Vite)
├── backend/           # API .NET (Crystal.API, Core, Infrastructure, IntegrationTests)
├── docker-compose.yaml
├── docker-compose.override.yaml   # pgAdmin + variables de dev
├── docker-compose.prod.yaml       # déploiement production
├── build-image.sh                 # build des images Docker
├── push-image.sh                  # push vers Docker Hub
└── start.sh                       # réinitialisation complète (migrations + build)
```

---

## Prérequis

1. **Git** — [git-scm.com](https://git-scm.com)
2. **Docker Desktop** — [docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop)

Vérifications :

```bash
git --version
docker --version
docker compose version
```

> **Sans Docker**, il est possible de lancer le backend avec le SDK .NET 9 et le frontend avec Node/pnpm, mais il faut installer **PostgreSQL** localement et aligner les URLs dans `frontend/src/api/apiBaseUrl.ts`. Pour une prise en main rapide, **Docker reste la voie la plus simple**.

---

## Démarrage rapide (Docker)

### 1. Cloner le dépôt

```bash
git clone <URL_DU_DEPOT>
mv ERP%20simplifi%C3%A9 ERP
cd ERP
```

**Conseil :** si Git crée un dossier avec des caractères encodés dans le nom (ex. `ERP%20simplifi%C3%A9`), renommez-le en un nom court sans espaces (ex. `ERP`) pour éviter des problèmes de chemins.

À la racine, vous devez voir les dossiers `frontend`, `backend` et les fichiers `docker-compose.yaml` / `docker-compose.override.yaml`.

### 2. Lancer l'environnement

À la **racine du projet** :

```bash
docker compose up -d --build
```

La première exécution peut prendre plusieurs minutes (téléchargement des images et compilation). Les fois suivantes :

```bash
docker compose up -d
```

### 3. Vérifier que tout tourne

```bash
docker compose ps
```

Services attendus : `db`, `backend`, `frontend`, `pgadmin`.

---

## Points d'accès


| Service             | URL                                                            | Notes                                         |
| ------------------- | -------------------------------------------------------------- | --------------------------------------------- |
| **Application web** | [http://localhost:3000](http://localhost:3000)                 | Interface React servie par Nginx              |
| **API (Swagger)**   | [http://localhost:8080/swagger](http://localhost:8080/swagger) | Documentation et tests des routes             |
| **PostgreSQL**      | `localhost:5433`                                               | Port mappé depuis le conteneur (5432 interne) |
| **pgAdmin**         | [http://localhost:5050](http://localhost:5050)                 | Administration visuelle de la BD              |


Le frontend appelle l'API via le proxy Nginx (`/api` → backend). Swagger et les tests directs utilisent le port **8080**.

### pgAdmin

- **Courriel :** `admin@crystal.com`
- **Mot de passe :** `Pizzapizza123`

Pour connecter le serveur PostgreSQL dans pgAdmin :

- **Hôte :** `db` (depuis pgAdmin dans Docker) ou `localhost` (depuis votre PC)
- **Port :** `5432` (conteneur) ou `5433` (depuis votre PC)
- **Utilisateur / base / mot de passe :** voir `docker-compose.yaml`

---

## Comptes de démonstration

Des comptes sont créés automatiquement au démarrage du backend :


| Rôle      | Courriel                  | Mot de passe   |
| --------- | ------------------------- | -------------- |
| Admin     | `admin@crystal.local`     | `ValidPass1!a` |
| Gérant    | `gerant@crystal.local`    | `ValidPass1!a` |
| Assistant | `assistant@crystal.local` | `ValidPass1!a` |
| Employé   | `employee@crystal.local`  | `ValidPass1!a` |


### Tester l'API via Swagger

1. Ouvrir [http://localhost:8080/swagger](http://localhost:8080/swagger)
2. Appeler `POST /api/auth/login` avec :
  ```json
   {
     "email": "admin@crystal.local",
     "password": "ValidPass1!a"
   }
  ```
3. Copier le token JWT retourné
4. Cliquer sur **Authorize** (en haut à droite)
5. Entrer : `Bearer <votre_token>`

---

## Arrêter l'environnement

À la racine du projet :

```bash
docker compose down
```

Les données PostgreSQL sont conservées grâce au volume Docker ; un prochain `docker compose up -d` retrouve la base.

Pour tout supprimer (y compris les volumes) :

```bash
docker compose down -v
```

---

## Scripts DevOps

**Prérequis Windows :** exécuter ces scripts dans **Git Bash**.


| Script             | Description                                                              |
| ------------------ | ------------------------------------------------------------------------ |
| `./build-image.sh` | Construit les images backend et frontend localement                      |
| `./push-image.sh`  | Pousse les images vers Docker Hub (`majeurbilly/crystal-`*)              |
| `./start.sh`       | Réinitialisation complète : migrations EF, build sans cache, redémarrage |


Avant de pousser les images : `docker login`

Pour la production, voir `docker-compose.prod.yaml` (variables d'environnement `DB_USER`, `DB_PASSWORD`, `DB_NAME`, `JWT_SECRET`).

---

## Développement sans Docker (optionnel)

### Backend

```bash
cd backend
dotnet restore Crystal.sln
dotnet run --project Crystal.API
```

Configuration par défaut : `backend/Crystal.API/appsettings.json` (PostgreSQL sur `localhost`).

### Frontend

```bash
cd frontend
pnpm install
pnpm dev
```

Ajuster `frontend/src/api/apiBaseUrl.ts` pour pointer vers l'API locale (ex. `http://localhost:8080`).

### Première installation du frontend (squelette Vite)

Si le dossier `frontend` n'a pas encore été initialisé :

```bash
cd frontend
pnpm create vite . --template react-ts
pnpm install
```

---

## Tests

### Backend (intégration)

```bash
cd backend
dotnet test Crystal.sln
```

### Frontend (Vitest)

```bash
cd frontend
pnpm test
```

Les pipelines CI sont définis dans `.azure-pipelines/`.

---

## Dépannage


| Problème                                   | Solution                                                                                   |
| ------------------------------------------ | ------------------------------------------------------------------------------------------ |
| `docker` non reconnu                       | Installer ou démarrer Docker Desktop, puis rouvrir le terminal                             |
| Port déjà utilisé (3000, 8080, 5433, 5050) | Fermer l'autre application ou modifier le mappage dans `docker-compose.yaml`               |
| Connexion échoue dans l'app                | Vérifier que `backend` est « Up » (`docker compose ps`) et tester Swagger sur le port 8080 |
| Logs des conteneurs                        | `docker compose logs -f` ou `docker compose logs backend`                                  |


---

## Fichiers utiles


| Fichier                                | Rôle                                            |
| -------------------------------------- | ----------------------------------------------- |
| `docker-compose.yaml`                  | Services principaux (PostgreSQL, API, frontend) |
| `docker-compose.override.yaml`         | pgAdmin et environnement de développement       |
| `frontend/nginx.conf`                  | Proxy `/api` et `/images` vers le backend       |
| `frontend/src/api/apiBaseUrl.ts`       | URL de base de l'API côté navigateur            |
| `backend/Crystal.API/appsettings.json` | Chaîne de connexion hors Docker                 |


---

*Projet ERP Crystal — équipe String.Empty*