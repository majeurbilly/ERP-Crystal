# 🐳 Guide d'infrastructure Docker (Équipe String.Empty)

Pour garantir que notre code fonctionne exactement de la même manière sur nos PC et en production, nous utilisons Docker. 

Oubliez les commandes manuelles compliquées : l'environnement local a été entièrement automatisé pour vous permettre de coder sans vous soucier de l'infrastructure !

---

## 🚀 1. Développer en local (Le Quotidien)

Nous utilisons maintenant **Docker Compose**. Un seul fichier lance le Frontend, le Backend, la Base de données et nos outils de développement simultanément.

### Comment lancer le projet complet :
1. Ouvre ton terminal à la **racine du projet** (pas dans `/backend` ni `/frontend`).
2. Lance la commande magique :
   ```bash
   docker-compose up -d --build
   // apres tu peux faire la commande de base
   docker compose up -d
   ```
*(Le `-d` permet de lancer les conteneurs en arrière-plan pour que tu puisses continuer à utiliser ton terminal).*

### 📍 Vos points d'accès locaux :
Une fois la commande lancée, voici où trouver vos outils :

| Service | URL locale | Notes |
| :--- | :--- | :--- |
| **💻 Frontend (Will)** | `http://localhost:3000` | Interface utilisateur (React/Vite). |
| **⚙️ Backend (Anto)** | `http://localhost:8080` | L'API de l'ERP. |
| **📖 Documentation API** | `http://localhost:8080/swagger` | Pour voir et tester les routes d'Anto en direct. |
| **🗄️ Base de données** | `localhost:5432` | Accessible via DataGrip ou pgAdmin. |

### 🛠️ L'outil pgAdmin (Visualisation de la BD)
Pour vous éviter de tout faire en ligne de commande, une interface web pour la base de données s'installe automatiquement avec le projet.
* **URL :** `http://localhost:5050`
* **Utilisateur :** `admin@crystal.com`
* **Mot de passe :** `Pizzapizza123`
*(Pour connecter le serveur la première fois dans l'interface, utilisez `db` comme nom d'hôte / Host name).*


## 📤 2. Les Scripts d'Automatisation (DevOps)

Quand vous avez terminé une grosse fonctionnalité et que les images doivent être mises à jour sur notre registre (Docker Hub), n'utilisez pas de commandes manuelles. 

**Prérequis Windows :** Ces scripts doivent **obligatoirement** être exécutés dans **Git Bash**.

À la racine du projet, vous avez accès à 3 scripts selon vos besoins :

* `./build.sh` : Construit uniquement les images localement sur votre PC.
* `./push.sh` : Envoie les images déjà construites vers le Docker Hub de l'équipe.
* `./run-all.sh` : **[Recommandé]** Fait les deux étapes l'une après l'autre de façon 100% automatisée.

*(Avant de pousser, assurez-vous d'être connecté à Docker Hub en tapant `docker login`).*

---

## ⚠️ Notes techniques initiales (À lire une fois)

### Pour Will (Frontend - React / Vite)
Si c'est la toute première fois que tu clones le projet sur ton PC, tu dois générer le squelette Vite autour du Dockerfile existant avant que Docker puisse le lire.
1. Va dans le dossier `/frontend` et lance : `pnpm create vite . --template react-ts` *(Choisis "Ignore files and continue" si averti).*
2. Lance `pnpm install` pour générer le `pnpm-lock.yaml`.
3. Tu es prêt à utiliser le `docker compose up -d` !

### Pour arrêter l'environnement
Quand vous avez fini de travailler, à la racine du projet, tapez :
```bash
docker compose down
```
Cela éteint proprement tous les conteneurs de l'ERP. Vos données dans la base de données seront conservées pour la prochaine fois grâce aux volumes Docker !
```