import { useDeleteDialog } from "../../context/DeleteDialogContext";
import { useTimeEntryMutations } from "../../api/mutations/hr/useTimeEntryMutations";
import { useSearchableQuery } from "../../data/hooks/useSearchableQuery";
import { timeEntriesCacheKey } from "../../data/cacheKeys";
import timeEntryService from "../../api/services/hr/timeEntryService";
import PageQueryWrapper from "../../components/layouts/PageQueryWrapper";
import { ROUTE_HR } from "../../data/routeNames";
import { timeEntryColumns } from "../../data/gridColumns";
import { CustomDataGrid } from "../../components/data-grids/CustomDataGrid";
import GenericPageLayout from "../../components/layouts/GenericPageLayout";
import { FORM_TYPES, useFormContainer } from "../../context/FormContext";
import type { TimeEntry } from "../../data/types/hr/timeEntry";
import { usePermissions } from "../../permissions/usePermissions";
import { ENTITY_TYPES } from "../../permissions/permissions";

export default function TimeEntriesPage() {
    const { canCreate, canUpdate, canDelete } = usePermissions(ENTITY_TYPES.TIME_ENTRY);
    const { openForm } = useFormContainer();
    const { openConfirmDeleteWindow } = useDeleteDialog();
    const { deleteTimeEntry: deleteTimeEntryMutation } = useTimeEntryMutations();

    const query = useSearchableQuery({
        queryKey: timeEntriesCacheKey.list(),
        queryFn: () => timeEntryService.getAll(),
        filterFn: (p_entry: TimeEntry, p_search: string) => {
            const normalizedSearch: string = p_search.toLowerCase();
            const employeeName: string =
                `${p_entry.employeeFirstName} ${p_entry.employeeLastName}`.toLowerCase();
            const scheduledShiftLabel: string =
                p_entry.scheduledShiftId !== null ? String(p_entry.scheduledShiftId) : "";
            return (
                employeeName.includes(normalizedSearch)
                || p_entry.date.includes(normalizedSearch)
                || p_entry.startTime.includes(normalizedSearch)
                || (p_entry.endTime ?? "").includes(normalizedSearch)
                || scheduledShiftLabel.includes(normalizedSearch)
            );
        },
    });

    return (
        <PageQueryWrapper
            isLoading={query.isLoading}
            error={query.error}
            refetch={query.refetch}
            errorReturnUrl={ROUTE_HR}
            errorReturnLabel="Retour au tableau de bord RH"
            customErrorMessage="Impossible de charger les pointages."
        >
            <GenericPageLayout title="Pointages">
                <CustomDataGrid
                    rows={query.filteredData}
                    columns={timeEntryColumns}
                    addLabel="Ajouter un pointage"
                    onAddClick={canCreate ? () => openForm(FORM_TYPES.TIME_ENTRY) : undefined}
                    onEditClick={
                        canUpdate
                            ? (p_entry: TimeEntry) => openForm(FORM_TYPES.TIME_ENTRY, p_entry)
                            : undefined
                    }
                    onDeleteClick={
                        canDelete
                            ? (p_entry: TimeEntry) =>
                                openConfirmDeleteWindow({
                                    id: String(p_entry.id),
                                    displayLabel: `${p_entry.employeeFirstName} ${p_entry.employeeLastName} (${p_entry.date})`,
                                    onDelete: deleteTimeEntryMutation,
                                })
                            : undefined
                    }
                    {...query.searchProps}
                />
            </GenericPageLayout>
        </PageQueryWrapper>
    );
}
