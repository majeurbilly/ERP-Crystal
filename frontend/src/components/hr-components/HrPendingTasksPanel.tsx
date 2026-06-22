import ArrowForwardIcon from "@mui/icons-material/ArrowForward";
import BeachAccessOutlinedIcon from "@mui/icons-material/BeachAccessOutlined";
import InboxOutlinedIcon from "@mui/icons-material/InboxOutlined";
import ScheduleOutlinedIcon from "@mui/icons-material/ScheduleOutlined";
import {
    Avatar,
    Box,
    Button,
    Chip,
    List,
    ListItem,
    ListItemAvatar,
    ListItemText,
    Paper,
    Tab,
    Tabs,
    Typography,
} from "@mui/material";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useMemo, useState } from "react";
import { Link } from "react-router-dom";
import leaveRequestService from "../../api/services/hr/leaveRequestService";
import timesheetService from "../../api/services/hr/timesheetService";
import { useLeaveRequestMutations } from "../../api/mutations/hr/useLeaveRequestMutations";
import { useTimesheetMutations } from "../../api/mutations/hr/useTimesheetMutations";
import { hrMetricsCacheKey, leaveRequestsCacheKey, timesheetsCacheKey } from "../../data/cacheKeys";
import { leaveTypeLabels } from "../../data/gridColumns";
import {
    LEAVE_REQUEST_STATUSES,
    type LeaveRequest,
} from "../../data/types/hr/leaveRequest";
import { TIMESHEET_STATUSES, type Timesheet } from "../../data/types/hr/timesheet";
import {
    ROUTE_LEAVE_REQUESTS,
    ROUTE_TIMESHEETS,
    ROUTE_TIMESHEET_DETAILS,
    buildLeaveRequestDetailsPath,
} from "../../data/routeNames";
import { notifyErrorMessage, notifySuccessMessage } from "../../data/utils/popupMessageManager";
import { extractApiErrorMessage } from "../../data/utils/extractApiErrorMessage";
import { usePermissions } from "../../permissions/usePermissions";
import { CRUD_OPERATIONS, ENTITY_TYPES } from "../../permissions/permissions";
import LeaveRequestApprovalActions from "./LeaveRequestApprovalActions";

const MAX_ITEMS: number = 6;

type PendingTab = "timesheets" | "leaves";

const periodFormatter = new Intl.DateTimeFormat("fr-CA", {
    year: "numeric",
    month: "short",
    day: "numeric",
});

function formatPeriod(p_start: string, p_end: string): string {
    const start: string = periodFormatter.format(new Date(`${p_start}T00:00:00`));
    const end: string = periodFormatter.format(new Date(`${p_end}T00:00:00`));
    return `${start} – ${end}`;
}

interface PendingTaskRowProps {
    primary: string;
    secondary: string;
    avatarColor: "primary" | "info";
    avatarIcon: React.ReactNode;
    detailLink?: string;
    disabled?: boolean;
    onApprove: () => void;
    onReject: () => void;
}

function PendingTaskRow({
    primary,
    secondary,
    avatarColor,
    avatarIcon,
    detailLink,
    disabled = false,
    onApprove,
    onReject,
}: PendingTaskRowProps) {
    return (
        <ListItem
            component={detailLink ? Link : "li"}
            to={detailLink}
            disablePadding
            sx={{
                px: 2,
                py: 1.25,
                borderBottom: "1px solid",
                borderColor: "divider",
                "&:last-of-type": { borderBottom: 0 },
                textDecoration: "none",
                color: "inherit",
                display: "grid",
                gridTemplateColumns: "auto 1fr auto",
                alignItems: "center",
                gap: 1.5,
                "&:hover": detailLink
                    ? { bgcolor: "action.hover" }
                    : undefined,
            }}
        >
            <ListItemAvatar sx={{ minWidth: 0, mr: 0 }}>
                <Avatar
                    sx={{
                        width: 40,
                        height: 40,
                        bgcolor: (theme) => theme.palette[avatarColor].light,
                        color: (theme) => theme.palette[avatarColor].dark,
                    }}
                >
                    {avatarIcon}
                </Avatar>
            </ListItemAvatar>
            <ListItemText
                primary={primary}
                secondary={secondary}
                primaryTypographyProps={{ fontWeight: 600, variant: "body2" }}
                secondaryTypographyProps={{ variant: "caption" }}
                sx={{ my: 0 }}
            />
            <Box
                onClick={(p_event) => p_event.preventDefault()}
                onMouseDown={(p_event) => p_event.stopPropagation()}
            >
                <LeaveRequestApprovalActions
                    variant="compact"
                    disabled={disabled}
                    onApprove={onApprove}
                    onReject={onReject}
                />
            </Box>
        </ListItem>
    );
}

