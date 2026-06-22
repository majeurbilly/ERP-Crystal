import { useEffect, useState } from "react";
import {
    Alert,
    FormControl,
    InputLabel,
    MenuItem,
    Select,
    Stack,
    TextField,
    Typography,
    type SelectChangeEvent,
} from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import CalendarMonthIcon from "@mui/icons-material/CalendarMonth";
import { useTimesheetMutations } from "../../../api/mutations/hr/useTimesheetMutations";
import locationService from "../../../api/services/inventory/locationService";
import { locationsCacheKey } from "../../../data/cacheKeys";
import { notifyErrorMessage, notifySuccessMessage } from "../../../data/utils/popupMessageManager";
import type {
    GenerateWeeklyTimesheetsFormData,
    GenerateWeeklyTimesheetsResult,
} from "../../../data/types/hr/timesheet";
import type { Location } from "../../../data/types/inventory/location";
import { extractApiErrorMessage } from "../../../data/utils/extractApiErrorMessage";
import { FormModal } from "../FormModal";

interface GenerateWeeklyTimesheetsFormProps {
    open: boolean;
    onClose: () => void;
}

const DATE_INPUT_MONDAY_MIN = "1970-01-05";

function formatDateInputValue(p_date: Date): string {
    const timezoneOffsetMs = p_date.getTimezoneOffset() * 60 * 1000;
    return new Date(p_date.getTime() - timezoneOffsetMs).toISOString().substring(0, 10);
}

function getCurrentWeekMondayDateValue(): string {
    const now = new Date();
    const day = now.getDay();
    const daysSinceMonday = day === 0 ? 6 : day - 1;
    const monday = new Date(now);
    monday.setDate(now.getDate() - daysSinceMonday);
    return formatDateInputValue(monday);
}

function getLastCompleteWeekMondayDateValue(): string {
    const currentWeekMonday = new Date(`${getCurrentWeekMondayDateValue()}T00:00:00`);
    currentWeekMonday.setDate(currentWeekMonday.getDate() - 7);
    return formatDateInputValue(currentWeekMonday);
}

function isMondayDateValue(p_value: string): boolean {
    const parsedDate = new Date(`${p_value}T00:00:00`);
    return !Number.isNaN(parsedDate.getTime()) && parsedDate.getDay() === 1;
}

function isCompletePastWeekDateValue(p_value: string): boolean {
    const parsedDate = new Date(`${p_value}T00:00:00`);
    if (Number.isNaN(parsedDate.getTime())) {
        return false;
    }

    const periodEnd = new Date(parsedDate);
    periodEnd.setDate(parsedDate.getDate() + 6);

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    return periodEnd < today;
}

export default function GenerateWeeklyTimesheetsForm({
    open,
    onClose,
}: GenerateWeeklyTimesheetsFormProps) {
    const { generateWeeklyTimesheets, isGeneratingWeeklyTimesheets } = useTimesheetMutations();

    const [periodStart, setPeriodStart] = useState<string>(getLastCompleteWeekMondayDateValue());
    const [locationId, setLocationId] = useState<string>("");
    const [periodStartError, setPeriodStartError] = useState<string>("");
    const [result, setResult] = useState<GenerateWeeklyTimesheetsResult | null>(null);

    const locationsQuery = useQuery<Location[], Error>({
        queryKey: locationsCacheKey.list(),
        queryFn: () => locationService.getAll(),
        enabled: open,
    });

    useEffect(() => {
        if (open) {
            setPeriodStart(getLastCompleteWeekMondayDateValue());
            setLocationId("");
            setPeriodStartError("");
            setResult(null);
        }
    }, [open]);

    const handleClose = (): void => {
        if (!isGeneratingWeeklyTimesheets) {
            onClose();
        }
    };

    const handleSubmit = async (p_event: React.FormEvent): Promise<void> => {
        p_event.preventDefault();
        setResult(null);

        if (!periodStart) {
            setPeriodStartError("La date de début est requise.");
            return;
        }

        if (!isMondayDateValue(periodStart)) {
            setPeriodStartError("La date de début doit être un lundi.");
            return;
        }

        if (!isCompletePastWeekDateValue(periodStart)) {
            setPeriodStartError("La semaine doit être complètement terminée.");
            return;
        }

        setPeriodStartError("");

        const formData: GenerateWeeklyTimesheetsFormData = {
            periodStart,
            locationId: locationId ? Number(locationId) : null,
        };

        try {
            const generationResult = await generateWeeklyTimesheets(formData);
            setResult(generationResult);
            notifySuccessMessage("Génération des feuilles de temps terminée.");
        } catch (error: unknown) {
            notifyErrorMessage(extractApiErrorMessage(error));
        }
    };

    const locations: Location[] = locationsQuery.data ?? [];

    return (
        <FormModal
            open={open}
            onClose={handleClose}
            title="Générer les feuilles de temps"
            onSubmit={handleSubmit}
            isSubmitting={isGeneratingWeeklyTimesheets}
            confirmLabel="Générer"
            confirmIcon={<CalendarMonthIcon />}
        >
            <Stack spacing={2}>
                <TextField
                    fullWidth
                    label="Début de semaine"
                    type="date"
                    value={periodStart}
                    onChange={(p_event) => setPeriodStart(p_event.target.value)}
                    InputLabelProps={{ shrink: true }}
                    inputProps={{
                        max: getLastCompleteWeekMondayDateValue(),
                        min: DATE_INPUT_MONDAY_MIN,
                        step: 7,
                    }}
                    required
                    error={!!periodStartError}
                    helperText={periodStartError || "Sélectionnez un lundi d'une semaine terminée."}
                />
                <FormControl fullWidth>
                    <InputLabel id="weekly-timesheets-location-label" shrink>
                        Succursale
                    </InputLabel>
                    <Select
                        labelId="weekly-timesheets-location-label"
                        label="Succursale"
                        value={locationId}
                        onChange={(p_event: SelectChangeEvent<string>) =>
                            setLocationId(p_event.target.value)
                        }
                        disabled={locationsQuery.isLoading}
                        displayEmpty
                        renderValue={(p_selected) => {
                            if (!p_selected) {
                                return "Toutes les succursales";
                            }

                            const selectedValue = String(p_selected);
                            const selectedLocation = locations.find(
                                (p_location: Location) => String(p_location.id) === selectedValue
                            );
                            return selectedLocation?.title ?? "";
                        }}
                    >
                        <MenuItem value="">Toutes les succursales</MenuItem>
                        {locations.map((p_location: Location) => (
                            <MenuItem key={p_location.id} value={String(p_location.id)}>
                                {p_location.title}
                            </MenuItem>
                        ))}
                    </Select>
                </FormControl>
                {result && (
                    <Alert severity="success">
                        <Typography variant="body2">
                            {`${result.createdCount} créée(s), ${result.existingCount} déjà existante(s), ${result.linkedTimeEntryCount} pointage(s) liés.`}
                        </Typography>
                    </Alert>
                )}
            </Stack>
        </FormModal>
    );
}
