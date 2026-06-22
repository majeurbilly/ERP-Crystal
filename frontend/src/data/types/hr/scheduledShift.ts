export interface ScheduledShift {
    id: number;
    employeeProfileId?: number | null;
    employeeFirstName?: string | null;
    employeeLastName?: string | null;
    jobPositionId?: number | null;
    jobPositionName?: string | null;
    jobPositionColor?: string | null;
    locationId?: number | null;
    locationTitle?: string | null;
    date: string;
    startTime: string;
    endTime: string;
    isDeleted: boolean;
}

export interface ScheduledShiftApiDto {
    id: number;
    employeeProfileId?: number | null;
    employeeFirstName?: string | null;
    employeeLastName?: string | null;
    jobPositionId?: number | null;
    jobPositionName?: string | null;
    jobPositionColor?: string | null;
    locationId?: number | null;
    locationTitle?: string | null;
    date: string;
    startTime: string;
    endTime: string;
}

export interface ScheduledShiftFormData {
    employeeProfileId?: number | null;
    jobPositionId?: number | null;
    locationId?: number;
    date: string;
    startTime: string;
    endTime: string;
}
