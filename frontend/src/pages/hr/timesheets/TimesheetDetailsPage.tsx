import { useEffect, useState, type FormEvent } from "react";
import { Box, Button, Paper, Stack, TextField, Typography } from "@mui/material";
import RefreshIcon from "@mui/icons-material/Refresh";
import { useQuery } from "@tanstack/react-query";
import { useParams } from "react-router-dom";
import timesheetService from "../../../api/services/hr/timesheetService";
import PageQueryWrapper from "../../../components/layouts/PageQueryWrapper";
import GenericPageLayout from "../../../components/layouts/GenericPageLayout";
import { CustomDataGrid, type DataGridAction } from "../../../components/data-grids/CustomDataGrid";
import {
    formatHours,
    getTimeEntryDurationHours,
    timeEntryColumns,
} from "../../../data/gridColumns";
import { timesheetsCacheKey } from "../../../data/cacheKeys";
import { ROUTE_MON_ESPACE, ROUTE_TIMESHEETS } from "../../../data/routeNames";
import { useTimesheetMutations } from "../../../api/mutations/hr/useTimesheetMutations";
import TimesheetStatusChip from "../../../components/hr-components/TimesheetStatusChip";
import type { Timesheet } from "../../../data/types/hr/timesheet";
import { TIMESHEET_STATUSES } from "../../../data/types/hr/timesheet";
import type { TimeEntry, TimeEntryFormData } from "../../../data/types/hr/timeEntry";
import { notifyErrorMessage, notifySuccessMessage } from "../../../data/utils/popupMessageManager";
import { extractApiErrorMessage } from "../../../data/utils/extractApiErrorMessage";
import { usePermissions } from "../../../permissions/usePermissions";
import { CRUD_OPERATIONS, ENTITY_TYPES } from "../../../permissions/permissions";
import { useDeleteDialog } from "../../../context/DeleteDialogContext";
import { FormModal } from "../../../components/forms/FormModal";
import { TimeSelectField } from "../../../components/forms/TimeSelectField";
import { normalizeTimeToHHmm } from "../../../data/data-mapper/hr/scheduledShiftMapper";
import { CancelButton, ConfirmButton } from "../../../components/buttons/AddEditDeleteButtons";

const periodDateFormatter = new Intl.DateTimeFormat("fr-CA", {
    year: "numeric",
    month: "long",
    day: "numeric",
});

function formatPeriodDate(p_value: string): string {
    const parsedDate: Date = new Date(`${p_value}T00:00:00`);
    if (Number.isNaN(parsedDate.getTime())) {
        return p_value;
    }
    return periodDateFormatter.format(parsedDate);
}

