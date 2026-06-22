import type { ReactElement } from "react";
import { Box, Card, CardActionArea, CardContent, Grid, Typography } from "@mui/material";
import GroupsIcon from "@mui/icons-material/Groups";
import PendingActionsIcon from "@mui/icons-material/PendingActions";
import EventBusyIcon from "@mui/icons-material/EventBusy";
import PaymentsIcon from "@mui/icons-material/Payments";
import { Link } from "react-router-dom";
import type { HrDashboardMetrics } from "../../data/types/hr/hrDashboardMetrics";
import { TIMESHEET_STATUSES } from "../../data/types/hr/timesheet";
import { LEAVE_REQUEST_STATUSES } from "../../data/types/hr/leaveRequest";
import {
    ROUTE_EMPLOYEE_PROFILES,
    ROUTE_LEAVE_REQUESTS,
    ROUTE_PAYROLL,
    ROUTE_TIMESHEETS,
} from "../../data/routeNames";

interface HrMetricsCardsProps {
    metrics: HrDashboardMetrics;
}

interface MetricCardConfig {
    label: string;
    value: string;
    icon: ReactElement;
    to?: string;
}

const payrollFormatter = new Intl.NumberFormat("fr-CA", {
    style: "currency",
    currency: "CAD",
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
});

function formatPayroll(p_amount: number): string {
    return payrollFormatter.format(p_amount);
}

export default function HrMetricsCards({ metrics }: HrMetricsCardsProps) {
    const cards: MetricCardConfig[] = [
        {
            label: "Employés actifs",
            value: String(metrics.totalActiveEmployees),
            icon: <GroupsIcon fontSize="large" color="primary" />,
            to: ROUTE_EMPLOYEE_PROFILES,
        },
        {
            label: "Feuilles de temps en attente",
            value: String(metrics.pendingTimesheetsCount),
            icon: <PendingActionsIcon fontSize="large" color="warning" />,
            to: `${ROUTE_TIMESHEETS}?status=${TIMESHEET_STATUSES.Submitted}`,
        },
        {
            label: "Demandes de congé en attente",
            value: String(metrics.pendingLeaveRequestsCount),
            icon: <EventBusyIcon fontSize="large" color="info" />,
            to: `${ROUTE_LEAVE_REQUESTS}?status=${LEAVE_REQUEST_STATUSES.Pending}`,
        },
        {
            label: "Masse salariale brute",
            value: formatPayroll(metrics.totalGrossPayroll),
            icon: <PaymentsIcon fontSize="large" color="success" />,
            to: ROUTE_PAYROLL,
        },
    ];

    return (
        <Grid container spacing={2} sx={{ mb: 3 }}>
            {cards.map((card: MetricCardConfig) => (
                <Grid key={card.label} size={{ xs: 12, sm: 6, md: 3 }}>
                    <Card variant="outlined" sx={{ height: "100%" }}>
                        {card.to ? (
                            <CardActionArea
                                component={Link}
                                to={card.to}
                                sx={{ height: "100%" }}
                            >
                                <CardContent>
                                    <MetricCardContent card={card} />
                                </CardContent>
                            </CardActionArea>
                        ) : (
                            <CardContent>
                                <MetricCardContent card={card} />
                            </CardContent>
                        )}
                    </Card>
                </Grid>
            ))}
        </Grid>
    );
}

function MetricCardContent({ card }: { card: MetricCardConfig }) {
    return (
        <Box
            sx={{
                display: "flex",
                alignItems: "flex-start",
                justifyContent: "space-between",
                gap: 1,
            }}
        >
            <Box>
                <Typography variant="body2" color="text.secondary" gutterBottom>
                    {card.label}
                </Typography>
                <Typography variant="h5" component="p" fontWeight={600}>
                    {card.value}
                </Typography>
            </Box>
            {card.icon}
        </Box>
    );
}
