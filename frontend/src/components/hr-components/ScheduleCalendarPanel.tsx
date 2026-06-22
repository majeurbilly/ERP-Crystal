import { useEffect, useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import {
    Box,
    Dialog,
    DialogContent,
    DialogTitle,
    Divider,
    Stack,
    TextField,
    ToggleButton,
    ToggleButtonGroup,
    Typography,
} from "@mui/material";
import { scheduledShiftsCacheKey } from "../../data/cacheKeys";
import scheduledShiftService from "../../api/services/hr/scheduledShiftService";
import type { ScheduledShift } from "../../data/types/hr/scheduledShift";
import MonthlyCalendar, { type CalendarEvent, type CalendarView } from "../calendar/MonthlyCalendar";
import { usePermissions } from "../../permissions/usePermissions";
import { CRUD_OPERATIONS, ENTITY_TYPES } from "../../permissions/permissions";

const CALENDAR_VIEW_LABELS: Record<CalendarView, string> = {
    day: "Jour",
    week: "Semaine",
    month: "Mois",
};

const SCHEDULE_SCOPE_ALL = "all";
const SCHEDULE_SCOPE_MINE = "mine";

type ScheduleScope = typeof SCHEDULE_SCOPE_ALL | typeof SCHEDULE_SCOPE_MINE;

interface ScheduleCalendarEvent extends CalendarEvent {
    shift: ScheduledShift;
}

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

function getEmployeeName(p_shift: ScheduledShift): string {
    if (p_shift.employeeFirstName || p_shift.employeeLastName) {
        return `${p_shift.employeeFirstName ?? ""} ${p_shift.employeeLastName ?? ""}`.trim();
    }
    return "Employé";
}

function getShiftLocationTitle(p_shift: ScheduledShift): string {
    return p_shift.locationTitle?.trim() || "Non assignee";
}

function getShiftJobPositionName(p_shift: ScheduledShift): string {
    return p_shift.jobPositionName?.trim() || "Non assigne";
}

interface ScheduleCalendarPanelProps {
    ownEmployeeProfileId?: number;
    showScopeFilter?: boolean;
    defaultScheduleScope?: ScheduleScope;
}

export default function ScheduleCalendarPanel({
    ownEmployeeProfileId,
    showScopeFilter = true,
    defaultScheduleScope = SCHEDULE_SCOPE_MINE,
}: ScheduleCalendarPanelProps) {
    const { ability } = usePermissions(ENTITY_TYPES.SCHEDULED_SHIFT);
    const canManageShifts = ability.can(CRUD_OPERATIONS.MANAGE, ENTITY_TYPES.SCHEDULED_SHIFT);

    const [calendarView, setCalendarView] = useState<CalendarView>("month");
    const [selectedDate, setSelectedDate] = useState<string>(getTodayKey());
    const [selectedMonth, setSelectedMonth] = useState<string>(getTodayKey().substring(0, 7));
    const [scheduleScope, setScheduleScope] = useState<ScheduleScope>(defaultScheduleScope);
    const [selectedShift, setSelectedShift] = useState<ScheduledShift | null>(null);

    const ownShiftsQuery = useQuery<ScheduledShift[], Error>({
        queryKey: scheduledShiftsCacheKey.list(),
        queryFn: () => scheduledShiftService.getAll(),
    });

    const teamShiftsQuery = useQuery<ScheduledShift[], Error>({
        queryKey: scheduledShiftsCacheKey.teamList(),
        queryFn: () => scheduledShiftService.getTeamSchedule(),
        enabled: scheduleScope === SCHEDULE_SCOPE_ALL && !canManageShifts,
    });

    const shiftsQuery = scheduleScope === SCHEDULE_SCOPE_ALL && !canManageShifts
        ? teamShiftsQuery
        : ownShiftsQuery;

    const shifts = shiftsQuery.data ?? [];

    useEffect(() => {
        if (shifts.length > 0 && !selectedMonth) {
            setSelectedMonth(getMonthKey(shifts[0].date));
        }
    }, [shifts, selectedMonth]);

    const scopedShifts = useMemo(() => {
        if (
            scheduleScope !== SCHEDULE_SCOPE_MINE
            || ownEmployeeProfileId === undefined
        ) {
            return shifts;
        }
        return shifts.filter((p_shift) => p_shift.employeeProfileId === ownEmployeeProfileId);
    }, [ownEmployeeProfileId, scheduleScope, shifts]);

    const visibleShifts = useMemo(() => {
        const month = selectedMonth || getMonthKey(selectedDate);
        const [startDate, endDate] = getVisibleDateRange(calendarView, selectedDate, month);
        return scopedShifts
            .filter((p_shift) => p_shift.date >= startDate && p_shift.date <= endDate)
            .sort((p_left, p_right) =>
                `${p_left.date} ${p_left.startTime}`.localeCompare(`${p_right.date} ${p_right.startTime}`)
            );
    }, [calendarView, scopedShifts, selectedDate, selectedMonth]);

    const calendarEvents: ScheduleCalendarEvent[] = useMemo(() => {
        return visibleShifts.map((p_shift) => ({
            id: p_shift.id,
            date: p_shift.date,
            title: `${p_shift.startTime} – ${p_shift.endTime}`,
            subtitle: getEmployeeName(p_shift),
            color: p_shift.jobPositionColor ?? undefined,
            shift: p_shift,
        }));
    }, [visibleShifts]);

    const showScopeToggle: boolean =
        showScopeFilter
        && ownEmployeeProfileId !== undefined;

    const currentMonth = selectedMonth || getTodayKey().substring(0, 7);

    return (
        <Box>
            <Stack
                direction={{ xs: "column", md: "row" }}
                spacing={2}
                sx={{ mb: 2, alignItems: { xs: "stretch", md: "center" } }}
            >
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
                <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
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
                    {showScopeToggle && (
                        <ToggleButtonGroup
                            exclusive
                            value={scheduleScope}
                            onChange={(_p_event, p_value: ScheduleScope | null) => {
                                if (p_value) {
                                    setScheduleScope(p_value);
                                }
                            }}
                            aria-label="Portée de l'horaire"
                            size="small"
                        >
                            <ToggleButton value={SCHEDULE_SCOPE_MINE} aria-label="Mon horaire">
                                Moi
                            </ToggleButton>
                            <ToggleButton value={SCHEDULE_SCOPE_ALL} aria-label="Horaire de l'équipe">
                                Équipe
                            </ToggleButton>
                        </ToggleButtonGroup>
                    )}
                </Stack>
            </Stack>
            <MonthlyCalendar
                month={currentMonth}
                date={selectedDate}
                view={calendarView}
                events={calendarEvents}
                onEventClick={(p_event) => setSelectedShift(p_event.shift)}
            />
            {!shiftsQuery.isLoading && visibleShifts.length === 0 && (
                <Box sx={{ mt: 2, color: "text.secondary" }}>
                    Aucun quart planifié pour cette période.
                </Box>
            )}
            <Dialog open={selectedShift !== null} onClose={() => setSelectedShift(null)} fullWidth maxWidth="sm">
                {selectedShift && (
                    <>
                        <DialogTitle>Details du quart</DialogTitle>
                        <DialogContent>
                            <Stack spacing={1.5} sx={{ mt: 1 }}>
                                <Box>
                                    <Typography variant="caption" color="text.secondary">
                                        Employe
                                    </Typography>
                                    <Typography>{getEmployeeName(selectedShift)}</Typography>
                                </Box>
                                <Divider />
                                <Box>
                                    <Typography variant="caption" color="text.secondary">
                                        Date
                                    </Typography>
                                    <Typography>{selectedShift.date}</Typography>
                                </Box>
                                <Box>
                                    <Typography variant="caption" color="text.secondary">
                                        Horaire
                                    </Typography>
                                    <Typography>{`${selectedShift.startTime} - ${selectedShift.endTime}`}</Typography>
                                </Box>
                                <Box>
                                    <Typography variant="caption" color="text.secondary">
                                        Poste
                                    </Typography>
                                    <Typography>{getShiftJobPositionName(selectedShift)}</Typography>
                                </Box>
                                <Box>
                                    <Typography variant="caption" color="text.secondary">
                                        Succursale
                                    </Typography>
                                    <Typography>{getShiftLocationTitle(selectedShift)}</Typography>
                                </Box>
                            </Stack>
                        </DialogContent>
                    </>
                )}
            </Dialog>
        </Box>
    );
}
