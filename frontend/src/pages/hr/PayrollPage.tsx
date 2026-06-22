import { useMemo } from "react";
import Button from "@mui/material/Button";
import Typography from "@mui/material/Typography";
import type { GridColDef } from "@mui/x-data-grid";
import { useSearchableQuery } from "../../data/hooks/useSearchableQuery";
import { payStubsCacheKey } from "../../data/cacheKeys";
import payrollService from "../../api/services/hr/payrollService";
import PageQueryWrapper from "../../components/layouts/PageQueryWrapper";
import { ROUTE_HR } from "../../data/routeNames";
import { payStubColumns } from "../../data/gridColumns";
import { CustomDataGrid } from "../../components/data-grids/CustomDataGrid";
import GenericPageLayout from "../../components/layouts/GenericPageLayout";
import { FORM_TYPES, useFormContainer } from "../../context/FormContext";
import type { PayStub } from "../../data/types/hr/payStub";
import { usePermissions } from "../../permissions/usePermissions";
import { ENTITY_TYPES } from "../../permissions/permissions";
import { usePayrollMutations } from "../../api/mutations/hr/usePayrollMutations";

export default function PayrollPage() {
    const { canCreate } = usePermissions(ENTITY_TYPES.PAYROLL);
    const { openForm } = useFormContainer();
    const { publishPayStub, isPublishingPayStub } = usePayrollMutations();

    const columns = useMemo<GridColDef<PayStub>[]>(() => {
        if (!canCreate) {
            return payStubColumns;
        }

        return [
            ...payStubColumns,
            {
                field: "publicationActions",
                headerName: "Publication",
                width: 130,
                minWidth: 130,
                sortable: false,
                filterable: false,
                disableColumnMenu: true,
                align: "center",
                headerAlign: "center",
                renderCell: (p_params) => {
                    if (p_params.row.isPublished) {
                        return (
                            <Typography variant="body2" color="text.secondary">
                                Publiée
                            </Typography>
                        );
                    }

                    return (
                        <Button
                            size="small"
                            variant="outlined"
                            disabled={isPublishingPayStub}
                            onClick={(p_event) => {
                                p_event.stopPropagation();
                                void publishPayStub(p_params.row.id);
                            }}
                        >
                            Publier
                        </Button>
                    );
                },
            },
        ];
    }, [canCreate, isPublishingPayStub, publishPayStub]);

    const query = useSearchableQuery({
        queryKey: payStubsCacheKey.list(),
        queryFn: () => payrollService.getStubs(),
        filterFn: (p_stub: PayStub, p_search: string) => {
            const normalizedSearch = p_search.toLowerCase();
            const employeeName =
                `${p_stub.employeeFirstName} ${p_stub.employeeLastName}`.toLowerCase();
            return (
                employeeName.includes(normalizedSearch)
                || p_stub.periodStartDate.includes(normalizedSearch)
                || p_stub.periodEndDate.includes(normalizedSearch)
                || (p_stub.isPublished ? "publiée" : "brouillon").includes(normalizedSearch)
                || String(p_stub.totalHours).includes(normalizedSearch)
                || String(p_stub.grossPay).includes(normalizedSearch)
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
            customErrorMessage="Impossible de charger les bulletins de paie."
        >
            <GenericPageLayout title="Paie">
                <CustomDataGrid
                    rows={query.filteredData}
                    columns={columns}
                    addLabel="Générer les fiches"
                    onAddClick={canCreate ? () => openForm(FORM_TYPES.PAYROLL_GENERATE) : undefined}
                    {...query.searchProps}
                />
            </GenericPageLayout>
        </PageQueryWrapper>
    );
}
