import { useEffect, useMemo, useState } from "react";
import { useNavigate, useSearchParams, Link as RouterLink } from "react-router-dom";
import {
    Box,
    Button,
    Link,
    Stack,
    Typography,
} from "@mui/material";
import CalendarMonthIcon from "@mui/icons-material/CalendarMonth";
import EventBusyIcon from "@mui/icons-material/EventBusy";
import PersonIcon from "@mui/icons-material/Person";
import AccessTimeIcon from "@mui/icons-material/AccessTime";
import DescriptionIcon from "@mui/icons-material/Description";
import AssignmentIndIcon from "@mui/icons-material/AssignmentInd";
import PaymentsIcon from "@mui/icons-material/Payments";
import { useQuery } from "@tanstack/react-query";
import GenericPageLayout from "../components/layouts/GenericPageLayout";
import PageQueryWrapper from "../components/layouts/PageQueryWrapper";
import { CustomDataGrid } from "../components/data-grids/CustomDataGrid";
import ScheduleCalendarPanel from "../components/hr-components/ScheduleCalendarPanel";
import MySpaceNavCards, { type MySpaceNavCardConfig } from "../components/my-space/MySpaceNavCards";
import EmployeeProfileSummaryCard from "../components/hr-components/EmployeeProfileSummaryCard";
import { useAuth } from "../context/AuthContext";
import {
    ROUTE_MY_PROFILE,
    ROUTE_DASHBOARD,
    ROUTE_EMPLOYMENT_CONTRACTS,
    ROUTE_PAYROLL,
    ROUTE_TIMESHEET_DETAILS,
    buildLeaveRequestDetailsPath,
} from "../data/routeNames";
import scheduledShiftService from "../api/services/hr/scheduledShiftService";
import leaveRequestService from "../api/services/hr/leaveRequestService";
import timeEntryService from "../api/services/hr/timeEntryService";
import timesheetService from "../api/services/hr/timesheetService";
import employeeProfileService from "../api/services/hr/employeeProfileService";
import {
    leaveRequestsCacheKey,
    scheduledShiftsCacheKey,
    timeEntriesCacheKey,
    timesheetsCacheKey,
    employeeProfilesCacheKey,
} from "../data/cacheKeys";
import { buildLeaveRequestColumns, timeEntryColumns, timesheetColumns } from "../data/gridColumns";
import { LEAVE_REQUEST_STATUSES } from "../data/types/hr/leaveRequest";
import { TIMESHEET_STATUSES } from "../data/types/hr/timesheet";
import { usePermissions } from "../permissions/usePermissions";
import { ENTITY_TYPES } from "../permissions/permissions";
import LeaveRequestForm from "../components/forms/hr/LeaveRequestForm";
import { findNextShift } from "../components/dashboard/DashboardWidgetGrid";

const TAB_KEYS = ["horaire", "conges", "fiche", "pointages", "feuille"] as const;
type TabKey = (typeof TAB_KEYS)[number];

function isTabKey(p_value: string | null): p_value is TabKey {
    return TAB_KEYS.includes(p_value as TabKey);
}

function getTodayMonthKey(): string {
    return new Date().toISOString().substring(0, 7);
}

function getTodayKey(): string {
    return new Date().toISOString().substring(0, 10);
}

