import { Outlet, useLocation } from "react-router-dom";
import Header from "./Header";
import Sidebar from "../sidebar/Sidebar";
import Toolbar from "@mui/material/Toolbar";
import Box from "@mui/material/Box";
import FormRoot from "../forms/FormRoot";
import RouteErrorBoundary from "../routes/RouteErrorBoundary";
import { notifyErrorMessage } from "../../data/utils/popupMessageManager";
import { useEffect } from "react";

const AppLayout = () => {

    const location = useLocation();

    useEffect(() => {
        if (location.state?.unauthorized) {
            notifyErrorMessage("pas permission mon coco");
            window.history.replaceState({}, document.title);
        }
    }, [location]);

    return (
        <>
            <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
                <Header />

                <Box sx={{ display: 'flex', flexGrow: 1 }}>
                    <Sidebar />
                    <Box
                        component="main"
                        sx={{
                            flexGrow: 1,
                            p: 3,
                            minWidth: 0,
                            display: 'flex',
                            flexDirection: 'column',
                            alignItems: 'center'
                        }}
                    >
                        <Toolbar />
                        <Box
                            sx={{
                                mt: 2,
                                width: '85%',
                                display: 'flex',
                                flexDirection: 'column'
                            }}
                        >
                            <RouteErrorBoundary>
                                <Outlet />
                            </RouteErrorBoundary>
                        </Box>
                    </Box>
                </Box>
            </Box>
            <FormRoot />
        </>
    );
};
export default AppLayout;