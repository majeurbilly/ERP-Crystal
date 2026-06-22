import apiClient from "../../apiClient";
import { BaseService } from "../baseService";
import { API_LEAVE_REQUESTS_URL } from "../../apiBaseUrl";
import type {
    LeaveRequest,
    LeaveRequestApiDto,
    LeaveRequestFormData,
    LeaveRequestStatus,
} from "../../../data/types/hr/leaveRequest";
import {
    leaveRequestMapper,
    mapLeaveRequestFormToApi,
    mapLeaveRequestStatusToApi,
    type LeaveRequestApiPayload,
} from "../../../data/data-mapper/hr/leaveRequestMapper";

class LeaveRequestService {
    private m_api = new BaseService<LeaveRequestApiDto, LeaveRequestApiPayload>(API_LEAVE_REQUESTS_URL);

    async getAll(): Promise<LeaveRequest[]> {
        const rawData: LeaveRequestApiDto[] = await this.m_api.getAll();
        return leaveRequestMapper.mapCollectionToDomain(rawData);
    }

    async getById(p_id: string): Promise<LeaveRequest> {
        const rawData: LeaveRequestApiDto = await this.m_api.getById(p_id);
        return leaveRequestMapper.mapToDomain(rawData);
    }

    async add(p_data: LeaveRequestFormData): Promise<LeaveRequest> {
        const payload: LeaveRequestApiPayload = mapLeaveRequestFormToApi(p_data);
        const response: LeaveRequestApiDto = await this.m_api.add(payload);
        return leaveRequestMapper.mapToDomain(response);
    }

    async updateStatus(p_id: number, p_status: LeaveRequestStatus): Promise<LeaveRequest> {
        const payload = mapLeaveRequestStatusToApi(p_status);
        const response = await apiClient.patch<LeaveRequestApiDto>(
            `${API_LEAVE_REQUESTS_URL}/${p_id}/status`,
            payload
        );
        return leaveRequestMapper.mapToDomain(response.data);
    }

    async delete(p_id: string): Promise<void> {
        await this.m_api.delete(p_id);
    }
}

const leaveRequestService = new LeaveRequestService();
export default leaveRequestService;
