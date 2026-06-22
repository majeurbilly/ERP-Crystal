import { Box, IconButton, TextField } from "@mui/material";
import InputAdornment from "@mui/material/InputAdornment";
import SearchIcon from '@mui/icons-material/Search';
import ClearIcon from '@mui/icons-material/Clear';

interface SearchBarProps {
    value: string;
    onChange: (value: string) => void;
}

export default function SearchBar({ value, onChange }: SearchBarProps) {

    const handleClear = () => {
        onChange("");
    }

    return (
        <>
            <Box
                sx={{
                    width: '100%'
                }}
            >
                <TextField
                    fullWidth
                    id="input-with-icon-textfield"
                    label="Recherche"
                    slotProps={{
                        input: {
                            startAdornment: (
                                <InputAdornment position="start">
                                    <SearchIcon />
                                </InputAdornment>
                            ),
                            endAdornment: value && (
                                <InputAdornment position="end">
                                    <IconButton
                                        aria-label="clear search"
                                        onClick={handleClear}
                                        edge="end"
                                    >
                                        <ClearIcon />
                                    </IconButton>
                                </InputAdornment>
                            )
                        },
                    }}
                    variant="filled"
                    value={value}
                    onChange={(e) => onChange(e.target.value)}
                />
            </Box>
        </>
    );
}