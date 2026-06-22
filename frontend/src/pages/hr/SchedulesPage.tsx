import { useEffect, useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import {
    Box,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    FormControl,
    InputLabel,
    MenuItem,
    Select,
    Stack,
    TextField,
    ToggleButton,
    ToggleButtonGroup,
    type SelectChangeEvent,
} from "@mui/material";
import { useDeleteDialog } from "../../context/DeleteDialogContext";
import { useScheduledShiftMutations } from "../../api/mutations/hr/useScheduledShiftMutations";
import {
    employeeProfilesCacheKey,
    locationsCacheKey,
    scheduledShiftsCacheKey,
} from "../../data/cacheKeys";
import scheduledShiftService from "../../api/services/hr/scheduledShiftService";
import employeeProfileService from "../../api/services/hr/employeeProfileService";
import locationService from "../../api/services/inventory/locationService";
import PageQueryWrapper from "../../components/layouts/PageQueryWrapper";
import { ROUTE_HR } from "../../data/routeNames";
import GenericPageLayout from "../../components/layouts/GenericPageLayout";
import { FORM_TYPES, useFormContainer } from "../../context/FormContext";
import type { ScheduledShift } from "../../data/types/hr/scheduledShift";
import type { EmployeeProfile } from "../../data/types/hr/employeeProfile";
import type { Location } from "../../data/types/inventory/location";
import { usePermissions } from "../../permissions/usePermissions";
import { ENTITY_TYPES } from "../../permissions/permissions";
import { AddButton, CancelButton, DeleteButton, EditButton } from "../../components/buttons/AddEditDeleteButtons";
import MonthlyCalendar, { type CalendarEvent, type CalendarView } from "../../components/calendar/MonthlyCalendar";
import { notifyErrorMessage } from "../../data/utils/popupMessageManager";

const ALL_LOCATIONS_VALUE = "all";
const CALENDAR_VIEW_LABELS: Record<CalendarView, string> = {
    day: "Jour",
    week: "Semaine",
    month: "Mois",
};

function getMonthKey(p_date: string): string {
    return p_date.substring(0, 7);
}

function getTodayKey(): string {
    const date = new Date();
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");

    return `${year}-${month}-${day}`;
}

function formatDateKey(p_date: Date): string {
    const year = p_date.getFullYear();
    const month = String(p_date.getMonth() + 1).padStart(2, "0");
    const day = String(p_date.getDate()).padStart(2, "0");

    return `${year}-${month}-${day}`;
}

function getVisibleDateRange(p_view: CalendarView, p_date: string, p_month: string): [string, string] {
    if (p_view === "month") {
        return [`${p_month}-01`, `${p_month}-31`];
    }

    if (p_view === "day") {
        return [p_date, p_date];
    }

    const selectedDate = new Date(`${p_date}T00:00:00`);
    const startDate = new Date(selectedDate);
    startDate.setDate(selectedDate.getDate() - selectedDate.getDay());
    const endDate = new Date(startDate);
    endDate.setDate(startDate.getDate() + 6);

    return [formatDateKey(startDate), formatDateKey(endDate)];
}

function formatLongDate(p_date: string): string {
    return new Intl.DateTimeFormat("fr-CA", {
        weekday: "long",
        year: "numeric",
        month: "long",
        day: "numeric",
    }).format(new Date(`${p_date}T00:00:00`));
}

function getEmployeeName(p_shift: ScheduledShift): string {
    if (p_shift.employeeFirstName || p_shift.employeeLastName) {
        return `${p_shift.employeeFirstName ?? ""} ${p_shift.employeeLastName ?? ""}`.trim();
    }

    return "Employé non assigné";
}

function getShiftLocationId(
    p_shift: ScheduledShift,
    p_employeeById: Map<number, EmployeeProfile>
): number | null {
    return p_shift.locationId
        ?? (p_shift.employeeProfileId ? p_employeeById.get(p_shift.employeeProfileId)?.locationId : null)
        ?? null;
}

function getShiftLocationTitle(
    p_shift: ScheduledShift,
    p_employeeById: Map<number, EmployeeProfile>,
    p_locations: Location[]
): string {
    const locationId = getShiftLocationId(p_shift, p_employeeById);

    return p_shift.locationTitle
        ?? p_locations.find((p_location) => p_location.id === locationId)?.title
        ?? "Non assignée";
}

interface ShiftCalendarEvent extends CalendarEvent {
    shift: ScheduledShift;
}

export default function SchedulesPage() {
    const { canCreate, canUpdate, canDelete } = usePermissions(ENTITY_TYPES.SCHEDULED_SHIFT);
    const { openForm } = useFormContainer();
    const { openConfirmDeleteWindow } = useDeleteDialog();
    const {
        deleteScheduledShift: deleteScheduledShiftMutation,
        updateScheduledShift: updateScheduledShiftMutation,
    } = useScheduledShiftMutations();
    const [selectedLocationId, setSelectedLocationId] = useState<string>(ALL_LOCATIONS_VALUE);
    const [calendarView, setCalendarView] = useState<CalendarView>("month");
    const [selectedDate, setSelectedDate] = useState<string>(getTodayKey());
    const [selectedMonth, setSelectedMonth] = useState<string>("");
    const [selectedShift, setSelectedShift] = useState<ScheduledShift | null>(null);

    const shiftsQuery = useQuery<ScheduledShift[], Error>({
        queryKey: scheduledShiftsCacheKey.list(),
        queryFn: () => scheduledShiftService.getAll(),
    });

    const employeesQuery = useQuery<EmployeeProfile[], Error>({
        queryKey: employeeProfilesCacheKey.list(),
        queryFn: () => employeeProfileService.getAll(),
    });

    const locationsQuery = useQuery<Location[], Error>({
        queryKey: locationsCacheKey.list(),
        queryFn: () => locationService.getAll(),
    });

    const shifts = shiftsQuery.data ?? [];
    const employees = employeesQuery.data ?? [];
    const locations = locationsQuery.data ?? [];

    useEffect(() => {
        if (!selectedMonth && shifts.length > 0) {
            setSelectedMonth(getMonthKey(shifts[0].date));
        }
    }, [selectedMonth, shifts]);

    const employeeById = useMemo(() => {
        return new Map(employees.map((p_employee) => [p_employee.id, p_employee]));
    }, [employees]);

    const visibleShifts = useMemo(() => {
        const month = selectedMonth || getMonthKey(selectedDate);
        const [startDate, endDate] = getVisibleDateRange(calendarView, selectedDate, month);

        return shifts
            .filter((p_shift) => p_shift.date >= startDate && p_shift.date <= endDate)
            .filter((p_shift) => {
                if (selectedLocationId === ALL_LOCATIONS_VALUE) {
                    return true;
                }

                return getShiftLocationId(p_shift, employeeById) === Number(selectedLocationId);
            })
            .sort((p_left, p_right) =>
                `${p_left.date} ${p_left.startTime}`.localeCompare(`${p_right.date} ${p_right.startTime}`)
            );
    }, [calendarView, employeeById, selectedDate, selectedLocationId, selectedMonth, shifts]);

    const calendarEvents = useMemo<ShiftCalendarEvent[]>(() => {
        return visibleShifts.map((p_shift) => ({
            id: p_shift.id,
            date: p_shift.date,
            title: `${p_shift.startTime}-${p_shift.endTime}`,
            subtitle: getEmployeeName(p_shift),
            color: p_shift.jobPositionColor ?? undefined,
            shift: p_shift,
        }));
    }, [visibleShifts]);

    const currentMonth = selectedMonth || new Date().toISOString().substring(0, 7);

    const isLoading = shiftsQuery.isLoading || employeesQuery.isLoading || locationsQuery.isLoading;
    const error = shiftsQuery.error ?? employeesQuery.error ?? locationsQuery.error;

    const handleDeleteShift = (p_shift: ScheduledShift): void => {
        setSelectedShift(null);
        openConfirmDeleteWindow({
            id: String(p_shift.id),
            displayLabel: `${getEmployeeName(p_shift)} (${p_shift.date})`,
            onDelete: deleteScheduledShiftMutation,
        });
    };

    const handleMoveShift = (p_event: ShiftCalendarEvent, p_date: string): void => {
        const shift = p_event.shift;
        const locationId = getShiftLocationId(shift, employeeById);
        const jobPositionId = shift.jobPositionId;

        if (!locationId || !jobPositionId) {
            notifyErrorMessage("Impossible de déplacer ce quart : succursale ou poste manquant.");
            return;
        }

        void updateScheduledShiftMutation({
            id: String(shift.id),
            data: {
                employeeProfileId: shift.employeeProfileId ?? null,
                jobPositionId,
                locationId,
                date: p_date,
                startTime: shift.startTime,
                endTime: shift.endTime,
            },
        }).catch(() => {
            notifyErrorMessage("Impossible de déplacer le quart de travail.");
        });
    };

    return (
        <PageQueryWrapper
            isLoading={isLoading}
            error={error}
            refetch={() => {
                void shiftsQuery.refetch();
                void employeesQuery.refetch();
                void locationsQuery.refetch();
            }}
            errorReturnUrl={ROUTE_HR}
            errorReturnLabel="Retour au tableau de bord RH"
            customErrorMessage="Impossible de charger la planification."
        >
            <GenericPageLayout title="Planification">
                <Stack
                    direction={{ xs: "column", md: "row" }}
                    spacing={2}
                    sx={{ mb: 2, alignItems: { xs: "stretch", md: "center" } }}
                >
                    <FormControl sx={{ minWidth: { xs: "100%", md: 260 } }}>
                        <InputLabel id="schedule-location-label">Succursale</InputLabel>
                        <Select
                            labelId="schedule-location-label"
                            label="Succursale"
                            value={selectedLocationId}
                            onChange={(p_event: SelectChangeEvent<string>) =>
                                setSelectedLocationId(p_event.target.value)
                            }
                        >
                            <MenuItem value={ALL_LOCATIONS_VALUE}>Toutes les succursales</MenuItem>
                            {locations.map((p_location) => (
                                <MenuItem key={p_location.id} value={String(p_location.id)}>
                                    {p_location.title}
                                </MenuItem>
                            ))}
                        </Select>
                    </FormControl>
                    <TextField
                        label="Période"
                        type={calendarView === "month" ? "month" : "date"}
                        value={calendarView === "month" ? selectedMonth : selectedDate}
                        onChange={(p_event) => {
                            if (calendarView === "month") {
                                setSelectedMonth(p_event.target.value);
                                setSelectedDate(`${p_event.target.value}-01`);
                            } else {
                                setSelectedDate(p_event.target.value);
                                setSelectedMonth(getMonthKey(p_event.target.value));
                            }
                        }}
                        InputLabelProps={{ shrink: true }}
                        sx={{ minWidth: { xs: "100%", md: 180 } }}
                    />
                    <ToggleButtonGroup
                        exclusive
                        value={calendarView}
                        onChange={(_p_event, p_value: CalendarView | null) => {
                            if (p_value) {
                                setCalendarView(p_value);
                            }
                        }}
                        aria-label="Vue de l'horaire"
                        size="small"
                    >
                        {(Object.keys(CALENDAR_VIEW_LABELS) as CalendarView[]).map((p_view) => (
                            <ToggleButton key={p_view} value={p_view} aria-label={CALENDAR_VIEW_LABELS[p_view]}>
                                {CALENDAR_VIEW_LABELS[p_view]}
                            </ToggleButton>
                        ))}
                    </ToggleButtonGroup>
                    <Box sx={{ flexGrow: 1 }} />
                    {canCreate && (
                        <AddButton
                            label="Ajouter un quart"
                            onClick={() =>
                                openForm(
                                    FORM_TYPES.SCHEDULED_SHIFT,
                                    selectedLocationId === ALL_LOCATIONS_VALUE
                                        ? null
                                        : { defaultLocationId: Number(selectedLocationId) }
                                )
                            }
                            sx={{ alignSelf: { xs: "stretch", md: "center" } }}
                        />
                    )}
                </Stack>

                <MonthlyCalendar
                    month={currentMonth}
                    date={selectedDate}
                    view={calendarView}
                    events={calendarEvents}
                    onEventClick={(p_event) => setSelectedShift(p_event.shift)}
                    onEventDrop={canUpdate ? handleMoveShift : undefined}
                />

                <Dialog open={!!selectedShift} onClose={() => setSelectedShift(null)} fullWidth maxWidth="sm">
                    {selectedShift && (
                        <>
                            <DialogTitle>Quart de travail</DialogTitle>
                            <DialogContent>
                                <Stack spacing={1.5} sx={{ mt: 1 }}>
                                    <Box><strong>Employé:</strong> {getEmployeeName(selectedShift)}</Box>
                                    <Box><strong>Poste:</strong> {selectedShift.jobPositionName ?? "Non assigné"}</Box>
                                    <Box><strong>Date:</strong> {formatLongDate(selectedShift.date)}</Box>
                                    <Box>
                                        <strong>Heures:</strong> {selectedShift.startTime} à {selectedShift.endTime}
                                    </Box>
                                    <Box>
                                        <strong>Succursale:</strong>{" "}
                                        {getShiftLocationTitle(selectedShift, employeeById, locations)}
                                    </Box>
                                </Stack>
                            </DialogContent>
                            <DialogActions>
                                <CancelButton
                                    label="Fermer"
                                    onClick={() => setSelectedShift(null)}
                                />
                                {canUpdate && (
                                    <EditButton
                                        label="Modifier"
                                        onClick={() => {
                                            openForm(FORM_TYPES.SCHEDULED_SHIFT, selectedShift);
                                            setSelectedShift(null);
                                        }}
                                    />
                                )}
                                {canDelete && (
                                    <DeleteButton
                                        label="Supprimer"
                                        onClick={() => handleDeleteShift(selectedShift)}
                                    />
                                )}
                            </DialogActions>
                        </>
                    )}
                </Dialog>
            </GenericPageLayout>
        </PageQueryWrapper>
    );
}
