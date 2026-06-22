import { Box, Button, Stack, Typography } from "@mui/material";

const WEEK_DAYS = ["Dim", "Lun", "Mar", "Mer", "Jeu", "Ven", "Sam"];
const LONG_WEEK_DAYS = ["Dimanche", "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi", "Samedi"];

export type CalendarView = "day" | "week" | "month";

export interface CalendarEvent {
    id: string | number;
    date: string;
    title: string;
    subtitle?: string;
    color?: string;
}

interface CalendarDay {
    date: string;
    dayNumber: number;
    isCurrentMonth: boolean;
    isToday: boolean;
}

interface MonthlyCalendarProps<TEvent extends CalendarEvent> {
    month: string;
    date?: string;
    view?: CalendarView;
    events: TEvent[];
    onEventClick?: (p_event: TEvent) => void;
    onEventDrop?: (p_event: TEvent, p_date: string) => void;
    renderEvent?: (p_event: TEvent) => React.ReactNode;
}

function formatDateKey(p_date: Date): string {
    const year = p_date.getFullYear();
    const month = String(p_date.getMonth() + 1).padStart(2, "0");
    const day = String(p_date.getDate()).padStart(2, "0");

    return `${year}-${month}-${day}`;
}

function buildCalendarDays(p_monthKey: string): CalendarDay[] {
    const [year, month] = p_monthKey.split("-").map(Number);
    const firstDay = new Date(year, month - 1, 1);
    const startDate = new Date(firstDay);

    startDate.setDate(firstDay.getDate() - firstDay.getDay());

    return Array.from({ length: 42 }, (_, p_index) => {
        const date = new Date(startDate);
        date.setDate(startDate.getDate() + p_index);

        return {
            date: formatDateKey(date),
            dayNumber: date.getDate(),
            isCurrentMonth: date.getMonth() === month - 1,
            isToday: formatDateKey(date) === formatDateKey(new Date()),
        };
    });
}

function buildWeekDays(p_dateKey: string): CalendarDay[] {
    const selectedDate = new Date(`${p_dateKey}T00:00:00`);
    const startDate = new Date(selectedDate);

    startDate.setDate(selectedDate.getDate() - selectedDate.getDay());

    return Array.from({ length: 7 }, (_, p_index) => {
        const date = new Date(startDate);
        date.setDate(startDate.getDate() + p_index);

        return {
            date: formatDateKey(date),
            dayNumber: date.getDate(),
            isCurrentMonth: true,
            isToday: formatDateKey(date) === formatDateKey(new Date()),
        };
    });
}

function buildDay(p_dateKey: string): CalendarDay[] {
    const date = new Date(`${p_dateKey}T00:00:00`);

    return [{
        date: formatDateKey(date),
        dayNumber: date.getDate(),
        isCurrentMonth: true,
        isToday: formatDateKey(date) === formatDateKey(new Date()),
    }];
}

function buildVisibleDays(p_view: CalendarView, p_monthKey: string, p_dateKey: string): CalendarDay[] {
    if (p_view === "day") {
        return buildDay(p_dateKey);
    }

    if (p_view === "week") {
        return buildWeekDays(p_dateKey);
    }

    return buildCalendarDays(p_monthKey);
}

function groupEventsByDate<TEvent extends CalendarEvent>(p_events: TEvent[]): Map<string, TEvent[]> {
    return p_events.reduce((p_acc, p_event) => {
        const dayEvents = p_acc.get(p_event.date) ?? [];
        dayEvents.push(p_event);
        p_acc.set(p_event.date, dayEvents);
        return p_acc;
    }, new Map<string, TEvent[]>());
}

