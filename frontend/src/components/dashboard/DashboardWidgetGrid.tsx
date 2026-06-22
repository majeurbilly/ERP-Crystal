import { Grid } from "@mui/material";
import CalendarMonthIcon from "@mui/icons-material/CalendarMonth";
import EventBusyIcon from "@mui/icons-material/EventBusy";
import InventoryIcon from "@mui/icons-material/Inventory";
import ManageAccountsIcon from "@mui/icons-material/ManageAccounts";
import { useQuery } from "@tanstack/react-query";
import { useMemo } from "react";
import DashboardWidget from "./DashboardWidget";
import PunchClockWidget from "./PunchClockWidget";
import HrMetricsCards from "../hr-components/HrMetricsCards";
import { usePermissions } from "../../permissions/usePermissions";
import { ENTITY_TYPES } from "../../permissions/permissions";
import { ROUTE_MON_ESPACE, ROUTE_LEAVE_REQUESTS, ROUTE_TIMESHEETS, ROUTE_LIST_USER_ROLES, ROUTE_IR, ROUTE_CATALOGUE, ROUTE_SCHEDULES } from "../../data/routeNames";
import scheduledShiftService from "../../api/services/hr/scheduledShiftService";
import leaveRequestService from "../../api/services/hr/leaveRequestService";
import { scheduledShiftsCacheKey, leaveRequestsCacheKey } from "../../data/cacheKeys";
import { useHrDashboardMetrics } from "../../api/queries/useHrDashboardMetrics";
import { useAuth } from "../../context/AuthContext";
import { LEAVE_REQUEST_STATUSES } from "../../data/types/hr/leaveRequest";
import type { ScheduledShift } from "../../data/types/hr/scheduledShift";
import { getLocalDateKey } from "../../data/utils/dateUtils";

export function findNextShift(p_shifts: ScheduledShift[]): ScheduledShift | null {
    const today = getLocalDateKey();
    const upcoming = p_shifts
        .filter((p_shift) => p_shift.date >= today)
        .sort((p_a, p_b) => p_a.date.localeCompare(p_b.date) || p_a.startTime.localeCompare(p_b.startTime));
    return upcoming[0] ?? null;
}

export default function DashboardWidgetGrid() {
    const { canRead: canReadHrDashboard } = usePermissions(ENTITY_TYPES.HR_DASHBOARD);
    const { canRead: canReadLeave } = usePermissions(ENTITY_TYPES.LEAVE_REQUEST);
    const { canRead: canReadShift } = usePermissions(ENTITY_TYPES.SCHEDULED_SHIFT);
    const { canRead: canReadInventory } = usePermissions(ENTITY_TYPES.INVENTORY_QUANTITY);
    const { canRead: canReadUserRole } = usePermissions(ENTITY_TYPES.USER_ROLE);
    const { user } = useAuth();

    const shiftsQuery = useQuery({
        queryKey: scheduledShiftsCacheKey.list(),
        queryFn: () => scheduledShiftService.getAll(),
        enabled: canReadShift,
    });

    const leavesQuery = useQuery({
        queryKey: leaveRequestsCacheKey.list(),
        queryFn: () => leaveRequestService.getAll(),
        enabled: canReadLeave,
    });

    const metricsQuery = useHrDashboardMetrics({ enabled: canReadHrDashboard });

    const nextShift = useMemo(
        () => (shiftsQuery.data ? findNextShift(shiftsQuery.data) : null),
        [shiftsQuery.data]
    );

    const pendingLeavesCount = useMemo(() => {
        if (!leavesQuery.data) {
            return 0;
        }
        return leavesQuery.data.filter((p_item) => p_item.status === LEAVE_REQUEST_STATUSES.Pending).length;
    }, [leavesQuery.data]);

    const monEspaceSchedule = `${ROUTE_MON_ESPACE}?tab=horaire`;
    const monEspaceLeaves = `${ROUTE_MON_ESPACE}?tab=conges`;

    return (
        <Grid container spacing={2}>
            <Grid size={{ xs: 12 }}>
                <PunchClockWidget />
            </Grid>

            {canReadShift && (
                <Grid size={{ xs: 12, sm: 6 }} flexGrow={1}>
                    <DashboardWidget
                        title="Prochain quart"
                        value={nextShift ? nextShift.date : "—"}
                        subtitle={nextShift ? `${nextShift.startTime} – ${nextShift.endTime}` : "Aucun quart planifié"}
                        icon={<CalendarMonthIcon color="primary" fontSize="large" />}
                        to={canReadHrDashboard ? ROUTE_SCHEDULES : monEspaceSchedule}
                    />
                </Grid>
            )}

            {canReadLeave && (
                <Grid size={{ xs: 12, sm: 6 }} flexGrow={1}>
                    <DashboardWidget
                        title={canReadHrDashboard ? "Congés en attente" : "Mes congés en attente"}
                        value={pendingLeavesCount}
                        subtitle="Demandes à traiter ou en cours"
                        icon={<EventBusyIcon color="info" fontSize="large" />}
                        to={canReadHrDashboard ? `${ROUTE_LEAVE_REQUESTS}?status=Pending` : monEspaceLeaves}
                    />
                </Grid>
            )}

            {canReadInventory && !canReadHrDashboard && (
                <Grid size={{ xs: 12, sm: 6 }} flexGrow={1}>
                    <DashboardWidget
                        title="Inventaire"
                        value="Consulter"
                        subtitle="Quantités par succursale"
                        icon={<InventoryIcon color="secondary" fontSize="large" />}
                        to={ROUTE_IR}
                    />
                </Grid>
            )}

            {canReadHrDashboard && metricsQuery.data && (
                <Grid size={{ xs: 12 }}>
                    <HrMetricsCards metrics={metricsQuery.data} />
                </Grid>
            )}

            {canReadHrDashboard && metricsQuery.data && (
                <>
                    <Grid size={{ xs: 12, sm: 6 }} flexGrow={1}>
                        <DashboardWidget
                            title="Feuilles de temps en attente"
                            value={metricsQuery.data.pendingTimesheetsCount}
                            icon={<EventBusyIcon color="warning" fontSize="large" />}
                            to={`${ROUTE_TIMESHEETS}?status=Submitted`}
                        />
                    </Grid>
                    <Grid size={{ xs: 12, sm: 6 }} flexGrow={1}>
                        <DashboardWidget
                            title="Alertes catalogue"
                            value="Voir"
                            subtitle="Articles et stock"
                            icon={<InventoryIcon color="error" fontSize="large" />}
                            to={ROUTE_CATALOGUE}
                        />
                    </Grid>
                </>
            )}

            {canReadUserRole && (
                <Grid size={{ xs: 12, sm: 6 }} flexGrow={1}>
                    <DashboardWidget
                        title="Rôles et permissions"
                        value="Gérer"
                        subtitle={`Connecté : ${user?.dynamicRole?.name ?? ""}`}
                        icon={<ManageAccountsIcon color="primary" fontSize="large" />}
                        to={ROUTE_LIST_USER_ROLES}
                    />
                </Grid>
            )}
        </Grid>
    );
}
