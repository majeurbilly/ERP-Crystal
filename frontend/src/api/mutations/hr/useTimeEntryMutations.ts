import { timeEntriesCacheKey } from "../../../data/cacheKeys";
import type { TimeEntry, TimeEntryFormData } from "../../../data/types/hr/timeEntry";
import timeEntryService from "../../services/hr/timeEntryService";
import { useGenericMutations } from "../useGenericMutation";

export const useTimeEntryMutations = () => {
    const mutations = useGenericMutations<TimeEntry, TimeEntryFormData>(
        timeEntryService,
        timeEntriesCacheKey.list(),
        (p_variables) => [timeEntriesCacheKey.details(p_variables.id)]
    );

    return {
        addTimeEntry: mutations.add,
        isAddingTimeEntry: mutations.isAdding,
        addTimeEntryError: mutations.addError,

        deleteTimeEntry: mutations.delete,
        isDeletingTimeEntry: mutations.isDeleting,
        deleteTimeEntryError: mutations.deleteError,

        updateTimeEntry: mutations.update,
        isUpdatingTimeEntry: mutations.isUpdating,
        updateTimeEntryError: mutations.updateError,
    };
};
