import { describe, expect, it } from "vitest";
import { defineAbilityFor } from "../../../permissions/permissions";
import { ENTITY_TYPES, CRUD_OPERATIONS } from "../../../permissions/permissions";
import type { SessionUser } from "../../../context/AuthContext";

const mockUser: SessionUser = {
    id: "1",
    dynamicRole: null,
    employeeProfile: undefined,
    userName: "admin",
    email: "admin@test.local",
};

describe("Phase 2 — defineAbilityFor avec permissions API", () => {
    it("accorde manage:all depuis les règles dynamiques", () => {
        const ability = defineAbilityFor(mockUser, [
            { action: CRUD_OPERATIONS.MANAGE, subject: ENTITY_TYPES.ALL },
        ]);

        expect(ability.can(CRUD_OPERATIONS.CREATE, ENTITY_TYPES.EMPLOYEE_PROFILE)).toBe(true);
        expect(ability.can(CRUD_OPERATIONS.DELETE, ENTITY_TYPES.USER_ROLE)).toBe(true);
    });

    it("accorde uniquement les permissions explicites pour l'employé", () => {
        const employeeUser: SessionUser = { ...mockUser, id: "2" };
        const ability = defineAbilityFor(employeeUser, [
            { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.SCHEDULED_SHIFT },
            { action: CRUD_OPERATIONS.CREATE, subject: ENTITY_TYPES.LEAVE_REQUEST },
        ]);

        expect(ability.can(CRUD_OPERATIONS.READ, ENTITY_TYPES.SCHEDULED_SHIFT)).toBe(true);
        expect(ability.can(CRUD_OPERATIONS.CREATE, ENTITY_TYPES.LEAVE_REQUEST)).toBe(true);
        expect(ability.can(CRUD_OPERATIONS.CREATE, ENTITY_TYPES.EMPLOYEE_PROFILE)).toBe(false);
        expect(ability.can(CRUD_OPERATIONS.READ, ENTITY_TYPES.USER_ROLE)).toBe(false);
    });
});
