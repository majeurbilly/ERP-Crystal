import { useDeleteDialog } from "../../context/DeleteDialogContext";
import { useJobPositionMutations } from "../../api/mutations/hr/useJobPositionMutations";
import { useSearchableQuery } from "../../data/hooks/useSearchableQuery";
import { jobPositionsCacheKey } from "../../data/cacheKeys";
import jobPositionService from "../../api/services/hr/jobPositionService";
import PageQueryWrapper from "../../components/layouts/PageQueryWrapper";
import { ROUTE_HR } from "../../data/routeNames";
import { jobPositionColumns } from "../../data/gridColumns";
import { CustomDataGrid } from "../../components/data-grids/CustomDataGrid";
import GenericPageLayout from "../../components/layouts/GenericPageLayout";
import { FORM_TYPES, useFormContainer } from "../../context/FormContext";
import type { JobPosition } from "../../data/types/hr/jobPosition";
import { usePermissions } from "../../permissions/usePermissions";
import { ENTITY_TYPES } from "../../permissions/permissions";

export default function JobPositionsPage() {
    const { canCreate, canUpdate, canDelete } = usePermissions(ENTITY_TYPES.JOB_POSITION);
    const { openForm } = useFormContainer();
    const { openConfirmDeleteWindow } = useDeleteDialog();
    const { deleteJobPosition: deleteJobPositionMutation } = useJobPositionMutations();

    const query = useSearchableQuery({
        queryKey: jobPositionsCacheKey.list(),
        queryFn: () => jobPositionService.getAll(),
        filterFn: (p_jobPosition: JobPosition, p_search: string) =>
            p_jobPosition.name.toLowerCase().includes(p_search.toLowerCase())
            || p_jobPosition.description.toLowerCase().includes(p_search.toLowerCase()),
    });

    return (
        <PageQueryWrapper
            isLoading={query.isLoading}
            error={query.error}
            refetch={query.refetch}
            errorReturnUrl={ROUTE_HR}
            errorReturnLabel="Retour au tableau de bord RH"
            customErrorMessage="Impossible de charger la liste des postes."
        >
            <GenericPageLayout title="Postes">
                <CustomDataGrid
                    rows={query.filteredData}
                    columns={jobPositionColumns}
                    addLabel="Ajouter un poste"
                    onAddClick={canCreate ? () => openForm(FORM_TYPES.JOB_POSITION) : undefined}
                    onEditClick={
                        canUpdate
                            ? (p_jobPosition: JobPosition) => openForm(FORM_TYPES.JOB_POSITION, p_jobPosition)
                            : undefined
                    }
                    onDeleteClick={
                        canDelete
                            ? (p_jobPosition: JobPosition) =>
                                openConfirmDeleteWindow({
                                    id: String(p_jobPosition.id),
                                    displayLabel: p_jobPosition.name,
                                    onDelete: deleteJobPositionMutation,
                                })
                            : undefined
                    }
                    {...query.searchProps}
                />
            </GenericPageLayout>
        </PageQueryWrapper>
    );
}
