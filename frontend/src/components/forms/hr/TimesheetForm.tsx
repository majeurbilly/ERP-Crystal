import { useEffect, useState } from "react";
import {
    FormControl,
    InputLabel,
    MenuItem,
    Select,
    TextField,
    type SelectChangeEvent,
} from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import type { TimesheetFormData } from "../../../data/types/hr/timesheet";
import { FormModal } from "../FormModal";
import { notifyErrorMessage, notifySuccessMessage } from "../../../data/utils/popupMessageManager";
import { useTimesheetMutations } from "../../../api/mutations/hr/useTimesheetMutations";
import { extractApiErrorMessage } from "../../../data/utils/extractApiErrorMessage";
import employeeProfileService from "../../../api/services/hr/employeeProfileService";
import { employeeProfilesCacheKey } from "../../../data/cacheKeys";
import type { EmployeeProfile } from "../../../data/types/hr/employeeProfile";

interface TimesheetFormProps {
    showTimesheetForm: boolean;
    setShowTimesheetForm: (p_value: boolean) => void;
}

interface TimesheetFormErrors {
    employeeProfileId: string;
    periodStart: string;
    periodEnd: string;
}

export default function TimesheetForm({
    showTimesheetForm,
    setShowTimesheetForm,
}: TimesheetFormProps) {
    const handleClose = (): void => setShowTimesheetForm(false);
    const { addTimesheet, isAddingTimesheet } = useTimesheetMutations();

    const [employeeProfileId, setEmployeeProfileId] = useState<string>("");
    const [periodStart, setPeriodStart] = useState<string>("");
    const [periodEnd, setPeriodEnd] = useState<string>("");
    const [errors, setErrors] = useState<TimesheetFormErrors>({
        employeeProfileId: "",
        periodStart: "",
        periodEnd: "",
    });

    const employeesQuery = useQuery<EmployeeProfile[], Error>({
        queryKey: employeeProfilesCacheKey.list(),
        queryFn: () => employeeProfileService.getAll(),
        enabled: showTimesheetForm,
    });

    useEffect(() => {
        if (showTimesheetForm) {
            setEmployeeProfileId("");
            setPeriodStart("");
            setPeriodEnd("");
            setErrors({
                employeeProfileId: "",
                periodStart: "",
                periodEnd: "",
            });
        }
    }, [showTimesheetForm]);

    const validate = (): boolean => {
        let isValid: boolean = true;
        const newErrors: TimesheetFormErrors = {
            employeeProfileId: "",
            periodStart: "",
            periodEnd: "",
        };

        if (!employeeProfileId) {
            newErrors.employeeProfileId = "L'employé est requis.";
            isValid = false;
        }

        if (!periodStart) {
            newErrors.periodStart = "La date de début est requise.";
            isValid = false;
        }

        if (!periodEnd) {
            newErrors.periodEnd = "La date de fin est requise.";
            isValid = false;
        }

        if (periodStart && periodEnd && periodEnd < periodStart) {
            newErrors.periodEnd =
                "La date de fin doit être postérieure ou égale à la date de début.";
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

        const formData: TimesheetFormData = {
            employeeProfileId: Number(employeeProfileId),
            periodStart,
            periodEnd,
            timeEntryIds: [],
        };

        try {
            await addTimesheet(formData);
            notifySuccessMessage("La feuille de temps a été ajoutée avec succès.");
            handleClose();
        } catch (error: unknown) {
            notifyErrorMessage(extractApiErrorMessage(error));
        }
    };

    const employees: EmployeeProfile[] = employeesQuery.data ?? [];

    return (
        <FormModal
            open={showTimesheetForm}
            onClose={handleClose}
            title="Ajouter une feuille de temps"
            onSubmit={handleSubmit}
            isSubmitting={isAddingTimesheet}
        >
            <FormControl fullWidth sx={{ mb: 2 }} required error={!!errors.employeeProfileId}>
                <InputLabel id="timesheet-employee-label">Employé</InputLabel>
                <Select
                    labelId="timesheet-employee-label"
                    label="Employé"
                    value={employeeProfileId}
                    onChange={(p_event: SelectChangeEvent<string>) =>
                        setEmployeeProfileId(p_event.target.value)
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
            <TextField
                fullWidth
                label="Début de période"
                type="date"
                value={periodStart}
                onChange={(p_event) => setPeriodStart(p_event.target.value)}
                InputLabelProps={{ shrink: true }}
                sx={{ mb: 2 }}
                required
                error={!!errors.periodStart}
                helperText={errors.periodStart}
            />
            <TextField
                fullWidth
                label="Fin de période"
                type="date"
                value={periodEnd}
                onChange={(p_event) => setPeriodEnd(p_event.target.value)}
                InputLabelProps={{ shrink: true }}
                sx={{ mb: 2 }}
                required
                error={!!errors.periodEnd}
                helperText={errors.periodEnd}
            />
        </FormModal>
    );
}
