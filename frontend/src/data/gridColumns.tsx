import { type GridColDef } from "@mui/x-data-grid";
import type { Item } from "./types/inventory/item";
import { getAssignedRoleDisplayName } from "./types/hr/userRoles";
import type { User } from "./types/hr/user";
import type { Location } from "./types/inventory/location";
import type { Category } from "./types/inventory/category";
import type { JobPosition } from "./types/hr/jobPosition";
import type { EmployeeProfile } from "./types/hr/employeeProfile";
import type { EmploymentContract } from "./types/hr/employmentContract";
import type { ScheduledShift } from "./types/hr/scheduledShift";
import type { TimeEntry } from "./types/hr/timeEntry";
import type { Timesheet } from "./types/hr/timesheet";
import type { PayStub } from "./types/hr/payStub";
import TimesheetStatusChip from "../components/hr-components/TimesheetStatusChip";
import type { ContractType, WageType } from "./types/hr/employmentContract";
import { CONTRACT_TYPE_LABELS, WAGE_TYPE_LABELS } from "./types/hr/employmentContract";
import { Box, Tooltip, Typography } from "@mui/material";
import StorefrontOutlinedIcon from "@mui/icons-material/StorefrontOutlined";
import type { LeaveRequest, LeaveType } from "./types/hr/leaveRequest";
import { LEAVE_TYPES, LEAVE_REQUEST_STATUSES } from "./types/hr/leaveRequest";
import LeaveRequestStatusChip from "../components/hr-components/LeaveRequestStatusChip";
import LeaveRequestApprovalActions from "../components/hr-components/LeaveRequestApprovalActions";
import { getInventoryDisplayColor } from "./utils/inventoryHelpers";
import type { Author } from "./types/inventory/author";
import type { DynamicUserRole, PermissionRule } from "./types/hr/dynamicUserRole";
import type { PermissionEntity } from "./types/permissionEntity";
import { getActionLabel, getEntityLabel } from "../permissions/permissionLabels";

const grossPayFormatter = new Intl.NumberFormat("fr-CA", {
    style: "currency",
    currency: "CAD",
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
});

export const itemColumns: GridColDef<Item>[] = [
    { field: "name", headerName: "Nom", flex: 1.5 },
    {
        field: "isBook",
        headerName: "Type",
        flex: 1.5,
        valueGetter: (value: boolean) => (value ? "Livre" : "Produit"),
    },
];

export const userColumns: GridColDef<User>[] = [
    { field: "userName", headerName: "Nom d'utilisateur", flex: 1.5 },
    { field: "email", headerName: "Courriel", flex: 1.5 },
    {
        field: "dynamicRoleName",
        headerName: "Rôle",
        flex: 1.5,
        valueGetter: (_value: string | null | undefined, row: User) =>
            getAssignedRoleDisplayName(row),
    }
];

export const locationColumns: GridColDef<Location>[] = [
    {
        field: "title",
        headerName: "Succursale",
        flex: 1.4,
        renderCell: (params) => (
            <Box sx={{ display: "flex", alignItems: "center", gap: 1, overflow: "hidden" }}>
                <StorefrontOutlinedIcon fontSize="small" color="primary" sx={{ flexShrink: 0 }} />
                <Typography variant="body2" fontWeight={600} noWrap>
                    {params.row.title}
                </Typography>
            </Box>
        ),
    },
    { field: "address", headerName: "Adresse", flex: 2 },
    {
        field: "description",
        headerName: "Description",
        flex: 1.6,
        valueFormatter: (value: string) => value?.trim() || "—",
    },
];

export const categoryColumns: GridColDef<Category>[] = [
    { field: "name", headerName: "Nom", flex: 1.5 },
];

export const userRoleColumns: GridColDef<DynamicUserRole>[] = [
    { field: "name", headerName: "Nom", flex: 1.5 },
];

export const jobPositionColumns: GridColDef<JobPosition>[] = [
    { field: "name", headerName: "Nom", flex: 1.2 },
    { field: "description", headerName: "Description", flex: 2 },
];

const hiringDateFormatter = new Intl.DateTimeFormat("fr-CA", {
    year: "numeric",
    month: "short",
    day: "numeric",
});

const contractDateFormatter = new Intl.DateTimeFormat("fr-CA", {
    year: "numeric",
    month: "short",
    day: "numeric",
});

const baseRateFormatter = new Intl.NumberFormat("fr-CA", {
    style: "currency",
    currency: "CAD",
});

