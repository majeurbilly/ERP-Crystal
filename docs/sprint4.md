# Itération 4 — Sprint finale

**Projet :** ERP Crystal  
**Objectif :** Livrer un ERP fonctionnel de bout en bout — backend et frontend alignés, permissions unifiées, portail employé, dashboard par rôle, et tests E2E Playwright solides sur les happy paths.

---

## Table des matières

1. [Contexte et état des lieux](#1-contexte-et-état-des-lieux)
2. [Vision du sprint](#2-vision-du-sprint)
3. [Décisions à trancher](#3-décisions-à-trancher)
4. [Épics et phases](#4-épics-et-phases)
5. [Épic 1 — Alignement backend / frontend](#5-épic-1--alignement-backend--frontend)
6. [Épic 2 — Rôles et permissions dynamiques](#6-épic-2--rôles-et-permissions-dynamiques)
7. [Épic 3 — Portail employé (Mon espace)](#7-épic-3--portail-employé-mon-espace)
8. [Épic 4 — Dashboard et widgets](#8-épic-4--dashboard-et-widgets)
9. [Épic 5 — Tests E2E Playwright](#9-épic-5--tests-e2e-playwright)
10. [Hors scope / backlog](#10-hors-scope--backlog)
11. [Matrice d'alignement API ↔ UI](#11-matrice-dalignement-api--ui)
12. [Comptes de test](#12-comptes-de-test)
13. [Définition of Done](#13-définition-of-done)
14. [Risques](#14-risques)

---

## 1. Contexte et état des lieux

### Stack

| Zone | Technologie |
|------|-------------|
| Backend | ASP.NET Core 9, Identity + JWT, EF Core, PostgreSQL |
| Frontend | React 19, TypeScript, Vite, MUI, TanStack Query, CASL |
| Infra | Docker Compose (`localhost:3000` / `localhost:8080`) |

### Ce qui fonctionne déjà

- Module RH backend riche : employés, contrats, congés, planning, pointages, feuilles de temps, paie, métriques.
- Module inventaire : catalogue, livres, catégories, succursales, quantités.
- Tests backend : unitaires + intégration (18 domaines).
- Tests frontend : Vitest + Testing Library (composants et pages RH).
- 4 rôles Identity : `Admin`, `Gerant`, `Assistant`, `Employee` (comptes seedés).

### Problèmes bloquants (résolus en itération 4)

| Problème | Statut |
|----------|--------|
| `USE_MOCK_API: true` | ✅ `false` — API .NET seule (Épic 1) |
| Permissions hybrides Identity vs mock | ✅ Permissions dynamiques API + CASL (Épic 2) |
| Dashboard stub | ✅ Widgets par rôle (Épic 4) |
| Pas de portail employé | ✅ `/mon-espace` (Épic 3) |
| Scoping employé absent | ✅ `EmployeeScopeService` (Épic 3) |
| Pas de tests E2E | ✅ Playwright 27 scénarios (Épic 5) |

### Deux concepts de « rôle » à ne pas confondre

| Concept | Exemple | Où c'est défini |
|---------|---------|-----------------|
| **Rôle applicatif** (sécurité) | Admin, Gérant, Assistant, Employé | `ApplicationRoles.cs`, JWT |
| **Rôle organisationnel RH** (métier) | Ventes, Logistique | `OrganizationalRole`, `/rh/referentiels/roles` |

Le sprint 4 concerne les **rôles applicatifs dynamiques** (permissions configurables), pas les rôles organisationnels RH.

---

## 2. Vision du sprint

À la fin de l'itération 4, un utilisateur peut :

1. Se connecter et voir un **dashboard adapté à son rôle**.
2. Accéder uniquement aux **menus et actions** autorisés par ses permissions.
3. En tant qu'**employé**, consulter son horaire, ses congés et sa fiche RH via **`/mon-espace`**.
4. En tant qu'**admin**, créer un **rôle personnalisé** (preset ou from scratch) et l'assigner à un utilisateur.
5. L'équipe dispose de **tests E2E Playwright** par rôle couvrant les happy paths critiques, exécutables en CI sur Docker Compose.

### Ordre d'exécution recommandé

```
Phase 1 — Alignement BE/FE
    ↓
Phase 2 — Permissions unifiées (API + UI)
    ↓
Phase 3 — Portail employé + Dashboard widgets
    ↓
Phase 4 — Tests E2E Playwright
```

Les E2E viennent **en dernier** : tester des mocks ne garantit rien.

---

## 3. Décisions à trancher

À valider en kickoff avant de coder.

| # | Question | Recommandation sprint finale |
|---|----------|------------------------------|
| D1 | Permissions par **entité CRUD** ou par **champ** (ex. masquer salaire) ? | Entité + CRUD (MVP). Champ par champ = optionnel sur 2–3 entités sensibles seulement. |
| D2 | Module Opérations (transferts / réceptions) ? | **Hors scope** — documenté dans README frontend mais non implémenté. |
| D3 | Bulletins de paie visibles par l'employé ? | À trancher (recommandé : lecture seule de **ses** bulletins). |
| D4 | Rôles dynamiques remplacent-ils Identity ? | Non — presets Identity + permissions dynamiques qui s'ajoutent. |
| D5 | Suppression complète de `USE_MOCK_API` et json-server ? | Oui, objectif fin de sprint. |
| D6 | API Auteurs ? | Aligner (créer endpoint) ou retirer du menu pour la finale. |

---

## 4. Épics et phases

| Épic | Titre | Priorité | Phase |
|------|-------|----------|-------|
| E1 | Alignement backend / frontend | P0 | 1 |
| E2 | Rôles et permissions dynamiques | P0 | 2 |
| E3 | Portail employé (Mon espace) | P1 | 3 |
| E4 | Dashboard et widgets | P1 | 3 |
| E5 | Tests E2E Playwright | P0 | 4 |

---

## 5. Épic 1 — Alignement backend / frontend

### Objectif

Une seule source de vérité : l'API .NET sur `localhost:8080`. Matrice documentée, écarts de modèle résolus.

### User stories

| ID | Story | Critères d'acceptation |
|----|-------|------------------------|
| E1-US1 | En tant que dev, je bascule tous les services métier critiques vers l'API réelle | `USE_MOCK_API = false` ; plus de dépendance json-server pour les flux RH et inventaire |
| E1-US2 | En tant que dev, j'ai une matrice API ↔ page à jour | Document section 11 complété ; chaque page a un endpoint validé |
| E1-US3 | En tant que dev, les DTO backend et frontend sont alignés | `ScheduledShift` : décision prise sur `locationId` (ajout backend ou retrait frontend) |
| E1-US4 | En tant qu'utilisateur connecté, mon profil employé vient de l'API | `AuthContext` utilise `GET /api/employee-profiles/me`, plus de mock hardcodé id `"101"` |

### Tâches techniques

- [x] Mettre `USE_MOCK_API` à `false` dans `frontend/src/api/apiBaseUrl.ts`
- [x] Migrer `employeeProfileService`, `jobPositionService`, `scheduledShiftService` vers l'API
- [x] Corriger le chargement du profil employé dans `AuthContext` via `GET /api/employee-profiles/me`
- [x] Résoudre l'écart `ScheduledShift` : `locationId` UI seulement ; succursale via `EmployeeProfile.locationId` (backend)
- [x] API Auteurs créée (`/api/authors`) + `authorService` branché
- [x] json-server retiré du workflow dev (presets locaux pour rôles/permissions)
- [x] Docker Compose suffit seul (`docker compose up -d`)

### Livrable

- [x] Matrice complétée → [`docs/phase1-alignement.md`](phase1-alignement.md)
- [x] Tests : `Phase1AlignmentIntegrationTests`, `AuthorsIntegrationTests`, Vitest `src/data/test/phase1/`

---

## 6. Épic 2 — Rôles et permissions dynamiques

### Objectif

Un admin peut créer, modifier et assigner des rôles avec des permissions par entité CRUD. Les 4 presets (Admin, Gérant, Assistant, Employé) servent de point de départ. Le backend **et** le frontend appliquent les mêmes règles.

### Modèle de permissions (niveau MVP)

Granularité : **entité + action CRUD** (déjà défini dans `frontend/src/permissions/permissions.ts`).

```
Exemples :
  read:hr_dashboard
  manage:employee_profile
  create:leave_request
  read:scheduled_shift
  manage:all          → Admin
```

Les permissions par **champ** (ex. masquer `salary` sur `employee_profile`) sont **hors scope MVP** sauf décision D1.

### User stories

| ID | Story | Critères d'acceptation |
|----|-------|------------------------|
| E2-US1 | En tant qu'admin, je crée un rôle dynamique avec un nom et des permissions | Formulaire fonctionnel (plus de TODO dans `UserRoleListPage`) ; persistance en base |
| E2-US2 | En tant qu'admin, je modifie les permissions d'un rôle existant | Édition depuis liste et page détail (`UserRoleDetailsPage`) |
| E2-US3 | En tant qu'admin, je supprime un rôle non assigné | Suppression avec confirmation ; impossible de supprimer son propre rôle |
| E2-US4 | En tant qu'admin, je pars d'un preset Admin/Gérant/Assistant/Employé | 4 boutons « Créer depuis preset » pré-remplissent les permissions |
| E2-US5 | En tant qu'admin, j'assigne un rôle dynamique à un utilisateur | Champ rôle sur fiche utilisateur ; reflété au prochain login |
| E2-US6 | En tant qu'utilisateur, le menu et les boutons respectent mes permissions | Sidebar, routes, boutons CRUD masqués selon CASL |
| E2-US7 | En tant que système, le backend refuse les actions non autorisées | Policy handler ou middleware ; tests d'intégration par rôle |

### Travail backend (à créer)

- [x] Entités : `DynamicRole`, `Permission` (action + subject), lien `ApplicationUser` ↔ `DynamicRole`
- [x] API CRUD : `api/roles`, `api/permission-entities`, assignation utilisateur (`dynamicRoleId`)
- [x] Endpoint `GET /api/users/me/permissions` (permissions effectives au login)
- [x] Filtre `RequirePermission` ASP.NET Core (remplace `[Authorize(Roles)]` sur endpoints sensibles)
- [x] Seed des 4 presets avec permissions documentées
- [x] Tests d'intégration : Employé ne peut pas `POST /api/employee-profiles` si pas `create:employee_profile`

### Travail frontend

- [x] Brancher `userRoleService`, `permissionEntityService` et `permissionService` sur l'API .NET
- [x] Compléter formulaires add/edit dans `UserRoleListPage` et `UserRoleDetailsPage` (`UserRoleForm`)
- [x] UI presets : 4 boutons rapides Admin / Gérant / Assistant / Employé
- [x] Charger permissions depuis l'API au login (`AppPermissionContext` + `AuthContext`)
- [x] Assignation rôle dynamique sur fiche utilisateur (`UserForm` + `dynamicRoleId`)

### Livrable

- [x] Documentation → [`docs/phase2-permissions.md`](phase2-permissions.md)
- [x] Tests : `Phase2PermissionsIntegrationTests`, `PermissionServiceTests`, Vitest `src/data/test/phase2/`

### Matrice permissions presets (proposition)

| Entité | Admin | Gérant | Assistant | Employé |
|--------|-------|--------|-----------|---------|
| `all` | manage | — | — | — |
| `hr_dashboard` | read | read | read | — |
| `employee_profile` | manage | manage | read | read (soi) |
| `leave_request` | manage | manage | create+read | create+read (soi) |
| `scheduled_shift` | manage | manage | read | read (soi) |
| `time_entry` | manage | manage | read+create | read+create (soi) |
| `timesheet` | manage | manage | read | read (soi) |
| `payroll` | manage | manage | — | read (soi, si D3) |
| `employment_contract` | manage | manage | read | — |
| `item` / `inventory_quantity` | manage | read+update | read | read |
| `location` | manage | read+update | read | read |
| `user_role` | manage | — | — | — |

> « soi » = scoping backend (Épic 3) — l'employé ne voit que ses propres enregistrements.

---

## 7. Épic 3 — Portail employé (Mon espace)

### Objectif

Les employés (et assistants selon permissions) accèdent à **leurs** informations RH sans passer par le menu gestionnaire `/rh`.

### Route proposée

**`/mon-espace`** — portail self-service (distinct de `/monprofil` qui gère le compte Identity).

### User stories

| ID | Story | Critères d'acceptation |
|----|-------|------------------------|
| E3-US1 | En tant qu'employé, je vois mon prochain quart de travail | Widget/page affiche le prochain `ScheduledShift` lié à mon profil |
| E3-US2 | En tant qu'employé, je consulte mon horaire complet | Calendrier filtré sur mes quarts uniquement |
| E3-US3 | En tant qu'employé, je crée une demande de congé | Formulaire `LeaveRequestForm` ; statut initial `Pending` |
| E3-US4 | En tant qu'employé, je suis l'état de mes demandes de congé | Liste filtrée ; pas de données des collègues |
| E3-US5 | En tant qu'employé, je consulte ma fiche RH | Données via `GET /api/employee-profiles/me` ; champs sensibles masqués selon permissions |
| E3-US6 | En tant qu'employé, je consulte mes pointages | Liste filtrée sur mon `employeeProfileId` |
| E3-US7 | En tant qu'employé, je consulte ma feuille de temps | Accès lecture seule à ma feuille courante |
| E3-US8 | En tant qu'employé, je change mon mot de passe | Lien vers `/monprofil` existant |

### Travail backend (scoping)

- [x] Filtrage « mes données » sur : `leave-requests`, `schedules`, `time-entries`, `timesheets`
- [x] Query param `?mine=true` ou filtrage automatique selon rôle + absence de permission `manage`
- [x] `GET /api/employee-profiles` : Employé sans `manage:employee_profile` → 403 ou redirection vers `/me`
- [x] Tests d'intégration : employé A ne lit pas les congés de employé B

### Travail frontend

- [x] Page `MonEspacePage` avec onglets ou sections : Horaire, Congés, Ma fiche, Pointages, Feuille de temps
- [x] Entrée menu sidebar visible pour Employé / Assistant (sans `read:hr_dashboard`)
- [x] Réutiliser composants existants (`SchedulesPage`, `LeaveRequestsPage`, etc.) en mode « self »

### Livrable

- [x] Documentation → [`docs/phase3-mon-espace.md`](phase3-mon-espace.md)
- [x] Tests : `Phase3EmployeeScopingIntegrationTests`, Vitest `src/data/test/phase3/`

---

## 8. Épic 4 — Dashboard et widgets

### Objectif

Le `/dashboard` devient la tour de contrôle par rôle. Les widgets sont des **raccourcis** vers les pages de travail (règle d'or du README frontend).

### État livré

`DashboardPage.tsx` affiche `DashboardWidgetGrid` (widgets conditionnels par permissions). Les métriques RH (`HrMetricsCards`) sont aussi sur le dashboard gérant/admin.

### Widgets par rôle (minimum viable)

| Rôle | Widget | Action au clic |
|------|--------|----------------|
| **Employé** | Prochain quart | → `/mon-espace` onglet Horaire |
| **Employé** | Mes congés en attente | → `/mon-espace` onglet Congés |
| **Assistant** | Prochain quart + tâches inventaire | → `/mon-espace` ou `/ir` |
| **Gérant** | Congés à approuver (compteur) | → `/rh/absences?status=Pending` |
| **Gérant** | Feuilles de temps en attente | → `/rh/feuilles-de-temps?status=Pending` |
| **Gérant** | Employés actifs / masse salariale | → `/rh` (réutiliser `HrMetricsCards`) |
| **Gérant** | Alertes stock (si dispo) | → `/catalogue` avec filtre rupture |
| **Admin** | Tous widgets Gérant + accès rôles | → `/user-roles` |

### User stories

| ID | Story | Critères d'acceptation |
|----|-------|------------------------|
| E4-US1 | En tant qu'utilisateur, mon dashboard affiche des widgets selon mon rôle | Composition dynamique basée sur permissions |
| E4-US2 | En tant qu'utilisateur, un clic sur un widget m'amène à la page filtrée | Query params ou état navigation pré-appliqué |
| E4-US3 | En tant que gérant, je vois les métriques RH sur le dashboard principal | `HrMetricsCards` déplacé ou dupliqué sur `/dashboard` |

### Tâches techniques

- [x] Composant `DashboardWidgetGrid` avec widgets conditionnels
- [x] Composants widgets : `NextShiftWidget`, `PendingLeaveWidget`, `HrMetricsWidget`, `InventoryAlertWidget`
- [x] Endpoints ou réutilisation `api/hr/metrics` pour les compteurs
- [x] Supprimer `console.log` de debug dans `DashboardPage.tsx`

### Livrable

- [x] Documentation → [`docs/phase3-mon-espace.md`](phase3-mon-espace.md) (Épics 3 + 4)
- [x] Tests Vitest `src/data/test/phase3/`

---

## 9. Épic 5 — Tests E2E Playwright

### Objectif

4 suites de tests par rôle couvrant les happy paths critiques. Exécution en CI sur Docker Compose, `USE_MOCK_API=false`.

### Structure proposée

```
e2e/
  fixtures/
    auth.ts              # helpers login par rôle
    storage/             # storageState par rôle (admin, gerant, assistant, employee)
  shared/
    login.spec.ts
    navigation.spec.ts
  admin/
    dashboard.spec.ts
    users.spec.ts
    roles-crud.spec.ts
  gerant/
    dashboard.spec.ts
    leave-approval.spec.ts
    employee-management.spec.ts
  assistant/
    dashboard.spec.ts
    inventory-read.spec.ts
  employee/
    mon-espace.spec.ts
    leave-request.spec.ts
    schedule.spec.ts
  playwright.config.ts
```

### Scénarios happy path minimum

#### Admin

| # | Scénario |
|---|----------|
| A1 | Login → dashboard avec widgets admin |
| A2 | Créer un rôle depuis preset « Employé » → assigner à un utilisateur |
| A3 | CRUD utilisateur |
| A4 | Accès menu RH complet |

#### Gérant

| # | Scénario |
|---|----------|
| G1 | Login → dashboard avec métriques RH |
| G2 | Approuver une demande de congé en attente |
| G3 | Créer / modifier une fiche employé |
| G4 | Consulter le planning |

#### Assistant

| # | Scénario |
|---|----------|
| AS1 | Login → dashboard assistant |
| AS2 | Créer une demande de congé pour soi |
| AS3 | Consulter inventaire (lecture) |
| AS4 | Pas d'accès paie / gestion utilisateurs |

#### Employé

| # | Scénario |
|---|----------|
| E1 | Login → redirection dashboard ou `/mon-espace` |
| E2 | Voir son prochain quart |
| E3 | Créer une demande de congé → statut « En attente » |
| E4 | Pas d'accès menu RH gestionnaire |
| E5 | Pas d'accès création employé / paie |

### Configuration technique

- [x] Ajouter `@playwright/test` au frontend (ou racine monorepo)
- [x] `playwright.config.ts` : `baseURL: http://localhost:3000`, `webServer` ou prérequis Docker Compose
- [x] Fixtures : `storageState` par rôle pour éviter re-login
- [x] Pipeline Azure DevOps : job Playwright après `docker compose up`
- [x] Données seed stables (comptes section 12)

### Livrable

- [x] Documentation → [`docs/phase4-e2e-playwright.md`](phase4-e2e-playwright.md)
- [x] Suites `e2e/{admin,gerant,assistant,employee,shared}/` + `fixtures/auth.setup.ts`

### Ce que les E2E ne remplacent pas

- Tests d'intégration backend (xUnit) — à conserver
- Vitest composant — utile pour formulaires RH isolés

---

## 10. Hors scope / backlog

Explicitement **exclu** de l'itération 4 :

| Élément | Raison |
|---------|--------|
| Module Opérations (`/operations/transferts`, `/operations/receptions`) | Non implémenté, README obsolète |
| Entités `Availability`, `Client`, `Receipt` | En base, pas d'API ni UI |
| Permissions par champ (sauf décision D1) | Scope trop large |
| Notifications (congé approuvé, etc.) | Nice-to-have |
| Audit trail modifications permissions | Backlog |
| App mobile dédiée | Responsive web suffit pour la finale |

---

## 11. Matrice d'alignement API ↔ UI

À compléter pendant l'Épic 1. Statut initial :

| Domaine | Endpoint backend | Route frontend | Mock ? | Aligné ? | Notes |
|---------|------------------|----------------|--------|----------|-------|
| Auth | `api/auth/login` | `/login` | Non | ✅ | |
| Utilisateurs | `api/users`, `users/me` | `/rh/utilisateurs`, `/monprofil` | Non | ✅ | |
| Catalogue | `api/items` | `/catalogue` | Non | ✅ | |
| Livres | `api/books` | détail livre | Non | ✅ | |
| Catégories | `api/categories` | `/livres/categories` | Non | ✅ | |
| Succursales | `api/locations` | liste succursales | Non | ✅ | |
| Inventaire | `api/inventory/*` | `/ir` | Non | ✅ | |
| Employés | `api/employee-profiles` | `/rh/employes` | Non | ✅ | Phase 1 |
| Profil moi | `api/employee-profiles/me` | `AuthContext` (login) | Non | ✅ | Phase 1 |
| Postes | `api/job-positions` | `/rh/referentiels/postes` | Non | ✅ | Phase 1 |
| Rôles org. | `api/organizational-roles` | `/rh/referentiels/roles` | Non | ✅ | |
| Contrats | `api/contracts` | `/rh/contrats-de-travail` | Non | ✅ | |
| Congés | `api/leave-requests` | `/rh/absences`, `/mon-espace` | Non | ✅ | Scoping employé (Épic 3) |
| Planning | `api/schedules` | `/rh/planning`, `/mon-espace` | Non | ✅ | Scoping employé |
| Pointages | `api/time-entries` | `/rh/pointages`, `/mon-espace` | Non | ✅ | Scoping employé |
| Feuilles temps | `api/timesheets` | `/rh/feuilles-de-temps`, `/mon-espace` | Non | ✅ | Scoping employé |
| Paie | `api/payroll/*` | `/rh/paie` | Non | ✅ | |
| Métriques RH | `api/hr/metrics` | `/dashboard`, `/rh` | Non | ✅ | Épic 4 |
| Rôles dynamiques | `api/roles` | `/roles` | Non | ✅ | Épic 2 |
| Entités permissions | `api/permission-entities` | `/permission-entities` | Non | ✅ | Épic 2 |
| Auteurs | `api/authors` | `/authors` | Non | ✅ | Phase 1 |
| Portail employé | scoping API | `/mon-espace` | Non | ✅ | Épic 3 |
| Dashboard | widgets + `api/hr/metrics` | `/dashboard` | Non | ✅ | Épic 4 |
| Opérations | **Absent** | README seulement | — | ❌ | Hors scope |

---

## 12. Comptes de test

Créés automatiquement par `DataSeeder` au démarrage backend.

| Rôle | Courriel | Mot de passe |
|------|----------|--------------|
| Admin | `admin@crystal.local` | `ValidPass1!a` |
| Gérant | `gerant@crystal.local` | `ValidPass1!a` |
| Assistant | `assistant@crystal.local` | `ValidPass1!a` |
| Employé | `employee@crystal.local` | `ValidPass1!a` |

Chaque compte doit être lié à un `EmployeeProfile` pour les tests E2E du portail employé.

---

## 13. Définition of Done

Une user story est **Done** quand :

- [ ] Code mergé sur la branche sprint
- [ ] Tests unitaires / intégration backend passent (si touché)
- [ ] Tests Vitest passent (si touché frontend)
- [ ] Pas de régression sur `docker compose up`
- [ ] `USE_MOCK_API = false` pour le domaine concerné
- [ ] Permissions cohérentes UI **et** API
- [ ] Revue de code par un pair
- [ ] E2E happy path ajouté ou mis à jour (Épic 5, ou story marquée « E2E à ajouter en Phase 4 »)

Le sprint est **Done** quand :

- [x] Les 5 épics livrés selon priorités P0
- [x] Matrice section 11 : lignes P0 en ✅
- [x] ≥ 3 scénarios E2E par rôle passent en CI (27 tests Playwright)
- [x] Demo bout en bout : login employé → mon-espace → congé → approbation gérant

---

## 14. Risques

| Risque | Probabilité | Impact | Mitigation |
|--------|-------------|--------|------------|
| Scope permissions dynamiques trop large | Haute | Retard E2E | MVP entité+CRUD ; presets seulement |
| Scoping backend complexe | Moyenne | Faille sécurité | Tests intégration par rôle dès Épic 2 |
| Playwright flaky en CI | Moyenne | CI rouge | `storageState`, seed stable, retry limité |
| Écart DTO non résolu (ScheduledShift) | Moyenne | Bug planning | Trancher semaine 1 |
| json-server encore requis | Haute | E2E invalides | Épic 1 = cutover obligatoire |

---

## Annexes

### Fichiers pivots

| Sujet | Chemin |
|-------|--------|
| Rôles backend | `backend/Crystal.Core/ApplicationRoles.cs` |
| Seed comptes | `backend/Crystal.Infrastructure/Data/DataSeeder.cs` |
| Permissions frontend | `frontend/src/permissions/permissions.ts` |
| Mock permissions | `frontend/db.json` |
| Switch mock/API | `frontend/src/api/apiBaseUrl.ts` |
| Routes | `frontend/src/data/routeNames.ts` |
| Sidebar + garde RH | `frontend/src/components/sidebar/Sidebar.tsx` |
| Dashboard | `frontend/src/pages/DashboardPage.tsx` |
| Rôles dynamiques UI | `frontend/src/pages/user-roles/` |
| Métriques RH | `frontend/src/components/hr/HrMetricsCards.tsx` |
| Tests E2E | `e2e/playwright.config.ts`, `package.json` (racine) |
| Docs phases | `docs/phase1-alignement.md` … `docs/phase4-e2e-playwright.md` |
| Nix | `flake.nix` — `nix run .#verify` |

### Références

- `README.md` — comptes test, Docker Compose
- `frontend/README.md` — vision dashboard, arborescence pages, règle d'or widgets
- [`docs/phase4-e2e-playwright.md`](phase4-e2e-playwright.md) — exécution Playwright

---

*Document généré pour l'itération 4 — Sprint finale ERP Crystal.*
