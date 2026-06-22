import { useEffect, useMemo, useState } from "react";
import {
    FormControl,
    FormHelperText,
    InputLabel,
    MenuItem,
    Select,
    TextField,
    type SelectChangeEvent,
} from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import type { ScheduledShift, ScheduledShiftFormData } from "../../../data/types/hr/scheduledShift";
import { FormModal } from "../FormModal";
import { TimeSelectField } from "../TimeSelectField";
import { notifyErrorMessage, notifySuccessMessage } from "../../../data/utils/popupMessageManager";
import { useScheduledShiftMutations } from "../../../api/mutations/hr/useScheduledShiftMutations";
import { extractApiErrorMessage } from "../../../data/utils/extractApiErrorMessage";
import employeeProfileService from "../../../api/services/hr/employeeProfileService";
import jobPositionService from "../../../api/services/hr/jobPositionService";
import locationService from "../../../api/services/inventory/locationService";
import scheduledShiftService from "../../../api/services/hr/scheduledShiftService";
import {
    employeeProfilesCacheKey,
    jobPositionsCacheKey,
    locationsCacheKey,
    scheduledShiftsCacheKey,
} from "../../../data/cacheKeys";
import type { EmployeeProfile } from "../../../data/types/hr/employeeProfile";
import type { JobPosition } from "../../../data/types/hr/jobPosition";
import type { Location } from "../../../data/types/inventory/location";
import { normalizeTimeToHHmm } from "../../../data/data-mapper/hr/scheduledShiftMapper";

interface ScheduledShiftFormProps {
    showScheduledShiftForm: boolean;
    setShowScheduledShiftForm: (p_value: boolean) => void;
    editScheduledShift: ScheduledShift | null;
    setEditScheduledShift?: (p_value: ScheduledShift | null) => void;
    defaultLocationId?: number | null;
}

interface ScheduledShiftFormErrors {
    locationId: string;
    date: string;
    startTime: string;
    endTime: string;
}

function hasTimeOverlap(
    p_leftStart: string,
    p_leftEnd: string,
    p_rightStart: string,
    p_rightEnd: string
): boolean {
    return p_leftStart < p_rightEnd && p_leftEnd > p_rightStart;
}