export const employmentContractColumns: GridColDef<EmploymentContract>[] = [
    {
        field: "contractType",
        headerName: "Type",
        flex: 0.8,
        valueFormatter: (p_value: ContractType) => CONTRACT_TYPE_LABELS[p_value] ?? p_value,
    },
    {
        field: "wageType",
        headerName: "Rémunération",
        flex: 0.9,
        valueFormatter: (p_value: WageType) => WAGE_TYPE_LABELS[p_value] ?? p_value,
    },
    {
        field: "baseRate",
        headerName: "Montant de base",
        flex: 1,
        valueFormatter: (p_value: number) => baseRateFormatter.format(p_value),
    },
    {
        field: "startDate",
        headerName: "Début",
        flex: 1,
        valueFormatter: (p_value: string) => formatContractDate(p_value),
    },
    {
        field: "endDate",
        headerName: "Fin",
        flex: 1,
        valueFormatter: (p_value: string | null) => (p_value ? formatContractDate(p_value) : "—"),
    },
];

function formatContractDate(p_value: string): string {
    const parsedDate: Date = new Date(`${p_value}T00:00:00`);
    if (Number.isNaN(parsedDate.getTime())) {
        return p_value;
    }
    return contractDateFormatter.format(parsedDate);
}

const leaveDateFormatter = new Intl.DateTimeFormat("fr-CA", {
    year: "numeric",
    month: "short",
    day: "numeric",
});

export const leaveTypeLabels: Record<LeaveType, string> = {
    [LEAVE_TYPES.Vacation]: "Vacances",
    [LEAVE_TYPES.Sick]: "Maladie",
    [LEAVE_TYPES.Unpaid]: "Sans solde",
    [LEAVE_TYPES.Other]: "Autre",
};

export function formatLeaveDate(p_value: string): string {
    const parsedDate: Date = new Date(`${p_value}T00:00:00`);
    if (Number.isNaN(parsedDate.getTime())) {
        return p_value;
    }
    return leaveDateFormatter.format(parsedDate);
}

export interface LeaveRequestColumnOptions {
    canManageStatus?: boolean;
    onApprove?: (p_leaveRequest: LeaveRequest) => void;
    onReject?: (p_leaveRequest: LeaveRequest) => void;
}

export function buildLeaveRequestColumns(
    p_options: LeaveRequestColumnOptions = {},
): GridColDef<LeaveRequest>[] {
    const columns: GridColDef<LeaveRequest>[] = [
        {
            field: "employeeName",
            headerName: "Employé",
            flex: 1.2,
            valueGetter: (_p_value: unknown, p_row: LeaveRequest) =>
                `${p_row.employeeFirstName} ${p_row.employeeLastName}`,
        },
        {
            field: "dateRange",
            headerName: "Dates",
            flex: 1.2,
            valueGetter: (_p_value: unknown, p_row: LeaveRequest) =>
                `${formatLeaveDate(p_row.startDate)} – ${formatLeaveDate(p_row.endDate)}`,
        },
        {
            field: "leaveType",
            headerName: "Type",
            flex: 0.9,
            valueFormatter: (p_value: LeaveType) => leaveTypeLabels[p_value] ?? p_value,
        },
        {
            field: "status",
            headerName: "Statut",
            flex: 0.9,
            renderCell: (p_params) => <LeaveRequestStatusChip status={p_params.row.status} />,
        },
        {
            field: "reason",
            headerName: "Motif",
            flex: 1.2,
            renderCell: (p_params) => {
                const reason: string | null = p_params.row.reason;
                const displayText: string = reason?.trim() ? reason : "—";

                return (
                    <Tooltip title={displayText} placement="top-start" enterDelay={400}>
                        <Typography
                            variant="body2"
                            sx={{
                                overflow: "hidden",
                                textOverflow: "ellipsis",
                                whiteSpace: "nowrap",
                                width: "100%",
                            }}
                        >
                            {displayText}
                        </Typography>
                    </Tooltip>
                );
            },
        },
    ];

    if (p_options.canManageStatus) {
        columns.push({
            field: "approvalActions",
            headerName: "Approbation",
            width: 110,
            minWidth: 110,
            maxWidth: 110,
            sortable: false,
            filterable: false,
            disableColumnMenu: true,
            align: "center",
            headerAlign: "center",
            renderCell: (p_params) => {
                const row: LeaveRequest = p_params.row;
                const isPending: boolean = row.status === LEAVE_REQUEST_STATUSES.Pending;

                if (!isPending || !p_options.onApprove || !p_options.onReject) {
                    return (
                        <Typography variant="body2" color="text.secondary">
                            —
                        </Typography>
                    );
                }

                return (
                    <Box
                        sx={{
                            display: "flex",
                            width: "100%",
                            height: "100%",
                            alignItems: "center",
                            justifyContent: "center",
                        }}
                        onClick={(p_event) => p_event.stopPropagation()}
                        onMouseDown={(p_event) => p_event.stopPropagation()}
                    >
                        <LeaveRequestApprovalActions
                            variant="compact"
                            onApprove={() => p_options.onApprove?.(row)}
                            onReject={() => p_options.onReject?.(row)}
                        />
                    </Box>
                );
            },
        });
    }

    return columns;
}

