import apiClient from "../../apiClient";
import { API_HR_METRICS_URL } from "../../apiBaseUrl";
import type { HrDashboardMetrics, HrDashboardMetricsApiDto } from "../../../data/types/hr/hrDashboardMetrics";

class HrMetricsService {
    async getDashboardMetrics(): Promise<HrDashboardMetrics> {
        const response = await apiClient.get<HrDashboardMetricsApiDto>(API_HR_METRICS_URL);
        return this.mapToDomain(response.data);
    }

    private mapToDomain(p_dto: HrDashboardMetricsApiDto): HrDashboardMetrics {
        return {
            totalActiveEmployees: p_dto.totalActiveEmployees,
            pendingTimesheetsCount: p_dto.pendingTimesheetsCount,
            pendingLeaveRequestsCount: p_dto.pendingLeaveRequestsCount,
            totalGrossPayroll: p_dto.totalGrossPayroll,
        };
    }
}

const hrMetricsService = new HrMetricsService();
export default hrMetricsService;
