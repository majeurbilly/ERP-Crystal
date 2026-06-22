import userService from "../../services/hr/userService";
import type { User, UserFormData } from "../../../data/types/hr/user";
import { usersCacheKey } from "../../../data/cacheKeys";
import { useGenericMutations } from "../useGenericMutation";
import { useMutation, useQueryClient } from "@tanstack/react-query";

export const useUserMutations = () => {
    const queryClient = useQueryClient();
    const mutations = useGenericMutations<User, UserFormData>(
        userService,
        usersCacheKey.list(),
        (variables) => [usersCacheKey.details(variables.id)]
    );

    const updateMeMutation = useMutation({
        mutationFn: (data: Partial<UserFormData>) => userService.updateMe(data),
        onSuccess: (updatedUser) => {
            queryClient.invalidateQueries({ queryKey: usersCacheKey.list() });
            queryClient.setQueryData(usersCacheKey.me(), updatedUser);
        }
    });

    return {
        addUser: mutations.add,
        isAddingUser: mutations.isAdding,
        addUserError: mutations.addError,

        deleteUser: mutations.delete,
        isDeletingUser: mutations.isDeleting,
        deleteUserError: mutations.deleteError,

        updateUser: mutations.update,
        isUpdatingUser: mutations.isUpdating,
        updateUserError: mutations.updateError,

        updateMe: updateMeMutation.mutateAsync,
        isUpdatingMe: updateMeMutation.isPending,
        updateMeError: updateMeMutation.error
    }
}