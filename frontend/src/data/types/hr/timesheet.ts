import type { TimeEntry } from "./timeEntry";
import type { TimeEntryApiDto } from "./timeEntry";

export const TIMESHEET_STATUSES = {
    Draft: "Draft",
    Submitted: "Submitted",
    Approved: "Approved",
    Rejected: "Rejected",
} as const;

export type TimesheetStatus = (typeof TIMESHEET_STATUSES)[keyof typeof TIMESHEET_STATUSES];

export interface Timesheet {
    id: number;
    employeeProfileId: number;
    employeeFirstName: string;
    employeeLastName: string;
    periodStart: string;
    periodEnd: string;
    status: TimesheetStatus;
    isPaid: boolean;
    timeEntries: TimeEntry[];
    isDeleted: boolean;
}

export interface TimesheetApiDto {
    id: number;
    employeeProfileId: number;
    employeeFirstName: string;
    employeeLastName: string;
    periodStart: string;
    periodEnd: string;
    status: string;
    isPaid: boolean;
    timeEntries: TimeEntryApiDto[];
}

export interface TimesheetFormData {
    employeeProfileId: number;
    periodStart: string;
    periodEnd: string;
    timeEntryIds: number[];
}

export interface TimesheetStatusUpdatePayload {
    status: number;
}

export interface GenerateWeeklyTimesheetsFormData {
    periodStart: string;
    locationId: number | null;
}

export interface GenerateWeeklyTimesheetsApiPayload {
    periodStart: string;
    locationId: number | null;
}

export interface GenerateWeeklyTimesheetsApiDto {
    periodStart: string;
    periodEnd: string;
    locationId: number | null;
    createdCount: number;
    existingCount: number;
    linkedTimeEntryCount: number;
    timesheets: TimesheetApiDto[];
}

export interface GenerateWeeklyTimesheetsResult {
    periodStart: string;
    periodEnd: string;
    locationId: number | null;
    createdCount: number;
    existingCount: number;
    linkedTimeEntryCount: number;
    timesheets: Timesheet[];
}
