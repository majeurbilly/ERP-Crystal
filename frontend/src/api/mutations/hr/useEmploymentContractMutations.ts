import { useMutation, useQueryClient } from "@tanstack/react-query";
import { employmentContractsCacheKey } from "../../../data/cacheKeys";
import type { EmploymentContractFormData } from "../../../data/types/hr/employmentContract";
import employmentContractService from "../../services/hr/employmentContractService";

export const useEmploymentContractMutations = (p_employeeProfileId?: string) => {
    const queryClient = useQueryClient();
    const invalidateCaches = (p_employeeId?: number): void => {
        void queryClient.invalidateQueries({ queryKey: employmentContractsCacheKey.list() });
        if (p_employeeProfileId) {
            void queryClient.invalidateQueries({ queryKey: employmentContractsCacheKey.byEmployee(p_employeeProfileId) });
        }
        if (p_employeeId !== undefined) {
            void queryClient.invalidateQueries({
                queryKey: employmentContractsCacheKey.byEmployee(String(p_employeeId)),
            });
        }
    };

    const addMutation = useMutation({
        mutationFn: (p_data: EmploymentContractFormData) => employmentContractService.add(p_data),
        onSuccess: (_data, p_variables) => invalidateCaches(p_variables.employeeProfileId),
    });

    const deleteMutation = useMutation({
        mutationFn: (p_id: string) => employmentContractService.delete(p_id),
        onSuccess: () => invalidateCaches(),
    });

    const updateMutation = useMutation({
        mutationFn: ({ id, data }: { id: string; data: EmploymentContractFormData }) =>
            employmentContractService.update(id, data),
        onSuccess: (_data, p_variables) => {
            invalidateCaches(p_variables.data.employeeProfileId);
            void queryClient.invalidateQueries({
                queryKey: employmentContractsCacheKey.details(p_variables.id),
            });
        },
    });

    return {
        addEmploymentContract: addMutation.mutateAsync,
        isAddingEmploymentContract: addMutation.isPending,
        addEmploymentContractError: addMutation.error,

        deleteEmploymentContract: deleteMutation.mutateAsync,
        isDeletingEmploymentContract: deleteMutation.isPending,
        deleteEmploymentContractError: deleteMutation.error,

        updateEmploymentContract: updateMutation.mutateAsync,
        isUpdatingEmploymentContract: updateMutation.isPending,
        updateEmploymentContractError: updateMutation.error,
    };
};
