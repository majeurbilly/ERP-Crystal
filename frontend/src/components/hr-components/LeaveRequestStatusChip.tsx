import { Chip } from "@mui/material";
import {
    LEAVE_REQUEST_STATUSES,
    type LeaveRequestStatus,
} from "../../data/types/hr/leaveRequest";

interface LeaveRequestStatusChipProps {
    status: LeaveRequestStatus;
}

const STATUS_LABELS: Record<LeaveRequestStatus, string> = {
    [LEAVE_REQUEST_STATUSES.Pending]: "En attente",
    [LEAVE_REQUEST_STATUSES.Approved]: "Approuvée",
    [LEAVE_REQUEST_STATUSES.Rejected]: "Refusée",
};

const STATUS_COLORS: Record<
    LeaveRequestStatus,
    "default" | "warning" | "success" | "error"
> = {
    [LEAVE_REQUEST_STATUSES.Pending]: "warning",
    [LEAVE_REQUEST_STATUSES.Approved]: "success",
    [LEAVE_REQUEST_STATUSES.Rejected]: "error",
};

export default function LeaveRequestStatusChip({ status }: LeaveRequestStatusChipProps) {
    return (
        <Chip
            label={STATUS_LABELS[status]}
            color={STATUS_COLORS[status]}
            size="small"
            variant="outlined"
        />
    );
}
