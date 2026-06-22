import * as React from 'react';
import AppBar from '@mui/material/AppBar';
import Box from '@mui/material/Box';
import Toolbar from '@mui/material/Toolbar';
import IconButton from '@mui/material/IconButton';
import Typography from '@mui/material/Typography';
import Menu from '@mui/material/Menu';
import Container from '@mui/material/Container';
import Avatar from '@mui/material/Avatar';
import Tooltip from '@mui/material/Tooltip';
import MenuItem from '@mui/material/MenuItem';
import { useAuth } from '../../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import { useMediaQuery } from '@mui/material';
import { useTheme } from '@mui/material/styles';
import MenuIcon from '@mui/icons-material/Menu';
import Brightness4Icon from '@mui/icons-material/Brightness4';
import Brightness7Icon from '@mui/icons-material/Brightness7';
import ListItemIcon from '@mui/material/ListItemIcon';
import { useState } from 'react';
import { useSidebar } from '../../context/SidebarContext';
import { useColorMode } from '../../context/CustomThemeContext';
import userService from '../../api/services/hr/userService';
import { getAssignedRoleDisplayName } from '../../data/types/hr/userRoles';
import { useQuery } from '@tanstack/react-query';
import { usersCacheKey } from '../../data/cacheKeys';
import { ROUTE_DASHBOARD, ROUTE_MY_PROFILE } from '../../data/routeNames';

function Header() {
  const [anchorElUser, setAnchorElUser] = useState<null | HTMLElement>(null);
  const theme = useTheme();
  const isDesktop = useMediaQuery(theme.breakpoints.up(1024));
  const navigate = useNavigate();
  const { user, logout } = useAuth();
  const { toggleSidebar, isOpen } = useSidebar();
  const { toggleColorMode } = useColorMode();
  const handleOpenUserMenu = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorElUser(event.currentTarget);
  };
  const handleCloseUserMenu = () => {
    setAnchorElUser(null);
  };
  const handleClickLogo = () => {
    navigate(ROUTE_DASHBOARD);
  }
  const handleClickProfile = () => {
    handleCloseUserMenu();
    navigate(ROUTE_MY_PROFILE);
  }
  const handleClickLogout = () => {
    handleCloseUserMenu();
    logout();
  }

  const { data: currentUser } = useQuery({
    queryKey: usersCacheKey.me(),
    queryFn: () => userService.getMe(),
    enabled: !!user?.id,
  });

  return (
    <AppBar position="fixed" sx={{ zIndex: (theme) => theme.zIndex.drawer + 1 }} elevation={2}>
      <Container maxWidth={false} disableGutters>
        <Toolbar
          disableGutters
          sx={{
            minHeight: 75,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            px: { xs: 2, md: 3 },
            width: '100%',
          }}
        >
          {isDesktop &&
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
              <Box onClick={handleClickLogo} component="img" className='logoStyle' src="/LogoCristal2.png" alt="logo" />
              <Typography variant="subtitle1" noWrap component="a" sx={{ fontFamily: 'Arial, Helvetica, sans-serif', fontWeight: 600, fontSize: '1.05rem', letterSpacing: '0.05rem', color: 'inherit', textDecoration: 'none', }}>
                Librairie Crystal
              </Typography>
            </Box>}
          <Box onClick={() => toggleSidebar(!isOpen)} sx={{ cursor: "pointer" }}>
            {!isDesktop && <MenuIcon />}
          </Box>
          <Box sx={{ flexGrow: 0 }}>
            <Tooltip title="Paramètres">
              <IconButton onClick={handleOpenUserMenu} sx={{ p: 0 }}>
                <Avatar>
                  {currentUser?.userName?.charAt(0).toUpperCase() ?? "?"}
                </Avatar>
              </IconButton>
            </Tooltip>
            <Menu sx={{ mt: '45px' }} anchorEl={anchorElUser} anchorOrigin={{ vertical: 'top', horizontal: 'right' }} transformOrigin={{ vertical: 'top', horizontal: 'right' }} open={Boolean(anchorElUser)} onClose={handleCloseUserMenu}>
              <Box sx={{ px: 2, py: 1.5, width: 240, bgcolor: "#e8f2ff", borderRadius: 2, mx: 1, mt: 1, mb: 1, boxShadow: "0px 2px 6px rgba(0,0,0,0.08)", }}>
                <Typography sx={{ fontWeight: 700, fontSize: "1rem", color: "#0b3d91", fontFamily: "Arial, Helvetica, sans-serif", }}>
                  {currentUser?.userName ?? "Utilisateur inconnu"}
                </Typography>
                <Typography sx={{ fontSize: "0.85rem", mt: 0.5, color: "#1d4f9c", fontFamily: "Arial, Helvetica, sans-serif", }}>
                  Rôle : <b>{user?.dynamicRole?.name ?? (currentUser ? getAssignedRoleDisplayName(currentUser) : "?")}</b>
                </Typography>
              </Box>
              <MenuItem onClick={handleClickProfile}>
                <Typography sx={{ fontFamily: "Arial, Helvetica, sans-serif", fontWeight: 500 }}>
                  Mon profil
                </Typography>
              </MenuItem>
              <MenuItem onClick={toggleColorMode}>
                <ListItemIcon>
                  {theme.palette.mode === 'dark'
                    ? <Brightness7Icon fontSize="small" />
                    : <Brightness4Icon fontSize="small" />}
                </ListItemIcon>
                <Typography sx={{ fontFamily: "Arial, Helvetica, sans-serif", fontWeight: 500 }}>
                  {theme.palette.mode === 'dark' ? 'Mode clair' : 'Mode sombre'}
                </Typography>
              </MenuItem>
              <MenuItem onClick={handleClickLogout} id="logout">
                <Typography sx={{ fontFamily: "Arial, Helvetica, sans-serif", fontWeight: 500 }}>
                  Déconnexion
                </Typography>
              </MenuItem>
            </Menu>
          </Box>
        </Toolbar>
      </Container>
    </AppBar>
  );
}

export default Header;
