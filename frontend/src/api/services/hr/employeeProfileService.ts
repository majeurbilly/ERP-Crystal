import { BaseService } from "../baseService";
import { API_EMPLOYEE_PROFILES_URL } from "../../apiBaseUrl";
import type {
    EmployeeProfile,
    EmployeeProfileApiDto,
    EmployeeProfileFormData,
} from "../../../data/types/hr/employeeProfile";
import {
    employeeProfileMapper,
    mapEmployeeProfileFormToApi,
} from "../../../data/data-mapper/hr/employeeProfileMapper";
import apiClient from "../../apiClient";

class EmployeeProfileService {
    private m_api = new BaseService<EmployeeProfileApiDto, EmployeeProfileFormData>(API_EMPLOYEE_PROFILES_URL);

    async getAll(): Promise<EmployeeProfile[]> {
        const rawData: EmployeeProfileApiDto[] = await this.m_api.getAll();
        return employeeProfileMapper.mapCollectionToDomain(rawData);
    }

    async getById(p_id: string): Promise<EmployeeProfile> {
        const rawData: EmployeeProfileApiDto = await this.m_api.getById(p_id);
        return employeeProfileMapper.mapToDomain(rawData);
    }

    async getMe(): Promise<EmployeeProfile> {
        const response = await apiClient.get<EmployeeProfileApiDto>(`${API_EMPLOYEE_PROFILES_URL}/me`);
        return employeeProfileMapper.mapToDomain(response.data);
    }

    async add(p_data: EmployeeProfileFormData): Promise<EmployeeProfile> {
        const payload: EmployeeProfileFormData = mapEmployeeProfileFormToApi(p_data);
        const response: EmployeeProfileApiDto = await this.m_api.add(payload);
        return employeeProfileMapper.mapToDomain(response);
    }

    async update(p_id: string, p_data: Partial<EmployeeProfileFormData>): Promise<EmployeeProfile> {
        const payload: EmployeeProfileFormData = mapEmployeeProfileFormToApi({
            firstName: p_data.firstName ?? "",
            lastName: p_data.lastName ?? "",
            email: p_data.email ?? "",
            applicationUserId: p_data.applicationUserId ?? null,
            salary: p_data.salary ?? 0,
            status: p_data.status ?? "",
            jobPositionId: p_data.jobPositionId ?? undefined,
            hiringDate: p_data.hiringDate ?? "",
            locationId: p_data.locationId,
        });
        const response: EmployeeProfileApiDto = await this.m_api.update(p_id, payload);
        return employeeProfileMapper.mapToDomain(response);
    }

    async delete(p_id: string): Promise<void> {
        await this.m_api.delete(p_id);
    }
}

const employeeProfileService = new EmployeeProfileService();
export default employeeProfileService;