export default function TimesheetDetailsPage() {
    const { id } = useParams();
    const {
        ability,
        canUpdate: canUpdateTimesheet,
        canDelete,
    } = usePermissions(ENTITY_TYPES.TIMESHEET);
    const { canRead: canReadHrDashboard } = usePermissions(ENTITY_TYPES.HR_DASHBOARD);
    const {
        canCreate: canCreateTimeEntry,
        canUpdate: canUpdateTimeEntry,
        canDelete: canRemoveTimeEntry,
    } = usePermissions(ENTITY_TYPES.TIME_ENTRY);
    const {
        deleteTimesheet,
        isDeletingTimesheet,
        reloadTimesheetTimeEntries,
        isReloadingTimesheetTimeEntries,
        addTimesheetTimeEntry,
        isAddingTimesheetTimeEntry,
        updateTimesheetTimeEntry,
        isUpdatingTimesheetTimeEntry,
        removeTimesheetTimeEntry,
        isRemovingTimesheetTimeEntry,
        updateTimesheetStatus,
        isUpdatingTimesheetStatus,
        updateTimesheetPaid,
        isUpdatingTimesheetPaid,
    } = useTimesheetMutations();
    const { openConfirmDeleteWindow } = useDeleteDialog();
    const [isTimeEntryFormOpen, setIsTimeEntryFormOpen] = useState<boolean>(false);
    const [editTimeEntry, setEditTimeEntry] = useState<TimeEntry | null>(null);
    const [timeEntryToRemove, setTimeEntryToRemove] = useState<TimeEntry | null>(null);

    const timesheetId: number = Number(id);
    const isValidTimesheetId: boolean = Number.isInteger(timesheetId) && timesheetId > 0;

    const timesheetQuery = useQuery<Timesheet, Error>({
        queryKey: timesheetsCacheKey.details(id ?? ""),
        queryFn: () => timesheetService.getById(id ?? ""),
        enabled: isValidTimesheetId,
    });

    const timesheet: Timesheet | undefined = timesheetQuery.data;
    const returnUrl = canReadHrDashboard ? ROUTE_TIMESHEETS : `${ROUTE_MON_ESPACE}?tab=feuille`;
    const returnLabel = canReadHrDashboard ? "Retour aux feuilles de temps" : "Retour Ã  Mon espace";
    const hasError: boolean =
        !!timesheetQuery.error || !isValidTimesheetId || (!timesheet && !timesheetQuery.isLoading);
    const totalHours: number = (timesheet?.timeEntries ?? []).reduce(
        (p_total: number, p_entry: TimeEntry) =>
            p_total + (getTimeEntryDurationHours(p_entry) ?? 0),
        0
    );

    const handleStatusChange = async (p_status: Timesheet["status"]): Promise<void> => {
        if (!timesheet) {
            return;
        }

        try {
            await updateTimesheetStatus({ id: timesheet.id, status: p_status });
            notifySuccessMessage("Le statut de la feuille de temps a Ã©tÃ© mis Ã  jour.");
        } catch (error: unknown) {
            notifyErrorMessage(extractApiErrorMessage(error));
        }
    };

    const handlePaidChange = async (): Promise<void> => {
        if (!timesheet) {
            return;
        }

        try {
            await updateTimesheetPaid({ id: timesheet.id, isPaid: !timesheet.isPaid });
            notifySuccessMessage(
                timesheet.isPaid
                    ? "La feuille de temps est maintenant non payée."
                    : "La feuille de temps est maintenant payée."
            );
        } catch (error: unknown) {
            notifyErrorMessage(extractApiErrorMessage(error));
        }
    };

    const handleDeleteClick = (): void => {
        if (!timesheet) {
            return;
        }

        openConfirmDeleteWindow({
            id: timesheet.id,
            displayLabel: `la feuille de temps de ${timesheet.employeeFirstName} ${timesheet.employeeLastName}`,
            onDelete: deleteTimesheet,
            isDeleting: isDeletingTimesheet,
            redirectUrl: returnUrl,
        });
    };

    const handleReloadTimeEntries = async (): Promise<void> => {
        if (!timesheet) {
            return;
        }

        try {
            await reloadTimesheetTimeEntries(timesheet.id);
            notifySuccessMessage("Les pointages liÃ©s Ã  la feuille de temps ont Ã©tÃ© rechargÃ©s.");
        } catch (error: unknown) {
            notifyErrorMessage(extractApiErrorMessage(error));
        }
    };

    const handleAddTimeEntryClick = (): void => {
        setEditTimeEntry(null);
        setIsTimeEntryFormOpen(true);
    };

    const handleEditTimeEntryClick = (p_timeEntry: TimeEntry): void => {
        setEditTimeEntry(p_timeEntry);
        setIsTimeEntryFormOpen(true);
    };

    const handleCloseTimeEntryForm = (): void => {
        setIsTimeEntryFormOpen(false);
        setEditTimeEntry(null);
    };

    const handleTimeEntrySubmit = async (p_data: TimeEntryFormData): Promise<void> => {
        if (!timesheet) {
            return;
        }

        try {
            if (editTimeEntry) {
                await updateTimesheetTimeEntry({
                    timesheetId: timesheet.id,
                    timeEntryId: editTimeEntry.id,
                    data: p_data,
                });
                notifySuccessMessage("Le pointage lie a la feuille de temps a ete modifie.");
            } else {
                await addTimesheetTimeEntry({
                    timesheetId: timesheet.id,
                    data: p_data,
                });
                notifySuccessMessage("Le pointage a ete ajoute a la feuille de temps.");
            }

            handleCloseTimeEntryForm();
        } catch (error: unknown) {
            notifyErrorMessage(extractApiErrorMessage(error));
        }
    };

    const handleRemoveTimeEntryClick = (p_timeEntry: TimeEntry): void => {
        setTimeEntryToRemove(p_timeEntry);
    };

    const handleConfirmRemoveTimeEntry = async (): Promise<void> => {
        if (!timesheet || !timeEntryToRemove) {
            return;
        }

        try {
            await removeTimesheetTimeEntry({
                timesheetId: timesheet.id,
                timeEntryId: timeEntryToRemove.id,
            });
            notifySuccessMessage("Le pointage a ete retire de la feuille de temps.");
            setTimeEntryToRemove(null);
        } catch (error: unknown) {
            notifyErrorMessage(extractApiErrorMessage(error));
        }
    };

    const showSubmitButton: boolean =
        ability.can(CRUD_OPERATIONS.SUBMIT, ENTITY_TYPES.TIMESHEET)
        && !!timesheet
        && (timesheet.status === TIMESHEET_STATUSES.Draft
            || timesheet.status === TIMESHEET_STATUSES.Rejected);

    const showApprovalButtons: boolean =
        ability.can(CRUD_OPERATIONS.APPROVE, ENTITY_TYPES.TIMESHEET)
        && !!timesheet
        && timesheet.status === TIMESHEET_STATUSES.Submitted;

    const canReloadTimeEntries: boolean =
        canUpdateTimesheet
        && !!timesheet
        && timesheet.status === TIMESHEET_STATUSES.Draft;

    const canEditLinkedTimeEntries: boolean =
        canUpdateTimesheet
        && !!timesheet
        && timesheet.status === TIMESHEET_STATUSES.Draft;

    const linkedTimeEntryActions: DataGridAction<TimeEntry>[] = [
        ...(canEditLinkedTimeEntries && canUpdateTimeEntry
            ? [
                {
                    type: "edit" as const,
                    tooltip: "Modifier",
                    ariaLabel: "Modifier",
                    onClick: handleEditTimeEntryClick,
                },
            ]
            : []),
        ...(canEditLinkedTimeEntries && canRemoveTimeEntry
            ? [
                {
                    type: "delete" as const,
                    tooltip: "Retirer de la feuille",
                    ariaLabel: "Retirer",
                    onClick: handleRemoveTimeEntryClick,
                },
            ]
            : []),
    ];

    return (
        <PageQueryWrapper
            isLoading={timesheetQuery.isLoading}
            error={
                hasError
                    ? (timesheetQuery.error ?? { message: "Feuille de temps introuvable" })
                    : null
            }
            refetch={timesheetQuery.refetch}
            errorReturnUrl={returnUrl}
            errorReturnLabel={returnLabel}
            customErrorMessage="Impossible de charger la feuille de temps."
        >
            {timesheet && (
                <GenericPageLayout
                    title="DÃ©tail de la feuille de temps"
                    onDeleteClick={canDelete ? handleDeleteClick : undefined}
                >
                    <Paper sx={{ p: 3, mb: 3 }}>
                        <Stack
                            direction={{ xs: "column", sm: "row" }}
                            spacing={2}
                            alignItems="flex-start"
                            justifyContent="space-between"
                        >
                            <Box
                                sx={{
                                    display: "flex",
                                    flexDirection: "column",
                                    alignItems: "flex-start",
                                    textAlign: "left",
                                    flex: 1,
                                    minWidth: 0,
                                }}
                            >
                                <Typography variant="h6">
                                    {`${timesheet.employeeFirstName} ${timesheet.employeeLastName}`}
                                </Typography>
                                <Typography variant="body2" color="text.secondary">
                                    {`PÃ©riode : ${formatPeriodDate(timesheet.periodStart)} â€“ ${formatPeriodDate(timesheet.periodEnd)}`}
                                </Typography>
                                <Typography variant="body2" color="text.secondary">
                                    {`Total : ${formatHours(totalHours)}`}
                                </Typography>
                                <Typography variant="body2" color="text.secondary">
                                    {`Paiement : ${timesheet.isPaid ? "Payée" : "Non payée"}`}
                                </Typography>
                            </Box>
                            <Stack
                                direction={{ xs: "row", sm: "column" }}
                                spacing={1}
                                alignItems={{ xs: "center", sm: "flex-end" }}
                            >
                                <TimesheetStatusChip status={timesheet.status} />
                                {canUpdateTimesheet && (
                                    <Button
                                        variant="outlined"
                                        size="small"
                                        onClick={() => {
                                            void handlePaidChange();
                                        }}
                                        disabled={isUpdatingTimesheetPaid}
                                    >
                                        {timesheet.isPaid
                                            ? "Marquer non payée"
                                            : "Marquer payée"}
                                    </Button>
                                )}
                                <Button
                                    variant="outlined"
                                    size="small"
                                    startIcon={<RefreshIcon />}
                                    onClick={() => {
                                        void handleReloadTimeEntries();
                                    }}
                                    disabled={
                                        !canReloadTimeEntries
                                        || timesheetQuery.isFetching
                                        || isReloadingTimesheetTimeEntries
                                    }
                                >
                                    Recharger
                                </Button>
                            </Stack>
                        </Stack>
                        {(showSubmitButton || showApprovalButtons) && (
                            <Stack direction="row" spacing={1} sx={{ mt: 2 }}>
                                {showSubmitButton && (
                                    <Button
                                        variant="contained"
                                        color="primary"
                                        disabled={isUpdatingTimesheetStatus}
                                        onClick={() => {
                                            void handleStatusChange(TIMESHEET_STATUSES.Submitted);
                                        }}
                                    >
                                        Soumettre
                                    </Button>
                                )}
                                {showApprovalButtons && (
                                    <>
                                        <CancelButton
                                            label={"Rejeter"}
                                            onClick={() => {
                                                void handleStatusChange(TIMESHEET_STATUSES.Rejected);
                                            }}
                                        />
                                        <ConfirmButton
                                            label={"Approuver"}
                                            onClick={() => {
                                                void handleStatusChange(TIMESHEET_STATUSES.Approved);
                                            }}
                                        />
                                    </>
                                )}
                            </Stack>
                        )}
                    </Paper>
                    <Typography variant="h6" sx={{ mb: 2 }}>
                        Pointages liÃ©s
                    </Typography>
                    <CustomDataGrid
                        rows={timesheet.timeEntries}
                        columns={timeEntryColumns}
                        addLabel="Ajouter un pointage"
                        onAddClick={
                            canEditLinkedTimeEntries && canCreateTimeEntry
                                ? handleAddTimeEntryClick
                                : undefined
                        }
                        actions={
                            linkedTimeEntryActions.length > 0
                                ? linkedTimeEntryActions
                                : undefined
                        }
                    />
                    <TimesheetTimeEntryForm
                        open={isTimeEntryFormOpen}
                        timesheet={timesheet}
                        editTimeEntry={editTimeEntry}
                        isSubmitting={isAddingTimesheetTimeEntry || isUpdatingTimesheetTimeEntry}
                        onClose={handleCloseTimeEntryForm}
                        onSubmit={handleTimeEntrySubmit}
                    />
                    <FormModal
                        open={timeEntryToRemove !== null}
                        title="Retirer le pointage?"
                        onClose={() => setTimeEntryToRemove(null)}
                        onConfirmClick={() => {
                            void handleConfirmRemoveTimeEntry();
                        }}
                        isSubmitting={isRemovingTimesheetTimeEntry}
                        confirmLabel={isRemovingTimesheetTimeEntry ? "Retrait..." : "Retirer"}
                    >
                        <Typography variant="body1" sx={{ color: "text.secondary" }}>
                            Le pointage restera disponible dans les pointages, mais ne sera plus
                            lie a cette feuille de temps.
                        </Typography>
                    </FormModal>
                </GenericPageLayout>
            )}
        </PageQueryWrapper>
    );
}

