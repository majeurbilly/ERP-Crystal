import { describe, expect, it } from "vitest";
import { mapScheduledShiftFormToApi, scheduledShiftMapper } from "../../data-mapper/hr/scheduledShiftMapper";
import type { EmployeeProfile } from "../../types/hr/employeeProfile";

describe("Phase 1 — scheduledShiftMapper", () => {
    it("inclut locationId dans le payload API", () => {
        const payload = mapScheduledShiftFormToApi({
            employeeProfileId: 5,
            jobPositionId: 2,
            locationId: 99,
            date: "2026-06-10",
            startTime: "09:00",
            endTime: "17:00",
        });

        expect(payload).toEqual({
            employeeProfileId: 5,
            locationId: 99,
            jobPositionId: 2,
            date: "2026-06-10",
            startTime: "09:00:00",
            endTime: "17:00:00",
        });
    });

    it("déduit le poste depuis l'employé lorsque jobPositionId est absent", () => {
        const employees: EmployeeProfile[] = [
            {
                id: 5,
                firstName: "Nico",
                lastName: "Test",
                email: "nico@test.local",
                hiringDate: "2026-01-01",
                jobPositionId: 3,
                jobPositionName: "Caissier",
                applicationUserId: null,
                salary: 40000,
                status: "Active",
                isDeleted: false,
            },
        ];

        const payload = mapScheduledShiftFormToApi(
            {
                employeeProfileId: 5,
                jobPositionId: null,
                date: "2026-06-13",
                startTime: "09:00",
                endTime: "17:00",
            },
            employees
        );

        expect(payload.employeeProfileId).toBe(5);
        expect(payload.jobPositionId).toBe(3);
    });

    it("envoie null pour employeeProfileId sur un quart ouvert par poste", () => {
        const payload = mapScheduledShiftFormToApi({
            employeeProfileId: null,
            jobPositionId: 2,
            date: "2026-06-13",
            startTime: "09:00",
            endTime: "17:00",
        });

        expect(payload.employeeProfileId).toBeNull();
        expect(payload.jobPositionId).toBe(2);
    });

    it("mappe la couleur du poste sur le quart", () => {
        const shift = scheduledShiftMapper.mapToDomain({
            id: 1,
            employeeProfileId: 5,
            employeeFirstName: "Nico",
            employeeLastName: "Test",
            jobPositionId: 3,
            jobPositionName: "Caissier",
            jobPositionColor: "#22C55E",
            date: "2026-06-13",
            startTime: "09:00:00",
            endTime: "17:00:00",
        });

        expect(shift.jobPositionColor).toBe("#22C55E");
    });
});
