import { useState } from "react";
import AccessTimeIcon from "@mui/icons-material/AccessTime";
import {
    Box,
    Button,
    ClickAwayListener,
    IconButton,
    InputAdornment,
    Paper,
    TextField,
    Typography,
} from "@mui/material";
import type { SxProps, Theme } from "@mui/material/styles";

const HOURS: number[] = Array.from({ length: 12 }, (_, p_index) => p_index + 1);
const MINUTES: number[] = [0, 15, 30, 45];

const timeButtonBaseSx: SxProps<Theme> = {
    borderWidth: 1.5,
    fontWeight: "bold",
    boxShadow: 1,
    "&:hover": {
        borderWidth: 1.5,
    },
};

function getTimeButtonSx(p_isSelected: boolean): SxProps<Theme> {
    return {
        ...timeButtonBaseSx,
        bgcolor: p_isSelected ? "actionButtons.confirm.bg" : "background.paper",
        color: p_isSelected ? "actionButtons.confirm.text" : "text.primary",
        borderColor: p_isSelected ? "actionButtons.confirm.bg" : "text.secondary",
        boxShadow: p_isSelected ? 3 : 1,
        "&:hover": {
            bgcolor: p_isSelected ? "actionButtons.confirm.bg" : "action.hover",
            borderColor: p_isSelected ? "actionButtons.confirm.bg" : "text.primary",
            borderWidth: 1.5,
        },
    };
}

const secondaryButtonSx: SxProps<Theme> = {
    ...timeButtonBaseSx,
    bgcolor: "background.paper",
    color: "text.primary",
    borderColor: "text.secondary",
    "&:hover": {
        bgcolor: "action.hover",
        borderColor: "text.primary",
        borderWidth: 1.5,
    },
};

interface TimeSelectFieldProps {
    label: string;
    value: string;
    onChange: (p_value: string) => void;
    error?: boolean;
    helperText?: string;
    required?: boolean;
    optionalLabel?: string;
}

function parseTime(p_value: string): { hour: number; minute: number } {
    const [hour, minute] = p_value.split(":").map(Number);

    if (Number.isInteger(hour) && Number.isInteger(minute)) {
        return { hour, minute };
    }

    return { hour: 9, minute: 0 };
}

function formatTime(p_hour: number, p_minute: number): string {
    return `${p_hour.toString().padStart(2, "0")}:${p_minute.toString().padStart(2, "0")}`;
}

function toClockHour(p_hour: number): number {
    return p_hour % 12 || 12;
}

function getHourFromClock(p_clockHour: number, p_isPm: boolean): number {
    if (p_isPm) {
        return p_clockHour === 12 ? 12 : p_clockHour + 12;
    }

    return p_clockHour === 12 ? 0 : p_clockHour;
}

