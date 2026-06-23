import ListItem from '@mui/material/ListItem';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import HomeIcon from '@mui/icons-material/Home';
import GroupsIcon from '@mui/icons-material/Groups'
import InventoryIcon from '@mui/icons-material/Inventory'
import AccountBoxIcon from '@mui/icons-material/AccountBox'
import StoreIcon from '@mui/icons-material/Store'
import ShoppingBagIcon from '@mui/icons-material/ShoppingBag';
import CategoryIcon from '@mui/icons-material/Category';
import MenuBookIcon from '@mui/icons-material/MenuBook';
import FaceIcon from '@mui/icons-material/Face';
import { Link, useLocation } from 'react-router-dom'
import {
    ROUTE_DASHBOARD,
    ROUTE_HR,
    ROUTE_CATALOGUE,
    ROUTE_MY_PROFILE,
    ROUTE_LOCATIONS,
    ROUTE_CATEGORY,
    ROUTE_IR,
    ROUTE_JOB_POSITIONS,
    ROUTE_EMPLOYEE_PROFILES,
    ROUTE_EMPLOYMENT_CONTRACTS,
    ROUTE_LEAVE_REQUESTS,
    ROUTE_SCHEDULES,
    ROUTE_LIST_AUTHORS,
    ROUTE_TIME_ENTRIES,
    ROUTE_TIMESHEETS,
    ROUTE_PAYROLL,
    ROUTE_LIST_USERS,
    ROUTE_LIST_USER_ROLES,
    ROUTE_MON_ESPACE,
} from '../../data/routeNames';
import WorkOutlineIcon from '@mui/icons-material/WorkOutline';
import PeopleIcon from '@mui/icons-material/People';
import ManageAccountsIcon from '@mui/icons-material/ManageAccounts';
import EventBusyIcon from '@mui/icons-material/EventBusy';
import CalendarMonthIcon from '@mui/icons-material/CalendarMonth';
import AccessTimeIcon from '@mui/icons-material/AccessTime';
import DescriptionIcon from '@mui/icons-material/Description';
import PaymentsIcon from '@mui/icons-material/Payments';
import AssignmentIndIcon from '@mui/icons-material/AssignmentInd';
import PersonPinIcon from '@mui/icons-material/PersonPin';
import { ENTITY_TYPES } from '../../permissions/permissions';
import { usePermissions } from '../../permissions/usePermissions';


interface SidebarListItemProps {
    text: string;
    icon: React.ReactElement;
    pathTo: string;
    matchPrefix?: boolean;
}

function isSidebarPathActive(p_currentPath: string, p_targetPath: string, p_matchPrefix: boolean): boolean {
    if (p_currentPath === p_targetPath) {
        return true;
    }

    if (!p_matchPrefix || p_targetPath === ROUTE_DASHBOARD) {
        return false;
    }

    return p_currentPath.startsWith(`${p_targetPath}/`);
}

const SidebarListItem = ({ text, icon, pathTo, matchPrefix = true }: SidebarListItemProps) => {
    const location = useLocation();
    const isActive = isSidebarPathActive(location.pathname, pathTo, matchPrefix);

    return (
        <ListItem key={text} disablePadding>
            <ListItemButton
                component={Link}
                to={pathTo}
                selected={isActive}
                sx={{
                    "&.Mui-selected": {
                        backgroundColor: "primary.main",
                        color: "primary.contrastText",
                        "&:hover": {
                            backgroundColor: "primary.dark",
                        },
                        "& .MuiListItemIcon-root": {
                            color: "primary.contrastText",
                        },
                        "& .MuiListItemText-primary": {
                            color: "primary.contrastText",
                            fontWeight: 700,
                        },
                    },
                }}
            >
                <ListItemIcon>
                    {icon}
                </ListItemIcon>
                <ListItemText
                    primary={text}
                    primaryTypographyProps={{
                        noWrap: true,
                    }}
                />
            </ListItemButton>
        </ListItem>
    );
}

export const DashboardListItem = () => {
    return (
        <SidebarListItem
            text={"Tableau de bord"}
            icon={<HomeIcon />}
            pathTo={ROUTE_DASHBOARD}
            matchPrefix={false}
        />
    );
}

export const CatalogListItem = () => {
    const { canRead } = usePermissions(ENTITY_TYPES.ITEM);
    if (canRead) return (
        <SidebarListItem
            text={"Catalogue"}
            icon={<ShoppingBagIcon />}
            pathTo={ROUTE_CATALOGUE}
        />
    )
}

export const MyProfileListItem = () => {
    const { canRead } = usePermissions(ENTITY_TYPES.ME);
    if (canRead) return (
        <SidebarListItem
            text={"Mon profil"}
            icon={<AccountBoxIcon />}
            pathTo={ROUTE_MY_PROFILE}
        />
    )
}

