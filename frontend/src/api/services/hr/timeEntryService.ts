import { BaseService } from "../baseService";
import { API_TIME_ENTRIES_URL } from "../../apiBaseUrl";
import apiClient from "../../apiClient";
import type {
    TimeEntry,
    TimeEntryApiDto,
    TimeEntryFormData,
} from "../../../data/types/hr/timeEntry";
import {
    mapTimeEntryFormToApi,
    timeEntryMapper,
    type TimeEntryApiPayload,
} from "../../../data/data-mapper/hr/timeEntryMapper";
import type { PunchEligibility, PunchEligibilityApiDto } from "../../../data/types/hr/punchEligibility";
import { mapPunchEligibilityToDomain } from "../../../data/types/hr/punchEligibility";

class TimeEntryService {
    private m_api = new BaseService<TimeEntryApiDto, TimeEntryApiPayload>(API_TIME_ENTRIES_URL);

    async getAll(): Promise<TimeEntry[]> {
        const rawData: TimeEntryApiDto[] = await this.m_api.getAll();
        return timeEntryMapper.mapCollectionToDomain(rawData);
    }

    async getById(p_id: string): Promise<TimeEntry> {
        const rawData: TimeEntryApiDto = await this.m_api.getById(p_id);
        return timeEntryMapper.mapToDomain(rawData);
    }

    async add(p_data: TimeEntryFormData): Promise<TimeEntry> {
        const payload: TimeEntryApiPayload = mapTimeEntryFormToApi(p_data);
        const response: TimeEntryApiDto = await this.m_api.add(payload);
        return timeEntryMapper.mapToDomain(response);
    }

    async update(p_id: string, p_data: Partial<TimeEntryFormData>): Promise<TimeEntry> {
        const payload: TimeEntryApiPayload = mapTimeEntryFormToApi({
            employeeProfileId: p_data.employeeProfileId ?? 0,
            scheduledShiftId: p_data.scheduledShiftId ?? null,
            date: p_data.date ?? "",
            startTime: p_data.startTime ?? "",
            endTime: p_data.endTime ?? null,
        });
        const response: TimeEntryApiDto = await this.m_api.update(
            p_id,
            payload as unknown as Partial<TimeEntryApiDto>
        );
        return timeEntryMapper.mapToDomain(response);
    }

    async delete(p_id: string): Promise<void> {
        await this.m_api.delete(p_id);
    }

    async getActive(): Promise<TimeEntry | null> {
        const response = await apiClient.get<TimeEntryApiDto | null>(`${API_TIME_ENTRIES_URL}/me/active`, {
            validateStatus: (p_status: number) => p_status === 200 || p_status === 204,
        });
        if (response.status === 204 || response.data === null) {
            return null;
        }
        return timeEntryMapper.mapToDomain(response.data);
    }

    async getPunchEligibility(): Promise<PunchEligibility> {
        const response = await apiClient.get<PunchEligibilityApiDto>(`${API_TIME_ENTRIES_URL}/me/punch-eligibility`);
        return mapPunchEligibilityToDomain(response.data);
    }

    async punchIn(): Promise<TimeEntry> {
        const response = await apiClient.post<TimeEntryApiDto>(`${API_TIME_ENTRIES_URL}/me/punch-in`);
        return timeEntryMapper.mapToDomain(response.data);
    }

    async punchOut(): Promise<TimeEntry> {
        const response = await apiClient.post<TimeEntryApiDto>(`${API_TIME_ENTRIES_URL}/me/punch-out`);
        return timeEntryMapper.mapToDomain(response.data);
    }
}

const timeEntryService = new TimeEntryService();
export default timeEntryService;
