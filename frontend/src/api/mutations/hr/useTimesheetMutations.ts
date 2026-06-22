import { useMutation, useQueryClient } from "@tanstack/react-query";
import { timesheetsCacheKey } from "../../../data/cacheKeys";
import type {
    GenerateWeeklyTimesheetsFormData,
    GenerateWeeklyTimesheetsResult,
    Timesheet,
    TimesheetFormData,
    TimesheetStatus,
} from "../../../data/types/hr/timesheet";
import type { TimeEntryFormData } from "../../../data/types/hr/timeEntry";
import timesheetService from "../../services/hr/timesheetService";

export const useTimesheetMutations = () => {
    const queryClient = useQueryClient();
    const listQueryKey = timesheetsCacheKey.list();

    const invalidateList = (): void => {
        void queryClient.invalidateQueries({ queryKey: listQueryKey });
    };

    const addMutation = useMutation({
        mutationFn: (p_data: TimesheetFormData) => timesheetService.add(p_data),
        onSuccess: () => invalidateList(),
    });

    const deleteMutation = useMutation({
        mutationFn: (p_id: string) => timesheetService.delete(p_id),
        onSuccess: (_data, p_id) => {
            queryClient.setQueryData<Timesheet[]>(listQueryKey, (p_existingTimesheets) =>
                (p_existingTimesheets ?? []).filter(
                    (p_timesheet) => p_timesheet.id !== Number(p_id)
                )
            );
            invalidateList();
        },
    });

    const updateMutation = useMutation({
        mutationFn: (p_variables: { id: string; data: TimesheetFormData }) =>
            timesheetService.update(p_variables.id, p_variables.data),
        onSuccess: (_data: Timesheet, p_variables) => {
            invalidateList();
            void queryClient.invalidateQueries({
                queryKey: timesheetsCacheKey.details(p_variables.id),
            });
        },
    });

    const updateStatusMutation = useMutation({
        mutationFn: (p_variables: { id: number; status: TimesheetStatus }) =>
            timesheetService.updateStatus(p_variables.id, p_variables.status),
        onSuccess: (_data: Timesheet, p_variables) => {
            invalidateList();
            void queryClient.invalidateQueries({
                queryKey: timesheetsCacheKey.details(String(p_variables.id)),
            });
        },
    });

    const updatePaidMutation = useMutation({
        mutationFn: (p_variables: { id: number; isPaid: boolean }) =>
            timesheetService.updatePaid(p_variables.id, p_variables.isPaid),
        onSuccess: (p_data: Timesheet) => {
            setTimesheetCaches(p_data);
            invalidateList();
        },
    });

    const reloadTimeEntriesMutation = useMutation({
        mutationFn: (p_id: number) => timesheetService.reloadTimeEntries(p_id),
        onSuccess: (p_data: Timesheet) => {
            setTimesheetCaches(p_data);
            invalidateList();
        },
    });

    const addTimeEntryMutation = useMutation({
        mutationFn: (p_variables: { timesheetId: number; data: TimeEntryFormData }) =>
            timesheetService.addTimeEntry(p_variables.timesheetId, p_variables.data),
        onSuccess: (p_data: Timesheet) => {
            setTimesheetCaches(p_data);
            invalidateList();
        },
    });

    const updateTimeEntryMutation = useMutation({
        mutationFn: (p_variables: {
            timesheetId: number;
            timeEntryId: number;
            data: TimeEntryFormData;
        }) =>
            timesheetService.updateTimeEntry(
                p_variables.timesheetId,
                p_variables.timeEntryId,
                p_variables.data
            ),
        onSuccess: (p_data: Timesheet) => {
            setTimesheetCaches(p_data);
            invalidateList();
        },
    });

    const removeTimeEntryMutation = useMutation({
        mutationFn: (p_variables: { timesheetId: number; timeEntryId: number }) =>
            timesheetService.removeTimeEntry(p_variables.timesheetId, p_variables.timeEntryId),
        onSuccess: (p_data: Timesheet) => {
            setTimesheetCaches(p_data);
            invalidateList();
        },
    });

    const generateWeeklyMutation = useMutation({
        mutationFn: (p_data: GenerateWeeklyTimesheetsFormData) =>
            timesheetService.generateWeekly(p_data),
        onSuccess: (p_data: GenerateWeeklyTimesheetsResult) => {
            queryClient.setQueryData<Timesheet[]>(listQueryKey, (p_existingTimesheets) => {
                const timesheetsById = new Map<number, Timesheet>(
                    (p_existingTimesheets ?? []).map((p_timesheet) => [p_timesheet.id, p_timesheet])
                );

                for (const generatedTimesheet of p_data.timesheets) {
                    timesheetsById.set(generatedTimesheet.id, generatedTimesheet);
                }

                return Array.from(timesheetsById.values());
            });
            invalidateList();
        },
    });

    function setTimesheetCaches(p_timesheet: Timesheet): void {
        queryClient.setQueryData(timesheetsCacheKey.details(String(p_timesheet.id)), p_timesheet);
        queryClient.setQueryData<Timesheet[]>(listQueryKey, (p_existingTimesheets) =>
            (p_existingTimesheets ?? []).map((p_existingTimesheet) =>
                p_existingTimesheet.id === p_timesheet.id ? p_timesheet : p_existingTimesheet
            )
        );
    }

    return {
        addTimesheet: addMutation.mutateAsync,
        isAddingTimesheet: addMutation.isPending,
        addTimesheetError: addMutation.error,

        deleteTimesheet: deleteMutation.mutateAsync,
        isDeletingTimesheet: deleteMutation.isPending,
        deleteTimesheetError: deleteMutation.error,

        updateTimesheet: updateMutation.mutateAsync,
        isUpdatingTimesheet: updateMutation.isPending,
        updateTimesheetError: updateMutation.error,

        updateTimesheetStatus: updateStatusMutation.mutateAsync,
        isUpdatingTimesheetStatus: updateStatusMutation.isPending,
        updateTimesheetStatusError: updateStatusMutation.error,

        updateTimesheetPaid: updatePaidMutation.mutateAsync,
        isUpdatingTimesheetPaid: updatePaidMutation.isPending,
        updateTimesheetPaidError: updatePaidMutation.error,

        reloadTimesheetTimeEntries: reloadTimeEntriesMutation.mutateAsync,
        isReloadingTimesheetTimeEntries: reloadTimeEntriesMutation.isPending,
        reloadTimesheetTimeEntriesError: reloadTimeEntriesMutation.error,

        addTimesheetTimeEntry: addTimeEntryMutation.mutateAsync,
        isAddingTimesheetTimeEntry: addTimeEntryMutation.isPending,
        addTimesheetTimeEntryError: addTimeEntryMutation.error,

        updateTimesheetTimeEntry: updateTimeEntryMutation.mutateAsync,
        isUpdatingTimesheetTimeEntry: updateTimeEntryMutation.isPending,
        updateTimesheetTimeEntryError: updateTimeEntryMutation.error,

        removeTimesheetTimeEntry: removeTimeEntryMutation.mutateAsync,
        isRemovingTimesheetTimeEntry: removeTimeEntryMutation.isPending,
        removeTimesheetTimeEntryError: removeTimeEntryMutation.error,

        generateWeeklyTimesheets: generateWeeklyMutation.mutateAsync,
        isGeneratingWeeklyTimesheets: generateWeeklyMutation.isPending,
        generateWeeklyTimesheetsError: generateWeeklyMutation.error,
    };
};
