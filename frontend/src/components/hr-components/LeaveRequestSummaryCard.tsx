import type { ReactNode } from "react";
import {
    Avatar,
    Box,
    Card,
    CardContent,
    Divider,
    Typography,
} from "@mui/material";
import EventBusyIcon from "@mui/icons-material/EventBusy";
import CalendarMonthIcon from "@mui/icons-material/CalendarMonth";
import NotesIcon from "@mui/icons-material/Notes";
import LeaveRequestStatusChip from "./LeaveRequestStatusChip";
import { formatLeaveDate, leaveTypeLabels } from "../../data/gridColumns";
import type { LeaveRequest } from "../../data/types/hr/leaveRequest";

interface LeaveRequestSummaryCardProps {
    leaveRequest: LeaveRequest;
    footer?: ReactNode;
}

function InfoRow({ icon, label, value }: { icon: ReactNode; label: string; value: ReactNode }) {
    return (
        <Box sx={{ display: "flex", gap: 1.5, alignItems: "flex-start" }}>
            <Box sx={{ color: "primary.main", mt: 0.25, display: "flex" }}>{icon}</Box>
            <Box sx={{ minWidth: 0, flex: 1 }}>
                <Typography variant="caption" color="text.secondary" display="block">
                    {label}
                </Typography>
                {typeof value === "string" ? (
                    <Typography variant="body1">{value}</Typography>
                ) : (
                    value
                )}
            </Box>
        </Box>
    );
}

export default function LeaveRequestSummaryCard({
    leaveRequest,
    footer,
}: LeaveRequestSummaryCardProps) {
    const initials =
        `${leaveRequest.employeeFirstName.charAt(0)}${leaveRequest.employeeLastName.charAt(0)}`.toUpperCase();
    const leaveTypeLabel = leaveTypeLabels[leaveRequest.leaveType] ?? leaveRequest.leaveType;
    const dateRange = `${formatLeaveDate(leaveRequest.startDate)} – ${formatLeaveDate(leaveRequest.endDate)}`;
    const hasReason = !!leaveRequest.reason?.trim();

    return (
        <Card variant="outlined" sx={{ maxWidth: 640 }}>
            <CardContent sx={{ display: "grid", gap: 2.5, textAlign: "left", p: 3 }}>
                <Box sx={{ display: "flex", gap: 2, alignItems: "center", flexWrap: "wrap" }}>
                    <Avatar
                        sx={{
                            width: 64,
                            height: 64,
                            bgcolor: "primary.main",
                            color: "primary.contrastText",
                            fontSize: "1.25rem",
                            fontWeight: 700,
                        }}
                    >
                        {initials}
                    </Avatar>
                    <Box sx={{ flex: 1, minWidth: 0 }}>
                        <Typography variant="h5" fontWeight={700} gutterBottom>
                            {leaveRequest.employeeFirstName} {leaveRequest.employeeLastName}
                        </Typography>
                        <LeaveRequestStatusChip status={leaveRequest.status} />
                    </Box>
                </Box>

                <Divider />

                <Box sx={{ display: "grid", gap: 2 }}>
                    <InfoRow
                        icon={<EventBusyIcon fontSize="small" />}
                        label="Type de congé"
                        value={leaveTypeLabel}
                    />
                    <InfoRow
                        icon={<CalendarMonthIcon fontSize="small" />}
                        label="Période"
                        value={dateRange}
                    />
                    <InfoRow
                        icon={<NotesIcon fontSize="small" />}
                        label="Motif"
                        value={
                            hasReason ? (
                                <Box
                                    sx={{
                                        mt: 0.5,
                                        p: 1.5,
                                        borderRadius: 1,
                                        bgcolor: "action.hover",
                                        border: "1px solid",
                                        borderColor: "divider",
                                    }}
                                >
                                    <Typography
                                        variant="body1"
                                        sx={{ whiteSpace: "pre-wrap", wordBreak: "break-word" }}
                                    >
                                        {leaveRequest.reason}
                                    </Typography>
                                </Box>
                            ) : (
                                <Typography variant="body1" color="text.secondary" fontStyle="italic">
                                    Aucun motif indiqué
                                </Typography>
                            )
                        }
                    />
                </Box>

                {footer && (
                    <>
                        <Divider />
                        {footer}
                    </>
                )}
            </CardContent>
        </Card>
    );
}
