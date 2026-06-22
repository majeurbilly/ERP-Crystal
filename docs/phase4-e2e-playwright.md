# Phase 4 — Tests E2E Playwright

**Statut :** Terminée  
**Épic :** E5 (Sprint 4)  
**Date :** Juin 2026

---

## Objectif

Quatre suites de tests Playwright couvrent les **happy paths** par rôle (Admin, Gérant, Assistant, Employé) contre la stack réelle (`USE_MOCK_API = false`, Docker Compose).

---

## Structure

```
e2e/
  playwright.config.ts       # Config principale
  fixtures/
    auth.ts                  # loginAs(), comptes seed
    api.ts                   # Helpers API (congés, profil)
    auth.setup.ts            # Génère storageState par rôle
    storage/                 # États de session (gitignored)
  shared/
    login.spec.ts            # Connexion / erreurs
    navigation.spec.ts       # Menu RH admin
  admin/                     # A1–A4
  gerant/                    # G1–G4
  assistant/                 # AS1–AS4
  employee/                  # E1–E5
```

---

## Prérequis

1. **Docker Compose** en cours d'exécution :

```bash
docker compose up -d --build
```

2. Frontend accessible sur `http://localhost:3000`, API sur `http://localhost:8080`.

3. Comptes seed (section 12 de `docs/sprint4.md`) :

| Rôle | Courriel | Mot de passe |
|------|----------|--------------|
| Admin | `admin@crystal.local` | `ValidPass1!a` |
| Gérant | `gerant@crystal.local` | `ValidPass1!a` |
| Assistant | `assistant@crystal.local` | `ValidPass1!a` |
| Employé | `employee@crystal.local` | `ValidPass1!a` |

---

## Exécution locale

```bash
# À la racine du dépôt
npm install
npm run e2e:install    # navigateur Chromium (première fois)
npm run e2e            # lance toute la suite

# Interface graphique
npm run e2e:ui
```

Variables optionnelles :

| Variable | Défaut | Description |
|----------|--------|-------------|
| `E2E_BASE_URL` | `http://localhost:3000` | URL du frontend |
| `E2E_API_URL` | `http://localhost:8080` | URL de l'API |

---

## Scénarios couverts

### Admin (4)

| ID | Fichier | Scénario |
|----|---------|----------|
| A1 | `admin/dashboard.spec.ts` | Dashboard + widgets admin / métriques RH |
| A2 | `admin/roles-crud.spec.ts` | Créer un rôle depuis preset « Employé » |
| A3 | `admin/users.spec.ts` | Créer un utilisateur |
| A4 | `shared/navigation.spec.ts` | Menu RH complet |

### Gérant (4)

| ID | Fichier | Scénario |
|----|---------|----------|
| G1 | `gerant/dashboard.spec.ts` | Métriques RH sur le dashboard |
| G2 | `gerant/leave-approval.spec.ts` | Approuver un congé en attente |
| G3 | `gerant/employee-management.spec.ts` | Ouvrir une fiche employé |
| G4 | `gerant/employee-management.spec.ts` | Consulter le planning |

### Assistant (4)

| ID | Fichier | Scénario |
|----|---------|----------|
| AS1 | `assistant/dashboard.spec.ts` | Dashboard assistant |
| AS2 | `assistant/leave-request.spec.ts` | Créer un congé pour soi |
| AS3 | `assistant/inventory-read.spec.ts` | Consulter le catalogue (lecture inventaire) |
| AS4 | `assistant/inventory-read.spec.ts` | Refus paie / utilisateurs |

### Employé (5)

| ID | Fichier | Scénario |
|----|---------|----------|
| E1 | `employee/mon-espace.spec.ts` | Dashboard + lien Mon espace |
| E2 | `employee/schedule.spec.ts` | Onglet horaire / widget prochain quart |
| E3 | `employee/leave-request.spec.ts` | Créer un congé « En attente » |
| E4 | `employee/mon-espace.spec.ts` | Pas de menu RH gestionnaire |
| E5 | `employee/mon-espace.spec.ts` | Redirection depuis `/rh/employes` |

---

## CI (Azure DevOps)

Pipeline : [`.azure-pipelines/azure-pipelines-e2e.yml`](../.azure-pipelines/azure-pipelines-e2e.yml)

1. `docker compose up -d --wait`
2. `npm run e2e` avec `CI=true` (retries ×2, 1 worker)
3. Publication du rapport HTML `playwright-report`

---

## Bonnes pratiques

- **storageState** : généré une fois par `auth.setup.ts` pour éviter un login par test.
- **Données uniques** : emails utilisateur et dates de congé basés sur `Date.now()` pour limiter les collisions.
- **API helpers** : `fixtures/api.ts` prépare les congés en attente avant les tests gérant (G2).

Les tests E2E **ne remplacent pas** les tests xUnit ni Vitest — ils valident les parcours utilisateur de bout en bout.

---

## Références

- [`docs/sprint4.md`](sprint4.md) — Épic 5
- [`docs/phase3-mon-espace.md`](phase3-mon-espace.md) — Portail employé testé en E1–E5
