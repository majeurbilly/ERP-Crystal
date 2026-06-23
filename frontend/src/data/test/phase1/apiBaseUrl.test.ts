import { describe, expect, it } from "vitest";
import {
    API_AUTHORS_URL,
    API_EMPLOYEE_PROFILES_URL,
    API_SCHEDULES_URL,
    API_URL,
    USE_MOCK_API,
} from "../../../api/apiBaseUrl";

describe("Phase 1 — apiBaseUrl", () => {
    it("USE_MOCK_API est désactivé pour les flux métier", () => {
        expect(USE_MOCK_API).toBe(false);
    });

    it("les endpoints RH pointent vers l'API .NET", () => {
        expect(API_EMPLOYEE_PROFILES_URL).toBe(`${API_URL}/employee-profiles`);
        expect(API_SCHEDULES_URL).toBe(`${API_URL}/schedules`);
        expect(API_AUTHORS_URL).toBe(`${API_URL}/authors`);
    });
});
