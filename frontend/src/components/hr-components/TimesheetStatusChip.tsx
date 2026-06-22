import { Chip } from "@mui/material";
import {
    TIMESHEET_STATUSES,
    type TimesheetStatus,
} from "../../data/types/hr/timesheet";

interface TimesheetStatusChipProps {
    status: TimesheetStatus;
}

const STATUS_LABELS: Record<TimesheetStatus, string> = {
    [TIMESHEET_STATUSES.Draft]: "Brouillon",
    [TIMESHEET_STATUSES.Submitted]: "Soumise",
    [TIMESHEET_STATUSES.Approved]: "Approuvée",
    [TIMESHEET_STATUSES.Rejected]: "Rejetée",
};

const STATUS_COLORS: Record<
    TimesheetStatus,
    "default" | "info" | "success" | "error"
> = {
    [TIMESHEET_STATUSES.Draft]: "default",
    [TIMESHEET_STATUSES.Submitted]: "info",
    [TIMESHEET_STATUSES.Approved]: "success",
    [TIMESHEET_STATUSES.Rejected]: "error",
};

export default function TimesheetStatusChip({ status }: TimesheetStatusChipProps) {
    return (
        <Chip
            label={STATUS_LABELS[status]}
            color={STATUS_COLORS[status]}
            size="small"
            variant="outlined"
        />
    );
}
