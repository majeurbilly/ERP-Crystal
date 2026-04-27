import { useTheme } from "@mui/material/styles";
import { useColorMode } from "../context/CustomThemeContext";
import IconButton from "@mui/material/IconButton";
import Brightness4Icon from '@mui/icons-material/Brightness4';
import Brightness7Icon from '@mui/icons-material/Brightness7';

export default function ToggleThemeButton() {
    const theme = useTheme();
    const { toggleColorMode } = useColorMode();

    return (
        <IconButton onClick={toggleColorMode} color="inherit">
            {theme.palette.mode === 'dark' ? <Brightness7Icon /> : <Brightness4Icon />}
        </IconButton>
    );
}