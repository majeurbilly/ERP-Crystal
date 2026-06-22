import { describe, expect, it } from "vitest";
import {
    ROUTE_LEAVE_REQUESTS,
    ROUTE_MON_ESPACE,
    ROUTE_SCHEDULES,
    ROUTE_TIMESHEETS,
} from "../../routeNames";
import { LEAVE_REQUEST_STATUSES } from "../../types/hr/leaveRequest";
import { TIMESHEET_STATUSES } from "../../types/hr/timesheet";

function buildEmployeeWidgetLinks(): { schedule: string; leaves: string } {
    return {
        schedule: `${ROUTE_MON_ESPACE}?tab=horaire`,
        leaves: `${ROUTE_MON_ESPACE}?tab=conges`,
    };
}

function buildManagerWidgetLinks(): { schedule: string; leaves: string; timesheets: string } {
    return {
        schedule: ROUTE_SCHEDULES,
        leaves: `${ROUTE_LEAVE_REQUESTS}?status=${LEAVE_REQUEST_STATUSES.Pending}`,
        timesheets: `${ROUTE_TIMESHEETS}?status=${TIMESHEET_STATUSES.Submitted}`,
    };
}

describe("Phase 3 — liens widgets dashboard", () => {
    it("dirige l'employé vers Mon espace avec onglets", () => {
        const links = buildEmployeeWidgetLinks();
        expect(links.schedule).toBe("/mon-espace?tab=horaire");
        expect(links.leaves).toBe("/mon-espace?tab=conges");
    });

    it("dirige le gérant vers les pages RH filtrées", () => {
        const links = buildManagerWidgetLinks();
        expect(links.schedule).toBe("/rh/planning");
        expect(links.leaves).toBe("/rh/absences?status=Pending");
        expect(links.timesheets).toBe("/rh/feuilles-de-temps?status=Submitted");
    });
});
