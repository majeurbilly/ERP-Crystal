import { useEffect, useState } from "react";
import {
    Alert,
    FormControl,
    InputLabel,
    MenuItem,
    Select,
    TextField,
    Typography,
    type SelectChangeEvent,
} from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import payrollService from "../../../api/services/hr/payrollService";
import locationService from "../../../api/services/inventory/locationService";
import { usePayrollMutations } from "../../../api/mutations/hr/usePayrollMutations";
import { useAuth } from "../../../context/AuthContext";
import { usePermissions } from "../../../permissions/usePermissions";
import { ENTITY_TYPES } from "../../../permissions/permissions";
import { locationsCacheKey, payPeriodsCacheKey } from "../../../data/cacheKeys";
import type { Location } from "../../../data/types/inventory/location";
import type { PayPeriod } from "../../../data/types/hr/payPeriod";
import { formatPayPeriodLabel } from "../../../data/types/hr/payPeriod";
import { notifyErrorMessage, notifySuccessMessage } from "../../../data/utils/popupMessageManager";
import { extractApiErrorMessage } from "../../../data/utils/extractApiErrorMessage";
import { FormModal } from "../../forms/FormModal";

interface PayrollGenerateFormProps {
    showPayrollGenerateForm: boolean;
    setShowPayrollGenerateForm: (p_value: boolean) => void;
}

interface PayrollGenerateFormErrors {
    locationId: string;
    periodStartDate: string;
}

const DATE_INPUT_MONDAY_MIN = "1970-01-05";

function formatDateInputValue(p_date: Date): string {
    const timezoneOffsetMs = p_date.getTimezoneOffset() * 60 * 1000;
    return new Date(p_date.getTime() - timezoneOffsetMs).toISOString().substring(0, 10);
}

