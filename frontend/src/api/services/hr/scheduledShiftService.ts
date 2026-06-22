import apiClient from "../../apiClient";
import { BaseService } from "../baseService";
import { API_SCHEDULES_URL } from "../../apiBaseUrl";
import type {
    ScheduledShift,
    ScheduledShiftApiDto,
    ScheduledShiftFormData,
} from "../../../data/types/hr/scheduledShift";
import {
    mapScheduledShiftFormToApi,
    scheduledShiftMapper,
    type ScheduledShiftApiPayload,
} from "../../../data/data-mapper/hr/scheduledShiftMapper";

class ScheduledShiftService {
    private m_api = new BaseService<ScheduledShiftApiDto, ScheduledShiftApiPayload>(API_SCHEDULES_URL);

    async getAll(): Promise<ScheduledShift[]> {
        const rawData: ScheduledShiftApiDto[] = await this.m_api.getAll();
        return scheduledShiftMapper.mapCollectionToDomain(rawData);
    }

    async getTeamSchedule(): Promise<ScheduledShift[]> {
        const response = await apiClient.get<ScheduledShiftApiDto[]>(`${API_SCHEDULES_URL}/team`);
        return scheduledShiftMapper.mapCollectionToDomain(response.data);
    }

    async getById(p_id: string): Promise<ScheduledShift> {
        const rawData: ScheduledShiftApiDto = await this.m_api.getById(p_id);
        return scheduledShiftMapper.mapToDomain(rawData);
    }

    async add(p_data: ScheduledShiftFormData): Promise<ScheduledShift> {
        const payload: ScheduledShiftApiPayload = mapScheduledShiftFormToApi(p_data);
        const response: ScheduledShiftApiDto = await this.m_api.add(payload);
        return scheduledShiftMapper.mapToDomain(response);
    }

    async update(
        p_id: string,
        p_data: Partial<ScheduledShift> | Partial<ScheduledShiftFormData>
    ): Promise<ScheduledShift> {
        const payload: ScheduledShiftApiPayload = mapScheduledShiftFormToApi({
            employeeProfileId: p_data.employeeProfileId ?? null,
            jobPositionId: p_data.jobPositionId ?? null,
            locationId: p_data.locationId ?? undefined,
            date: p_data.date ?? "",
            startTime: p_data.startTime ?? "",
            endTime: p_data.endTime ?? "",
        });
        const response: ScheduledShiftApiDto = await this.m_api.update(
            p_id,
            payload as unknown as Partial<ScheduledShiftApiDto>
        );
        return scheduledShiftMapper.mapToDomain(response);
    }

    async delete(p_id: string): Promise<void> {
        await this.m_api.delete(p_id);
    }
}

const scheduledShiftService = new ScheduledShiftService();
export default scheduledShiftService;
