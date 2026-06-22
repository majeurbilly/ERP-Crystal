import type {
    LeaveRequest,
    LeaveRequestApiDto,
    LeaveRequestFormData,
    LeaveRequestStatus,
    LeaveType,
} from "../../types/hr/leaveRequest";
import { LEAVE_REQUEST_STATUSES, LEAVE_TYPES } from "../../types/hr/leaveRequest";
import { createDataMapper } from "../dataMapper";

const LEAVE_TYPE_API_VALUES: Record<LeaveType, number> = {
    [LEAVE_TYPES.Vacation]: 0,
    [LEAVE_TYPES.Sick]: 1,
    [LEAVE_TYPES.Unpaid]: 2,
    [LEAVE_TYPES.Other]: 3,
};

const LEAVE_REQUEST_STATUS_API_VALUES: Record<LeaveRequestStatus, number> = {
    [LEAVE_REQUEST_STATUSES.Pending]: 0,
    [LEAVE_REQUEST_STATUSES.Approved]: 1,
    [LEAVE_REQUEST_STATUSES.Rejected]: 2,
};

function normalizeDate(p_date: string | null | undefined): string {
    if (p_date === null || p_date === undefined || p_date.trim().length === 0) {
        return "";
    }
    return p_date.length >= 10 ? p_date.substring(0, 10) : p_date;
}

function parseLeaveType(p_value: string): LeaveType {
    const values: LeaveType[] = Object.values(LEAVE_TYPES);
    const match: LeaveType | undefined = values.find((p_type: LeaveType) => p_type === p_value);
    return match ?? LEAVE_TYPES.Vacation;
}

function parseLeaveRequestStatus(p_value: string): LeaveRequestStatus {
    const values: LeaveRequestStatus[] = Object.values(LEAVE_REQUEST_STATUSES);
    const match: LeaveRequestStatus | undefined = values.find(
        (p_status: LeaveRequestStatus) => p_status === p_value
    );
    return match ?? LEAVE_REQUEST_STATUSES.Pending;
}

export const leaveRequestMapper = createDataMapper<LeaveRequestApiDto, LeaveRequest>({
    toDomain: (p_dto: LeaveRequestApiDto): LeaveRequest => ({
        id: p_dto.id,
        employeeProfileId: p_dto.employeeProfileId,
        employeeFirstName: p_dto.employeeFirstName,
        employeeLastName: p_dto.employeeLastName,
        leaveType: parseLeaveType(p_dto.leaveType),
        status: parseLeaveRequestStatus(p_dto.status),
        startDate: normalizeDate(p_dto.startDate),
        endDate: normalizeDate(p_dto.endDate),
        reason: p_dto.reason ?? null,
        isDeleted: false,
    }),
    toApi: (p_domain: LeaveRequest): LeaveRequestApiDto => ({
        id: p_domain.id,
        employeeProfileId: p_domain.employeeProfileId,
        employeeFirstName: p_domain.employeeFirstName,
        employeeLastName: p_domain.employeeLastName,
        leaveType: p_domain.leaveType,
        status: p_domain.status,
        startDate: normalizeDate(p_domain.startDate),
        endDate: normalizeDate(p_domain.endDate),
        reason: p_domain.reason,
    }),
});

export interface LeaveRequestApiPayload {
    employeeProfileId: number;
    leaveType: number;
    startDate: string;
    endDate: string;
    reason: string | null;
}

export function mapLeaveRequestFormToApi(p_form: LeaveRequestFormData): LeaveRequestApiPayload {
    const reason: string | null =
        p_form.reason && p_form.reason.trim().length > 0 ? p_form.reason.trim() : null;

    return {
        employeeProfileId: p_form.employeeProfileId,
        leaveType: LEAVE_TYPE_API_VALUES[p_form.leaveType],
        startDate: normalizeDate(p_form.startDate),
        endDate: normalizeDate(p_form.endDate),
        reason,
    };
}

export interface LeaveRequestStatusApiPayload {
    status: number;
}

export function mapLeaveRequestStatusToApi(
    p_status: LeaveRequestStatus
): LeaveRequestStatusApiPayload {
    return {
        status: LEAVE_REQUEST_STATUS_API_VALUES[p_status],
    };
}
