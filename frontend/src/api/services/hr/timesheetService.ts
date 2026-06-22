import apiClient from "../../apiClient";
import { BaseService } from "../baseService";
import { API_TIMESHEETS_URL } from "../../apiBaseUrl";
import type {
    GenerateWeeklyTimesheetsApiDto,
    GenerateWeeklyTimesheetsApiPayload,
    GenerateWeeklyTimesheetsFormData,
    GenerateWeeklyTimesheetsResult,
    Timesheet,
    TimesheetApiDto,
    TimesheetFormData,
    TimesheetStatus,
} from "../../../data/types/hr/timesheet";
import {
    mapGenerateWeeklyTimesheetsFormToApi,
    mapGenerateWeeklyTimesheetsResultToDomain,
    mapTimesheetFormToApi,
    mapTimesheetStatusToApi,
    timesheetMapper,
    type TimesheetApiPayload,
} from "../../../data/data-mapper/hr/timesheetMapper";
import type { TimeEntryFormData } from "../../../data/types/hr/timeEntry";
import {
    mapTimeEntryFormToApi,
    type TimeEntryApiPayload,
} from "../../../data/data-mapper/hr/timeEntryMapper";

class TimesheetService {
    private m_api = new BaseService<TimesheetApiDto, TimesheetApiPayload>(API_TIMESHEETS_URL);

    async getAll(): Promise<Timesheet[]> {
        const rawData: TimesheetApiDto[] = await this.m_api.getAll();
        return timesheetMapper.mapCollectionToDomain(rawData);
    }

    async getById(p_id: string): Promise<Timesheet> {
        const rawData: TimesheetApiDto = await this.m_api.getById(p_id);
        return timesheetMapper.mapToDomain(rawData);
    }

    async add(p_data: TimesheetFormData): Promise<Timesheet> {
        const payload: TimesheetApiPayload = mapTimesheetFormToApi(p_data);
        const response: TimesheetApiDto = await this.m_api.add(payload);
        return timesheetMapper.mapToDomain(response);
    }

    async update(p_id: string, p_data: TimesheetFormData): Promise<Timesheet> {
        const payload: TimesheetApiPayload = mapTimesheetFormToApi(p_data);
        const response: TimesheetApiDto = await this.m_api.update(
            p_id,
            payload as unknown as Partial<TimesheetApiDto>
        );
        return timesheetMapper.mapToDomain(response);
    }

    async updateStatus(p_id: number, p_status: TimesheetStatus): Promise<Timesheet> {
        const payload = mapTimesheetStatusToApi(p_status);
        const response = await apiClient.patch<TimesheetApiDto>(
            `${API_TIMESHEETS_URL}/${p_id}/status`,
            payload
        );
        return timesheetMapper.mapToDomain(response.data);
    }

    async updatePaid(p_id: number, p_isPaid: boolean): Promise<Timesheet> {
        const response = await apiClient.patch<TimesheetApiDto>(
            `${API_TIMESHEETS_URL}/${p_id}/paid`,
            { isPaid: p_isPaid }
        );
        return timesheetMapper.mapToDomain(response.data);
    }

    async generateWeekly(
        p_data: GenerateWeeklyTimesheetsFormData
    ): Promise<GenerateWeeklyTimesheetsResult> {
        const payload: GenerateWeeklyTimesheetsApiPayload =
            mapGenerateWeeklyTimesheetsFormToApi(p_data);
        const response = await apiClient.post<GenerateWeeklyTimesheetsApiDto>(
            `${API_TIMESHEETS_URL}/generate-weekly`,
            payload
        );
        return mapGenerateWeeklyTimesheetsResultToDomain(response.data);
    }

    async reloadTimeEntries(p_id: number): Promise<Timesheet> {
        const response = await apiClient.post<TimesheetApiDto>(
            `${API_TIMESHEETS_URL}/${p_id}/reload-time-entries`
        );
        return timesheetMapper.mapToDomain(response.data);
    }

    async addTimeEntry(p_id: number, p_data: TimeEntryFormData): Promise<Timesheet> {
        const payload: TimeEntryApiPayload = mapTimeEntryFormToApi(p_data);
        const response = await apiClient.post<TimesheetApiDto>(
            `${API_TIMESHEETS_URL}/${p_id}/time-entries`,
            payload
        );
        return timesheetMapper.mapToDomain(response.data);
    }

    async updateTimeEntry(
        p_id: number,
        p_timeEntryId: number,
        p_data: TimeEntryFormData
    ): Promise<Timesheet> {
        const payload: TimeEntryApiPayload = mapTimeEntryFormToApi(p_data);
        const response = await apiClient.put<TimesheetApiDto>(
            `${API_TIMESHEETS_URL}/${p_id}/time-entries/${p_timeEntryId}`,
            payload
        );
        return timesheetMapper.mapToDomain(response.data);
    }

    async removeTimeEntry(p_id: number, p_timeEntryId: number): Promise<Timesheet> {
        const response = await apiClient.delete<TimesheetApiDto>(
            `${API_TIMESHEETS_URL}/${p_id}/time-entries/${p_timeEntryId}`
        );
        return timesheetMapper.mapToDomain(response.data);
    }

    async delete(p_id: string): Promise<void> {
        await this.m_api.delete(p_id);
    }
}

const timesheetService = new TimesheetService();
export default timesheetService;
