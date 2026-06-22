# Permissions par périmètre succursale (LocationScope)

**Statut :** Terminée (Sprint 4 — refonte inventaire)  
**Date :** Juin 2026

---

## Contexte

Avant cette refonte, les droits d'écriture sur l'inventaire étaient déduits côté frontend à partir de `employeeProfile.locationId` : un employé ne pouvait modifier que le stock de **sa** succursale d'affectation RH, indépendamment de son rôle dynamique.

Ce couplage était rigide et empêchait des scénarios métier légitimes :

- un **Gérant** doit pouvoir modifier l'inventaire de **toutes** les succursales ;
- un rôle **Employé (Saint-Foy)** doit pouvoir modifier uniquement certaines succursales, sans être lié au profil RH ;
- un rôle **Employé** global doit pouvoir couvrir toutes les succursales actuelles **et futures**.

---

## Modèle de données

### `RolePermissions`

| Colonne | Type | Description |
|---------|------|-------------|
| `Action` | `string` | `read`, `update`, `manage`, etc. |
| `Subject` | `string` | `inventory_quantity` pour l'inventaire |
| `LocationScope` | `string?` | `"all"` ou `"specific"` — **obligatoire** pour `inventory_quantity` |
| `ScopedLocations` | table de liaison | Succursales autorisées quand `LocationScope = "specific"` |

### Valeurs de `LocationScope`

| Valeur | Effet sur les écritures inventaire |
|--------|-------------------------------------|
| `"all"` | Modification autorisée sur **toute** succursale (y compris celles créées plus tard) |
| `"specific"` | Modification autorisée uniquement si `locationId` ∈ `ScopedLocations` |
| `null` / vide | **Refusé** — configuration invalide (rétrocompat assurée par le seed et le script SQL de secours) |

---

## Règles d'évaluation (backend)

Implémentées dans `PermissionService.RulesGrantPermissionForLocation` :

1. `manage:all` → accès total.
2. Sujet ≠ `inventory_quantity` → évaluation classique (`RulesGrantPermission`).
3. `manage:inventory_quantity` → accès total inventaire.
4. Pour `inventory_quantity` :
   - `LocationScope = "all"` → autorisé ;
   - `LocationScope = "specific"` + `locationId` dans la liste → autorisé ;
   - sinon → refusé.

### Lecture vs écriture

| Opération | Comportement V1 |
|-----------|-----------------|
| **GET** inventaire | Lecture globale pour tous les utilisateurs authentifiés (pas de filtrage par succursale) |
| **PUT / POST** quantité | Contrôle via `UserHasPermissionForLocationAsync(..., "update", "inventory_quantity", locationId)` |

---

## Presets par défaut

Définis dans `PresetRolePermissions.cs` (backend) et `defaultRolePermissions.ts` (frontend) :

| Rôle | Inventaire |
|------|------------|
| **Admin** | `manage:all` |
| **Gérant** | `read` + `update` avec `LocationScope = "all"` |
| **Assistant** | `read` avec `LocationScope = "all"` |
| **Employé** | Pas de droit inventaire par défaut (lecture catalogue/succursales seulement) |

Les rôles personnalisés (ex. « Employé (Saint-Foy) ») se configurent via l'interface **Rôles** avec scope `specific` et sélection des succursales.

---

## Frontend

### Hook `useScopedPermissions`

Expose `canPerformOnLocation(action, subject, locationId)` en s'appuyant sur les règles brutes chargées via `GET /api/users/me/permissions` (et non plus sur `employeeProfile.locationId`).

Utilisé par :

- `LocationInventoryQuantityPage`
- `ItemInventoryQuantityTable`
- `ItemDetailsPage`
- `InventoryQuantityForm` (filtrage des succursales proposées à l'ajout)

### `useEmployeeBranchRowGuard`

Conservé **uniquement** pour la gestion des **succursales** (`LocationsPage`, `LocationDetailsPage`) : un employé non admin ne modifie que la fiche de sa succursale RH. Ce mécanisme est **indépendant** de l'inventaire.

---

## Initialisation et migration des données

### Seed (`DataSeeder`)

- **Premier démarrage** : `SeedDynamicRolesAsync` crée les presets via `PresetRolePermissions` (scopes inventaire inclus).
- **Bases existantes** : `SyncPresetRoleInventoryScopesAsync` s'exécute à chaque seed et :
  - applique `LocationScope = "all"` aux lignes `inventory_quantity` sans scope ;
  - ajoute les règles inventaire manquantes sur les presets Gérant / Assistant.

### Script SQL de secours

Fichier : `backend/scripts/repair-inventory-permission-scopes.sql`

À exécuter manuellement si une base de développement n'a pas encore reçu la synchronisation C# (ex. avant redémarrage de l'API).

---

## Fichiers clés

| Couche | Fichier |
|--------|---------|
| Entités | `Crystal.Core/Entities/RolePermission.cs`, `RolePermissionLocation.cs` |
| Constantes | `Crystal.Core/Authorization/LocationScopes.cs` |
| Presets | `Crystal.Infrastructure/Authorization/PresetRolePermissions.cs` |
| Évaluation | `Crystal.Infrastructure/Services/PermissionService.cs` |
| API inventaire | `Crystal.API/Controllers/InventoryController.cs` |
| Seed / sync | `Crystal.Infrastructure/Data/DataSeeder.cs` |
| Frontend | `frontend/src/permissions/useScopedPermissions.ts`, `scopedPermissionRules.ts` |
| UI rôles | `frontend/src/components/forms/UserRoleForm.tsx` |

---

## Schéma simplifié

```
┌──────────────────┐     permissions + LocationScope     ┌─────────────────────┐
│  DynamicRole     │ ──────────────────────────────────► │ PermissionService   │
│  (preset/custom) │                                     │ RulesGrant...ForLoc │
└──────────────────┘                                     └──────────┬──────────┘
                                                                    │
                    ┌───────────────────────────────────────────────┼────────────────────────┐
                    ▼                                               ▼                        ▼
         InventoryController                              useScopedPermissions          UserRoleForm
         (écritures API)                                  (boutons UI)                (configuration)
```

---

## Pourquoi l'inventaire ne dépend plus de `employeeProfile.locationId`

`employeeProfile.locationId` décrit l'**affectation RH** d'un employé (paie, horaires, congés). Ce n'est pas un modèle d'autorisation métier pour l'inventaire : deux employés de la même succursale peuvent avoir des droits différents, et un gérant n'est pas limité à sa succursale d'affectation.

La source de vérité est désormais le **rôle dynamique** et son périmètre `LocationScope` / `LocationIds`, aligné entre la base de données, l'API et le frontend.
