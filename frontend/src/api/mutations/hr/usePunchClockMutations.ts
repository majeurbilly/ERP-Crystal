import { useMutation, useQueryClient } from "@tanstack/react-query";
import timeEntryService from "../../services/hr/timeEntryService";
import { timeEntriesCacheKey } from "../../../data/cacheKeys";
import type { TimeEntry } from "../../../data/types/hr/timeEntry";

function invalidatePunchClockQueries(p_queryClient: ReturnType<typeof useQueryClient>): void {
    void p_queryClient.invalidateQueries({ queryKey: timeEntriesCacheKey.punchEligibility() });
    void p_queryClient.invalidateQueries({ queryKey: timeEntriesCacheKey.active() });
    void p_queryClient.invalidateQueries({ queryKey: timeEntriesCacheKey.list() });
}

export function usePunchClockMutations() {
    const queryClient = useQueryClient();

    const punchInMutation = useMutation<TimeEntry, Error>({
        mutationFn: () => timeEntryService.punchIn(),
        onSuccess: () => invalidatePunchClockQueries(queryClient),
    });

    const punchOutMutation = useMutation<TimeEntry, Error>({
        mutationFn: () => timeEntryService.punchOut(),
        onSuccess: () => invalidatePunchClockQueries(queryClient),
    });

    return {
        punchIn: punchInMutation.mutateAsync,
        isPunchingIn: punchInMutation.isPending,
        punchInError: punchInMutation.error,

        punchOut: punchOutMutation.mutateAsync,
        isPunchingOut: punchOutMutation.isPending,
        punchOutError: punchOutMutation.error,
    };
}
