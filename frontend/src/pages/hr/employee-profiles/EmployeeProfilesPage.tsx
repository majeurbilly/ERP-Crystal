import { useNavigate } from "react-router-dom";
import { useDeleteDialog } from "../../../context/DeleteDialogContext";
import { useEmployeeProfileMutations } from "../../../api/mutations/hr/useEmployeeProfileMutations";
import { useSearchableQuery } from "../../../data/hooks/useSearchableQuery";
import { employeeProfilesCacheKey } from "../../../data/cacheKeys";
import employeeProfileService from "../../../api/services/hr/employeeProfileService";
import PageQueryWrapper from "../../../components/layouts/PageQueryWrapper";
import { ROUTE_EMPLOYEE_PROFILE_DETAILS, ROUTE_HR } from "../../../data/routeNames";
import { employeeProfileColumns } from "../../../data/gridColumns";
import { CustomDataGrid } from "../../../components/data-grids/CustomDataGrid";
import GenericPageLayout from "../../../components/layouts/GenericPageLayout";
import { FORM_TYPES, useFormContainer } from "../../../context/FormContext";
import type { EmployeeProfile } from "../../../data/types/hr/employeeProfile";
import { usePermissions } from "../../../permissions/usePermissions";
import { ENTITY_TYPES } from "../../../permissions/permissions";

export default function EmployeeProfilesPage() {
    const navigate = useNavigate();
    const { canCreate, canUpdate, canDelete } = usePermissions(ENTITY_TYPES.EMPLOYEE_PROFILE);
    const { openForm } = useFormContainer();
    const { openConfirmDeleteWindow } = useDeleteDialog();
    const { deleteEmployeeProfile: deleteEmployeeProfileMutation } = useEmployeeProfileMutations();

    const query = useSearchableQuery({
        queryKey: employeeProfilesCacheKey.list(),
        queryFn: () => employeeProfileService.getAll(),
        filterFn: (p_employee: EmployeeProfile, p_search: string) => {
            const normalizedSearch: string = p_search.toLowerCase();
            const fullName: string = `${p_employee.firstName} ${p_employee.lastName}`.toLowerCase();
            return (
                fullName.includes(normalizedSearch)
                || p_employee.email.toLowerCase().includes(normalizedSearch)
                || p_employee.jobPositionName.toLowerCase().includes(normalizedSearch)
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
            customErrorMessage="Impossible de charger l'annuaire des employés."
        >
            <GenericPageLayout title="Employés">
                <CustomDataGrid
                    rows={query.filteredData}
                    columns={employeeProfileColumns}
                    addLabel="Ajouter un employé"
                    onAddClick={canCreate ? () => openForm(FORM_TYPES.EMPLOYEE_PROFILE) : undefined}
                    onEditClick={
                        canUpdate
                            ? (p_employee: EmployeeProfile) =>
                                openForm(FORM_TYPES.EMPLOYEE_PROFILE, p_employee)
                            : undefined
                    }
                    onDeleteClick={
                        canDelete
                            ? (p_employee: EmployeeProfile) =>
                                openConfirmDeleteWindow({
                                    id: String(p_employee.id),
                                    displayLabel: `${p_employee.firstName} ${p_employee.lastName}`,
                                    onDelete: deleteEmployeeProfileMutation,
                                })
                            : undefined
                    }
                    onRowClick={(p_params) =>
                        navigate(ROUTE_EMPLOYEE_PROFILE_DETAILS.replace(":id", String(p_params.id)))
                    }
                    {...query.searchProps}
                />
            </GenericPageLayout>
        </PageQueryWrapper>
    );
}
