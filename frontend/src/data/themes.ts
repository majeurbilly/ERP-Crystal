import { createTheme, type ThemeOptions } from '@mui/material/styles';

declare module '@mui/material/styles' {
    interface ActionButtonColor {
        bg: string;
        text: string;
    }

    interface SidebarPalette {
        bg: string;
        text: string;
    }

    interface Palette {
        inventory: {
            good: string;
            low: string;
            empty: string;
        };
        actionButtons: {
            add: ActionButtonColor;
            edit: ActionButtonColor;
            delete: ActionButtonColor;
            confirm: ActionButtonColor;
            cancel: ActionButtonColor;
        };
        sidebar: SidebarPalette;
    }
    interface PaletteOptions {
        inventory?: {
            good?: string;
            low?: string;
            empty?: string;
        }
        actionButtons?: {
            add?: ActionButtonColor;
            edit?: ActionButtonColor;
            delete?: ActionButtonColor;
            confirm?: ActionButtonColor;
            cancel?: ActionButtonColor;
        };
        sidebar?: SidebarPalette;
    }
}

const baseComponentOverrides: ThemeOptions['components'] = {
    MuiButton: {
        styleOverrides: {
            root: {
                borderRadius: 6,
                textTransform: 'none',
                fontWeight: 'bold',
            }
        }
    },
    MuiAppBar: {
        styleOverrides: {
            root: ({ theme }) => ({
                backgroundColor: theme.palette.background.paper,
                color: theme.palette.text.primary,
            }),
        },
    },

    MuiInputBase: {
        styleOverrides: {
            root: ({ theme }) => ({
                '& input::-webkit-calendar-picker-indicator': {
                    filter: theme.palette.getContrastText(theme.palette.background.paper) === '#fff'
                        ? 'invert(1)'
                        : 'none',
                    cursor: 'pointer',
                },
            }),
        },
    },
}

export const lightTheme = createTheme({
    palette: {
        mode: 'light',
        primary: { main: '#1565c0', contrastText: '#ffffff' },
        secondary: { main: '#181818', contrastText: '#ffffff' },
        inventory: {
            good: '#2e7d32',
            low: '#ed6c02',
            empty: '#d32f2f'
        },
        actionButtons: {
            add: { bg: '#1b5e20', text: '#ffffff' },
            edit: { bg: '#e65100', text: '#ffffff' },
            delete: { bg: '#c62828', text: '#ffffff' },
            confirm: { bg: '#1b5e20', text: '#ffffff' },
            cancel: { bg: '#c62828', text: '#ffffff' }
        },
        sidebar: {
            bg: '#ffffff',
            text: '#181818',
        },
    },
    components: baseComponentOverrides
});

export const darkTheme = createTheme({
    palette: {
        mode: 'dark',
        primary: { main: '#64b5f6', contrastText: '#0d1117' },
        secondary: { main: '#e0e0e0', contrastText: '#0d1117' },
        inventory: {
            good: '#66bb6a',
            low: '#ffb74d',
            empty: '#f44336'
        },
        actionButtons: {
            add: { bg: '#81c784', text: '#000000' },
            edit: { bg: '#ffb74d', text: '#000000' },
            delete: { bg: '#f44336', text: '#000000' },
            confirm: { bg: '#81c784', text: '#000000' },
            cancel: { bg: '#f44336', text: '#000000' }
        },
        sidebar: {
            bg: '#1a1d24',
            text: '#f3f4f6',
        },
    },
    components: baseComponentOverrides
});