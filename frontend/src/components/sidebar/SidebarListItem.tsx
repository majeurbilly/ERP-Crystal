import ListItem from '@mui/material/ListItem';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import HomeIcon from '@mui/icons-material/Home';
import GroupsIcon from '@mui/icons-material/Groups'
import InventoryIcon from '@mui/icons-material/Inventory'
import AccountBoxIcon from '@mui/icons-material/AccountBox'
import { Link, useLocation } from 'react-router-dom'
import { ROUTE_DASHBOARD, ROUTE_HR, ROUTE_MY_PROFILE, ROUTE_ROOT } from '../../data/routeNames';


interface SidebarListItemProps {
    text: string;
    icon: React.ReactElement;
    pathTo: string;
}

const SidebarListItem = ({ text, icon, pathTo }: SidebarListItemProps) => {
    const location = useLocation();
    const isActive = location.pathname === pathTo;

    return (
        <ListItem key={text} disablePadding>
            <ListItemButton
                component={Link}
                to={pathTo}
                disabled={isActive}
                sx={{
                    "&.Mui-selected": {
                        backgroundColor: 'action.selected',
                        "& .MuiListItemIcon-root": {
                            color: 'text.primary',
                        },
                        "& .MuiListItemText-root": {
                            color: 'text.primary',
                        },
                    }
                }}
            >
                <ListItemIcon>
                    {icon}
                </ListItemIcon>
                <ListItemText primary={text} />
            </ListItemButton>
        </ListItem>
    );
}

export const DashboardListItem = () => {
    return (
        <SidebarListItem text={"Dashboard"} icon={<HomeIcon />} pathTo={ROUTE_DASHBOARD} />
    );
}

export const InventoryListItem = () => {
    return (
        <SidebarListItem text={"Inventaire"} icon={<InventoryIcon />} pathTo={ROUTE_ROOT} />
    )
}

export const ProfileListItem = () => {
    return (
        <SidebarListItem text={"Mon profil"} icon={<AccountBoxIcon />} pathTo={ROUTE_MY_PROFILE} />
    )
}

export const HRListItem = () => {
    return (
        <SidebarListItem text={"RH"} icon={<GroupsIcon />} pathTo={ROUTE_HR} />
    )
}