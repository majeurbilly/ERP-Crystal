import { useMutation, useQueryClient } from "@tanstack/react-query";
import { leaveRequestsCacheKey } from "../../../data/cacheKeys";
import type {
    LeaveRequest,
    LeaveRequestFormData,
    LeaveRequestStatus,
} from "../../../data/types/hr/leaveRequest";
import leaveRequestService from "../../services/hr/leaveRequestService";

export const useLeaveRequestMutations = () => {
    const queryClient = useQueryClient();
    const listQueryKey = leaveRequestsCacheKey.list();

    const invalidateList = (): void => {
        void queryClient.invalidateQueries({ queryKey: listQueryKey });
    };

    const addMutation = useMutation({
        mutationFn: (p_data: LeaveRequestFormData) => leaveRequestService.add(p_data),
        onSuccess: () => invalidateList(),
    });

    const deleteMutation = useMutation({
        mutationFn: (p_id: string) => leaveRequestService.delete(p_id),
        onSuccess: () => invalidateList(),
    });

    const updateStatusMutation = useMutation({
        mutationFn: (p_variables: { id: number; status: LeaveRequestStatus }) =>
            leaveRequestService.updateStatus(p_variables.id, p_variables.status),
        onSuccess: (_data: LeaveRequest, p_variables) => {
            invalidateList();
            void queryClient.invalidateQueries({
                queryKey: leaveRequestsCacheKey.details(String(p_variables.id)),
            });
        },
    });

    return {
        addLeaveRequest: addMutation.mutateAsync,
        isAddingLeaveRequest: addMutation.isPending,
        addLeaveRequestError: addMutation.error,

        deleteLeaveRequest: deleteMutation.mutateAsync,
        isDeletingLeaveRequest: deleteMutation.isPending,
        deleteLeaveRequestError: deleteMutation.error,

        updateLeaveRequestStatus: updateStatusMutation.mutateAsync,
        isUpdatingLeaveRequestStatus: updateStatusMutation.isPending,
        updateLeaveRequestStatusError: updateStatusMutation.error,
    };
};