export default function ScheduledShiftForm({
    showScheduledShiftForm,
    setShowScheduledShiftForm,
    editScheduledShift,
    setEditScheduledShift,
    defaultLocationId = null,
}: ScheduledShiftFormProps) {
    const handleClose = (): void => setShowScheduledShiftForm(false);
    const {
        addScheduledShift,
        isAddingScheduledShift,
        updateScheduledShift,
        isUpdatingScheduledShift,
    } = useScheduledShiftMutations();

    const isEditMode: boolean = editScheduledShift !== null;

    const [locationId, setLocationId] = useState<string>("");
    const [employeeProfileId, setEmployeeProfileId] = useState<string>("");
    const [jobPositionId, setJobPositionId] = useState<string>("");
    const [date, setDate] = useState<string>("");
    const [startTime, setStartTime] = useState<string>("");
    const [endTime, setEndTime] = useState<string>("");
    const [errors, setErrors] = useState<ScheduledShiftFormErrors>({
        locationId: "",
        date: "",
        startTime: "",
        endTime: "",
    });

    const locationsQuery = useQuery<Location[], Error>({
        queryKey: locationsCacheKey.list(),
        queryFn: () => locationService.getAll(),
        enabled: showScheduledShiftForm,
    });

    const employeesQuery = useQuery<EmployeeProfile[], Error>({
        queryKey: employeeProfilesCacheKey.list(),
        queryFn: () => employeeProfileService.getAll(),
        enabled: showScheduledShiftForm,
    });

    const jobPositionsQuery = useQuery<JobPosition[], Error>({
        queryKey: jobPositionsCacheKey.list(),
        queryFn: () => jobPositionService.getAll(),
        enabled: showScheduledShiftForm,
    });

    const scheduledShiftsQuery = useQuery<ScheduledShift[], Error>({
        queryKey: scheduledShiftsCacheKey.list(),
        queryFn: () => scheduledShiftService.getAll(),
        enabled: showScheduledShiftForm,
    });

    useEffect(() => {
        if (!showScheduledShiftForm) {
            return;
        }

        if (editScheduledShift) {
            setLocationId(editScheduledShift.locationId ? String(editScheduledShift.locationId) : "");
            setEmployeeProfileId(editScheduledShift.employeeProfileId ? String(editScheduledShift.employeeProfileId) : "");
            setJobPositionId(editScheduledShift.jobPositionId ? String(editScheduledShift.jobPositionId) : "");
            setDate(editScheduledShift.date);
            setStartTime(normalizeTimeToHHmm(editScheduledShift.startTime));
            setEndTime(normalizeTimeToHHmm(editScheduledShift.endTime));
        } else {
            setLocationId(defaultLocationId ? String(defaultLocationId) : "");
            setEmployeeProfileId("");
            setJobPositionId("");
            setDate("");
            setStartTime("");
            setEndTime("");
        }

        setErrors({
            locationId: "",
            date: "",
            startTime: "",
            endTime: "",
        });
    }, [defaultLocationId, editScheduledShift, showScheduledShiftForm]);

    const validate = (): boolean => {
        let isValid: boolean = true;
        const newErrors: ScheduledShiftFormErrors = {
            locationId: "",
            date: "",
            startTime: "",
            endTime: "",
        };

        if (!locationId) {
            newErrors.locationId = "La succursale est requise.";
            isValid = false;
        }

        if (!date) {
            newErrors.date = "La date est requise.";
            isValid = false;
        }

        if (!startTime) {
            newErrors.startTime = "L'heure de début est requise.";
            isValid = false;
        }

        if (!endTime) {
            newErrors.endTime = "L'heure de fin est requise.";
            isValid = false;
        }

        const normalizedStart: string = normalizeTimeToHHmm(startTime);
        const normalizedEnd: string = normalizeTimeToHHmm(endTime);
        if (normalizedStart && normalizedEnd && normalizedEnd <= normalizedStart) {
            newErrors.endTime = "L'heure de fin doit être postérieure à l'heure de début.";
            isValid = false;
        }

        setErrors(newErrors);
        return isValid;
    };

    const handleSubmit = async (p_event: React.FormEvent): Promise<void> => {
        p_event.preventDefault();
        if (!validate()) {
            return;
        }

        const selectedEmployee: EmployeeProfile | undefined = employeeProfileId
            ? employees.find((p_employee) => String(p_employee.id) === employeeProfileId)
            : undefined;
        const resolvedJobPositionId: number | null = jobPositionId
            ? Number(jobPositionId)
            : selectedEmployee && selectedEmployee.jobPositionId > 0
                ? selectedEmployee.jobPositionId
                : null;

        const formData: ScheduledShiftFormData = {
            locationId: Number(locationId),
            employeeProfileId: employeeProfileId ? Number(employeeProfileId) : null,
            jobPositionId: resolvedJobPositionId,
            date,
            startTime: normalizeTimeToHHmm(startTime),
            endTime: normalizeTimeToHHmm(endTime),
        };

        try {
            if (isEditMode && editScheduledShift) {
                await updateScheduledShift({
                    id: String(editScheduledShift.id),
                    data: formData,
                });
                notifySuccessMessage("Le quart planifié a été modifié avec succès.");
                if (setEditScheduledShift) {
                    setEditScheduledShift(null);
                }
            } else {
                await addScheduledShift(formData);
                notifySuccessMessage("Le quart planifié a été ajouté avec succès.");
            }
            handleClose();
        } catch (error: unknown) {
            notifyErrorMessage(extractApiErrorMessage(error));
        }
    };

    const locations: Location[] = locationsQuery.data ?? [];
    const employees: EmployeeProfile[] = employeesQuery.data ?? [];
    const jobPositions: JobPosition[] = jobPositionsQuery.data ?? [];
    const scheduledShifts: ScheduledShift[] = scheduledShiftsQuery.data ?? [];
    const selectedLocationValue = locations.some(
        (p_location) => String(p_location.id) === locationId
    ) ? locationId : "";
    const availableEmployees: EmployeeProfile[] = useMemo(() => {
        const selectedLocationId = Number(locationId);
        const normalizedStart = normalizeTimeToHHmm(startTime);
        const normalizedEnd = normalizeTimeToHHmm(endTime);
        const canCheckAvailability = !!date && !!normalizedStart && !!normalizedEnd;

        if (!selectedLocationId) {
            return [];
        }

        return employees.filter((p_employee) => {
            if (p_employee.locationId !== selectedLocationId) {
                return false;
            }

            if (!canCheckAvailability) {
                return true;
            }

            return !scheduledShifts.some((p_shift) => {
                if (
                    p_shift.isDeleted
                    || p_shift.id === editScheduledShift?.id
                    || p_shift.employeeProfileId !== p_employee.id
                    || p_shift.date !== date
                ) {
                    return false;
                }

                return hasTimeOverlap(
                    normalizedStart,
                    normalizedEnd,
                    normalizeTimeToHHmm(p_shift.startTime),
                    normalizeTimeToHHmm(p_shift.endTime)
                );
            });
        });
    }, [date, editScheduledShift?.id, employees, endTime, locationId, scheduledShifts, startTime]);

    useEffect(() => {
        if (
            employeeProfileId
            && !employeesQuery.isLoading
            && !scheduledShiftsQuery.isLoading
            && !availableEmployees.some((p_employee) => String(p_employee.id) === employeeProfileId)
        ) {
            setEmployeeProfileId("");
        }
    }, [availableEmployees, employeeProfileId, employeesQuery.isLoading, scheduledShiftsQuery.isLoading]);

    return (
        <FormModal
            open={showScheduledShiftForm}
            onClose={handleClose}
            title={isEditMode ? "Modifier un quart planifié" : "Ajouter un quart planifié"}
            onSubmit={handleSubmit}
            isSubmitting={isEditMode ? isUpdatingScheduledShift : isAddingScheduledShift}
        >
            <FormControl fullWidth sx={{ mb: 2 }} required error={!!errors.locationId}>
                <InputLabel id="shift-location-label">Succursale</InputLabel>
                <Select
                    labelId="shift-location-label"
                    label="Succursale"
                    value={selectedLocationValue}
                    onChange={(p_event: SelectChangeEvent<string>) =>
                        setLocationId(p_event.target.value)
                    }
                    disabled={locationsQuery.isLoading}
                >
                    {locations.map((p_location: Location) => (
                        <MenuItem key={p_location.id} value={String(p_location.id)}>
                            {p_location.title}
                        </MenuItem>
                    ))}
                </Select>
                {!!errors.locationId && <FormHelperText>{errors.locationId}</FormHelperText>}
            </FormControl>
            <FormControl fullWidth sx={{ mb: 2 }}>
                <InputLabel id="shift-employee-label">Employé</InputLabel>
                <Select
                    labelId="shift-employee-label"
                    label="Employé"
                    value={employeeProfileId}
                    onChange={(p_event: SelectChangeEvent<string>) =>
                        setEmployeeProfileId(p_event.target.value)
                    }
                    disabled={!locationId || employeesQuery.isLoading || scheduledShiftsQuery.isLoading}
                >
                    <MenuItem value="">Aucun employé assigné</MenuItem>
                    {availableEmployees.map((p_employee: EmployeeProfile) => (
                        <MenuItem key={p_employee.id} value={String(p_employee.id)}>
                            {`${p_employee.firstName} ${p_employee.lastName}`}
                        </MenuItem>
                    ))}
                </Select>
                {!locationId && (
                    <FormHelperText>Choisis une succursale pour voir les employés disponibles.</FormHelperText>
                )}
            </FormControl>
            <FormControl fullWidth sx={{ mb: 2 }}>
                <InputLabel id="shift-job-position-label">Poste</InputLabel>
                <Select
                    labelId="shift-job-position-label"
                    label="Poste"
                    value={jobPositionId}
                    onChange={(p_event: SelectChangeEvent<string>) =>
                        setJobPositionId(p_event.target.value)
                    }
                    disabled={jobPositionsQuery.isLoading}
                >
                    <MenuItem value="">Aucun poste assigné</MenuItem>
                    {jobPositions.map((p_position: JobPosition) => (
                        <MenuItem key={p_position.id} value={String(p_position.id)}>
                            {p_position.name}
                        </MenuItem>
                    ))}
                </Select>
            </FormControl>
            <TextField
                fullWidth
                label="Date"
                type="date"
                value={date}
                onChange={(p_event) => setDate(p_event.target.value)}
                InputLabelProps={{ shrink: true }}
                sx={{ mb: 2 }}
                required
                error={!!errors.date}
                helperText={errors.date}
            />
            <TimeSelectField
                label="Heure de début"
                value={startTime}
                onChange={setStartTime}
                required
                error={!!errors.startTime}
                helperText={errors.startTime}
            />
            <TimeSelectField
                label="Heure de fin"
                value={endTime}
                onChange={setEndTime}
                required
                error={!!errors.endTime}
                helperText={errors.endTime}
            />
        </FormModal>
    );
}