export default function MonthlyCalendar<TEvent extends CalendarEvent>({
    month,
    date,
    view = "month",
    events,
    onEventClick,
    onEventDrop,
    renderEvent,
}: MonthlyCalendarProps<TEvent>) {
    const selectedDate = date ?? `${month}-01`;
    const calendarDays = buildVisibleDays(view, month, selectedDate);
    const eventsByDate = groupEventsByDate(events);
    const eventById = new Map(events.map((p_event) => [String(p_event.id), p_event]));
    const columns = view === "day" ? 1 : 7;
    const headerLabels = view === "day"
        ? [LONG_WEEK_DAYS[new Date(`${selectedDate}T00:00:00`).getDay()]]
        : WEEK_DAYS;

    const handleDragStart = (
        p_event: React.DragEvent<HTMLElement>,
        p_calendarEvent: TEvent
    ): void => {
        p_event.dataTransfer.effectAllowed = "move";
        p_event.dataTransfer.setData("text/plain", String(p_calendarEvent.id));
    };

    const handleDrop = (p_event: React.DragEvent<HTMLDivElement>, p_date: string): void => {
        if (!onEventDrop) {
            return;
        }

        p_event.preventDefault();
        const eventId = p_event.dataTransfer.getData("text/plain");
        const droppedEvent = eventById.get(eventId);

        if (!droppedEvent || droppedEvent.date === p_date) {
            return;
        }

        onEventDrop(droppedEvent, p_date);
    };

    return (
        <Box
            sx={{
                display: "grid",
                gridTemplateColumns: `repeat(${columns}, minmax(0, 1fr))`,
                border: "1px solid",
                borderColor: "divider",
            }}
        >
            {headerLabels.map((p_day) => (
                <Box
                    key={p_day}
                    sx={{
                        px: 1,
                        py: 1,
                        bgcolor: "action.hover",
                        borderRight: "1px solid",
                        borderBottom: "1px solid",
                        borderColor: "divider",
                        fontWeight: "bold",
                        textAlign: "left",
                    }}
                >
                    {p_day}
                </Box>
            ))}
            {calendarDays.map((p_day) => {
                const dayEvents = eventsByDate.get(p_day.date) ?? [];

                return (
                    <Box
                        key={p_day.date}
                        data-testid={p_day.isToday ? "today-calendar-day" : `calendar-day-${p_day.date}`}
                        onDragOver={(p_event) => {
                            if (onEventDrop) {
                                p_event.preventDefault();
                                p_event.dataTransfer.dropEffect = "move";
                            }
                        }}
                        onDrop={(p_event) => handleDrop(p_event, p_day.date)}
                        sx={{
                            minHeight: 132,
                            ...(view === "day" ? { minHeight: 420 } : {}),
                            p: 1,
                            borderRight: "1px solid",
                            borderBottom: "1px solid",
                            borderColor: p_day.isToday ? "actionButtons.confirm.bg" : "divider",
                            borderWidth: p_day.isToday ? 2 : 1,
                            bgcolor: p_day.isCurrentMonth ? "background.paper" : "action.hover",
                            opacity: p_day.isCurrentMonth ? 1 : 0.65,
                            textAlign: "left",
                            overflow: "hidden",
                            boxShadow: p_day.isToday ? 3 : "none",
                        }}
                    >
                        <Box sx={{ display: "flex", alignItems: "center", gap: 0.75, mb: 1 }}>
                            <Typography
                                variant="body2"
                                sx={{
                                    fontWeight: "bold",
                                    width: 28,
                                    height: 28,
                                    borderRadius: "50%",
                                    display: "inline-flex",
                                    alignItems: "center",
                                    justifyContent: "center",
                                    bgcolor: p_day.isToday ? "actionButtons.confirm.bg" : "transparent",
                                    color: p_day.isToday ? "actionButtons.confirm.text" : "text.primary",
                                }}
                            >
                                {p_day.dayNumber}
                            </Typography>
                            {p_day.isToday && (
                                <Typography
                                    variant="caption"
                                    sx={{
                                        display: { xs: "none", sm: "inline" },
                                        fontWeight: "bold",
                                        color: "text.secondary",
                                        overflow: "hidden",
                                        textOverflow: "ellipsis",
                                        whiteSpace: "nowrap",
                                    }}
                                >
                                    Aujourd'hui
                                </Typography>
                            )}
                        </Box>
                        <Stack spacing={0.75}>
                            {dayEvents.map((p_event) => (
                                <Button
                                    key={p_event.id}
                                    component="div"
                                    role="button"
                                    tabIndex={0}
                                    variant="contained"
                                    disabled={!onEventClick && !onEventDrop}
                                    onClick={() => onEventClick?.(p_event)}
                                    draggable={!!onEventDrop}
                                    onDragStart={(p_dragEvent) => {
                                        p_dragEvent.stopPropagation();
                                        handleDragStart(p_dragEvent, p_event);
                                    }}
                                    sx={{
                                        display: "block",
                                        width: "100%",
                                        minWidth: 0,
                                        px: 1,
                                        py: 0.75,
                                        bgcolor: p_event.color ?? "actionButtons.edit.bg",
                                        color: p_event.color ? "#fff" : "actionButtons.edit.text",
                                        textAlign: "left",
                                        fontWeight: "bold",
                                        lineHeight: 1.25,
                                        cursor: onEventDrop ? "grab" : "pointer",
                                        "&:active": {
                                            cursor: onEventDrop ? "grabbing" : "pointer",
                                        },
                                        "&:hover": {
                                            bgcolor: p_event.color ?? "actionButtons.edit.bg",
                                            opacity: 0.9,
                                        },
                                        "&.Mui-disabled": {
                                            bgcolor: p_event.color ?? "actionButtons.edit.bg",
                                            color: p_event.color ? "#fff" : "actionButtons.edit.text",
                                            opacity: 1,
                                        },
                                    }}
                                >
                                    {renderEvent ? (
                                        renderEvent(p_event)
                                    ) : (
                                        <>
                                            <Box component="span" sx={{ display: "block" }}>
                                                {p_event.title}
                                            </Box>
                                            {p_event.subtitle && (
                                                <Box
                                                    component="span"
                                                    sx={{
                                                        display: "block",
                                                        overflow: "hidden",
                                                        textOverflow: "ellipsis",
                                                        whiteSpace: "nowrap",
                                                    }}
                                                >
                                                    {p_event.subtitle}
                                                </Box>
                                            )}
                                        </>
                                    )}
                                </Button>
                            ))}
                        </Stack>
                    </Box>
                );
            })}
        </Box>
    );
}
