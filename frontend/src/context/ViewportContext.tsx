import { useMediaQuery, useTheme } from "@mui/material";

export const useIsDesktop = () => {
    const theme = useTheme();
    return useMediaQuery(theme.breakpoints.up(1024));
};