export default function HrPendingTasksPanel() {
    const queryClient = useQueryClient();
    const { ability } = usePermissions(ENTITY_TYPES.TIMESHEET);
    const { canUpdate: canUpdateLeaveRequests } = usePermissions(ENTITY_TYPES.LEAVE_REQUEST);
    const { updateTimesheetStatus, isUpdatingTimesheetStatus } = useTimesheetMutations();
    const { updateLeaveRequestStatus, isUpdatingLeaveRequestStatus } = useLeaveRequestMutations();
    const canApproveTimesheets: boolean = ability.can(
        CRUD_OPERATIONS.APPROVE,
        ENTITY_TYPES.TIMESHEET
    );

    const showPanel: boolean = canApproveTimesheets || canUpdateLeaveRequests;

    const timesheetsQuery = useQuery<Timesheet[], Error>({
        queryKey: timesheetsCacheKey.list(),
        queryFn: () => timesheetService.getAll(),
        enabled: showPanel && canApproveTimesheets,
    });

    const leaveRequestsQuery = useQuery<LeaveRequest[], Error>({
        queryKey: leaveRequestsCacheKey.list(),
        queryFn: () => leaveRequestService.getAll(),
        enabled: showPanel && canUpdateLeaveRequests,
    });

    const allPendingTimesheets: Timesheet[] = useMemo(
        () =>
            (timesheetsQuery.data ?? []).filter(
                (p_item: Timesheet) => p_item.status === TIMESHEET_STATUSES.Submitted
            ),
        [timesheetsQuery.data]
    );

    const allPendingLeaveRequests: LeaveRequest[] = useMemo(
        () =>
            (leaveRequestsQuery.data ?? []).filter(
                (p_item: LeaveRequest) => p_item.status === LEAVE_REQUEST_STATUSES.Pending
            ),
        [leaveRequestsQuery.data]
    );

    const pendingTimesheets: Timesheet[] = allPendingTimesheets.slice(0, MAX_ITEMS);
    const pendingLeaveRequests: LeaveRequest[] = allPendingLeaveRequests.slice(0, MAX_ITEMS);

    const showTimesheetTab: boolean = canApproveTimesheets && allPendingTimesheets.length > 0;
    const showLeaveTab: boolean = canUpdateLeaveRequests && allPendingLeaveRequests.length > 0;

    const defaultTab: PendingTab = showTimesheetTab ? "timesheets" : "leaves";
    const [activeTab, setActiveTab] = useState<PendingTab>(defaultTab);

    const resolvedTab: PendingTab =
        activeTab === "timesheets" && showTimesheetTab
            ? "timesheets"
            : activeTab === "leaves" && showLeaveTab
                ? "leaves"
                : defaultTab;

    if (!showPanel) {
        return null;
    }

    if (!showTimesheetTab && !showLeaveTab) {
        return null;
    }

    const totalPending: number = allPendingTimesheets.length + allPendingLeaveRequests.length;
    const isBusy: boolean = isUpdatingTimesheetStatus || isUpdatingLeaveRequestStatus;

    const invalidateMetrics = async (): Promise<void> => {
        await queryClient.invalidateQueries({ queryKey: hrMetricsCacheKey.dashboard() });
    };

    const handleApproveTimesheet = async (p_timesheet: Timesheet): Promise<void> => {
        try {
            await updateTimesheetStatus({
                id: p_timesheet.id,
                status: TIMESHEET_STATUSES.Approved,
            });
            await invalidateMetrics();
            notifySuccessMessage("Feuille de temps approuvée.");
        } catch (error: unknown) {
            notifyErrorMessage(extractApiErrorMessage(error));
        }
    };

    const handleRejectTimesheet = async (p_timesheet: Timesheet): Promise<void> => {
        try {
            await updateTimesheetStatus({
                id: p_timesheet.id,
                status: TIMESHEET_STATUSES.Rejected,
            });
            await invalidateMetrics();
            notifySuccessMessage("Feuille de temps refusée.");
        } catch (error: unknown) {
            notifyErrorMessage(extractApiErrorMessage(error));
        }
    };

    const handleApproveLeave = async (p_leaveRequest: LeaveRequest): Promise<void> => {
        try {
            await updateLeaveRequestStatus({
                id: p_leaveRequest.id,
                status: LEAVE_REQUEST_STATUSES.Approved,
            });
            await invalidateMetrics();
            notifySuccessMessage("Demande de congé approuvée.");
        } catch (error: unknown) {
            notifyErrorMessage(extractApiErrorMessage(error));
        }
    };

    const handleRejectLeave = async (p_leaveRequest: LeaveRequest): Promise<void> => {
        try {
            await updateLeaveRequestStatus({
                id: p_leaveRequest.id,
                status: LEAVE_REQUEST_STATUSES.Rejected,
            });
            await invalidateMetrics();
            notifySuccessMessage("Demande de congé refusée.");
        } catch (error: unknown) {
            notifyErrorMessage(extractApiErrorMessage(error));
        }
    };

    return (
        <Paper
            elevation={0}
            sx={{
                mt: 3,
                border: "1px solid",
                borderColor: "divider",
                borderRadius: 2,
                overflow: "hidden",
                bgcolor: "background.paper",
            }}
        >
            <Box
                sx={{
                    px: 2.5,
                    py: 2,
                    bgcolor: "background.default",
                    borderBottom: "1px solid",
                    borderColor: "divider",
                    display: "flex",
                    alignItems: "flex-start",
                    justifyContent: "space-between",
                    gap: 2,
                }}
            >
                <Box sx={{ display: "flex", gap: 1.5, alignItems: "flex-start" }}>
                    <InboxOutlinedIcon color="warning" sx={{ mt: 0.25 }} />
                    <Box sx={{ textAlign: 'left' }}>
                        <Typography variant="h6" lineHeight={1.3} color="text.primary" sx={{ mb: 0.5 }}>
                            À traiter maintenant
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                            Validez les demandes en un clic, sans quitter l&apos;accueil.
                        </Typography>
                    </Box>
                </Box>
                <Chip
                    label={`${totalPending} en attente`}
                    color="warning"
                    size="small"
                    variant="outlined"
                />
            </Box>

            {showTimesheetTab && showLeaveTab && (
                <Tabs
                    value={resolvedTab}
                    onChange={(_p_event, p_value: PendingTab) => setActiveTab(p_value)}
                    variant="fullWidth"
                    textColor="primary"
                    indicatorColor="primary"
                    sx={{
                        borderBottom: 1,
                        borderColor: "divider",
                        minHeight: 44,
                        "& .MuiTab-root": {
                            textTransform: "none",
                            fontWeight: 600,
                            color: "text.secondary",
                            "&.Mui-selected": {
                                color: "primary.main",
                            }
                        },
                    }}
                >
                    <Tab
                        value="timesheets"
                        label={`Feuilles de temps (${allPendingTimesheets.length})`}
                    />
                    <Tab
                        value="leaves"
                        label={`Congés (${allPendingLeaveRequests.length})`}
                    />
                </Tabs>
            )}

            <List disablePadding dense>
                {resolvedTab === "timesheets" &&
                    pendingTimesheets.map((p_timesheet: Timesheet) => (
                        <PendingTaskRow
                            key={`ts-${p_timesheet.id}`}
                            primary={`${p_timesheet.employeeFirstName} ${p_timesheet.employeeLastName}`}
                            secondary={formatPeriod(p_timesheet.periodStart, p_timesheet.periodEnd)}
                            avatarColor="primary"
                            avatarIcon={<ScheduleOutlinedIcon fontSize="small" />}
                            detailLink={ROUTE_TIMESHEET_DETAILS.replace(
                                ":id",
                                String(p_timesheet.id)
                            )}
                            disabled={isBusy}
                            onApprove={() => void handleApproveTimesheet(p_timesheet)}
                            onReject={() => void handleRejectTimesheet(p_timesheet)}
                        />
                    ))}

                {resolvedTab === "leaves" &&
                    pendingLeaveRequests.map((p_leaveRequest: LeaveRequest) => (
                        <PendingTaskRow
                            key={`lr-${p_leaveRequest.id}`}
                            primary={`${p_leaveRequest.employeeFirstName} ${p_leaveRequest.employeeLastName}`}
                            secondary={`${leaveTypeLabels[p_leaveRequest.leaveType]} · ${formatPeriod(
                                p_leaveRequest.startDate,
                                p_leaveRequest.endDate
                            )}`}
                            avatarColor="info"
                            avatarIcon={<BeachAccessOutlinedIcon fontSize="small" />}
                            detailLink={buildLeaveRequestDetailsPath(p_leaveRequest.id)}
                            disabled={isBusy}
                            onApprove={() => void handleApproveLeave(p_leaveRequest)}
                            onReject={() => void handleRejectLeave(p_leaveRequest)}
                        />
                    ))}
            </List>

            <Box
                sx={{
                    px: 2,
                    py: 1.5,
                    borderTop: "1px solid",
                    borderColor: "divider",
                    bgcolor: "background.default",
                    display: "flex",
                    justifyContent: "flex-end",
                }}
            >
                {resolvedTab === "timesheets" ? (
                    <Button
                        component={Link}
                        to={`${ROUTE_TIMESHEETS}?status=${TIMESHEET_STATUSES.Submitted}`}
                        size="small"
                        endIcon={<ArrowForwardIcon />}
                        sx={{ textTransform: "none", color: "primary.main" }}
                    >
                        Voir toutes les feuilles de temps
                    </Button>
                ) : (
                    <Button
                        component={Link}
                        to={`${ROUTE_LEAVE_REQUESTS}?status=${LEAVE_REQUEST_STATUSES.Pending}`}
                        size="small"
                        endIcon={<ArrowForwardIcon />}
                        sx={{ textTransform: "none", color: "primary.main" }}
                    >
                        Voir toutes les demandes de congé
                    </Button>
                )}
            </Box>
        </Paper>
    );
}
