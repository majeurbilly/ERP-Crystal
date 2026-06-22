import { useState } from "react";
import { useNavigate } from "react-router-dom";
import type { GridColDef } from "@mui/x-data-grid";
import { useSearchableQuery } from "../../data/hooks/useSearchableQuery";
import { employmentContractsCacheKey } from "../../data/cacheKeys";
import employmentContractService from "../../api/services/hr/employmentContractService";
import PageQueryWrapper from "../../components/layouts/PageQueryWrapper";
import { ROUTE_EMPLOYEE_PROFILE_DETAILS, ROUTE_HR } from "../../data/routeNames";
import { employmentContractColumns } from "../../data/gridColumns";
import { CustomDataGrid } from "../../components/data-grids/CustomDataGrid";
import GenericPageLayout from "../../components/layouts/GenericPageLayout";
import type { EmploymentContract } from "../../data/types/hr/employmentContract";
import EmploymentContractForm from "../../components/forms/hr/EmploymentContractForm";
import { usePermissions } from "../../permissions/usePermissions";
import { ENTITY_TYPES } from "../../permissions/permissions";

const employmentContractListColumns: GridColDef<EmploymentContract>[] = [
    {
        field: "employeeName",
        headerName: "Employé",
        flex: 1.2,
        valueGetter: (_p_value: unknown, p_row: EmploymentContract) =>
            `${p_row.employeeFirstName} ${p_row.employeeLastName}`,
    },
    ...employmentContractColumns,
];

export default function EmploymentContractsPage() {
    const navigate = useNavigate();
    const { canCreate } = usePermissions(ENTITY_TYPES.EMPLOYMENT_CONTRACT);
    const { canRead: canReadHrDashboard } = usePermissions(ENTITY_TYPES.HR_DASHBOARD);
    const [showEmploymentContractForm, setShowEmploymentContractForm] = useState(false);

    const query = useSearchableQuery({
        queryKey: employmentContractsCacheKey.list(),
        queryFn: () => employmentContractService.getAll(),
        filterFn: (p_contract: EmploymentContract, p_search: string) => {
            const normalizedSearch = p_search.toLowerCase();
            const employeeName =
                `${p_contract.employeeFirstName} ${p_contract.employeeLastName}`.toLowerCase();
            return (
                employeeName.includes(normalizedSearch)
                || p_contract.contractType.toLowerCase().includes(normalizedSearch)
                || p_contract.wageType.toLowerCase().includes(normalizedSearch)
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
            customErrorMessage="Impossible de charger les contrats de travail."
        >
            <GenericPageLayout title="Contrats de travail">
                <CustomDataGrid
                    rows={query.filteredData}
                    columns={employmentContractListColumns}
                    addLabel="Ajouter un contrat"
                    onAddClick={canCreate ? () => setShowEmploymentContractForm(true) : undefined}
                    onRowClick={
                        canReadHrDashboard
                            ? (p_params) =>
                                navigate(
                                    ROUTE_EMPLOYEE_PROFILE_DETAILS.replace(
                                        ":id",
                                        String(p_params.row.employeeProfileId)
                                    )
                                )
                            : undefined
                    }
                    {...query.searchProps}
                />
            </GenericPageLayout>
            <EmploymentContractForm
                showEmploymentContractForm={showEmploymentContractForm}
                setShowEmploymentContractForm={setShowEmploymentContractForm}
                editEmploymentContract={null}
            />
        </PageQueryWrapper>
    );
}