interface TimesheetTimeEntryFormProps {
    open: boolean;
    timesheet: Timesheet;
    editTimeEntry: TimeEntry | null;
    isSubmitting: boolean;
    onClose: () => void;
    onSubmit: (p_data: TimeEntryFormData) => Promise<void>;
}

interface TimesheetTimeEntryFormErrors {
    date: string;
    startTime: string;
    endTime: string;
}

function TimesheetTimeEntryForm({
    open,
    timesheet,
    editTimeEntry,
    isSubmitting,
    onClose,
    onSubmit,
}: TimesheetTimeEntryFormProps) {
    const [date, setDate] = useState<string>(timesheet.periodStart);
    const [startTime, setStartTime] = useState<string>("");
    const [endTime, setEndTime] = useState<string>("");
    const [errors, setErrors] = useState<TimesheetTimeEntryFormErrors>({
        date: "",
        startTime: "",
        endTime: "",
    });

    const isEditMode: boolean = editTimeEntry !== null;

    useEffect(() => {
        if (!open) {
            return;
        }

        if (editTimeEntry) {
            setDate(editTimeEntry.date);
            setStartTime(normalizeTimeToHHmm(editTimeEntry.startTime));
            setEndTime(editTimeEntry.endTime ? normalizeTimeToHHmm(editTimeEntry.endTime) : "");
        } else {
            setDate(timesheet.periodStart);
            setStartTime("");
            setEndTime("");
        }

        setErrors({
            date: "",
            startTime: "",
            endTime: "",
        });
    }, [editTimeEntry, open, timesheet.periodStart]);

    const validate = (): boolean => {
        let isValid: boolean = true;
        const newErrors: TimesheetTimeEntryFormErrors = {
            date: "",
            startTime: "",
            endTime: "",
        };

        if (!date) {
            newErrors.date = "La date est requise.";
            isValid = false;
        } else if (date < timesheet.periodStart || date > timesheet.periodEnd) {
            newErrors.date = "La date doit etre dans la periode de la feuille.";
            isValid = false;
        }

        if (!startTime) {
            newErrors.startTime = "L'heure de debut est requise.";
            isValid = false;
        }

        const normalizedStart: string = startTime ? normalizeTimeToHHmm(startTime) : "";
        const normalizedEnd: string = endTime.trim().length > 0 ? normalizeTimeToHHmm(endTime) : "";
        if (normalizedStart && normalizedEnd && normalizedEnd <= normalizedStart) {
            newErrors.endTime = "L'heure de fin doit etre posterieure a l'heure de debut.";
            isValid = false;
        }

        setErrors(newErrors);
        return isValid;
    };

    const handleSubmit = async (p_event: FormEvent): Promise<void> => {
        p_event.preventDefault();
        if (!validate()) {
            return;
        }

        const normalizedEnd: string = endTime.trim().length > 0 ? normalizeTimeToHHmm(endTime) : "";

        await onSubmit({
            employeeProfileId: timesheet.employeeProfileId,
            scheduledShiftId: editTimeEntry?.scheduledShiftId ?? null,
            date,
            startTime: normalizeTimeToHHmm(startTime),
            endTime: normalizedEnd.length > 0 ? normalizedEnd : null,
        });
    };

    return (
        <FormModal
            open={open}
            onClose={onClose}
            title={isEditMode ? "Modifier un pointage" : "Ajouter un pointage"}
            onSubmit={handleSubmit}
            isSubmitting={isSubmitting}
        >
            <TextField
                fullWidth
                label="Employe"
                value={`${timesheet.employeeFirstName} ${timesheet.employeeLastName}`}
                disabled
                sx={{ mb: 2 }}
            />
            <TextField
                fullWidth
                label="Date"
                type="date"
                value={date}
                onChange={(p_event) => setDate(p_event.target.value)}
                InputLabelProps={{ shrink: true }}
                inputProps={{
                    min: timesheet.periodStart,
                    max: timesheet.periodEnd,
                }}
                sx={{ mb: 2 }}
                required
                error={!!errors.date}
                helperText={errors.date}
            />
            <TimeSelectField
                label="Heure de debut"
                value={startTime}
                onChange={setStartTime}
                required
                error={!!errors.startTime}
                helperText={errors.startTime}
            />
            <TimeSelectField
                label="Heure de fin (optionnel)"
                value={endTime}
                onChange={setEndTime}
                error={!!errors.endTime}
                helperText={errors.endTime}
            />
        </FormModal>
    );
}
