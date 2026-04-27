
import { createTheme } from '@mui/material/styles';


export const lightTheme = createTheme({
    palette: {
        mode: 'light',
        primary: {
            main: 'rgb(255, 255, 255)',
        },
        secondary: {
            main: '#dc004e'
        }
    }
});

export const darkTheme = createTheme({
    palette: {
        mode: 'dark',
        primary: {
            main: 'rgb(24, 24, 24)',
        },
        secondary: {
            main: '#dc004e'
        }
    }
});