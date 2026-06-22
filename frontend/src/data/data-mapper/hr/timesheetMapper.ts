import type {
    Timesheet,
    TimesheetApiDto,
    TimesheetFormData,
    TimesheetStatus,
    TimesheetStatusUpdatePayload,
    GenerateWeeklyTimesheetsApiDto,
    GenerateWeeklyTimesheetsFormData,
    GenerateWeeklyTimesheetsResult,
    GenerateWeeklyTimesheetsApiPayload,
} from "../../types/hr/timesheet";
import { TIMESHEET_STATUSES } from "../../types/hr/timesheet";
import type { TimeEntry, TimeEntryApiDto } from "../../types/hr/timeEntry";
import { createDataMapper } from "../dataMapper";
import { normalizeTimeToHHmm } from "./scheduledShiftMapper";

const TIMESHEET_STATUS_API_VALUES: Record<TimesheetStatus, number> = {
    [TIMESHEET_STATUSES.Draft]: 0,
    [TIMESHEET_STATUSES.Submitted]: 1,
    [TIMESHEET_STATUSES.Approved]: 2,
    [TIMESHEET_STATUSES.Rejected]: 3,
};

function normalizeDate(p_date: string | null | undefined): string {
    if (p_date === null || p_date === undefined || p_date.trim().length === 0) {
        return "";
    }
    return p_date.length >= 10 ? p_date.substring(0, 10) : p_date;
}

function normalizeEndTime(p_time: string | null | undefined): string | null {
    if (p_time === null || p_time === undefined || p_time.trim().length === 0) {
        return null;
    }
    return normalizeTimeToHHmm(p_time);
}

function mapTimeEntryApiToDomain(p_dto: TimeEntryApiDto): TimeEntry {
    return {
        id: p_dto.id,
        employeeProfileId: p_dto.employeeProfileId,
        employeeFirstName: p_dto.employeeFirstName,
        employeeLastName: p_dto.employeeLastName,
        scheduledShiftId: p_dto.scheduledShiftId,
        date: normalizeDate(p_dto.date),
        startTime: normalizeTimeToHHmm(p_dto.startTime),
        endTime: normalizeEndTime(p_dto.endTime),
        isDeleted: false,
    };
}

function parseTimesheetStatus(p_value: string): TimesheetStatus {
    const values: TimesheetStatus[] = Object.values(TIMESHEET_STATUSES);
    const match: TimesheetStatus | undefined = values.find(
        (p_status: TimesheetStatus) => p_status === p_value
    );
    return match ?? TIMESHEET_STATUSES.Draft;
}

export const timesheetMapper = createDataMapper<TimesheetApiDto, Timesheet>({
    toDomain: (p_dto: TimesheetApiDto): Timesheet => ({
        id: p_dto.id,
        employeeProfileId: p_dto.employeeProfileId,
        employeeFirstName: p_dto.employeeFirstName,
        employeeLastName: p_dto.employeeLastName,
        periodStart: normalizeDate(p_dto.periodStart),
        periodEnd: normalizeDate(p_dto.periodEnd),
        status: parseTimesheetStatus(p_dto.status),
        isPaid: p_dto.isPaid,
        timeEntries: (p_dto.timeEntries ?? []).map(mapTimeEntryApiToDomain),
        isDeleted: false,
    }),
    toApi: (p_domain: Timesheet): TimesheetApiDto => ({
        id: p_domain.id,
        employeeProfileId: p_domain.employeeProfileId,
        employeeFirstName: p_domain.employeeFirstName,
        employeeLastName: p_domain.employeeLastName,
        periodStart: normalizeDate(p_domain.periodStart),
        periodEnd: normalizeDate(p_domain.periodEnd),
        status: p_domain.status,
        isPaid: p_domain.isPaid,
        timeEntries: [],
    }),
});

export interface TimesheetApiPayload {
    employeeProfileId: number;
    periodStart: string;
    periodEnd: string;
    timeEntryIds: number[];
}

export function mapTimesheetFormToApi(p_form: TimesheetFormData): TimesheetApiPayload {
    return {
        employeeProfileId: p_form.employeeProfileId,
        periodStart: normalizeDate(p_form.periodStart),
        periodEnd: normalizeDate(p_form.periodEnd),
        timeEntryIds: p_form.timeEntryIds,
    };
}

export function mapTimesheetStatusToApi(
    p_status: TimesheetStatus
): TimesheetStatusUpdatePayload {
    return { status: TIMESHEET_STATUS_API_VALUES[p_status] };
}

export function mapGenerateWeeklyTimesheetsFormToApi(
    p_form: GenerateWeeklyTimesheetsFormData
): GenerateWeeklyTimesheetsApiPayload {
    return {
        periodStart: normalizeDate(p_form.periodStart),
        locationId: p_form.locationId,
    };
}

export function mapGenerateWeeklyTimesheetsResultToDomain(
    p_dto: GenerateWeeklyTimesheetsApiDto
): GenerateWeeklyTimesheetsResult {
    return {
        periodStart: normalizeDate(p_dto.periodStart),
        periodEnd: normalizeDate(p_dto.periodEnd),
        locationId: p_dto.locationId,
        createdCount: p_dto.createdCount,
        existingCount: p_dto.existingCount,
        linkedTimeEntryCount: p_dto.linkedTimeEntryCount,
        timesheets: timesheetMapper.mapCollectionToDomain(p_dto.timesheets ?? []),
    };
}
