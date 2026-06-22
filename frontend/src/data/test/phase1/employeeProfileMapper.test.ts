import { describe, expect, it } from "vitest";
import {
    employeeProfileMapper,
    mapEmployeeProfileFormToApi,
} from "../../data-mapper/hr/employeeProfileMapper";

describe("Phase 1 — employeeProfileMapper", () => {
    it("mappe locationId et locationTitle depuis l'API", () => {
        const profile = employeeProfileMapper.mapToDomain({
            id: 1,
            firstName: "Émilie",
            lastName: "Employée",
            email: "employee@crystal.local",
            applicationUserId: "user-1",
            hiringDate: "2024-01-15",
            salary: 43000,
            status: "Active",
            jobPositionId: 2,
            jobPositionName: "Caissier",
            locationId: 3,
            locationTitle: "Succursale Sainte-Foy",
        });

        expect(profile.locationId).toBe(3);
        expect(profile.locationTitle).toBe("Succursale Sainte-Foy");
    });

    it("envoie locationId dans le payload de création/mise à jour", () => {
        const payload = mapEmployeeProfileFormToApi({
            firstName: "Alice",
            lastName: "Martin",
            email: "alice@test.ca",
            applicationUserId: null,
            salary: 50000,
            status: "Active",
            jobPositionId: 1,
            hiringDate: "2024-06-01",
            locationId: 3,
        });

        expect(payload.locationId).toBe(3);
    });
});
