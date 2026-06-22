import { describe, expect, it } from "vitest";
import {
    canUpdateInventoryOnAnyLocation,
    rulesGrantPermissionForLocation,
} from "../../../permissions/scopedPermissionRules";
import { CRUD_OPERATIONS, ENTITY_TYPES } from "../../../permissions/permissions";
import { LOCATION_SCOPES } from "../../../permissions/permissionLabels";

describe("scopedPermissionRules", () => {
    it("accorde l'accès global avec manage/all", () => {
        const result = rulesGrantPermissionForLocation(
            [{ action: CRUD_OPERATIONS.MANAGE, subject: ENTITY_TYPES.ALL }],
            CRUD_OPERATIONS.UPDATE,
            ENTITY_TYPES.INVENTORY_QUANTITY,
            99
        );

        expect(result).toBe(true);
    });

    it("accorde l'inventaire sur toutes les succursales avec scope all", () => {
        const rules = [{
            action: CRUD_OPERATIONS.UPDATE,
            subject: ENTITY_TYPES.INVENTORY_QUANTITY,
            locationScope: LOCATION_SCOPES.ALL,
        }];

        expect(rulesGrantPermissionForLocation(rules, CRUD_OPERATIONS.UPDATE, ENTITY_TYPES.INVENTORY_QUANTITY, 1)).toBe(true);
        expect(rulesGrantPermissionForLocation(rules, CRUD_OPERATIONS.UPDATE, ENTITY_TYPES.INVENTORY_QUANTITY, 2)).toBe(true);
    });

    it("restreint l'inventaire aux succursales spécifiques", () => {
        const rules = [{
            action: CRUD_OPERATIONS.UPDATE,
            subject: ENTITY_TYPES.INVENTORY_QUANTITY,
            locationScope: LOCATION_SCOPES.SPECIFIC,
            locationIds: [1],
        }];

        expect(rulesGrantPermissionForLocation(rules, CRUD_OPERATIONS.UPDATE, ENTITY_TYPES.INVENTORY_QUANTITY, 1)).toBe(true);
        expect(rulesGrantPermissionForLocation(rules, CRUD_OPERATIONS.UPDATE, ENTITY_TYPES.INVENTORY_QUANTITY, 2)).toBe(false);
    });

    it("restreint manage inventaire au périmètre spécifique", () => {
        const rules = [{
            action: CRUD_OPERATIONS.MANAGE,
            subject: ENTITY_TYPES.INVENTORY_QUANTITY,
            locationScope: LOCATION_SCOPES.SPECIFIC,
            locationIds: [2],
        }];

        expect(rulesGrantPermissionForLocation(rules, CRUD_OPERATIONS.UPDATE, ENTITY_TYPES.INVENTORY_QUANTITY, 2)).toBe(true);
        expect(rulesGrantPermissionForLocation(rules, CRUD_OPERATIONS.UPDATE, ENTITY_TYPES.INVENTORY_QUANTITY, 1)).toBe(false);
    });

    it("refuse l'inventaire sans scope explicite", () => {
        const rules = [{
            action: CRUD_OPERATIONS.UPDATE,
            subject: ENTITY_TYPES.INVENTORY_QUANTITY,
        }];

        expect(rulesGrantPermissionForLocation(rules, CRUD_OPERATIONS.UPDATE, ENTITY_TYPES.INVENTORY_QUANTITY, 1)).toBe(false);
    });

    it("détecte si l'utilisateur peut modifier l'inventaire quelque part", () => {
        const rules = [{
            action: CRUD_OPERATIONS.UPDATE,
            subject: ENTITY_TYPES.INVENTORY_QUANTITY,
            locationScope: LOCATION_SCOPES.SPECIFIC,
            locationIds: [3],
        }];

        expect(canUpdateInventoryOnAnyLocation(rules, false)).toBe(true);
        expect(canUpdateInventoryOnAnyLocation([], false)).toBe(false);
        expect(canUpdateInventoryOnAnyLocation(rules, true)).toBe(true);
    });
});
