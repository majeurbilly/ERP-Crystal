import { useMutation, useQueryClient } from "@tanstack/react-query";
import { payPeriodsCacheKey, payStubsCacheKey, timesheetsCacheKey } from "../../../data/cacheKeys";
import type {
    GeneratePayrollForPeriodRequest,
    GeneratePayrollForPeriodResult,
    GeneratePayrollRequest,
    PayStub
} from "../../../data/types/hr/payStub";
import payrollService from "../../services/hr/payrollService";

export const usePayrollMutations = () => {
    const queryClient = useQueryClient();
    const listQueryKey = payStubsCacheKey.list();

    const generateMutation = useMutation({
        mutationFn: (p_data: GeneratePayrollRequest) => payrollService.generatePayStub(p_data),
        onSuccess: () => {
            void queryClient.invalidateQueries({ queryKey: listQueryKey });
            void queryClient.invalidateQueries({ queryKey: payPeriodsCacheKey.list() });
        },
    });

    const generateForPeriodMutation = useMutation({
        mutationFn: (p_data: GeneratePayrollForPeriodRequest) =>
            payrollService.generatePayrollForPeriod(p_data),
        onSuccess: () => {
            void queryClient.invalidateQueries({ queryKey: listQueryKey });
        },
    });

    const publishMutation = useMutation({
        mutationFn: (p_id: number) => payrollService.publishPayStub(p_id),
        onSuccess: () => {
            void queryClient.invalidateQueries({ queryKey: listQueryKey });
            void queryClient.invalidateQueries({ queryKey: timesheetsCacheKey.all });
        },
    });

    return {
        generatePayStub: generateMutation.mutateAsync,
        isGeneratingPayStub: generateMutation.isPending,
        generatePayStubError: generateMutation.error,
        generatedPayStub: generateMutation.data as PayStub | undefined,
        generatePayrollForPeriod: generateForPeriodMutation.mutateAsync,
        isGeneratingPayrollForPeriod: generateForPeriodMutation.isPending,
        generatePayrollForPeriodError: generateForPeriodMutation.error,
        generatedPayrollForPeriod: generateForPeriodMutation.data as
            | GeneratePayrollForPeriodResult
            | undefined,
        publishPayStub: publishMutation.mutateAsync,
        isPublishingPayStub: publishMutation.isPending,
        publishPayStubError: publishMutation.error,
        publishedPayStub: publishMutation.data as PayStub | undefined,
    };
};
