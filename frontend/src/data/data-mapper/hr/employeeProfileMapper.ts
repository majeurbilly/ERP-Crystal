import type {
    EmployeeProfile,
    EmployeeProfileApiDto,
    EmployeeProfileFormData,
} from "../../types/hr/employeeProfile";
import { createDataMapper } from "../dataMapper";

function normalizeHiringDate(p_date: string): string {
    if (p_date.length >= 10) {
        return p_date.substring(0, 10);
    }
    return p_date;
}

export const employeeProfileMapper = createDataMapper<EmployeeProfileApiDto, EmployeeProfile>({
    toDomain: (p_dto: EmployeeProfileApiDto): EmployeeProfile => ({
        id: p_dto.id,
        firstName: p_dto.firstName,
        lastName: p_dto.lastName,
        email: p_dto.email,
        hiringDate: normalizeHiringDate(p_dto.hiringDate),
        jobPositionId: p_dto.jobPositionId,
        jobPositionName: p_dto.jobPositionName,
        applicationUserId: p_dto.applicationUserId ?? null,
        salary: p_dto.salary,
        status: p_dto.status,
        isDeleted: false,
        locationId: p_dto.locationId,
        locationTitle: p_dto.locationTitle ?? null,
    }),
    toApi: (p_domain: EmployeeProfile): EmployeeProfileApiDto => ({
        id: p_domain.id,
        firstName: p_domain.firstName,
        lastName: p_domain.lastName,
        email: p_domain.email,
        applicationUserId: p_domain.applicationUserId,
        hiringDate: normalizeHiringDate(p_domain.hiringDate),
        salary: p_domain.salary,
        status: p_domain.status,
        jobPositionId: p_domain.jobPositionId,
        jobPositionName: p_domain.jobPositionName,
        locationId: p_domain.locationId,
        locationTitle: p_domain.locationTitle ?? null,
    }),
});

export function mapEmployeeProfileFormToApi(p_form: EmployeeProfileFormData): EmployeeProfileFormData {
    const applicationUserId: string | null =
        p_form.applicationUserId && p_form.applicationUserId.trim().length > 0
            ? p_form.applicationUserId.trim()
            : null;

    return {
        firstName: p_form.firstName.trim(),
        lastName: p_form.lastName.trim(),
        email: p_form.email.trim(),
        applicationUserId,
        salary: p_form.salary,
        status: p_form.status.trim(),
        jobPositionId: p_form.jobPositionId ?? undefined,
        hiringDate: normalizeHiringDate(p_form.hiringDate),
        locationId: p_form.locationId,
    };
}
