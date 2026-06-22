import { describe, expect, it } from "vitest";
import { mapTimesheetStatusToApi } from "../../data-mapper/hr/timesheetMapper";
import { TIMESHEET_STATUSES } from "../../types/hr/timesheet";

describe("timesheetMapper", () => {
    it("should map timesheet status updates to API enum values", () => {
        expect(mapTimesheetStatusToApi(TIMESHEET_STATUSES.Draft)).toEqual({ status: 0 });
        expect(mapTimesheetStatusToApi(TIMESHEET_STATUSES.Submitted)).toEqual({ status: 1 });
        expect(mapTimesheetStatusToApi(TIMESHEET_STATUSES.Approved)).toEqual({ status: 2 });
        expect(mapTimesheetStatusToApi(TIMESHEET_STATUSES.Rejected)).toEqual({ status: 3 });
    });
});
