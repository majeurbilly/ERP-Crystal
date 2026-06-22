import { userRolesCacheKey } from "../../../data/cacheKeys";
import type { DynamicUserRole } from "../../../data/types/hr/dynamicUserRole"
import userRoleService from "../../services/hr/userRoleService";
import { useGenericMutations } from "../useGenericMutation"

export const useUserRoleMutations = () => {
    const mutations = useGenericMutations<DynamicUserRole>(
        userRoleService,
        userRolesCacheKey.list(),
        (variables) => [userRolesCacheKey.details(variables.id)]
    );

    return {
        addUserRole: mutations.add,
        isAddingUserRole: mutations.isAdding,
        addUserRoleError: mutations.addError,

        deleteUserRole: mutations.delete,
        isDeletingUserRole: mutations.isDeleting,
        deleteUserRoleError: mutations.deleteError,

        updateUserRole: mutations.update,
        isUpdatingUserRole: mutations.isUpdating,
        updateUserRoleError: mutations.updateError,
    }
}