import { scheduledShiftsCacheKey, timeEntriesCacheKey } from "../../../data/cacheKeys";
import type { ScheduledShift, ScheduledShiftFormData } from "../../../data/types/hr/scheduledShift";
import scheduledShiftService from "../../services/hr/scheduledShiftService";
import { useGenericMutations } from "../useGenericMutation";

export const useScheduledShiftMutations = () => {
    const mutations = useGenericMutations<ScheduledShift, ScheduledShiftFormData>(
        scheduledShiftService,
        scheduledShiftsCacheKey.all,
        (p_variables) => [scheduledShiftsCacheKey.details(p_variables.id)],
        [timeEntriesCacheKey.punchEligibility()]
    );

    return {
        addScheduledShift: mutations.add,
        isAddingScheduledShift: mutations.isAdding,
        addScheduledShiftError: mutations.addError,

        deleteScheduledShift: mutations.delete,
        isDeletingScheduledShift: mutations.isDeleting,
        deleteScheduledShiftError: mutations.deleteError,

        updateScheduledShift: mutations.update,
        isUpdatingScheduledShift: mutations.isUpdating,
        updateScheduledShiftError: mutations.updateError,
    };
};
