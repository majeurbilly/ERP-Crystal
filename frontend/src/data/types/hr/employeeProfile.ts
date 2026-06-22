export interface EmployeeProfile {
    id: number;
    firstName: string;
    lastName: string;
    email: string;
    hiringDate: string;
    jobPositionId: number;
    jobPositionName: string;
    applicationUserId: string | null;
    salary: number;
    status: string;
    isDeleted: boolean;
    locationId?: number;
    locationTitle?: string | null;
}

export interface EmployeeProfileApiDto {
    id: number;
    firstName: string;
    lastName: string;
    email: string;
    applicationUserId: string | null;
    hiringDate: string;
    salary: number;
    status: string;
    jobPositionId: number;
    jobPositionName: string;
    locationId?: number;
    locationTitle?: string | null;
}

export interface EmployeeProfileFormData {
    firstName: string;
    lastName: string;
    email: string;
    applicationUserId: string | null;
    salary: number;
    status: string;
    jobPositionId?: number;
    hiringDate: string;
    locationId?: number;
}