export function TimeSelectField({
    label,
    value,
    onChange,
    error = false,
    helperText,
    required = false,
    optionalLabel = "Effacer",
}: TimeSelectFieldProps) {
    const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null);
    const initialTime = parseTime(value);
    const [selectedHour, setSelectedHour] = useState<number>(initialTime.hour);
    const [selectedMinute, setSelectedMinute] = useState<number>(initialTime.minute);
    const [isPm, setIsPm] = useState<boolean>(initialTime.hour >= 12);
    const open = Boolean(anchorEl);
    const selectedClockHour = toClockHour(selectedHour);

    const openClock = (p_anchor: HTMLElement): void => {
        const parsedTime = parseTime(value);

        setSelectedHour(parsedTime.hour);
        setSelectedMinute(parsedTime.minute);
        setIsPm(parsedTime.hour >= 12);
        setAnchorEl(p_anchor);
    };

    const handleHourClick = (p_clockHour: number): void => {
        setSelectedHour(getHourFromClock(p_clockHour, isPm));
    };

    const handlePeriodChange = (p_nextIsPm: boolean): void => {
        setIsPm(p_nextIsPm);
        setSelectedHour(getHourFromClock(selectedClockHour, p_nextIsPm));
    };

    const handleConfirm = (): void => {
        onChange(formatTime(selectedHour, selectedMinute));
        setAnchorEl(null);
    };

    const handleClear = (): void => {
        onChange("");
        setAnchorEl(null);
    };

    return (
        <>
            <TextField
                fullWidth
                label={label}
                value={value}
                onClick={(p_event) => openClock(p_event.currentTarget)}
                onKeyDown={(p_event) => {
                    if (p_event.key === "Enter" || p_event.key === " ") {
                        p_event.preventDefault();
                        openClock(p_event.currentTarget);
                    }
                }}
                inputProps={{ readOnly: true }}
                InputLabelProps={{ shrink: true }}
                InputProps={{
                    endAdornment: (
                        <InputAdornment position="end">
                            <IconButton
                                aria-label={`Choisir ${label.toLowerCase()}`}
                                edge="end"
                                onClick={(p_event) => {
                                    p_event.stopPropagation();
                                    openClock(p_event.currentTarget);
                                }}
                            >
                                <AccessTimeIcon />
                            </IconButton>
                        </InputAdornment>
                    ),
                }}
                sx={{ mb: 2, cursor: "pointer" }}
                required={required}
                error={error}
                helperText={helperText}
            />
            {open && (
                <Box
                    sx={{
                        position: "fixed",
                        inset: 0,
                        zIndex: (p_theme) => p_theme.zIndex.modal + 1,
                        display: "flex",
                        alignItems: "center",
                        justifyContent: "center",
                        pointerEvents: "none",
                    }}
                >
                    <ClickAwayListener onClickAway={() => setAnchorEl(null)}>
                        <Paper
                            elevation={8}
                            sx={{
                                border: "1px solid",
                                borderColor: "divider",
                                pointerEvents: "auto",
                            }}
                        >
                        <Box sx={{ width: 320, p: 2 }}>
                            <Typography
                                variant="subtitle1"
                                sx={{
                                    fontWeight: "bold",
                                    mb: 1.5,
                                    px: 1,
                                    py: 0.75,
                                    borderRadius: 1,
                                    bgcolor: "actionButtons.confirm.bg",
                                    color: "actionButtons.confirm.text",
                                    textAlign: "center",
                                    boxShadow: 1,
                                }}
                            >
                                {formatTime(selectedHour, selectedMinute)}
                            </Typography>
                            <Box
                                sx={{
                                    position: "relative",
                                    width: 236,
                                    height: 236,
                                    mx: "auto",
                                    mb: 2,
                                    borderRadius: "50%",
                                    bgcolor: "background.default",
                                    border: "2px solid",
                                    borderColor: "divider",
                                }}
                            >
                                {HOURS.map((p_hour, p_index) => {
                                    const angle = (p_index / HOURS.length) * 360 - 60;
                                    const x = 92 + 92 * Math.cos((angle * Math.PI) / 180);
                                    const y = 92 + 92 * Math.sin((angle * Math.PI) / 180);
                                    const isSelected = p_hour === selectedClockHour;

                                    return (
                                        <Button
                                            key={p_hour}
                                            aria-label={`${p_hour} heure`}
                                            variant={isSelected ? "contained" : "outlined"}
                                            onClick={() => handleHourClick(p_hour)}
                                            sx={{
                                                position: "absolute",
                                                left: x,
                                                top: y,
                                                minWidth: 52,
                                                width: 52,
                                                height: 52,
                                                borderRadius: "50%",
                                                bgcolor: isSelected ? "actionButtons.confirm.bg" : "transparent",
                                                color: isSelected ? "actionButtons.confirm.text" : "text.primary",
                                                borderColor: "transparent",
                                                borderWidth: 0,
                                                boxShadow: isSelected ? 3 : "none",
                                                fontSize: 16,
                                                fontWeight: "bold",
                                                textTransform: "none",
                                                "&:hover": {
                                                    bgcolor: isSelected ? "actionButtons.confirm.bg" : "action.hover",
                                                    borderColor: "transparent",
                                                    borderWidth: 0,
                                                },
                                            }}
                                        >
                                            {p_hour}
                                        </Button>
                                    );
                                })}
                            </Box>
                            <Box sx={{ display: "flex", gap: 1, mb: 2 }}>
                                <Button
                                    fullWidth
                                    variant={!isPm ? "contained" : "outlined"}
                                    sx={getTimeButtonSx(!isPm)}
                                    onClick={() => handlePeriodChange(false)}
                                >
                                    AM
                                </Button>
                                <Button
                                    fullWidth
                                    variant={isPm ? "contained" : "outlined"}
                                    sx={getTimeButtonSx(isPm)}
                                    onClick={() => handlePeriodChange(true)}
                                >
                                    PM
                                </Button>
                            </Box>
                            <Box sx={{ display: "grid", gridTemplateColumns: "repeat(4, 1fr)", gap: 1, mb: 2 }}>
                                {MINUTES.map((p_minute) => (
                                    <Button
                                        key={p_minute}
                                        variant={selectedMinute === p_minute ? "contained" : "outlined"}
                                        sx={getTimeButtonSx(selectedMinute === p_minute)}
                                        onClick={() => setSelectedMinute(p_minute)}
                                    >
                                        :{p_minute.toString().padStart(2, "0")}
                                    </Button>
                                ))}
                            </Box>
                            <Box sx={{ display: "flex", justifyContent: "flex-end", gap: 1 }}>
                                {!required && (
                                    <Button variant="outlined" sx={secondaryButtonSx} onClick={handleClear}>
                                        {optionalLabel}
                                    </Button>
                                )}
                                <Button variant="outlined" sx={secondaryButtonSx} onClick={() => setAnchorEl(null)}>
                                    Annuler
                                </Button>
                                <Button variant="contained" sx={getTimeButtonSx(true)} onClick={handleConfirm}>
                                    OK
                                </Button>
                            </Box>
                        </Box>
                    </Paper>
                    </ClickAwayListener>
                </Box>
            )}
        </>
    );
}
