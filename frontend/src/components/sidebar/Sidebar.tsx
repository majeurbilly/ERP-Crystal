import Box from '@mui/material/Box';
import Drawer from '@mui/material/Drawer';
import List from '@mui/material/List';
import Divider from '@mui/material/Divider';
import { DashboardListItem, InventoryListItem, ProfileListItem, HRListItem } from './SidebarListItem';
import useMediaQuery from '@mui/material/useMediaQuery';
import { useTheme } from '@mui/material/styles';
import Toolbar from '@mui/material/Toolbar';
import { useSidebar } from '../../context/SidebarContext';
import { useAuth } from '../../context/AuthContext';

export default function Sidebar() {
    const { isOpen, toggleSidebar } = useSidebar();
    const theme = useTheme();
    const isDesktop = useMediaQuery(theme.breakpoints.up(1024));

    const { role } = useAuth();

    const isHRVisible = () => {
        return role === "admin" || role === "gerant";
    }

    const DrawerList = (
        <Box sx={{ width: 250 }} role="presentation" onClick={isDesktop ? undefined : () => toggleSidebar(false)}>
            <List>
                <DashboardListItem />
                <InventoryListItem />
                <ProfileListItem />
            </List>
            <Divider />
            {isHRVisible() &&
                <HRListItem />
            }
        </Box>
    );

    return (
        <>
            <Drawer
                variant={isDesktop ? "permanent" : "temporary"}
                open={isDesktop ? true : isOpen}
                onClose={() => toggleSidebar(false)}
                sx={{
                    color: 'primary.main',
                    width: 250,
                    flexShrink: 0,
                    zIndex: isDesktop ? theme.zIndex.appBar - 1 : theme.zIndex.drawer,
                    '& .MuiDrawer-paper': {
                        width: 250,
                        boxSizing: 'border-box',
                        backgroundColor: 'primary.main',
                        color: 'primary.contrastText',
                        zIndex: isDesktop ? theme.zIndex.appBar - 1 : theme.zIndex.drawer,
                    },
                }}
            >
                {isDesktop && <Toolbar />}
                {DrawerList}
            </Drawer>

        </>
    );
}