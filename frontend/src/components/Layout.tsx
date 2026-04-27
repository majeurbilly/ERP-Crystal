import { Outlet } from "react-router-dom";
import ResponsiveAppBar from "./Header";
import Sidebar from "./sidebar/Sidebar";
// import useMediaQuery from "@mui/material/useMediaQuery";
import Toolbar from "@mui/material/Toolbar";
import Box from "@mui/material/Box";

const Layout = () => {
    // const isDesktop = useMediaQuery('(min-width:1024px)');

    return (
        <>
            <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
                <ResponsiveAppBar />

                <Box sx={{ display: 'flex', flexGrow: 1 }}>
                    <Sidebar />
                    <Box
                        component="main"
                        sx={{
                            flexGrow: 1,
                            p: 3,
                            width: '100%',
                            display: 'flex',
                            flexDirection: 'column'
                        }}
                    >
                        <Toolbar />
                        <Box sx={{ mt: 2 }}>
                            <Outlet />
                        </Box>
                    </Box>
                </Box>
            </Box>
        </>
    );
};
export default Layout;