export const LocationsListItem = () => {
    const { canRead } = usePermissions(ENTITY_TYPES.LOCATION);
    if (canRead) return (
        <SidebarListItem
            text={"Succursales"}
            icon={<StoreIcon />}
            pathTo={ROUTE_LOCATIONS}
        />
    )
}

export const HRListItem = () => {
    const { canRead } = usePermissions(ENTITY_TYPES.HR_DASHBOARD);
    if (canRead) return (
        <SidebarListItem
            text={"Accueil RH"}
            icon={<GroupsIcon />}
            pathTo={ROUTE_HR}
            matchPrefix={false}
        />
    )
}

export const JobPositionsListItem = () => {
    const { canRead } = usePermissions(ENTITY_TYPES.JOB_POSITION)
    if (canRead) return (
        <SidebarListItem
            text={"Postes"}
            icon={<WorkOutlineIcon />}
            pathTo={ROUTE_JOB_POSITIONS}
        />
    )
}

export const EmployeeProfilesListItem = () => {
    const { canRead } = usePermissions(ENTITY_TYPES.EMPLOYEE_PROFILE);
    if (canRead) return (
        <SidebarListItem
            text={"Employés"}
            icon={<PeopleIcon />}
            pathTo={ROUTE_EMPLOYEE_PROFILES}
        />
    )
}

export const UsersListItem = () => {
    const { canRead } = usePermissions(ENTITY_TYPES.USER);
    if (canRead) return (
        <SidebarListItem
            text={"Utilisateurs"}
            icon={<ManageAccountsIcon />}
            pathTo={ROUTE_LIST_USERS}
        />
    )
}

export const EmploymentContractsListItem = () => {
    const { canRead } = usePermissions(ENTITY_TYPES.EMPLOYMENT_CONTRACT);
    if (canRead) return (
        <SidebarListItem
            text={"Contrats de travail"}
            icon={<AssignmentIndIcon />}
            pathTo={ROUTE_EMPLOYMENT_CONTRACTS}
        />
    )
}

export const LeaveRequestsListItem = () => {
    const { canRead } = usePermissions(ENTITY_TYPES.LEAVE_REQUEST);
    if (canRead) return (
        <SidebarListItem
            text={"Congés"}
            icon={<EventBusyIcon />}
            pathTo={ROUTE_LEAVE_REQUESTS}
        />
    )
}

export const SchedulesListItem = () => {
    const { canRead } = usePermissions(ENTITY_TYPES.SCHEDULED_SHIFT);
    if (canRead) return (
        <SidebarListItem
            text={"Planification"}
            icon={<CalendarMonthIcon />}
            pathTo={ROUTE_SCHEDULES}
        />
    )
}

export const TimeEntriesListItem = () => {
    const { canRead } = usePermissions(ENTITY_TYPES.TIME_ENTRY);
    if (canRead) return (
        <SidebarListItem
            text={"Pointages"}
            icon={<AccessTimeIcon />}
            pathTo={ROUTE_TIME_ENTRIES}
        />
    )
}

export const TimesheetsListItem = () => {
    const { canRead } = usePermissions(ENTITY_TYPES.TIMESHEET);
    if (canRead) return (
        <SidebarListItem
            text={"Feuilles de temps"}
            icon={<DescriptionIcon />}
            pathTo={ROUTE_TIMESHEETS}
        />
    )
}

export const PayrollListItem = () => {
    const { canRead } = usePermissions(ENTITY_TYPES.PAYROLL);
    if (canRead) return (
        <SidebarListItem
            text={"Paie"}
            icon={<PaymentsIcon />}
            pathTo={ROUTE_PAYROLL}
        />
    )
}

export const CategoryListItem = () => {
    const { canRead } = usePermissions(ENTITY_TYPES.CATEGORY);
    if (canRead) return (
        <SidebarListItem
            text={"Catégories"}
            icon={<CategoryIcon />}
            pathTo={ROUTE_CATEGORY}
        />
    )
}

export const IRListItem = () => {
    return (
        <SidebarListItem
            text={"Gestion d'inventaire"}
            icon={<InventoryIcon />}
            pathTo={ROUTE_IR}
        />
    )
}

export const AuthorListItem = () => {
    const { canRead } = usePermissions(ENTITY_TYPES.AUTHOR);
    if (canRead) return (
        <SidebarListItem
            text={"Auteurs"}
            icon={<MenuBookIcon />}
            pathTo={ROUTE_LIST_AUTHORS}
        />
    )
}

export const UserRolesListItem = () => {
    const { canRead } = usePermissions(ENTITY_TYPES.USER_ROLE);
    if (canRead) return (
        <SidebarListItem
            text={"Roles"}
            icon={<FaceIcon />}
            pathTo={ROUTE_LIST_USER_ROLES}
        />
    );
}

export const MonEspaceListItem = () => {
    return (
        <SidebarListItem
            text={"Mon espace"}
            icon={<PersonPinIcon />}
            pathTo={ROUTE_MON_ESPACE}
        />
    );
}
