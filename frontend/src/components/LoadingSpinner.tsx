import { Box } from '@mui/material';
import { MoonLoader } from 'react-spinners';
import { useTheme } from '@mui/material';


export default function LoadingSpinner() {
    const theme = useTheme();
    const spinnerColor = theme.palette?.secondary?.main || '#36d7b7';
    return (

        <Box sx={{
            width: '100%',
            display: 'flex',
            justifyContent: 'center',
            padding: '2rem'
        }}>
            <MoonLoader loading={true} color={spinnerColor} size={50} />
        </Box>

    );
}