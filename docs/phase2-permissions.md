# Phase 2 — Permissions unifiées (API + UI)

**Statut :** Terminée  
**Épic :** E2 (Sprint 4)  
**Date :** Juin 2026

---

## Objectif

Un administrateur peut créer, modifier et assigner des rôles dynamiques avec des permissions par entité CRUD. Le backend **et** le frontend appliquent les mêmes règles via une source de vérité unique en base de données.

---

## Architecture

```
┌─────────────────┐     JWT (rôle Identity)     ┌──────────────────┐
│  ApplicationUser│ ──────────────────────────► │ Auth ASP.NET     │
│  DynamicRoleId? │                             │ [Authorize]      │
└────────┬────────┘                             └──────────────────┘
         │
         ▼
┌─────────────────┐     RolePermission          ┌──────────────────┐
│  DynamicRole    │ ◄── (action + subject) ──── │ PermissionService│
│  (4 presets +   │                             │ RequirePermission│
│   custom roles) │                             └──────────────────┘
└─────────────────┘
         │
         ▼
┌─────────────────┐     GET /users/me/permissions   ┌──────────────────┐
│  Frontend       │ ◄────────────────────────────── │ CASL Ability     │
│  AuthContext    │                                 │ Sidebar / boutons│
└─────────────────┘                                 └──────────────────┘
```

### Règles de résolution

| Situation | Rôle effectif |
|-----------|---------------|
| `ApplicationUser.DynamicRoleId` renseigné | Permissions du rôle dynamique assigné |
| Sinon | Preset correspondant au rôle Identity (Admin, Gerant, Assistant, Employee) |

### Hiérarchie des permissions

| Règle | Effet |
|-------|-------|
| `manage:all` | Accès complet (Admin) |
| `manage:{subject}` | CRUD complet sur l'entité |
| `{action}:{subject}` | Action précise (ex. `read:scheduled_shift`) |

---

## API Backend

| Endpoint | Méthode | Permission requise | Description |
|----------|---------|-------------------|-------------|
| `/api/roles` | GET | `read:user_role` | Liste tous les rôles |
| `/api/roles/{id}` | GET | `read:user_role` | Détail d'un rôle |
| `/api/roles` | POST | `create:user_role` | Créer un rôle (ou depuis preset via `presetId`) |
| `/api/roles/{id}` | PUT | `update:user_role` | Modifier nom + permissions |
| `/api/roles/{id}` | DELETE | `delete:user_role` | Supprimer (interdit si preset ou assigné) |
| `/api/permission-entities` | GET | `read:user_role` | Liste des entités configurables |
| `/api/users/me/permissions` | GET | Authentifié | Permissions effectives de l'utilisateur courant |

### Exemple — créer depuis un preset

```json
POST /api/roles
{
  "name": "Superviseur RH",
  "presetId": "Assistant",
  "permissions": []
}
```

### Exemple — permissions utilisateur

```json
GET /api/users/me/permissions
{
  "roleId": "Employee",
  "roleName": "Employé",
  "permissions": [
    { "action": "read", "subject": "scheduled_shift" },
    { "action": "create", "subject": "leave_request" }
  ]
}
```

---

## Entités backend

| Entité | Table | Description |
|--------|-------|-------------|
| `DynamicRole` | `DynamicRoles` | Rôle nommé (preset ou custom) |
| `RolePermission` | `RolePermissions` | Paire action + subject |
| `ApplicationUser.DynamicRoleId` | `AspNetUsers` | Lien optionnel utilisateur → rôle |

Migration : `AddDynamicRolesAndPermissions`

---

## Presets (matrice MVP)

| Entité | Admin | Gérant | Assistant | Employé |
|--------|-------|--------|-----------|---------|
| `all` | manage | — | — | — |
| `hr_dashboard` | read | read | read | — |
| `employee_profile` | manage | manage | read | read |
| `leave_request` | manage | manage | create+read | create+read |
| `scheduled_shift` | manage | manage | read | read |
| `time_entry` | manage | manage | read+create | read+create |
| `timesheet` | manage | manage | read | read |
| `payroll` | manage | manage | — | — |
| `employment_contract` | manage | manage | read | — |
| `item` / `inventory_quantity` | manage | read+update | read | read |
| `location` | manage | read+update | read | read |
| `user_role` | manage | — | — | — |

