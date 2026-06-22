# Phase 3 — Portail employé + Dashboard widgets

**Statut :** Terminée  
**Épics :** E3 (Mon espace) + E4 (Dashboard widgets) — Sprint 4  
**Date :** Juin 2026

---

## Objectif

Les employés et assistants accèdent à **leurs** données RH via `/mon-espace`, sans menu gestionnaire `/rh`. Le tableau de bord `/dashboard` affiche des **widgets** selon les permissions, avec des liens vers les pages de travail filtrées.

---

## Architecture

```
┌──────────────────┐     JWT + permissions      ┌─────────────────────────┐
│  Employé         │ ─────────────────────────► │ EmployeeScopeService    │
│  (sans manage)   │                            │ filtre par profil RH    │
└──────────────────┘                            └───────────┬─────────────┘
                                                              │
                    leave-requests / schedules /              ▼
                    time-entries / timesheets         Données « mes données »
┌──────────────────┐
│  Gérant / Admin  │ ── manage:{subject} ──► accès global (tous les profils)
└──────────────────┘

┌──────────────────┐     permissions CASL     ┌─────────────────────────┐
│  /dashboard      │ ◄─────────────────────── │ DashboardWidgetGrid     │
│  widgets         │     liens contextuels    │ employé → /mon-espace   │
└──────────────────┘                          │ gérant  → /rh?status=…  │
                                              └─────────────────────────┘
```

### Règle de scoping backend

| Situation | Comportement |
|-----------|--------------|
| Permission `manage:{subject}` | Liste et détail sur **tous** les enregistrements |
| Sinon, utilisateur lié à un `EmployeeProfile` | Filtrage automatique sur `EmployeeProfileId` |
| `GET /api/employee-profiles` sans `manage:employee_profile` | **403 Forbidden** |
| `GET /api/employee-profiles/me` | Profil RH de l'utilisateur connecté |

Subjects concernés : `leave_request`, `scheduled_shift`, `time_entry`, `timesheet`, `employee_profile`.

---

## Portail Mon espace (`/mon-espace`)

| Onglet | Contenu | API |
|--------|---------|-----|
| **Horaire** | Calendrier mensuel (quarts planifiés) | `GET /api/schedules` |
| **Congés** | Liste + formulaire création (`selfMode`) | `GET/POST /api/leave-requests` |
| **Ma fiche** | Nom, poste, statut ; salaire si `read:hr_dashboard` | `GET /api/employee-profiles/me` |
| **Pointages** | Grille lecture seule | `GET /api/time-entries` |
| **Feuille de temps** | Grille lecture seule | `GET /api/timesheets` |

Navigation par query param : `/mon-espace?tab=horaire|conges|fiche|pointages|feuille`.

L'entrée **Mon espace** apparaît dans la sidebar si l'utilisateur **n'a pas** `read:hr_dashboard` et a au moins `read:leave_request` ou `read:scheduled_shift`.

Lien mot de passe : onglet Ma fiche → `/monprofil`.

---

## Dashboard widgets (`/dashboard`)

| Permission | Widget | Destination au clic |
|------------|--------|------------------------|
| `read:scheduled_shift` | Prochain quart | Employé → `/mon-espace?tab=horaire` ; Gérant → `/rh/planning` |
| `read:leave_request` | Congés en attente | Employé → `/mon-espace?tab=conges` ; Gérant → `/rh/absences?status=Pending` |
| `read:inventory_quantity` (sans HR dashboard) | Inventaire | `/ir` |
| `read:hr_dashboard` | Métriques RH (`HrMetricsCards`) | — |
| `read:hr_dashboard` | Feuilles en attente | `/rh/feuilles-de-temps?status=Submitted` |
| `read:hr_dashboard` | Alertes catalogue | `/catalogue` |
| `read:user_role` | Rôles et permissions | `/user-roles` |

---

## Fichiers principaux

### Backend

| Fichier | Rôle |
|---------|------|
| `Crystal.Core/Interfaces/Services/IEmployeeScopeService.cs` | Contrat scoping |
| `Crystal.Infrastructure/Services/EmployeeScopeService.cs` | Résolution profil + `CanManage` |
| `Crystal.API/Extensions/ControllerExtensions.cs` | `GetCurrentUserId()` |
| Repositories HR | `GetByEmployeeProfileIdAsync` |
| Controllers HR | Passent `userId` aux services |

### Frontend

| Fichier | Rôle |
|---------|------|
| `frontend/src/pages/MonEspacePage.tsx` | Portail 5 onglets |
| `frontend/src/components/dashboard/DashboardWidgetGrid.tsx` | Widgets conditionnels |
| `frontend/src/components/hr/forms/LeaveRequestForm.tsx` | Prop `selfMode` |
| `frontend/src/pages/hr/LeaveRequestsPage.tsx` | Filtre `?status=` |
| `frontend/src/pages/hr/TimesheetsPage.tsx` | Filtre `?status=` |

---

## Tests

### Backend (intégration)

```bash
cd backend
dotnet test --filter "FullyQualifiedName~Phase3EmployeeScopingIntegrationTests"
```

Scénarios couverts :
- Employé A ne voit que ses congés et quarts
- Employé ne peut pas lire le congé de l'employé B (404)
- Gérant voit tous les congés
- `GET /api/employee-profiles` → 403 pour employé

### Frontend (Vitest)

```bash
cd frontend
pnpm test --run src/data/test/phase3
```

- `findNextShift.test.ts` — calcul du prochain quart
- `leaveRequestFormSelfMode.test.ts` — formulaire sans liste employés
- `dashboardWidgetRoutes.test.ts` — URLs widgets employé vs gérant

---

## Comptes de démonstration

| Rôle | Courriel | Mot de passe |
|------|----------|--------------|
| Admin | `admin@crystal.local` | `ValidPass1!a` |
| Gérant | `gerant@crystal.local` | `ValidPass1!a` |
| Assistant | `assistant@crystal.local` | `ValidPass1!a` |
| Employé | `employee@crystal.local` | `ValidPass1!a` |

Le seed `SeedHrReferenceDataAsync` lie chaque compte test à un profil RH (requis pour Mon espace en Docker).

---

## Vérification manuelle rapide

1. Se connecter en **employé** → sidebar **Mon espace** visible, pas de menu RH complet.
2. Onglet Congés → créer une demande → statut « Pending », visible uniquement pour soi.
3. Se connecter en **gérant** → dashboard avec métriques RH et liens vers absences/feuilles filtrées.
4. API : `GET /api/leave-requests` en employé ne retourne que son `employeeProfileId`.
