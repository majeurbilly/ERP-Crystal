# Phase 5 — Nettoyage et finalisation unification des rôles

**Statut :** Terminée  
**Date :** Juin 2026

---

## Objectif

Clore la migration « type de compte Identity » → **rôle dynamique unique** (`DynamicRoleId`) : retirer le code mort, aligner la documentation et les tests E2E.

---

## Changements backend

| Élément | Action |
|---------|--------|
| `DataSeeder.SeedUsersAsync` | Plus de seed `AspNetRoles` ni `AddToRoleAsync` |
| `BackfillUserDynamicRolesAsync` | Défaut `Employee` sans lecture Identity |
| `Program.cs` | `RoleClaimType` retiré de la config JWT |

`ApplicationRoles` est conservé comme **identifiants des presets** (`Admin`, `Gerant`, `Assistant`, `Employee`) dans la table `DynamicRoles`.

---

## Changements frontend

| Fichier / élément | Action |
|-------------------|--------|
| `User.role` | Supprimé du type domaine |
| `userMapper` | Payload API = `dynamicRoleId` uniquement |
| `userRoles.ts` | `PRESET_ROLE_IDS` + `resolvePresetRoleFromAssignedRole` |
| `isUserRole.ts`, `devAuth.ts`, `permissionRulesDefinitions.ts`, `mockAuthContext.ts` | Supprimés |

---

## Tests E2E

`e2e/admin/users.spec.ts` — scénario A3 enrichi :
- sélection du rôle **Employé** à la création ;
- vérification de la colonne **Rôle** dans la grille.

---

## Architecture finale

```
Utilisateur
  └── DynamicRoleId (obligatoire, FK DynamicRoles)
        └── Permissions (RequirePermission + CASL)
JWT → identifiant utilisateur seulement
```

Un seul champ **Rôle** dans les formulaires (liste `/roles` : presets + rôles personnalisés).

---

## Vérification locale

```bash
# Backend
cd backend
dotnet build
dotnet test Crystal.UnitTests
dotnet test Crystal.IntegrationTests --filter "FullyQualifiedName~Auth|User|Phase2"

# Frontend
cd frontend
npm run build
npm run test -- --run src/data/test/phase2/userMapper.test.ts

# E2E (Docker Compose requis)
npm run e2e -- e2e/admin/users.spec.ts
```
