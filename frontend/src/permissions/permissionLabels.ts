import type { PermissionRule } from "../data/types/hr/dynamicUserRole";
import { CRUD_OPERATIONS, ENTITY_TYPES } from "./permissions";

export const LOCATION_SCOPES = {
    ALL: "all",
    SPECIFIC: "specific",
} as const;

const ENTITY_LABELS: Record<string, string> = {
    me: "Mon profil",
    all: "Toute l'application",
    location: "Succursales",
    item: "Catalogue",
    user: "Utilisateurs",
    category: "Catégories",
    inventory_quantity: "Inventaire",
    hr_dashboard: "Tableau de bord RH",
    job_position: "Postes",
    employee_profile: "Employés",
    employment_contract: "Contrats de travail",
    leave_request: "Congés",
    scheduled_shift: "Planification",
    time_entry: "Pointages",
    timesheet: "Feuilles de temps",
    payroll: "Paie",
    author: "Auteurs",
    permission: "Permissions système",
    user_role: "Rôles",
};

const ACTION_LABELS: Record<string, string> = {
    [CRUD_OPERATIONS.CREATE]: "Ajouter",
    [CRUD_OPERATIONS.READ]: "Consulter",
    [CRUD_OPERATIONS.UPDATE]: "Modifier",
    [CRUD_OPERATIONS.DELETE]: "Supprimer",
    [CRUD_OPERATIONS.SUBMIT]: "Soumettre",
    [CRUD_OPERATIONS.APPROVE]: "Approuver",
    [CRUD_OPERATIONS.MANAGE]: "Accès complet",
};

const ACTION_DESCRIPTIONS: Record<string, string> = {
    [CRUD_OPERATIONS.CREATE]: "Peut créer de nouveaux éléments",
    [CRUD_OPERATIONS.READ]: "Peut voir les informations",
    [CRUD_OPERATIONS.UPDATE]: "Peut modifier les informations existantes",
    [CRUD_OPERATIONS.DELETE]: "Peut supprimer des éléments",
    [CRUD_OPERATIONS.SUBMIT]: "Peut soumettre pour approbation",
    [CRUD_OPERATIONS.APPROVE]: "Peut approuver ou refuser",
    [CRUD_OPERATIONS.MANAGE]: "Peut tout faire dans cette section",
};

export interface GroupedPermissions {
    subject: string;
    subjectLabel: string;
    rules: PermissionRule[];
}

export function getEntityLabel(p_subject: string): string {
    return ENTITY_LABELS[p_subject] ?? p_subject.replace(/_/g, " ");
}

export function getActionLabel(p_action: string): string {
    return ACTION_LABELS[p_action] ?? p_action;
}

export function getActionDescription(p_action: string): string {
    return ACTION_DESCRIPTIONS[p_action] ?? "";
}

export function isInventoryPermission(p_subject: string): boolean {
    return p_subject === ENTITY_TYPES.INVENTORY_QUANTITY;
}

export function formatPermissionSentence(
    p_rule: PermissionRule,
    p_locationTitlesById?: Record<number, string>,
): string {
    if (p_rule.action === CRUD_OPERATIONS.MANAGE && p_rule.subject === "all") {
        return "Accès administrateur à toute l'application";
    }

    if (isInventoryPermission(p_rule.subject)) {
        const actionLabel = p_rule.action === CRUD_OPERATIONS.MANAGE
            ? "tout faire sur"
            : getActionLabel(p_rule.action).toLowerCase();
        const scopeSuffix = formatInventoryScopeSuffix(p_rule, p_locationTitlesById);
        return `Peut ${actionLabel} l'inventaire${scopeSuffix}`;
    }

    if (p_rule.action === CRUD_OPERATIONS.MANAGE) {
        return `Accès complet — ${getEntityLabel(p_rule.subject)}`;
    }

    return `${getActionLabel(p_rule.action)} — ${getEntityLabel(p_rule.subject)}`;
}

export function formatInventoryScopeSuffix(
    p_rule: PermissionRule,
    p_locationTitlesById?: Record<number, string>,
): string {
    if (p_rule.locationScope === LOCATION_SCOPES.ALL) {
        return " de toutes les succursales";
    }

    if (p_rule.locationScope === LOCATION_SCOPES.SPECIFIC && p_rule.locationIds && p_rule.locationIds.length > 0) {
        const labels = p_rule.locationIds.map((p_id: number) =>
            p_locationTitlesById?.[p_id] ?? `succursale #${p_id}`,
        );
        return ` — ${labels.join(", ")}`;
    }

    return "";
}

export function groupPermissionsByEntity(p_permissions: PermissionRule[]): GroupedPermissions[] {
    const groups = new Map<string, PermissionRule[]>();

    p_permissions.forEach((p_rule: PermissionRule) => {
        const existing = groups.get(p_rule.subject) ?? [];
        existing.push(p_rule);
        groups.set(p_rule.subject, existing);
    });

    return Array.from(groups.entries())
        .map(([p_subject, p_rules]) => ({
            subject: p_subject,
            subjectLabel: getEntityLabel(p_subject),
            rules: p_rules,
        }))
        .sort((p_a, p_b) => p_a.subjectLabel.localeCompare(p_b.subjectLabel, "fr"));
}

export function isFullAdminAccess(p_permissions: PermissionRule[]): boolean {
    return p_permissions.some(
        (p_rule) => p_rule.action === CRUD_OPERATIONS.MANAGE && p_rule.subject === "all",
    );
}
