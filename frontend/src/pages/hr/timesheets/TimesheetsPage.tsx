import { useNavigate, useSearchParams } from "react-router-dom";
import { useMemo, useState } from "react";
import { Box, Button } from "@mui/material";
import CalendarMonthIcon from "@mui/icons-material/CalendarMonth";
import { useSearchableQuery } from "../../../data/hooks/useSearchableQuery";
import { timesheetsCacheKey } from "../../../data/cacheKeys";
import timesheetService from "../../../api/services/hr/timesheetService";
import PageQueryWrapper from "../../../components/layouts/PageQueryWrapper";
import { ROUTE_HR, ROUTE_TIMESHEET_DETAILS } from "../../../data/routeNames";
import { timesheetColumns } from "../../../data/gridColumns";
import { CustomDataGrid } from "../../../components/data-grids/CustomDataGrid";
import GenericPageLayout from "../../../components/layouts/GenericPageLayout";
import { FORM_TYPES, useFormContainer } from "../../../context/FormContext";
import type { Timesheet } from "../../../data/types/hr/timesheet";
import { usePermissions } from "../../../permissions/usePermissions";
import { CRUD_OPERATIONS, ENTITY_TYPES } from "../../../permissions/permissions";
import GenerateWeeklyTimesheetsForm from "../../../components/forms/hr/GenerateWeeklyTimesheetsForm";
import { useTimesheetMutations } from "../../../api/mutations/hr/useTimesheetMutations";
import { useDeleteDialog } from "../../../context/DeleteDialogContext";

export default function TimesheetsPage() {
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();
    const statusFilter = searchParams.get("status");
    const { ability, canCreate, canDelete } = usePermissions(ENTITY_TYPES.TIMESHEET);
    const { openForm } = useFormContainer();
    const { deleteTimesheet, isDeletingTimesheet } = useTimesheetMutations();
    const { openConfirmDeleteWindow } = useDeleteDialog();
    const [showGenerateWeeklyForm, setShowGenerateWeeklyForm] = useState<boolean>(false);

    const query = useSearchableQuery({
        queryKey: timesheetsCacheKey.list(),
        queryFn: () => timesheetService.getAll(),
        filterFn: (p_timesheet: Timesheet, p_search: string) => {
            const normalizedSearch: string = p_search.toLowerCase();
            const employeeName: string =
                `${p_timesheet.employeeFirstName} ${p_timesheet.employeeLastName}`.toLowerCase();
            return (
                employeeName.includes(normalizedSearch)
                || p_timesheet.periodStart.includes(normalizedSearch)
                || p_timesheet.periodEnd.includes(normalizedSearch)
                || p_timesheet.status.toLowerCase().includes(normalizedSearch)
            );
        },
    });

    const displayData = useMemo(() => {
        if (!statusFilter) {
            return query.filteredData;
        }
        return query.filteredData.filter((p_item) => p_item.status === statusFilter);
    }, [query.filteredData, statusFilter]);

    const canGenerateWeeklyTimesheets: boolean =
        canCreate || ability.can(CRUD_OPERATIONS.SUBMIT, ENTITY_TYPES.TIMESHEET);

    const handleDeleteClick = (p_timesheet: Timesheet): void => {
        openConfirmDeleteWindow({
            id: p_timesheet.id,
            displayLabel: `la feuille de temps de ${p_timesheet.employeeFirstName} ${p_timesheet.employeeLastName}`,
            onDelete: deleteTimesheet,
            isDeleting: isDeletingTimesheet,
        });
    };

    return (
        <PageQueryWrapper
            isLoading={query.isLoading}
            error={query.error}
            refetch={query.refetch}
            errorReturnUrl={ROUTE_HR}
            errorReturnLabel="Retour au tableau de bord RH"
            customErrorMessage="Impossible de charger les feuilles de temps."
        >
            <GenericPageLayout title="Feuilles de temps">
                <Box sx={{ mb: 2, display: "flex", justifyContent: "flex-end", gap: 1 }}>
                    {canGenerateWeeklyTimesheets && (
                        <Button
                            variant="outlined"
                            startIcon={<CalendarMonthIcon />}
                            onClick={() => setShowGenerateWeeklyForm(true)}
                        >
                            Générer une semaine
                        </Button>
                    )}
                </Box>
                <CustomDataGrid
                    rows={displayData}
                    columns={timesheetColumns}
                    addLabel="Ajouter une feuille de temps"
                    onAddClick={canCreate ? () => openForm(FORM_TYPES.TIMESHEET) : undefined}
                    onDeleteClick={canDelete ? handleDeleteClick : undefined}
                    onRowClick={(p_params) =>
                        navigate(ROUTE_TIMESHEET_DETAILS.replace(":id", String(p_params.id)))
                    }
                    {...query.searchProps}
                />
                <GenerateWeeklyTimesheetsForm
                    open={showGenerateWeeklyForm}
                    onClose={() => setShowGenerateWeeklyForm(false)}
                />
            </GenericPageLayout>
        </PageQueryWrapper>
    );
}
