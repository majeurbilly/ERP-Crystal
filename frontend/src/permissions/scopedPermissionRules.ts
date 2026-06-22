import type { PermissionRule } from "../data/types/hr/dynamicUserRole";
import { LOCATION_SCOPES } from "./permissionLabels";
import { CRUD_OPERATIONS, ENTITY_TYPES } from "./permissions";

function inventoryRuleGrantsActionAtLocation(
    p_rule: PermissionRule,
    p_action: string,
    p_locationId: number
): boolean {
    if (p_rule.subject !== ENTITY_TYPES.INVENTORY_QUANTITY) {
        return false;
    }

    if (p_rule.action !== p_action && p_rule.action !== CRUD_OPERATIONS.MANAGE) {
        return false;
    }

    if (!p_rule.locationScope) {
        return false;
    }

    if (p_rule.locationScope === LOCATION_SCOPES.ALL) {
        return true;
    }

    return p_rule.locationScope === LOCATION_SCOPES.SPECIFIC
        && (p_rule.locationIds ?? []).includes(p_locationId);
}

export function rulesGrantPermission(
    p_rules: PermissionRule[],
    p_action: string,
    p_subject: string
): boolean {
    if (p_rules.some(
        (p_rule) => p_rule.action === CRUD_OPERATIONS.MANAGE && p_rule.subject === ENTITY_TYPES.ALL
    )) {
        return true;
    }

    if (p_rules.some(
        (p_rule) => p_rule.action === CRUD_OPERATIONS.MANAGE && p_rule.subject === p_subject
    )) {
        return true;
    }

    return p_rules.some(
        (p_rule) => p_rule.action === p_action && p_rule.subject === p_subject
    );
}

export function rulesGrantPermissionForLocation(
    p_rules: PermissionRule[],
    p_action: string,
    p_subject: string,
    p_locationId: number
): boolean {
    if (p_rules.some(
        (p_rule) => p_rule.action === CRUD_OPERATIONS.MANAGE && p_rule.subject === ENTITY_TYPES.ALL
    )) {
        return true;
    }

    if (p_subject !== ENTITY_TYPES.INVENTORY_QUANTITY) {
        return rulesGrantPermission(p_rules, p_action, p_subject);
    }

    return p_rules.some(
        (p_rule) => inventoryRuleGrantsActionAtLocation(p_rule, p_action, p_locationId)
    );
}

export function canUpdateInventoryOnAnyLocation(
    p_rules: PermissionRule[],
    p_isSuperAdmin: boolean
): boolean {
    if (p_isSuperAdmin) {
        return true;
    }

    if (p_rules.some(
        (p_rule) => p_rule.action === CRUD_OPERATIONS.MANAGE && p_rule.subject === ENTITY_TYPES.ALL
    )) {
        return true;
    }

    const inventoryWriteRules = p_rules.filter(
        (p_rule) => p_rule.subject === ENTITY_TYPES.INVENTORY_QUANTITY
            && (p_rule.action === CRUD_OPERATIONS.UPDATE || p_rule.action === CRUD_OPERATIONS.MANAGE)
    );

    for (const rule of inventoryWriteRules) {
        if (!rule.locationScope) {
            continue;
        }

        if (rule.locationScope === LOCATION_SCOPES.ALL) {
            return true;
        }

        if (rule.locationScope === LOCATION_SCOPES.SPECIFIC && (rule.locationIds?.length ?? 0) > 0) {
            return true;
        }
    }

    return false;
}