const shiftDateFormatter = new Intl.DateTimeFormat("fr-CA", {
    year: "numeric",
    month: "short",
    day: "numeric",
});

function formatShiftDate(p_value: string): string {
    const parsedDate: Date = new Date(`${p_value}T00:00:00`);
    if (Number.isNaN(parsedDate.getTime())) {
        return p_value;
    }
    return shiftDateFormatter.format(parsedDate);
}

function parseTimeToMinutes(p_value: string | null | undefined): number | null {
    if (!p_value || p_value.trim().length === 0) {
        return null;
    }

    const [hours, minutes] = p_value.split(":").map(Number);
    if (!Number.isFinite(hours) || !Number.isFinite(minutes)) {
        return null;
    }

    return hours * 60 + minutes;
}

export function getTimeEntryDurationHours(p_entry: TimeEntry): number | null {
    const startMinutes: number | null = parseTimeToMinutes(p_entry.startTime);
    const endMinutes: number | null = parseTimeToMinutes(p_entry.endTime);

    if (startMinutes === null || endMinutes === null || endMinutes <= startMinutes) {
        return null;
    }

    return (endMinutes - startMinutes) / 60;
}

export function formatHours(p_hours: number | null): string {
    if (p_hours === null) {
        return "—";
    }

    return `${p_hours.toLocaleString("fr-CA", {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2,
    })} h`;
}

export const payStubColumns: GridColDef<PayStub>[] = [
    {
        field: "employeeName",
        headerName: "Employé",
        flex: 1.2,
        valueGetter: (_p_value: unknown, p_row: PayStub) =>
            `${p_row.employeeFirstName} ${p_row.employeeLastName}`,
    },
    {
        field: "periodStartDate",
        headerName: "Début de période",
        flex: 1,
        valueFormatter: (p_value: string) => formatShiftDate(p_value),
    },
    {
        field: "periodEndDate",
        headerName: "Fin de période",
        flex: 1,
        valueFormatter: (p_value: string) => formatShiftDate(p_value),
    },
    {
        field: "totalHours",
        headerName: "Heures totales",
        flex: 0.8,
        valueFormatter: (p_value: number) => String(p_value),
    },
    {
        field: "grossPay",
        headerName: "Salaire brut",
        flex: 1,
        valueFormatter: (p_value: number) => grossPayFormatter.format(p_value),
    },
    {
        field: "isPublished",
        headerName: "Statut",
        flex: 0.8,
        valueFormatter: (p_value: boolean) => (p_value ? "Publiée" : "Brouillon"),
    },
];

export const timesheetColumns: GridColDef<Timesheet>[] = [
    {
        field: "employeeName",
        headerName: "Employé",
        flex: 1.2,
        valueGetter: (_p_value: unknown, p_row: Timesheet) =>
            `${p_row.employeeFirstName} ${p_row.employeeLastName}`,
    },
    {
        field: "periodStart",
        headerName: "Début de période",
        flex: 1,
        valueFormatter: (p_value: string) => formatShiftDate(p_value),
    },
    {
        field: "periodEnd",
        headerName: "Fin de période",
        flex: 1,
        valueFormatter: (p_value: string) => formatShiftDate(p_value),
    },
    {
        field: "status",
        headerName: "Statut",
        flex: 0.9,
        renderCell: (p_params) => <TimesheetStatusChip status={p_params.row.status} />,
    },
    {
        field: "isPaid",
        headerName: "Paiement",
        flex: 0.8,
        valueFormatter: (p_value: boolean) => (p_value ? "Payée" : "Non payée"),
    },
];

