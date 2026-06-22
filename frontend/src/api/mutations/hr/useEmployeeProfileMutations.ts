import { employeeProfilesCacheKey } from "../../../data/cacheKeys";
import type { EmployeeProfile, EmployeeProfileFormData } from "../../../data/types/hr/employeeProfile";
import employeeProfileService from "../../services/hr/employeeProfileService";
import { useGenericMutations } from "../useGenericMutation";

export const useEmployeeProfileMutations = () => {
    const mutations = useGenericMutations<EmployeeProfile, EmployeeProfileFormData>(
        employeeProfileService,
        employeeProfilesCacheKey.list(),
        (p_variables) => [employeeProfilesCacheKey.details(p_variables.id)]
    );

    return {
        addEmployeeProfile: mutations.add,
        isAddingEmployeeProfile: mutations.isAdding,
        addEmployeeProfileError: mutations.addError,

        deleteEmployeeProfile: mutations.delete,
        isDeletingEmployeeProfile: mutations.isDeleting,
        deleteEmployeeProfileError: mutations.deleteError,

        updateEmployeeProfile: mutations.update,
        isUpdatingEmployeeProfile: mutations.isUpdating,
        updateEmployeeProfileError: mutations.updateError,
    };
};
