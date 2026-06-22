import CheckIcon from "@mui/icons-material/Check";
import { Box, FormHelperText, Tooltip, Typography } from "@mui/material";
import {
    JOB_POSITION_COLOR_PALETTE,
    type JobPositionColorOption,
} from "../../data/types/hr/jobPositionColors";

interface ColorPalettePickerProps {
    value: string;
    onChange: (p_hex: string) => void;
    label?: string;
    error?: string;
    options?: JobPositionColorOption[];
}

export default function ColorPalettePicker({
    value,
    onChange,
    label = "Couleur",
    error,
    options = JOB_POSITION_COLOR_PALETTE,
}: ColorPalettePickerProps) {
    return (
        <Box sx={{ mb: 2 }}>
            <Typography
                component="label"
                variant="body2"
                sx={{ display: "block", mb: 1, fontWeight: 500 }}
            >
                {label}
            </Typography>
            <Box
                role="radiogroup"
                aria-label={label}
                sx={{
                    display: "grid",
                    gridTemplateColumns: "repeat(auto-fill, minmax(44px, 1fr))",
                    gap: 1,
                    maxWidth: 360,
                }}
            >
                {options.map((option) => {
                    const isSelected = option.hex.toUpperCase() === value.toUpperCase();

                    return (
                        <Tooltip key={option.hex} title={option.label} arrow>
                            <Box
                                role="radio"
                                aria-checked={isSelected}
                                aria-label={option.label}
                                tabIndex={0}
                                onClick={() => onChange(option.hex)}
                                onKeyDown={(p_event) => {
                                    if (p_event.key === "Enter" || p_event.key === " ") {
                                        p_event.preventDefault();
                                        onChange(option.hex);
                                    }
                                }}
                                sx={{
                                    width: 44,
                                    height: 44,
                                    borderRadius: 1.5,
                                    bgcolor: option.hex,
                                    cursor: "pointer",
                                    border: 2,
                                    borderColor: isSelected ? "primary.main" : "divider",
                                    boxShadow: isSelected ? 2 : 0,
                                    display: "flex",
                                    alignItems: "center",
                                    justifyContent: "center",
                                    transition: "transform 0.15s ease, box-shadow 0.15s ease",
                                    "&:hover": {
                                        transform: "scale(1.06)",
                                        boxShadow: 2,
                                    },
                                }}
                            >
                                {isSelected && (
                                    <CheckIcon
                                        sx={{
                                            fontSize: 22,
                                            color: "#fff",
                                            filter: "drop-shadow(0 1px 2px rgba(0,0,0,0.45))",
                                        }}
                                    />
                                )}
                            </Box>
                        </Tooltip>
                    );
                })}
            </Box>
            {error ? (
                <FormHelperText error sx={{ mt: 1 }}>
                    {error}
                </FormHelperText>
            ) : (
                <FormHelperText sx={{ mt: 1 }}>
                    Choisissez une couleur pour identifier ce poste dans le calendrier.
                </FormHelperText>
            )}
        </Box>
    );
}
