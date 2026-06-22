export interface TimeEntry {
    id: number;
    employeeProfileId: number;
    employeeFirstName: string;
    employeeLastName: string;
    scheduledShiftId: number | null;
    date: string;
    startTime: string;
    endTime: string | null;
    isDeleted: boolean;
}

export interface TimeEntryApiDto {
    id: number;
    employeeProfileId: number;
    employeeFirstName: string;
    employeeLastName: string;
    scheduledShiftId: number | null;
    date: string;
    startTime: string;
    endTime: string | null;
}

export interface TimeEntryFormData {
    employeeProfileId: number;
    scheduledShiftId: number | null;
    date: string;
    startTime: string;
    endTime: string | null;
}
