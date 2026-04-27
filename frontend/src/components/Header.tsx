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
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import LogoutButton from './buttons/LogoutButton';
import { useMediaQuery } from '@mui/material';
import { useTheme } from '@mui/material/styles';
import MenuIcon from '@mui/icons-material/Menu';
import { useState } from 'react';
import { useSidebar } from '../context/SidebarContext';

function ResponsiveAppBar() {
  const [anchorElUser, setAnchorElUser] = useState<null | HTMLElement>(null);
  const theme = useTheme();
  const isDesktop = useMediaQuery(theme.breakpoints.up(1024));

  const navigate = useNavigate();
  const { role } = useAuth();
  const settings = [<LogoutButton />];

  const { toggleSidebar, isOpen } = useSidebar();

  const handleOpenUserMenu = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorElUser(event.currentTarget);
  };
  const handleCloseUserMenu = () => {
    setAnchorElUser(null);
  };

  const handleClickLogo = () => {
    navigate('/dashboard/' + role);
  }

  return (
    <AppBar position="fixed" sx={{ zIndex: (theme) => theme.zIndex.drawer - 1 }} elevation={2}>
      <Container maxWidth="xl">
        <Toolbar disableGutters sx={{ minHeight: 75, display: 'flex', alignItems: 'center', justifyContent: 'space-between', }}>
          {isDesktop &&
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
              <Box onClick={handleClickLogo} component="img" src="/LogoCristal.png" alt="logo" sx={{ height: 55 }} />
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
                <Avatar alt="User" src="/static/images/avatar/2.jpg" />
              </IconButton>
            </Tooltip>
            <Menu sx={{ mt: '45px' }} anchorEl={anchorElUser} anchorOrigin={{ vertical: 'top', horizontal: 'right' }} transformOrigin={{ vertical: 'top', horizontal: 'right' }} open={Boolean(anchorElUser)} onClose={handleCloseUserMenu}>
              {settings.map((setting: any) => (
                <MenuItem key={setting.id} onClick={handleCloseUserMenu}>
                  <Typography sx={{ fontFamily: 'Arial, Helvetica, sans-serif', fontSize: '0.95rem', }}>
                    {setting}
                  </Typography>
                </MenuItem>
              ))}
            </Menu>
          </Box>
        </Toolbar>
      </Container>
    </AppBar>
  );
}

export default ResponsiveAppBar;