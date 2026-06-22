import { useQuery } from "@tanstack/react-query";
import timeEntryService from "../services/hr/timeEntryService";
import { timeEntriesCacheKey } from "../../data/cacheKeys";
import type { TimeEntry } from "../../data/types/hr/timeEntry";

export function useActiveTimeEntry(p_options?: { enabled?: boolean }) {
    return useQuery<TimeEntry | null, Error>({
        queryKey: timeEntriesCacheKey.active(),
        queryFn: () => timeEntryService.getActive(),
        enabled: p_options?.enabled ?? true,
        refetchInterval: 60_000,
    });
}
