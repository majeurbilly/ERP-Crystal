import { jobPositionsCacheKey } from "../../../data/cacheKeys";
import type { JobPosition, JobPositionFormData } from "../../../data/types/hr/jobPosition";
import jobPositionService from "../../services/hr/jobPositionService";
import { useGenericMutations } from "../useGenericMutation";

export const useJobPositionMutations = () => {
    const mutations = useGenericMutations<JobPosition, JobPositionFormData>(
        jobPositionService,
        jobPositionsCacheKey.list(),
        (p_variables) => [jobPositionsCacheKey.details(p_variables.id)]
    );

    return {
        addJobPosition: mutations.add,
        isAddingJobPosition: mutations.isAdding,
        addJobPositionError: mutations.addError,

        deleteJobPosition: mutations.delete,
        isDeletingJobPosition: mutations.isDeleting,
        deleteJobPositionError: mutations.deleteError,

        updateJobPosition: mutations.update,
        isUpdatingJobPosition: mutations.isUpdating,
        updateJobPositionError: mutations.updateError,
    };
};