function addDaysToDateValue(p_value: string, p_days: number): string {
    const date = new Date(`${p_value}T00:00:00`);
    date.setDate(date.getDate() + p_days);
    return formatDateInputValue(date);
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

export default function PayrollGenerateForm({
    showPayrollGenerateForm,
    setShowPayrollGenerateForm,
}: PayrollGenerateFormProps) {
    const handleClose = (): void => setShowPayrollGenerateForm(false);
    const { generatePayrollForPeriod, isGeneratingPayrollForPeriod } = usePayrollMutations();
    const { user } = useAuth();
    const { isSuperAdmin } = usePermissions(ENTITY_TYPES.PAYROLL);

    const [locationId, setLocationId] = useState<string>("");
    const [periodStartDate, setPeriodStartDate] = useState<string>(getLastCompleteWeekMondayDateValue());
    const [errors, setErrors] = useState<PayrollGenerateFormErrors>({
        locationId: "",
        periodStartDate: "",
    });

    const payPeriodsQuery = useQuery<PayPeriod[], Error>({
        queryKey: payPeriodsCacheKey.list(),
        queryFn: () => payrollService.getPeriods(),
        enabled: showPayrollGenerateForm,
    });

    const locationsQuery = useQuery<Location[], Error>({
        queryKey: locationsCacheKey.list(),
        queryFn: () => locationService.getAll(),
        enabled: showPayrollGenerateForm,
    });

    const payPeriods = payPeriodsQuery.data ?? [];
    const locations = locationsQuery.data ?? [];
    const ownLocationId = user?.employeeProfile?.locationId;
    const selectableLocations = isSuperAdmin
        ? locations
        : locations.filter((p_location) => p_location.id === ownLocationId);

    useEffect(() => {
        if (showPayrollGenerateForm) {
            setLocationId(isSuperAdmin ? "" : ownLocationId ? String(ownLocationId) : "");
            setPeriodStartDate(getLastCompleteWeekMondayDateValue());
            setErrors({
                locationId: "",
                periodStartDate: "",
            });
        }
    }, [isSuperAdmin, ownLocationId, showPayrollGenerateForm]);

    const validate = (): boolean => {
        let isValid = true;
        const newErrors: PayrollGenerateFormErrors = {
            locationId: "",
            periodStartDate: "",
        };

        if (!isSuperAdmin && !locationId) {
            newErrors.locationId = "La succursale est requise.";
            isValid = false;
        }

        if (!periodStartDate) {
            newErrors.periodStartDate = "La date de dÃ©but est requise.";
            isValid = false;
        } else if (!isMondayDateValue(periodStartDate)) {
            newErrors.periodStartDate = "La date de dÃ©but doit Ãªtre un lundi.";
            isValid = false;
        } else if (!isCompletePastWeekDateValue(periodStartDate)) {
            newErrors.periodStartDate = "La semaine doit Ãªtre complÃ¨tement terminÃ©e.";
            isValid = false;
        }

        setErrors(newErrors);
        return isValid;
    };

    const resolvePayPeriodId = async (): Promise<number> => {
        const periodEndDate = addDaysToDateValue(periodStartDate, 6);
        const existingPeriod = payPeriods.find(
            (p_period) =>
                p_period.startDate === periodStartDate
                && p_period.endDate === periodEndDate
        );

        if (existingPeriod) {
            return existingPeriod.id;
        }

        const createdPeriod = await payrollService.createPeriod({
            startDate: periodStartDate,
            endDate: periodEndDate,
        });
        return createdPeriod.id;
    };

    const handleSubmit = async (p_event: React.FormEvent): Promise<void> => {
        p_event.preventDefault();
        if (isLoadingOptions) {
            return;
        }

        if (!validate()) {
            return;
        }

        try {
            const resolvedPayPeriodId = await resolvePayPeriodId();
            const result = await generatePayrollForPeriod({
                payPeriodId: resolvedPayPeriodId,
                locationId: locationId ? Number(locationId) : null,
            });
            notifySuccessMessage(
                `${result.createdCount} fiche(s) gÃ©nÃ©rÃ©e(s), ${result.existingCount} dÃ©jÃ  existante(s), ${result.skippedCount} ignorÃ©e(s).`
            );
            handleClose();
        } catch (error: unknown) {
            notifyErrorMessage(extractApiErrorMessage(error));
        }
    };

    const periodEndDate = addDaysToDateValue(periodStartDate, 6);
    const isLoadingOptions = payPeriodsQuery.isLoading || locationsQuery.isLoading;

    return (
        <FormModal
            open={showPayrollGenerateForm}
            onClose={handleClose}
            title="GÃ©nÃ©rer les fiches de paie"
            onSubmit={handleSubmit}
            isSubmitting={isGeneratingPayrollForPeriod || isLoadingOptions}
            confirmLabel="GÃ©nÃ©rer"
        >
            <Alert severity="info" sx={{ mb: 2 }}>
                Les fiches seront gÃ©nÃ©rÃ©es pour les feuilles de temps approuvÃ©es couvrant exactement la semaine terminÃ©e et la succursale choisies.
            </Alert>

            <FormControl fullWidth sx={{ mb: 2 }} required={!isSuperAdmin} error={!!errors.locationId}>
                <InputLabel id="payroll-location-label" shrink>
                    Succursale
                </InputLabel>
                <Select
                    labelId="payroll-location-label"
                    label="Succursale"
                    value={locationId}
                    displayEmpty
                    renderValue={(p_selected) => {
                        if (p_selected === "") {
                            return "Toutes les succursales";
                        }

                        return selectableLocations.find(
                            (p_location) => String(p_location.id) === p_selected
                        )?.title ?? "";
                    }}
                    onChange={(p_event: SelectChangeEvent<string>) =>
                        setLocationId(p_event.target.value)
                    }
                    disabled={isLoadingOptions || isGeneratingPayrollForPeriod || !isSuperAdmin}
                >
                    {isSuperAdmin && <MenuItem value="">Toutes les succursales</MenuItem>}
                    {selectableLocations.map((p_location) => (
                        <MenuItem key={p_location.id} value={String(p_location.id)}>
                            {p_location.title}
                        </MenuItem>
                    ))}
                </Select>
                {errors.locationId && (
                    <Typography variant="caption" color="error" sx={{ mt: 0.5 }}>
                        {errors.locationId}
                    </Typography>
                )}
            </FormControl>

            <TextField
                fullWidth
                label="DÃ©but de semaine"
                type="date"
                value={periodStartDate}
                onChange={(p_event) => setPeriodStartDate(p_event.target.value)}
                InputLabelProps={{ shrink: true }}
                inputProps={{
                    max: getLastCompleteWeekMondayDateValue(),
                    min: DATE_INPUT_MONDAY_MIN,
                    step: 7,
                }}
                sx={{ mb: 2 }}
                required
                error={!!errors.periodStartDate}
                helperText={
                    errors.periodStartDate
                    || "SÃ©lectionnez un lundi d'une semaine complÃ¨tement terminÃ©e."
                }
            />

            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                {`PÃ©riode gÃ©nÃ©rÃ©e : ${formatPayPeriodLabel({
                    startDate: periodStartDate,
                    endDate: periodEndDate,
                })}`}
            </Typography>
        </FormModal>
    );
}
