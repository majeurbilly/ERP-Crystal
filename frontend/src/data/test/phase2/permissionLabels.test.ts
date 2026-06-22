import { describe, expect, it } from "vitest";
import {
    formatPermissionSentence,
    getActionLabel,
    getEntityLabel,
    groupPermissionsByEntity,
    isFullAdminAccess,
    LOCATION_SCOPES,
} from "../../../permissions/permissionLabels";
import { CRUD_OPERATIONS } from "../../../permissions/permissions";

describe("permissionLabels", () => {
    it("traduit les sections en français", () => {
        expect(getEntityLabel("employee_profile")).toBe("Employés");
        expect(getEntityLabel("leave_request")).toBe("Congés");
    });

    it("traduit les droits en français", () => {
        expect(getActionLabel(CRUD_OPERATIONS.READ)).toBe("Consulter");
        expect(getActionLabel(CRUD_OPERATIONS.SUBMIT)).toBe("Soumettre");
        expect(getActionLabel(CRUD_OPERATIONS.APPROVE)).toBe("Approuver");
        expect(getActionLabel(CRUD_OPERATIONS.MANAGE)).toBe("Accès complet");
    });

    it("formate une phrase lisible pour l'inventaire avec périmètre", () => {
        expect(formatPermissionSentence({
            action: CRUD_OPERATIONS.UPDATE,
            subject: "inventory_quantity",
            locationScope: LOCATION_SCOPES.ALL,
        })).toBe("Peut modifier l'inventaire de toutes les succursales");

        expect(formatPermissionSentence({
            action: CRUD_OPERATIONS.UPDATE,
            subject: "inventory_quantity",
            locationScope: LOCATION_SCOPES.SPECIFIC,
            locationIds: [1],
        }, { 1: "Saint-Foy" })).toBe("Peut modifier l'inventaire — Saint-Foy");
    });

    it("formate une phrase lisible", () => {
        expect(formatPermissionSentence({
            action: CRUD_OPERATIONS.READ,
            subject: "location",
        })).toBe("Consulter — Succursales");
    });

    it("détecte l'accès administrateur complet", () => {
        expect(isFullAdminAccess([{ action: CRUD_OPERATIONS.MANAGE, subject: "all" }])).toBe(true);
        expect(isFullAdminAccess([{ action: CRUD_OPERATIONS.READ, subject: "location" }])).toBe(false);
    });

    it("regroupe les permissions par section", () => {
        const grouped = groupPermissionsByEntity([
            { action: CRUD_OPERATIONS.READ, subject: "location" },
            { action: CRUD_OPERATIONS.UPDATE, subject: "location" },
            { action: CRUD_OPERATIONS.READ, subject: "payroll" },
        ]);

        expect(grouped).toHaveLength(2);
        expect(grouped.find((p_group) => p_group.subject === "location")?.rules).toHaveLength(2);
    });
});
