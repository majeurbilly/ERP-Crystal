import { describe, expect, it, vi, beforeEach } from "vitest";
import permissionEntityService from "../../../api/services/hr/permissionEntityService";
import apiClient from "../../../api/apiClient";

vi.mock("../../../api/apiClient", () => ({
    default: {
        get: vi.fn(),
    },
}));

describe("Phase 2 — permissionEntityService", () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it("charge les entités depuis l'API", async () => {
        vi.mocked(apiClient.get).mockResolvedValue({
            data: [{ id: "employee_profile" }, { id: "user_role" }],
        } as never);

        const entities = await permissionEntityService.getAll();

        expect(apiClient.get).toHaveBeenCalledWith("/api/permission-entities");
        expect(entities).toHaveLength(2);
        expect(entities[0].id).toBe("employee_profile");
    });

    it("utilise le fallback local si l'API échoue", async () => {
        vi.mocked(apiClient.get).mockRejectedValue(new Error("Network error"));

        const entities = await permissionEntityService.getAll();

        expect(entities.length).toBeGreaterThan(0);
        expect(entities.some((p_entity) => p_entity.id === "item")).toBe(true);
    });
});
