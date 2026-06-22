import { useEffect, useMemo, useState } from "react";
import {
    FormControl,
    InputLabel,
    MenuItem,
    Select,
    TextField,
    type SelectChangeEvent,
} from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import type { TimeEntry, TimeEntryFormData } from "../../../data/types/hr/timeEntry";
import { FormModal } from "../FormModal";
import { TimeSelectField } from "../TimeSelectField";
import { notifyErrorMessage, notifySuccessMessage } from "../../../data/utils/popupMessageManager";
import { useTimeEntryMutations } from "../../../api/mutations/hr/useTimeEntryMutations";
import { extractApiErrorMessage } from "../../../data/utils/extractApiErrorMessage";
import employeeProfileService from "../../../api/services/hr/employeeProfileService";
import scheduledShiftService from "../../../api/services/hr/scheduledShiftService";
import { employeeProfilesCacheKey, scheduledShiftsCacheKey } from "../../../data/cacheKeys";
import type { EmployeeProfile } from "../../../data/types/hr/employeeProfile";
import type { ScheduledShift } from "../../../data/types/hr/scheduledShift";
import { normalizeTimeToHHmm } from "../../../data/data-mapper/hr/scheduledShiftMapper";

const NO_SCHEDULED_SHIFT_VALUE = "__none_shift__";

interface TimeEntryFormProps {
    showTimeEntryForm: boolean;
    setShowTimeEntryForm: (p_value: boolean) => void;
    editTimeEntry: TimeEntry | null;
    setEditTimeEntry?: (p_value: TimeEntry | null) => void;
}

interface TimeEntryFormErrors {
    employeeProfileId: string;
    date: string;
    startTime: string;
    endTime: string;
}

function formatScheduledShiftLabel(p_shift: ScheduledShift): string {
    return `#${p_shift.id} — ${p_shift.employeeFirstName} ${p_shift.employeeLastName} (${p_shift.date} ${p_shift.startTime}–${p_shift.endTime})`;
}

