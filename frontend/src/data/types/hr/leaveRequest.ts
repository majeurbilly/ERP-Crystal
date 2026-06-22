export const LEAVE_TYPES = {
    Vacation: "Vacation",
    Sick: "Sick",
    Unpaid: "Unpaid",
    Other: "Other",
} as const;

export const LEAVE_REQUEST_STATUSES = {
    Pending: "Pending",
    Approved: "Approved",
    Rejected: "Rejected",
} as const;

export type LeaveType = (typeof LEAVE_TYPES)[keyof typeof LEAVE_TYPES];
export type LeaveRequestStatus = (typeof LEAVE_REQUEST_STATUSES)[keyof typeof LEAVE_REQUEST_STATUSES];

export interface LeaveRequest {
    id: number;
    employeeProfileId: number;
    employeeFirstName: string;
    employeeLastName: string;
    leaveType: LeaveType;
    status: LeaveRequestStatus;
    startDate: string;
    endDate: string;
    reason: string | null;
    isDeleted: boolean;
}

export interface LeaveRequestApiDto {
    id: number;
    employeeProfileId: number;
    employeeFirstName: string;
    employeeLastName: string;
    leaveType: string;
    status: string;
    startDate: string;
    endDate: string;
    reason: string | null;
}

export interface LeaveRequestFormData {
    employeeProfileId: number;
    leaveType: LeaveType;
    startDate: string;
    endDate: string;
    reason: string | null;
}

export interface LeaveRequestStatusUpdatePayload {
    status: LeaveRequestStatus;
}