> Le scoping « mes données uniquement » (employé ne voit que ses enregistrements) est prévu en **Épic 3**.

---

## Frontend

### Services branchés sur l'API

| Service | Endpoint | Rôle |
|---------|----------|------|
| `userRoleService` | `/api/roles` | CRUD rôles dynamiques |
| `permissionEntityService` | `/api/permission-entities` | Entités pour le formulaire |
| `permissionService` | `/api/users/me/permissions` | Permissions au login |

### Composants

| Composant | Fichier | Fonction |
|-----------|---------|----------|
| `UserRoleForm` | `components/forms/UserRoleForm.tsx` | Création/édition + 4 boutons preset |
| `UserRoleListPage` | `pages/user-roles/UserRoleListPage.tsx` | Liste avec add/edit/delete |
| `UserRoleDetailsPage` | `pages/user-roles/UserRoleDetailsPage.tsx` | Détail permissions + édition |
| `UserForm` | `components/forms/UserForm.tsx` | Sélecteur rôle dynamique (`dynamicRoleId`) |
| `AppPermissionContext` | `permissions/AppPermissionContext.tsx` | CASL via `me/permissions` |

### Flux au login

1. JWT → rôle Identity (`AuthContext`)
2. `GET /api/users/me/permissions` → règles effectives
3. `defineAbilityFor(user, permissions)` → ability CASL
4. Sidebar et boutons CRUD masqués selon `usePermissions()`

---

## Tests

### Backend — intégration

Fichier : `Crystal.IntegrationTests/Permissions/Phase2PermissionsIntegrationTests.cs`

| Test | Vérifie |
|------|---------|
| `GetMyPermissions_ReturnsPresetPermissions_ForAdmin` | Admin a `manage:all` |
| `GetMyPermissions_ReturnsEmployeePermissions_WithoutHrDashboard` | Employé sans accès RH dashboard |
| `Roles_GetAll_Returns200_ForAdmin` | Admin liste les rôles |
| `Roles_GetAll_Returns403_ForEmployee` | Employé bloqué |
| `Roles_CreateCustomRole_Returns201_ForAdmin` | Création rôle custom |
| `Roles_CreateFromPreset_Returns201_WithPresetPermissions` | Copie depuis preset |
| `Roles_DeletePreset_Returns409_ForAdmin` | Presets non supprimables |
| `Roles_DeleteAssignedRole_Returns409` | Rôle assigné non supprimable |
| `EmployeeProfiles_Create_Returns403_ForEmployee` | Employé ne crée pas de profil |
| `EmployeeProfiles_Create_Returns201_ForGerant` | Gérant peut créer |
| `PermissionEntities_GetAll_Returns200_ForAdmin` | Liste entités |

### Backend — unitaires

Fichier : `Crystal.UnitTests/Services/PermissionServiceTests.cs` — logique `manage:all`, `manage:subject`, règle exacte.

### Frontend — Vitest

Dossier : `frontend/src/data/test/phase2/`

| Fichier | Couverture |
|---------|------------|
| `userRoleMapper.test.ts` | `permissions` en tableau, `isPreset` |
| `userMapper.test.ts` | `dynamicRoleId` API ↔ domaine |
| `defineAbilityFor.test.ts` | CASL avec règles dynamiques |
| `permissionEntityService.test.ts` | Appel API + fallback |

---

## Comptes de test

| Rôle | Email | Mot de passe |
|------|-------|--------------|
| Admin | `admin@crystal.local` | `ValidPass1!a` |
| Gérant | `gerant@crystal.local` | `ValidPass1!a` |
| Assistant | `assistant@crystal.local` | `ValidPass1!a` |
| Employé | `employee@crystal.local` | `ValidPass1!a` |

---

## Commandes

```bash
# Backend
cd backend
dotnet ef database update --project Crystal.Infrastructure --startup-project Crystal.API
dotnet test --filter "FullyQualifiedName~Phase2Permissions"

# Frontend
cd frontend
pnpm run build
pnpm test --run src/data/test/phase2
```

---

## Prochaine étape

**Phase 3 — Portail employé** : scoping « mes données » sur congés, planning, pointages (`docs/sprint4.md`, Épic 3).
