import { useState, useEffect } from 'react';
import Box from '@mui/material/Box';
import Collapse from '@mui/material/Collapse';
import Drawer from '@mui/material/Drawer';
import List from '@mui/material/List';
import Divider from '@mui/material/Divider';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemText from '@mui/material/ListItemText';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import {
    LocationsListItem,
    DashboardListItem,
    CatalogListItem,
    HRListItem,
    JobPositionsListItem,
    EmployeeProfilesListItem,
    LeaveRequestsListItem,
    SchedulesListItem,
    TimeEntriesListItem,
    TimesheetsListItem,
    PayrollListItem,
    UsersListItem,
    EmploymentContractsListItem,
    CategoryListItem,
    IRListItem,
    AuthorListItem,
    UserRolesListItem,
    MonEspaceListItem,
} from './SidebarListItem';
import { useTheme } from '@mui/material/styles';
import Toolbar from '@mui/material/Toolbar';
import { useSidebar } from '../../context/SidebarContext';
import { useIsDesktop } from '../../context/ViewportContext';
import { usePermissions } from '../../permissions/usePermissions';
import { ENTITY_TYPES } from '../../permissions/permissions';

const STORAGE_KEY_INVENTORY = 'sidebar_inventory_open';
const STORAGE_KEY_HR = 'sidebar_hr_open';

export default function Sidebar() {
    const { isOpen, toggleSidebar } = useSidebar();
    const theme = useTheme();
    const isDesktop = useIsDesktop();

    const [isInventoryOpen, setIsInventoryOpen] = useState<boolean>(() => {
        return localStorage.getItem('sidebar_inventory_open') === 'true';
    });

    const [isHrOpen, setIsHrOpen] = useState<boolean>(() => {
        return localStorage.getItem('sidebar_hr_open') === 'true';
    });

    useEffect(() => {
        localStorage.setItem(STORAGE_KEY_INVENTORY, String(isInventoryOpen));
    }, [isInventoryOpen]);

    useEffect(() => {
        localStorage.setItem(STORAGE_KEY_HR, String(isHrOpen));
    }, [isHrOpen]);

    const { canRead: canReadHrDashboard } = usePermissions(ENTITY_TYPES.HR_DASHBOARD);
    const { canRead: canReadLeave } = usePermissions(ENTITY_TYPES.LEAVE_REQUEST);
    const { canRead: canReadShift } = usePermissions(ENTITY_TYPES.SCHEDULED_SHIFT);
    const { canRead: canReadEmploymentContract } = usePermissions(ENTITY_TYPES.EMPLOYMENT_CONTRACT);
    const { canRead: canReadPayroll } = usePermissions(ENTITY_TYPES.PAYROLL);
    const { canRead: canReadInventoryQuantity } = usePermissions(ENTITY_TYPES.INVENTORY_QUANTITY);

    const showMonEspace = !canReadHrDashboard && (canReadLeave || canReadShift || canReadEmploymentContract || canReadPayroll);

    const sectionButtonSx = {
        color: 'sidebar.text',
        fontWeight: 700,
        '& .MuiListItemText-primary': {
            fontWeight: 700,
        },
    };

    const DrawerList = (
        <Box
            sx={{ width: 250, maxWidth: "100%", overflowX: "hidden" }}
            role="presentation"
            onClick={isDesktop ? undefined : () => toggleSidebar(false)}
        >
            <List>
                <DashboardListItem />
                {showMonEspace && <MonEspaceListItem />}
            </List>
            <Divider />
            <List>
                <ListItemButton
                    onClick={(p_event) => {
                        p_event.stopPropagation();
                        setIsInventoryOpen((p_isOpen) => !p_isOpen);
                    }}
                    sx={sectionButtonSx}
                >
                    <ListItemText primary="Inventaire" />
                    {isInventoryOpen ? <ExpandLessIcon /> : <ExpandMoreIcon />}
                </ListItemButton>
                <Collapse in={isInventoryOpen} timeout="auto" unmountOnExit>
                    <List component="div" disablePadding>
                        {canReadHrDashboard && canReadInventoryQuantity && <IRListItem />}
                        <CatalogListItem />
                        <CategoryListItem />
                        <LocationsListItem />
                        <AuthorListItem />
                    </List>
                </Collapse>
            </List>
            <Divider />
            {canReadHrDashboard && (
                <List>
                    <ListItemButton
                        onClick={(p_event) => {
                            p_event.stopPropagation();
                            setIsHrOpen((p_isOpen) => !p_isOpen);
                        }}
                        sx={sectionButtonSx}
                    >
                        <ListItemText primary="Ressources humaines" />
                        {isHrOpen ? <ExpandLessIcon /> : <ExpandMoreIcon />}
                    </ListItemButton>
                    <Collapse in={isHrOpen} timeout="auto" unmountOnExit>
                        <List component="div" disablePadding>
                            <HRListItem />
                            <EmployeeProfilesListItem />
                            <UsersListItem />
                            <UserRolesListItem />
                            <EmploymentContractsListItem />
                            <LeaveRequestsListItem />
                            <SchedulesListItem />
                            <TimeEntriesListItem />
                            <TimesheetsListItem />
                            {canReadPayroll && <PayrollListItem />}
                            <JobPositionsListItem />
                        </List>
                    </Collapse>
                </List>
            )}
        </Box>
    );

    return (
        <>
            <Drawer
                variant={isDesktop ? "permanent" : "temporary"}
                open={isDesktop ? true : isOpen}
                onClose={() => toggleSidebar(false)}
                sx={{
                    color: 'sidebar.text',
                    width: 250,
                    flexShrink: 0,
                    zIndex: isDesktop ? theme.zIndex.appBar - 1 : theme.zIndex.drawer,
                    '& .MuiDrawer-paper': {
                        width: 250,
                        boxSizing: 'border-box',
                        overflowX: 'hidden',
                        backgroundColor: 'sidebar.bg',
                        color: 'sidebar.text',
                        zIndex: isDesktop ? theme.zIndex.appBar - 1 : theme.zIndex.drawer,
                    },
                }}
            >
                <Toolbar />
                {DrawerList}
            </Drawer>
        </>
    );
}