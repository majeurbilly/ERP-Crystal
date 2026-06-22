import { describe, expect, it } from "vitest";
import permissionEntityService from "../../../api/services/hr/permissionEntityService";

describe("Phase 1 — permissionEntityService", () => {
    it("retourne les entités depuis ENTITY_TYPES sans json-server", async () => {
        const entities = await permissionEntityService.getAll();

        expect(entities.length).toBeGreaterThan(10);
        expect(entities.some((p_entity) => p_entity.id === "employee_profile")).toBe(true);
        expect(entities.some((p_entity) => p_entity.id === "leave_request")).toBe(true);
    });
});
