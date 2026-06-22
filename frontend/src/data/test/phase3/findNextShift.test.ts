import { describe, expect, it } from "vitest";
import { findNextShift } from "../../../components/dashboard/DashboardWidgetGrid";
import { getLocalDateKey } from "../../utils/dateUtils";
import type { ScheduledShift } from "../../types/hr/scheduledShift";

function buildShift(p_id: number, p_date: string, p_start: string, p_end: string): ScheduledShift {
    return {
        id: p_id,
        employeeProfileId: 1,
        employeeFirstName: "Test",
        employeeLastName: "User",
        jobPositionId: 1,
        jobPositionName: "Caissier",
        date: p_date,
        startTime: p_start,
        endTime: p_end,
        isDeleted: false,
    };
}

describe("Phase 3 — findNextShift", () => {
    it("retourne le prochain quart à partir d'aujourd'hui", () => {
        const today = getLocalDateKey();
        const tomorrowDate = new Date();
        tomorrowDate.setDate(tomorrowDate.getDate() + 1);
        const tomorrow = getLocalDateKey(tomorrowDate);

        const shifts = [
            buildShift(1, "2020-01-01", "09:00", "17:00"),
            buildShift(2, tomorrow, "10:00", "18:00"),
            buildShift(3, today, "14:00", "22:00"),
        ];

        const next = findNextShift(shifts);
        expect(next?.id).toBe(3);
    });

    it("retourne null si aucun quart futur", () => {
        const shifts = [buildShift(1, "2020-01-01", "09:00", "17:00")];
        expect(findNextShift(shifts)).toBeNull();
    });

    it("retourne null pour une liste vide", () => {
        expect(findNextShift([])).toBeNull();
    });
});
