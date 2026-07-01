# Manifestes Kubernetes — Crystal ERP

Déploiement de la stack Crystal ERP (PostgreSQL, API .NET 9, frontend React/nginx) sur Kubernetes, compatible ArgoCD GitOps.

## Architecture

```
Ingress (crystal.local)
    ├── /api, /images  ──► crystal-backend (:8080) ──► crystal-postgres (:5432)
    └── /              ──► crystal-frontend  (:80)

Job PreSync (ArgoCD) : crystal-migration ──► dotnet Crystal.API.dll --migrate
```

Le frontend utilise des **URLs relatives** (`/api`, `/images`). L'Ingress route directement le trafic API et statique vers le backend, sans problème CORS.

## Prérequis

- Cluster Kubernetes (minikube, kind, AKS, GKE, etc.)
- `kubectl` et `kustomize` (intégré à `kubectl apply -k`)
- Contrôleur Ingress NGINX installé sur le cluster
- Images Docker disponibles :
  - `majeurbilly/crystal-backend:latest`
  - `majeurbilly/crystal-frontend:latest`

## Déploiement manuel (kubectl)

### 1. Créer les secrets

```bash
cp k8s/secrets.example.yaml k8s/secrets.yaml
# Éditer secrets.yaml avec des valeurs de production
kubectl apply -f k8s/secrets.yaml
```

> **Important :** ne jamais committer `secrets.yaml`. Le fichier est ignoré par `.gitignore`.

### 2. Appliquer les manifestes

```bash
kubectl apply -k k8s/
```

Le Job `crystal-migration` exécute `dotnet Crystal.API.dll --migrate` avant le démarrage de l'API.

### 3. Accéder à l'application

Ajouter l'hôte local (ou adapter `ingress.yaml`) :

```
127.0.0.1  crystal.local
```

```bash
kubectl get ingress -n crystal-erp
```

## Déploiement GitOps (ArgoCD)

ArgoCD détecte nativement le répertoire Kustomize `k8s/`.

### 1. Enregistrer l'application

```bash
kubectl apply -f k8s/argocd/application.yaml
```

### 2. Configurer les secrets (hors Git)

Les secrets ne sont pas versionnés. Les créer manuellement sur le cluster :

```bash
kubectl apply -f k8s/secrets.yaml
```

Ou utiliser [Sealed Secrets](https://github.com/bitnami-labs/sealed-secrets) / [External Secrets Operator](https://external-secrets.io/) pour une gestion GitOps des secrets.

### 3. Synchronisation

Une fois les fichiers poussés sur `main` et les secrets configurés, l'application ArgoCD passe à **Synced** / **Healthy**.

| Paramètre ArgoCD | Valeur |
|------------------|--------|
| **Repo URL** | `https://github.com/majeurbilly/ERP-Crystal.git` |
| **Path** | `k8s` |
| **Tool** | Kustomize (auto-détecté) |

Le Job `crystal-migration` est annoté `argocd.argoproj.io/hook: PreSync` : les migrations EF s'exécutent automatiquement à chaque synchronisation, avant le déploiement du backend.

## Fichiers

| Fichier | Rôle |
|---------|------|
| `namespace.yaml` | Namespace `crystal-erp` |
| `secrets.example.yaml` | Modèle de secrets (DB + JWT) |
| `configmap-nginx.yaml` | Configuration nginx du frontend (fallback Docker Compose) |
| `postgres-statefulset.yaml` | PostgreSQL 18 avec PVC |
| `postgres-service.yaml` | Service headless PostgreSQL |
| `backend-deployment.yaml` | API ASP.NET Core |
| `backend-service.yaml` | Service ClusterIP backend |
| `backend-pvc.yaml` | Stockage persistant des images uploadées |
| `frontend-deployment.yaml` | SPA React servie par nginx |
| `frontend-service.yaml` | Service ClusterIP frontend |
| `ingress.yaml` | Routage `/api`, `/images` → backend ; `/` → frontend |
| `migration-job.yaml` | Migrations EF Core (`--migrate`) avec hook ArgoCD PreSync |
| `kustomization.yaml` | Assemblage Kustomize + remplacement d'images |
| `argocd/application.yaml` | Déclaration Application ArgoCD |

## Personnalisation

- **Images :** modifier la section `images` dans `kustomization.yaml`
- **Hostname :** adapter `host` dans `ingress.yaml`
- **Stockage :** ajuster les tailles des PVC (`postgres-statefulset.yaml`, `backend-pvc.yaml`)
- **Réplicas :** augmenter `replicas` dans les Deployments (le backend nécessite un stockage partagé ou ReadWriteMany pour plusieurs pods)

## Points d'attention

1. **Secrets JWT/DB** : utiliser des valeurs fortes, distinctes de la configuration de développement.
2. **Seed de données** : le peuplement initial (`DataSeeder`) ne s'exécute qu'en `Development`.
3. **Rebuild des images** : après modification du backend (`--migrate`) ou du frontend (URLs relatives), reconstruire et pousser les images Docker.

## Vérification

```bash
kubectl get all -n crystal-erp
kubectl logs job/crystal-migration -n crystal-erp
kubectl logs -l app.kubernetes.io/name=crystal-backend -n crystal-erp
kubectl logs -l app.kubernetes.io/name=crystal-frontend -n crystal-erp
```
