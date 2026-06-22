import { useQuery } from "@tanstack/react-query";
import timeEntryService from "../services/hr/timeEntryService";
import { timeEntriesCacheKey } from "../../data/cacheKeys";
import type { PunchEligibility } from "../../data/types/hr/punchEligibility";

export function usePunchEligibility(p_options?: { enabled?: boolean }) {
    return useQuery<PunchEligibility, Error>({
        queryKey: timeEntriesCacheKey.punchEligibility(),
        queryFn: () => timeEntryService.getPunchEligibility(),
        enabled: p_options?.enabled ?? true,
        refetchInterval: 30_000,
        refetchOnWindowFocus: true,
    });
}
