import { describe, expect, it } from "vitest";
import { userMapper } from "../../data-mapper/hr/userMapper";
import { PRESET_ROLE_IDS } from "../../types/hr/userRoles";

describe("Phase 5 — userMapper", () => {
    it("mappe dynamicRoleId et dynamicRoleName depuis l'API", () => {
        const user = userMapper.mapToDomain({
            id: "u1",
            email: "test@test.local",
            userName: "test",
            dynamicRoleId: "custom-role-id",
            dynamicRoleName: "Rôle personnalisé",
        });

        expect(user.dynamicRoleId).toBe("custom-role-id");
        expect(user.dynamicRoleName).toBe("Rôle personnalisé");
    });

    it("envoie uniquement dynamicRoleId dans le payload API", () => {
        const api = userMapper.mapToApi({
            id: "u1",
            email: "test@test.local",
            userName: "test",
            dynamicRoleId: PRESET_ROLE_IDS.EMPLOYE,
        });

        expect(api.dynamicRoleId).toBe(PRESET_ROLE_IDS.EMPLOYE);
        expect(api).not.toHaveProperty("Role");
        expect(api).not.toHaveProperty("roles");
    });

    it("utilise le preset Employé par défaut si dynamicRoleId absent", () => {
        const api = userMapper.mapToApi({
            id: "u1",
            email: "test@test.local",
            userName: "test",
        });

        expect(api.dynamicRoleId).toBe(PRESET_ROLE_IDS.EMPLOYE);
    });
});
