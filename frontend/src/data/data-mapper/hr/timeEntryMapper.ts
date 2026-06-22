import type {
    TimeEntry,
    TimeEntryApiDto,
    TimeEntryFormData,
} from "../../types/hr/timeEntry";
import { createDataMapper } from "../dataMapper";
import {
    formatTimeForApi,
    normalizeTimeToHHmm,
} from "./scheduledShiftMapper";

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

function formatEndTimeForApi(p_time: string | null): string | null {
    if (p_time === null || p_time.trim().length === 0) {
        return null;
    }
    return formatTimeForApi(p_time);
}

export const timeEntryMapper = createDataMapper<TimeEntryApiDto, TimeEntry>({
    toDomain: (p_dto: TimeEntryApiDto): TimeEntry => ({
        id: p_dto.id,
        employeeProfileId: p_dto.employeeProfileId,
        employeeFirstName: p_dto.employeeFirstName,
        employeeLastName: p_dto.employeeLastName,
        scheduledShiftId: p_dto.scheduledShiftId,
        date: normalizeDate(p_dto.date),
        startTime: normalizeTimeToHHmm(p_dto.startTime),
        endTime: normalizeEndTime(p_dto.endTime),
        isDeleted: false,
    }),
    toApi: (p_domain: TimeEntry): TimeEntryApiDto => ({
        id: p_domain.id,
        employeeProfileId: p_domain.employeeProfileId,
        employeeFirstName: p_domain.employeeFirstName,
        employeeLastName: p_domain.employeeLastName,
        scheduledShiftId: p_domain.scheduledShiftId,
        date: normalizeDate(p_domain.date),
        startTime: formatTimeForApi(p_domain.startTime),
        endTime: formatEndTimeForApi(p_domain.endTime),
    }),
});

export interface TimeEntryApiPayload {
    employeeProfileId: number;
    scheduledShiftId: number | null;
    date: string;
    startTime: string;
    endTime: string | null;
}

export function mapTimeEntryFormToApi(p_form: TimeEntryFormData): TimeEntryApiPayload {
    return {
        employeeProfileId: p_form.employeeProfileId,
        scheduledShiftId: p_form.scheduledShiftId,
        date: normalizeDate(p_form.date),
        startTime: formatTimeForApi(p_form.startTime),
        endTime: formatEndTimeForApi(p_form.endTime),
    };
}