export default function MySpacePage() {
    const navigate = useNavigate();
    const { user } = useAuth();
    const [searchParams, setSearchParams] = useSearchParams();
    const tabParam = searchParams.get("tab");
    const [activeTab, setActiveTab] = useState<TabKey>(isTabKey(tabParam) ? tabParam : "horaire");
    const [showLeaveForm, setShowLeaveForm] = useState(false);
    const { canCreate } = usePermissions(ENTITY_TYPES.LEAVE_REQUEST);
    const { canRead: canReadHrDashboard } = usePermissions(ENTITY_TYPES.HR_DASHBOARD);
    const { canRead: canReadEmploymentContract } = usePermissions(ENTITY_TYPES.EMPLOYMENT_CONTRACT);
    const { canRead: canReadPayroll } = usePermissions(ENTITY_TYPES.PAYROLL);

    useEffect(() => {
        if (isTabKey(tabParam)) {
            setActiveTab(tabParam);
        }
    }, [tabParam]);

    const handleTabSelect = (p_value: string): void => {
        if (!isTabKey(p_value)) {
            return;
        }
        setActiveTab(p_value);
        setSearchParams({ tab: p_value });
    };

    const shiftsQuery = useQuery({
        queryKey: scheduledShiftsCacheKey.list(),
        queryFn: () => scheduledShiftService.getAll(),
    });

    const leavesQuery = useQuery({
        queryKey: leaveRequestsCacheKey.list(),
        queryFn: () => leaveRequestService.getAll(),
    });

    const timeEntriesQuery = useQuery({
        queryKey: timeEntriesCacheKey.list(),
        queryFn: () => timeEntryService.getAll(),
    });

    const timesheetsQuery = useQuery({
        queryKey: timesheetsCacheKey.list(),
        queryFn: () => timesheetService.getAll(),
    });

    const profileQuery = useQuery({
        queryKey: employeeProfilesCacheKey.me(),
        queryFn: () => employeeProfileService.getMe(),
    });

    const leaveColumns = useMemo(
        () => buildLeaveRequestColumns().filter((p_col) => p_col.field !== "employeeName"),
        []
    );

    const navCards: MySpaceNavCardConfig[] = useMemo(() => {
        const today = getTodayKey();
        const monthKey = getTodayMonthKey();
        const shifts = shiftsQuery.data ?? [];
        const leaves = leavesQuery.data ?? [];
        const timeEntries = timeEntriesQuery.data ?? [];
        const timesheets = timesheetsQuery.data ?? [];
        const profile = profileQuery.data;

        const upcomingShifts = shifts.filter((p_shift) => p_shift.date >= today);
        const monthShifts = shifts.filter((p_shift) => p_shift.date.startsWith(monthKey));
        const nextShift = findNextShift(shifts);
        const pendingLeaves = leaves.filter((p_item) => p_item.status === LEAVE_REQUEST_STATUSES.Pending).length;
        const submittedTimesheets = timesheets.filter((p_item) => p_item.status === TIMESHEET_STATUSES.Submitted).length;

        return [
            {
                id: "horaire",
                label: "Horaire",
                preview: nextShift
                    ? `Prochain : ${nextShift.date} (${nextShift.startTime})`
                    : upcomingShifts.length > 0
                        ? `${monthShifts.length} quarts ce mois`
                        : "Aucun quart planifié",
                icon: <CalendarMonthIcon fontSize="large" />,
            },
            {
                id: "conges",
                label: "Congés",
                preview: pendingLeaves > 0
                    ? `${pendingLeaves} demande${pendingLeaves > 1 ? "s" : ""} en attente`
                    : leaves.length > 0
                        ? `${leaves.length} demande${leaves.length > 1 ? "s" : ""} au total`
                        : "Aucune demande",
                icon: <EventBusyIcon fontSize="large" />,
            },
            {
                id: "fiche",
                label: "Ma fiche",
                preview: profile
                    ? `${profile.firstName} ${profile.lastName}`
                    : "Profil non lié",
                icon: <PersonIcon fontSize="large" />,
            },
            {
                id: "pointages",
                label: "Pointages",
                preview: timeEntries.length > 0
                    ? `${timeEntries.length} entrée${timeEntries.length > 1 ? "s" : ""}`
                    : "Aucun pointage",
                icon: <AccessTimeIcon fontSize="large" />,
            },
            {
                id: "feuille",
                label: "Feuille de temps",
                preview: submittedTimesheets > 0
                    ? `${submittedTimesheets} en attente d'approbation`
                    : timesheets.length > 0
                        ? `${timesheets.length} feuille${timesheets.length > 1 ? "s" : ""}`
                        : "Aucune feuille",
                icon: <DescriptionIcon fontSize="large" />,
            },
        ];
    }, [shiftsQuery.data, leavesQuery.data, timeEntriesQuery.data, timesheetsQuery.data, profileQuery.data]);

    return (
        <GenericPageLayout title="Mon espace">
            <MySpaceNavCards
                cards={navCards}
                activeId={activeTab}
                onSelect={handleTabSelect}
            />

            {activeTab === "horaire" && (
                <PageQueryWrapper
                    isLoading={shiftsQuery.isLoading}
                    error={shiftsQuery.error}
                    refetch={shiftsQuery.refetch}
                    errorReturnUrl={ROUTE_DASHBOARD}
                    errorReturnLabel="Retour au tableau de bord"
                >
                    <ScheduleCalendarPanel ownEmployeeProfileId={profileQuery.data?.id} />
                </PageQueryWrapper>
            )}

            {activeTab === "conges" && (
                <PageQueryWrapper
                    isLoading={leavesQuery.isLoading}
                    error={leavesQuery.error}
                    refetch={leavesQuery.refetch}
                    errorReturnUrl={ROUTE_DASHBOARD}
                    errorReturnLabel="Retour au tableau de bord"
                >
                    {canCreate && (
                        <Box sx={{ mb: 2 }}>
                            <Button
                                variant="contained"
                                onClick={() => setShowLeaveForm(true)}
                                sx={{
                                    bgcolor: "actionButtons.add.bg",
                                    color: "actionButtons.add.text",
                                    "&:hover": { bgcolor: "actionButtons.add.bg", opacity: 0.9 },
                                }}
                            >
                                Demander un congé
                            </Button>
                        </Box>
                    )}
                    <CustomDataGrid
                        rows={leavesQuery.data ?? []}
                        columns={leaveColumns}
                        onRowClick={(p_params) => navigate(buildLeaveRequestDetailsPath(p_params.id))}
                        sx={{
                            "& .MuiDataGrid-row": { cursor: "pointer" },
                        }}
                    />
                    <LeaveRequestForm
                        showLeaveRequestForm={showLeaveForm}
                        setShowLeaveRequestForm={setShowLeaveForm}
                        selfMode
                        defaultEmployeeProfileId={user?.employeeProfile?.id}
                    />
                </PageQueryWrapper>
            )}

            {activeTab === "fiche" && (
                <PageQueryWrapper
                    isLoading={profileQuery.isLoading}
                    error={profileQuery.error}
                    refetch={profileQuery.refetch}
                    errorReturnUrl={ROUTE_DASHBOARD}
                    errorReturnLabel="Retour au tableau de bord"
                >
                    {profileQuery.data && (
                        <EmployeeProfileSummaryCard
                            profile={profileQuery.data}
                            showSalary={canReadHrDashboard}
                            footer={
                                <Stack spacing={1.5} alignItems="flex-start">
                                    <Typography>
                                        <Link component={RouterLink} to={ROUTE_MY_PROFILE}>
                                            Modifier mon courriel / mot de passe
                                        </Link>
                                    </Typography>
                                    <Stack direction={{ xs: "column", sm: "row" }} spacing={1}>
                                        {canReadEmploymentContract && (
                                            <Button
                                                component={RouterLink}
                                                to={ROUTE_EMPLOYMENT_CONTRACTS}
                                                variant="outlined"
                                                startIcon={<AssignmentIndIcon />}
                                            >
                                                Mes contrats
                                            </Button>
                                        )}
                                        {canReadPayroll && (
                                            <Button
                                                component={RouterLink}
                                                to={ROUTE_PAYROLL}
                                                variant="outlined"
                                                startIcon={<PaymentsIcon />}
                                            >
                                                Mes fiches de paie
                                            </Button>
                                        )}
                                    </Stack>
                                </Stack>
                            }
                        />
                    )}
                </PageQueryWrapper>
            )}

            {activeTab === "pointages" && (
                <PageQueryWrapper
                    isLoading={timeEntriesQuery.isLoading}
                    error={timeEntriesQuery.error}
                    refetch={timeEntriesQuery.refetch}
                    errorReturnUrl={ROUTE_DASHBOARD}
                    errorReturnLabel="Retour au tableau de bord"
                >
                    <CustomDataGrid rows={timeEntriesQuery.data ?? []} columns={timeEntryColumns} />
                </PageQueryWrapper>
            )}

            {activeTab === "feuille" && (
                <PageQueryWrapper
                    isLoading={timesheetsQuery.isLoading}
                    error={timesheetsQuery.error}
                    refetch={timesheetsQuery.refetch}
                    errorReturnUrl={ROUTE_DASHBOARD}
                    errorReturnLabel="Retour au tableau de bord"
                >
                    <CustomDataGrid
                        rows={timesheetsQuery.data ?? []}
                        columns={timesheetColumns}
                        onRowClick={(p_params) =>
                            navigate(ROUTE_TIMESHEET_DETAILS.replace(":id", String(p_params.id)))
                        }
                        sx={{
                            "& .MuiDataGrid-row": { cursor: "pointer" },
                        }}
                    />
                </PageQueryWrapper>
            )}
        </GenericPageLayout>
    );
}