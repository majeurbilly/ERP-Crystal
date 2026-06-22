# Phase 1 — Alignement Backend / Frontend

**Statut :** Terminée  
**Épic :** E1 (Sprint 4)  
**Date :** Juin 2026

---

## Objectif

Une seule source de vérité pour les flux métier : l'API ASP.NET Core sur `http://localhost:8080`. Plus de dépendance à json-server pour l'inventaire et le module RH.

---

## Décisions prises

| Sujet | Décision |
|-------|----------|
| `USE_MOCK_API` | `false` — services RH et inventaire branchés sur l'API .NET |
| `ScheduledShift.locationId` | **Non ajouté** au backend. Le filtrage par succursale utilise `EmployeeProfile.locationId`. Le champ `locationId` du formulaire planning est **UI uniquement**. |
| `EmployeeProfile.locationId` | **Ajouté** au backend (FK optionnelle vers `Location`) + `locationTitle` dans le DTO |
| API Auteurs | **Créée** — `GET/POST/PUT/DELETE /api/authors` |
| Rôles dynamiques (json-server) | Remplacés par **presets locaux** (`defaultRolePermissions.ts`) jusqu'à l'Épic 2 |
| Entités permissions | Liste statique depuis `ENTITY_TYPES` (plus de json-server) |
| Seed développement | Profils RH liés aux 4 comptes test + postes + rôles org. au démarrage Docker |
| Seed tests intégration | Sans profils RH (évite les conflits FK / 409) — voir `SeedForIntegrationTestsAsync` |

---

## Matrice d'alignement API ↔ UI

| Domaine | Endpoint backend | Route frontend | Statut |
|---------|------------------|----------------|--------|
| Auth | `api/auth/login` | `/login` | ✅ |
| Utilisateurs | `api/users`, `users/me` | `/rh/utilisateurs`, `/monprofil` | ✅ |
| Catalogue | `api/items` | `/catalogue` | ✅ |
| Livres | `api/books` | détail livre | ✅ |
| Catégories | `api/categories` | `/livres/categories` | ✅ |
| Succursales | `api/locations` | liste succursales | ✅ |
| Inventaire | `api/inventory/*` | `/ir` | ✅ |
| Employés | `api/employee-profiles` | `/rh/employes` | ✅ |
| Profil moi | `api/employee-profiles/me` | `AuthContext` au login | ✅ |
| Postes | `api/job-positions` | `/rh/referentiels/postes` | ✅ |
| Rôles org. | `api/organizational-roles` | `/rh/referentiels/roles` | ✅ |
| Contrats | `api/contracts` | `/rh/contrats-de-travail` | ✅ |
| Congés | `api/leave-requests` | `/rh/absences` | ✅ |
| Planning | `api/schedules` | `/rh/planning` | ✅ |
| Pointages | `api/time-entries` | `/rh/pointages` | ✅ |
| Feuilles temps | `api/timesheets` | `/rh/feuilles-de-temps` | ✅ |
| Paie | `api/payroll/*` | `/rh/paie` | ✅ |
| Métriques RH | `api/hr/metrics` | `/rh` | ✅ |
| Auteurs | `api/authors` | `/authors` | ✅ |
| Rôles dynamiques | `api/roles` | `/user-roles` | ✅ Épic 2 |
| Permissions | `api/users/me/permissions` | `AppPermissionContext` | ✅ Épic 2 |
| Entités permissions | `api/permission-entities` | `UserRoleForm` | ✅ Épic 2 |
| Dashboard widgets | partiel (`hr/metrics`) | `/dashboard` | ⏳ Épic 4 |
| Opérations | absent | README seulement | ❌ hors scope |

---

## Changements techniques

### Backend

- Migration `AddEmployeeProfileLocationId` — colonne `LocationId` sur `EmployeeProfiles`
- `AuthorsController` + service + repository
- `DataSeeder.SeedHrReferenceDataAsync` — profils pour `admin`, `gerant`, `assistant`, `employee@crystal.local`
- `DataSeeder.SeedForIntegrationTestsAsync` — seed minimal pour les tests (sans profils RH)

### Frontend

- `apiBaseUrl.ts` — `USE_MOCK_API = false`, `API_AUTHORS_URL`
- Services RH toujours sur l'API : `employeeProfileService`, `jobPositionService`, `scheduledShiftService`
- `authorService` → `api/authors`
- `AuthContext` → `employeeProfileService.getMe()` (plus de mock id `"101"`)
- `defaultRolePermissions.ts` — 4 presets Admin/Gérant/Assistant/Employé
- `scheduledShiftMapper` — payload sans `locationId`
- `permissionEntityService` — liste locale

### Fichiers pivots modifiés

```
frontend/src/api/apiBaseUrl.ts
frontend/src/context/AuthContext.tsx
frontend/src/permissions/defaultRolePermissions.ts
backend/Crystal.API/Controllers/AuthorsController.cs
backend/Crystal.Infrastructure/Data/DataSeeder.cs
backend/Crystal.Core/Entities/EmployeeProfile.cs
```

---

## Tests de validation

### Backend (xUnit)

```bash
cd backend
dotnet test
```

| Suite | Fichier | Rôle |
|-------|---------|------|
| Alignement Phase 1 | `Crystal.IntegrationTests/Alignment/Phase1AlignmentIntegrationTests.cs` | `employee-profiles/me`, schedules, job-positions, authors |
| Auteurs | `Crystal.IntegrationTests/Authors/AuthorsIntegrationTests.cs` | CRUD + autorisation |
| Régression | 189 tests d'intégration + 18 unitaires | Tous passent |

### Frontend (Vitest)

```bash
cd frontend
pnpm test --run src/data/test/phase1
```

| Fichier | Vérifie |
|---------|---------|
| `apiBaseUrl.test.ts` | `USE_MOCK_API === false`, URLs API |
| `defaultRolePermissions.test.ts` | 4 presets, permissions Employé |
| `scheduledShiftMapper.test.ts` | Pas de `locationId` dans le payload |
| `employeeProfileMapper.test.ts` | `locationId` + `locationTitle` |
| `permissionEntityService.test.ts` | Entités sans json-server |

---

## Démarrage local

```bash
# À la racine — suffit, plus besoin de json-server
docker compose up -d --build
```

| Service | URL |
|---------|-----|
| Frontend | http://localhost:3000 |
| API | http://localhost:8080 |
| Swagger | http://localhost:8080/swagger |

**Comptes test** (profils RH seedés en dev) :

| Rôle | Email | Mot de passe |
|------|-------|--------------|
| Admin | admin@crystal.local | ValidPass1!a |
| Gérant | gerant@crystal.local | ValidPass1!a |
| Assistant | assistant@crystal.local | ValidPass1!a |
| Employé | employee@crystal.local | ValidPass1!a |

---

## Prochaine étape

**Phase 2 — Épic 2 :** API backend pour rôles/permissions dynamiques, remplacement des presets locaux.