export const timeEntryColumns: GridColDef<TimeEntry>[] = [
    {
        field: "employeeName",
        headerName: "Employé",
        flex: 1.2,
        valueGetter: (_p_value: unknown, p_row: TimeEntry) =>
            `${p_row.employeeFirstName} ${p_row.employeeLastName}`,
    },
    {
        field: "date",
        headerName: "Date",
        flex: 1,
        valueFormatter: (p_value: string) => formatShiftDate(p_value),
    },
    { field: "startTime", headerName: "Début", flex: 0.7 },
    {
        field: "endTime",
        headerName: "Fin",
        flex: 0.7,
        valueFormatter: (p_value: string | null) =>
            p_value !== null && p_value.trim().length > 0 ? p_value : "—",
    },
    {
        field: "durationHours",
        headerName: "Heures",
        flex: 0.8,
        valueGetter: (_p_value: unknown, p_row: TimeEntry) =>
            getTimeEntryDurationHours(p_row),
        valueFormatter: (p_value: number | null) => formatHours(p_value),
    },
];

export const scheduledShiftColumns: GridColDef<ScheduledShift>[] = [
    {
        field: "employeeName",
        headerName: "Employé",
        flex: 1.2,
        valueGetter: (_p_value: unknown, p_row: ScheduledShift) =>
            `${p_row.employeeFirstName} ${p_row.employeeLastName}`,
    },
    { field: "jobPositionName", headerName: "Poste", flex: 1 },
    {
        field: "date",
        headerName: "Date",
        flex: 1,
        valueFormatter: (p_value: string) => formatShiftDate(p_value),
    },
    { field: "startTime", headerName: "Début", flex: 0.7 },
    { field: "endTime", headerName: "Fin", flex: 0.7 },
];

export const employeeProfileColumns: GridColDef<EmployeeProfile>[] = [
    {
        field: "fullName",
        headerName: "Nom",
        flex: 1.2,
        valueGetter: (_p_value: unknown, p_row: EmployeeProfile) =>
            `${p_row.firstName} ${p_row.lastName}`,
    },
    { field: "email", headerName: "Courriel", flex: 1.2 },
    { field: "jobPositionName", headerName: "Poste (réf.)", flex: 1 },
    { field: "locationTitle", headerName: "Succursale", flex: 1 },
    {
        field: "hiringDate",
        headerName: "Date d'embauche",
        flex: 1,
        valueFormatter: (p_value: string | null | undefined) => {
            if (!p_value) {
                return "";
            }
            const parsedDate: Date = new Date(`${p_value}T00:00:00`);
            if (Number.isNaN(parsedDate.getTime())) {
                return p_value;
            }
            return hiringDateFormatter.format(parsedDate);
        },
    },
];

function createQuantityColumn<Row extends { quantity: number }>(): GridColDef<Row> {
    return {
        field: "quantity",
        headerName: "Quantité",
        width: 150,
        renderCell: (params) => {
            const qty = Number(params.value);

            return (
                <Box
                    sx={{
                        fontWeight: "bold",
                        color: (theme) => getInventoryDisplayColor(qty, theme),
                    }}
                >
                    {qty}
                </Box>
            );
        },
    };
}

export interface LocationInventoryRow {
    id: number;
    name: string;
    isBook: boolean;
    quantity: number;
    quantityRecordId: string;
}

export const locationInventoryColumns: GridColDef<LocationInventoryRow>[] = [
    { field: "name", headerName: "Nom", flex: 1.5 },
    {
        field: "isBook",
        headerName: "Type",
        flex: 1.5,
        valueGetter: (value: boolean) => (value ? "Livre" : "Produit"),
    },
    createQuantityColumn<LocationInventoryRow>(),
];

export interface ItemInventoryRow {
    id: number | string;
    title: string;
    quantity: number;
};

export const itemInventoryColumns: GridColDef<ItemInventoryRow>[] = [
    {
        field: "title",
        headerName: "Succursale",
        flex: 2
    },
    {
        field: "quantity",
        headerName: "Quantité en Stock",
        type: "number",
        flex: 1,
        align: "right",
        headerAlign: "right",
        renderCell: (params) => {
            const qty = Number(params.value);

            return (
                <Box
                    sx={{
                        fontWeight: 'bold',
                        color: (theme) => getInventoryDisplayColor(qty, theme)
                    }}
                >
                    {qty}
                </Box>
            );
        }
    }
];

export const authorColumns: GridColDef<Author>[] = [
    { field: "name", headerName: "Nom", flex: 1.5 },
];

export const permissionEntityColumns: GridColDef<PermissionEntity>[] = [
    { field: "id", headerName: "Id", flex: 1.5 },
];

export const permissionColumns: GridColDef<PermissionRule>[] = [
    {
        field: "action",
        headerName: "Droit accordé",
        flex: 1.5,
        valueGetter: (_, p_row) => getActionLabel(p_row.action),
    },
    {
        field: "subject",
        headerName: "Section",
        flex: 1.5,
        valueGetter: (_, p_row) => getEntityLabel(p_row.subject),
    },
]
