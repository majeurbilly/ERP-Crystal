import { describe, expect, it } from "vitest";
import { userRoleMapper } from "../../data-mapper/hr/userRoleMapper";

describe("Phase 2 — userRoleMapper", () => {
    it("mappe permissions comme tableau (pas objet)", () => {
        const domain = userRoleMapper.mapToDomain({
            id: "role-1",
            name: "Test",
            isPreset: false,
            permissions: [
                { action: "read", subject: "item" },
            ],
        });

        expect(Array.isArray(domain.permissions)).toBe(true);
        expect(domain.permissions).toHaveLength(1);
        expect(domain.permissions[0]).toEqual({ action: "read", subject: "item" });
    });

    it("utilise un tableau vide si permissions absentes", () => {
        const domain = userRoleMapper.mapToDomain({
            id: "role-2",
            name: "Vide",
            permissions: undefined as unknown as [],
        });

        expect(domain.permissions).toEqual([]);
    });

    it("inclut isPreset dans le mapping API", () => {
        const api = userRoleMapper.mapToApi({
            id: "Admin",
            name: "Administrateur",
            isPreset: true,
            permissions: [{ action: "manage", subject: "all" }],
        });

        expect(api.isPreset).toBe(true);
        expect(api.permissions).toHaveLength(1);
    });
});
