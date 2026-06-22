import { describe, expect, it } from "vitest";
import { getAllDefaultRoles, getDefaultRoleById } from "../../../permissions/defaultRolePermissions";
import { PRESET_ROLE_IDS } from "../../types/hr/userRoles";
import { CRUD_OPERATIONS, ENTITY_TYPES } from "../../../permissions/permissions";

describe("Phase 1 — defaultRolePermissions", () => {
    it("expose les 4 presets Admin/Gérant/Assistant/Employé", () => {
        const roles = getAllDefaultRoles();
        expect(roles).toHaveLength(4);
        expect(roles.map((p_role) => p_role.id)).toEqual(
            expect.arrayContaining([
                PRESET_ROLE_IDS.ADMIN,
                PRESET_ROLE_IDS.GERANT,
                PRESET_ROLE_IDS.ASSISTANT,
                PRESET_ROLE_IDS.EMPLOYE,
            ])
        );
    });

    it("Admin a manage:all", () => {
        const admin = getDefaultRoleById(PRESET_ROLE_IDS.ADMIN);
        expect(admin?.permissions).toContainEqual({
            action: "manage",
            subject: ENTITY_TYPES.ALL,
        });
    });

    it("Employé a read sur scheduled_shift sans hr_dashboard", () => {
        const employee = getDefaultRoleById(PRESET_ROLE_IDS.EMPLOYE);
        expect(employee?.permissions).toContainEqual({
            action: "read",
            subject: ENTITY_TYPES.SCHEDULED_SHIFT,
        });
        expect(employee?.permissions).toContainEqual({
            action: "read",
            subject: ENTITY_TYPES.EMPLOYMENT_CONTRACT,
        });
        expect(employee?.permissions).toContainEqual({
            action: "read",
            subject: ENTITY_TYPES.PAYROLL,
        });
        expect(employee?.permissions.some((p_perm) => p_perm.subject === ENTITY_TYPES.HR_DASHBOARD)).toBe(false);
    });

    it("Employé ne peut pas changer le statut des feuilles de temps", () => {
        const employee = getDefaultRoleById(PRESET_ROLE_IDS.EMPLOYE);

        expect(employee?.permissions).not.toContainEqual({
            action: CRUD_OPERATIONS.SUBMIT,
            subject: ENTITY_TYPES.TIMESHEET,
        });
        expect(employee?.permissions).not.toContainEqual({
            action: CRUD_OPERATIONS.APPROVE,
            subject: ENTITY_TYPES.TIMESHEET,
        });
    });
});
