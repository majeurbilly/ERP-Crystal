import { useState } from "react";
import { Typography } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import { useParams } from "react-router-dom";
import employeeProfileService from "../../../api/services/hr/employeeProfileService";
import employmentContractService from "../../../api/services/hr/employmentContractService";
import PageQueryWrapper from "../../../components/layouts/PageQueryWrapper";
import GenericPageLayout from "../../../components/layouts/GenericPageLayout";
import EmployeeProfileSummaryCard from "../../../components/hr-components/EmployeeProfileSummaryCard";
import { CustomDataGrid } from "../../../components/data-grids/CustomDataGrid";
import { employmentContractColumns } from "../../../data/gridColumns";
import {
    employeeProfilesCacheKey,
    employmentContractsCacheKey,
} from "../../../data/cacheKeys";
import { ROUTE_EMPLOYEE_PROFILES } from "../../../data/routeNames";
import { useDeleteDialog } from "../../../context/DeleteDialogContext";
import { useEmploymentContractMutations } from "../../../api/mutations/hr/useEmploymentContractMutations";
import EmploymentContractForm from "../../../components/forms/hr/EmploymentContractForm";
import type { EmploymentContract } from "../../../data/types/hr/employmentContract";
import type { EmployeeProfile } from "../../../data/types/hr/employeeProfile";
import { usePermissions } from "../../../permissions/usePermissions";
import { ENTITY_TYPES } from "../../../permissions/permissions";

export default function EmployeeProfileDetailsPage() {
    const { id } = useParams();
    const { canCreate, canUpdate, canDelete } = usePermissions(ENTITY_TYPES.EMPLOYMENT_CONTRACT);
    const { canRead: canReadEmployeeSalary } = usePermissions(ENTITY_TYPES.HR_DASHBOARD);
    const { openConfirmDeleteWindow } = useDeleteDialog();
    const { deleteEmploymentContract: deleteEmploymentContractMutation } =
        useEmploymentContractMutations(id ?? "");

    const [showContractForm, setShowContractForm] = useState<boolean>(false);
    const [editContract, setEditContract] = useState<EmploymentContract | null>(null);

    const employeeId: number = Number(id);
    const isValidEmployeeId: boolean = Number.isInteger(employeeId) && employeeId > 0;

    const employeeQuery = useQuery<EmployeeProfile, Error>({
        queryKey: employeeProfilesCacheKey.details(id ?? ""),
        queryFn: () => employeeProfileService.getById(id ?? ""),
        enabled: isValidEmployeeId,
    });

    const contractsQuery = useQuery<EmploymentContract[], Error>({
        queryKey: employmentContractsCacheKey.byEmployee(id ?? ""),
        queryFn: () => employmentContractService.getByEmployeeId(employeeId),
        enabled: isValidEmployeeId,
    });

    const employee: EmployeeProfile | undefined = employeeQuery.data;
    const contracts: EmploymentContract[] = contractsQuery.data ?? [];
    const isLoading: boolean = employeeQuery.isLoading || contractsQuery.isLoading;
    const hasError: boolean =
        !!employeeQuery.error
        || !!contractsQuery.error
        || !isValidEmployeeId
        || (!employee && !employeeQuery.isLoading);

    const handleOpenAddContract = (): void => {
        setEditContract(null);
        setShowContractForm(true);
    };

    const handleOpenEditContract = (p_contract: EmploymentContract): void => {
        setEditContract(p_contract);
        setShowContractForm(true);
    };

    const handleCloseContractForm = (): void => {
        setShowContractForm(false);
        setEditContract(null);
    };

    return (
        <PageQueryWrapper
            isLoading={isLoading}
            error={hasError ? (employeeQuery.error ?? contractsQuery.error ?? { message: "Employé introuvable" }) : null}
            refetch={() => {
                void employeeQuery.refetch();
                void contractsQuery.refetch();
            }}
            errorReturnUrl={ROUTE_EMPLOYEE_PROFILES}
            errorReturnLabel="Retour à l'annuaire"
            customErrorMessage={
                !isValidEmployeeId
                    ? "Identifiant d'employé invalide."
                    : "Impossible de charger la fiche employé."
            }
        >
            {employee && (
                <GenericPageLayout title={`${employee.firstName} ${employee.lastName}`}>
                    <EmployeeProfileSummaryCard
                        profile={employee}
                        showSalary={canReadEmployeeSalary}
                    />

                    <Typography variant="h6" sx={{ mb: 2, mt: 3 }}>
                        Contrats de travail
                    </Typography>
                    <CustomDataGrid
                        rows={contracts}
                        columns={employmentContractColumns}
                        addLabel="Ajouter un contrat"
                        onAddClick={canCreate ? handleOpenAddContract : undefined}
                        onEditClick={canUpdate ? handleOpenEditContract : undefined}
                        onDeleteClick={
                            canDelete
                                ? (p_contract: EmploymentContract) =>
                                    openConfirmDeleteWindow({
                                        id: String(p_contract.id),
                                        displayLabel: `${p_contract.contractType} (${p_contract.startDate})`,
                                        onDelete: deleteEmploymentContractMutation,
                                    })
                                : undefined
                        }
                    />

                    <EmploymentContractForm
                        employeeProfileId={employee.id}
                        showEmploymentContractForm={showContractForm}
                        setShowEmploymentContractForm={handleCloseContractForm}
                        editEmploymentContract={editContract}
                        setEditEmploymentContract={setEditContract}
                    />
                </GenericPageLayout>
            )}
        </PageQueryWrapper>
    );
}