export default function TimeEntryForm({
    showTimeEntryForm,
    setShowTimeEntryForm,
    editTimeEntry,
    setEditTimeEntry,
}: TimeEntryFormProps) {
    const handleClose = (): void => setShowTimeEntryForm(false);
    const {
        addTimeEntry,
        isAddingTimeEntry,
        updateTimeEntry,
        isUpdatingTimeEntry,
    } = useTimeEntryMutations();

    const isEditMode: boolean = editTimeEntry !== null;

    const [employeeProfileId, setEmployeeProfileId] = useState<string>("");
    const [scheduledShiftId, setScheduledShiftId] = useState<string>(NO_SCHEDULED_SHIFT_VALUE);
    const [date, setDate] = useState<string>("");
    const [startTime, setStartTime] = useState<string>("");
    const [endTime, setEndTime] = useState<string>("");
    const [errors, setErrors] = useState<TimeEntryFormErrors>({
        employeeProfileId: "",
        date: "",
        startTime: "",
        endTime: "",
    });

    const employeesQuery = useQuery<EmployeeProfile[], Error>({
        queryKey: employeeProfilesCacheKey.list(),
        queryFn: () => employeeProfileService.getAll(),
        enabled: showTimeEntryForm,
    });

    const scheduledShiftsQuery = useQuery<ScheduledShift[], Error>({
        queryKey: scheduledShiftsCacheKey.list(),
        queryFn: () => scheduledShiftService.getAll(),
        enabled: showTimeEntryForm,
    });

    useEffect(() => {
        if (showTimeEntryForm) {
            if (editTimeEntry) {
                setEmployeeProfileId(String(editTimeEntry.employeeProfileId));
                setScheduledShiftId(
                    editTimeEntry.scheduledShiftId !== null
                        ? String(editTimeEntry.scheduledShiftId)
                        : NO_SCHEDULED_SHIFT_VALUE
                );
                setDate(editTimeEntry.date);
                setStartTime(normalizeTimeToHHmm(editTimeEntry.startTime));
                setEndTime(
                    editTimeEntry.endTime !== null
                        ? normalizeTimeToHHmm(editTimeEntry.endTime)
                        : ""
                );
            } else {
                setEmployeeProfileId("");
                setScheduledShiftId(NO_SCHEDULED_SHIFT_VALUE);
                setDate("");
                setStartTime("");
                setEndTime("");
            }
            setErrors({
                employeeProfileId: "",
                date: "",
                startTime: "",
                endTime: "",
            });
        }
    }, [editTimeEntry, showTimeEntryForm]);

    const validate = (): boolean => {
        let isValid: boolean = true;
        const newErrors: TimeEntryFormErrors = {
            employeeProfileId: "",
            date: "",
            startTime: "",
            endTime: "",
        };

        if (!employeeProfileId) {
            newErrors.employeeProfileId = "L'employé est requis.";
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

        const normalizedStart: string = normalizeTimeToHHmm(startTime);
        const normalizedEnd: string = endTime.trim().length > 0 ? normalizeTimeToHHmm(endTime) : "";
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

        const normalizedEnd: string = endTime.trim().length > 0 ? normalizeTimeToHHmm(endTime) : "";

        const formData: TimeEntryFormData = {
            employeeProfileId: Number(employeeProfileId),
            scheduledShiftId:
                scheduledShiftId === NO_SCHEDULED_SHIFT_VALUE
                    ? null
                    : Number(scheduledShiftId),
            date,
            startTime: normalizeTimeToHHmm(startTime),
            endTime: normalizedEnd.length > 0 ? normalizedEnd : null,
        };

        try {
            if (isEditMode && editTimeEntry) {
                await updateTimeEntry({
                    id: String(editTimeEntry.id),
                    data: formData,
                });
                notifySuccessMessage("Le pointage a été modifié avec succès.");
                if (setEditTimeEntry) {
                    setEditTimeEntry(null);
                }
            } else {
                await addTimeEntry(formData);
                notifySuccessMessage("Le pointage a été ajouté avec succès.");
            }
            handleClose();
        } catch (error: unknown) {
            notifyErrorMessage(extractApiErrorMessage(error));
        }
    };

    const employees: EmployeeProfile[] = employeesQuery.data ?? [];
    const allScheduledShifts: ScheduledShift[] = scheduledShiftsQuery.data ?? [];

    const filteredScheduledShifts = useMemo(() => {
        if (!employeeProfileId) {
            return [];
        }
        return allScheduledShifts.filter((p_shift) => {
            if (p_shift.employeeProfileId !== Number(employeeProfileId)) {
                return false;
            }
            if (!date) {
                return true;
            }
            return p_shift.date === date;
        });
    }, [allScheduledShifts, date, employeeProfileId]);

    const selectedShiftValue =
        scheduledShiftId === NO_SCHEDULED_SHIFT_VALUE
            ? NO_SCHEDULED_SHIFT_VALUE
            : filteredScheduledShifts.some((p_shift) => String(p_shift.id) === scheduledShiftId)
                ? scheduledShiftId
                : NO_SCHEDULED_SHIFT_VALUE;

    const handleEmployeeChange = (p_value: string): void => {
        setEmployeeProfileId(p_value);
        setScheduledShiftId(NO_SCHEDULED_SHIFT_VALUE);
    };

    const handleScheduledShiftChange = (p_value: string): void => {
        setScheduledShiftId(p_value);
        if (p_value === NO_SCHEDULED_SHIFT_VALUE) {
            return;
        }
        const selectedShift = allScheduledShifts.find((p_shift) => String(p_shift.id) === p_value);
        if (!selectedShift) {
            return;
        }
        setDate(selectedShift.date);
        setStartTime(normalizeTimeToHHmm(selectedShift.startTime));
        setEndTime(normalizeTimeToHHmm(selectedShift.endTime));
    };

    return (
        <FormModal
            open={showTimeEntryForm}
            onClose={handleClose}
            title={isEditMode ? "Modifier un pointage" : "Ajouter un pointage"}
            onSubmit={handleSubmit}
            isSubmitting={isEditMode ? isUpdatingTimeEntry : isAddingTimeEntry}
        >
            <FormControl fullWidth sx={{ mb: 2 }} required error={!!errors.employeeProfileId}>
                <InputLabel id="time-entry-employee-label">Employé</InputLabel>
                <Select
                    labelId="time-entry-employee-label"
                    label="Employé"
                    value={employeeProfileId}
                    onChange={(p_event: SelectChangeEvent<string>) =>
                        handleEmployeeChange(p_event.target.value)
                    }
                    disabled={employeesQuery.isLoading}
                >
                    {employees.map((p_employee: EmployeeProfile) => (
                        <MenuItem key={p_employee.id} value={String(p_employee.id)}>
                            {`${p_employee.firstName} ${p_employee.lastName}`}
                        </MenuItem>
                    ))}
                </Select>
            </FormControl>
            <FormControl fullWidth sx={{ mb: 2 }}>
                <InputLabel id="time-entry-shift-label">Quart planifié (optionnel)</InputLabel>
                <Select
                    labelId="time-entry-shift-label"
                    label="Quart planifié (optionnel)"
                    value={selectedShiftValue}
                    onChange={(p_event: SelectChangeEvent<string>) =>
                        handleScheduledShiftChange(p_event.target.value)
                    }
                    disabled={scheduledShiftsQuery.isLoading || !employeeProfileId}
                >
                    <MenuItem value={NO_SCHEDULED_SHIFT_VALUE}>Aucun quart lié</MenuItem>
                    {filteredScheduledShifts.map((p_shift: ScheduledShift) => (
                        <MenuItem key={p_shift.id} value={String(p_shift.id)}>
                            {formatScheduledShiftLabel(p_shift)}
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
                label="Heure de fin (optionnel)"
                value={endTime}
                onChange={setEndTime}
                error={!!errors.endTime}
                helperText={errors.endTime}
            />
        </FormModal>
    );
}
