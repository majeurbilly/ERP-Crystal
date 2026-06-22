import type {
    ScheduledShift,
    ScheduledShiftApiDto,
    ScheduledShiftFormData,
} from "../../types/hr/scheduledShift";
import type { EmployeeProfile } from "../../types/hr/employeeProfile";
import { createDataMapper } from "../dataMapper";
import { resolveJobPositionColor } from "../../types/hr/jobPositionColors";

function normalizeDate(p_date: string | null | undefined): string {
    if (p_date === null || p_date === undefined || p_date.trim().length === 0) {
        return "";
    }
    return p_date.length >= 10 ? p_date.substring(0, 10) : p_date;
}

/** Normalizes API or HTML time values to HH:mm for display and form inputs. */
export function normalizeTimeToHHmm(p_time: string | null | undefined): string {
    if (p_time === null || p_time === undefined || p_time.trim().length === 0) {
        return "";
    }
    const trimmed: string = p_time.trim();
    const parts: string[] = trimmed.split(":");
    if (parts.length < 2) {
        return trimmed;
    }
    const hours: string = parts[0].padStart(2, "0");
    const minutes: string = parts[1].padStart(2, "0");
    return `${hours}:${minutes}`;
}

/** Formats HH:mm for API TimeOnly (HH:mm:ss). */
export function formatTimeForApi(p_time: string): string {
    const normalized: string = normalizeTimeToHHmm(p_time);
    if (normalized.length === 5) {
        return `${normalized}:00`;
    }
    return normalized;
}

export const scheduledShiftMapper = createDataMapper<ScheduledShiftApiDto, ScheduledShift>({
    toDomain: (p_dto: ScheduledShiftApiDto): ScheduledShift => ({
        id: p_dto.id,
        employeeProfileId: p_dto.employeeProfileId ?? null,
        employeeFirstName: p_dto.employeeFirstName ?? null,
        employeeLastName: p_dto.employeeLastName ?? null,
        jobPositionId: p_dto.jobPositionId ?? null,
        jobPositionName: p_dto.jobPositionName ?? null,
        jobPositionColor: resolveJobPositionColor(p_dto.jobPositionColor),
        locationId: p_dto.locationId ?? null,
        locationTitle: p_dto.locationTitle ?? null,
        date: normalizeDate(p_dto.date),
        startTime: normalizeTimeToHHmm(p_dto.startTime),
        endTime: normalizeTimeToHHmm(p_dto.endTime),
        isDeleted: false,
    }),
    toApi: (p_domain: ScheduledShift): ScheduledShiftApiDto => ({
        id: p_domain.id,
        employeeProfileId: p_domain.employeeProfileId,
        employeeFirstName: p_domain.employeeFirstName,
        employeeLastName: p_domain.employeeLastName,
        jobPositionId: p_domain.jobPositionId,
        jobPositionName: p_domain.jobPositionName,
        jobPositionColor: resolveJobPositionColor(p_domain.jobPositionColor),
        locationId: p_domain.locationId ?? null,
        locationTitle: p_domain.locationTitle ?? null,
        date: normalizeDate(p_domain.date),
        startTime: formatTimeForApi(p_domain.startTime),
        endTime: formatTimeForApi(p_domain.endTime),
    }),
});

export interface ScheduledShiftApiPayload {
    employeeProfileId: number | null;
    locationId: number;
    jobPositionId: number;
    date: string;
    startTime: string;
    endTime: string;
}

export function mapScheduledShiftFormToApi(
    p_form: ScheduledShiftFormData,
    p_employees?: EmployeeProfile[]
): ScheduledShiftApiPayload {
    const employeeProfileId: number | null =
        p_form.employeeProfileId !== null
            && p_form.employeeProfileId !== undefined
            && p_form.employeeProfileId > 0
            ? p_form.employeeProfileId
            : null;

    let jobPositionId: number =
        p_form.jobPositionId !== null
            && p_form.jobPositionId !== undefined
            && p_form.jobPositionId > 0
            ? p_form.jobPositionId
            : 0;

    if (employeeProfileId !== null && jobPositionId <= 0 && p_employees) {
        const employee: EmployeeProfile | undefined = p_employees.find(
            (p_employee) => p_employee.id === employeeProfileId
        );
        if (employee && employee.jobPositionId > 0) {
            jobPositionId = employee.jobPositionId;
        }
    }

    return {
        employeeProfileId,
        locationId: p_form.locationId ?? 0,
        jobPositionId,
        date: normalizeDate(p_form.date),
        startTime: formatTimeForApi(p_form.startTime),
        endTime: formatTimeForApi(p_form.endTime),
    };
}
