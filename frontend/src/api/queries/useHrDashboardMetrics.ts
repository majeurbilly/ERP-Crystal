import { useQuery } from "@tanstack/react-query";
import hrMetricsService from "../services/hr/hrMetricsService";
import { HR_METRICS_CACHE_KEY } from "../../data/cacheKeys";
import type { HrDashboardMetrics } from "../../data/types/hr/hrDashboardMetrics";

export function useHrDashboardMetrics(p_options?: { enabled?: boolean }) {
    return useQuery<HrDashboardMetrics, Error>({
        queryKey: HR_METRICS_CACHE_KEY,
        queryFn: () => hrMetricsService.getDashboardMetrics(),
        enabled: p_options?.enabled ?? true,
    });
}
