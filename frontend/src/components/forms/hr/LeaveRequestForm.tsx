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
import type { LeaveRequestFormData, LeaveType } from "../../../data/types/hr/leaveRequest";
import { LEAVE_TYPES } from "../../../data/types/hr/leaveRequest";
import { FormModal } from "../FormModal";
import { notifyErrorMessage, notifySuccessMessage } from "../../../data/utils/popupMessageManager";
import { useLeaveRequestMutations } from "../../../api/mutations/hr/useLeaveRequestMutations";
import { extractApiErrorMessage } from "../../../data/utils/extractApiErrorMessage";
import employeeProfileService from "../../../api/services/hr/employeeProfileService";
import { employeeProfilesCacheKey } from "../../../data/cacheKeys";
import type { EmployeeProfile } from "../../../data/types/hr/employeeProfile";

interface LeaveRequestFormProps {
    showLeaveRequestForm: boolean;
    setShowLeaveRequestForm: (p_value: boolean) => void;
    selfMode?: boolean;
    defaultEmployeeProfileId?: number;
}

interface LeaveRequestFormErrors {
    employeeProfileId: string;
    leaveType: string;
    startDate: string;
    endDate: string;
}

const LEAVE_TYPE_OPTIONS: LeaveType[] = [
    LEAVE_TYPES.Vacation,
    LEAVE_TYPES.Sick,
    LEAVE_TYPES.Unpaid,
    LEAVE_TYPES.Other,
];

const LEAVE_TYPE_LABELS: Record<LeaveType, string> = {
    [LEAVE_TYPES.Vacation]: "Vacances",
    [LEAVE_TYPES.Sick]: "Maladie",
    [LEAVE_TYPES.Unpaid]: "Sans solde",
    [LEAVE_TYPES.Other]: "Autre",
};

export default function LeaveRequestForm({
    showLeaveRequestForm,
    setShowLeaveRequestForm,
    selfMode = false,
    defaultEmployeeProfileId,
}: LeaveRequestFormProps) {
    const handleClose = (): void => setShowLeaveRequestForm(false);
    const { addLeaveRequest, isAddingLeaveRequest } = useLeaveRequestMutations();

    const [employeeProfileId, setEmployeeProfileId] = useState<string>("");
    const [leaveType, setLeaveType] = useState<LeaveType>(LEAVE_TYPES.Vacation);
    const [startDate, setStartDate] = useState<string>("");
    const [endDate, setEndDate] = useState<string>("");
    const [reason, setReason] = useState<string>("");
    const [errors, setErrors] = useState<LeaveRequestFormErrors>({
        employeeProfileId: "",
        leaveType: "",
        startDate: "",
        endDate: "",
    });

    const employeesQuery = useQuery<EmployeeProfile[], Error>({
        queryKey: employeeProfilesCacheKey.list(),
        queryFn: () => employeeProfileService.getAll(),
        enabled: showLeaveRequestForm && !selfMode,
    });

    useEffect(() => {
        if (showLeaveRequestForm) {
            if (selfMode && defaultEmployeeProfileId) {
                setEmployeeProfileId(String(defaultEmployeeProfileId));
            } else {
                setEmployeeProfileId("");
            }
            setLeaveType(LEAVE_TYPES.Vacation);
            setStartDate("");
            setEndDate("");
            setReason("");
            setErrors({
                employeeProfileId: "",
                leaveType: "",
                startDate: "",
                endDate: "",
            });
        }
    }, [showLeaveRequestForm, selfMode, defaultEmployeeProfileId]);

    const validate = (): boolean => {
        let isValid: boolean = true;
        const newErrors: LeaveRequestFormErrors = {
            employeeProfileId: "",
            leaveType: "",
            startDate: "",
            endDate: "",
        };

        if (!employeeProfileId) {
            newErrors.employeeProfileId = "L'employé est requis.";
            isValid = false;
        }

        if (!startDate) {
            newErrors.startDate = "La date de début est requise.";
            isValid = false;
        }

        if (!endDate) {
            newErrors.endDate = "La date de fin est requise.";
            isValid = false;
        }

        if (startDate && endDate && endDate < startDate) {
            newErrors.endDate = "La date de fin doit être postérieure ou égale à la date de début.";
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

        const formData: LeaveRequestFormData = {
            employeeProfileId: Number(employeeProfileId),
            leaveType,
            startDate,
            endDate,
            reason: reason.trim().length > 0 ? reason.trim() : null,
        };

        try {
            await addLeaveRequest(formData);
            notifySuccessMessage("La demande de congé a été ajoutée avec succès.");
            handleClose();
        } catch (error: unknown) {
            notifyErrorMessage(extractApiErrorMessage(error));
        }
    };

    const employees: EmployeeProfile[] = employeesQuery.data ?? [];

    return (
        <FormModal
            open={showLeaveRequestForm}
            onClose={handleClose}
            title="Ajouter une demande de congé"
            onSubmit={handleSubmit}
            isSubmitting={isAddingLeaveRequest}
        >
            {!selfMode && (
                <FormControl fullWidth sx={{ mb: 2 }} required error={!!errors.employeeProfileId}>
                    <InputLabel id="leave-employee-label">Employé</InputLabel>
                    <Select
                        labelId="leave-employee-label"
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
            )}
            <FormControl fullWidth sx={{ mb: 2 }} required error={!!errors.leaveType}>
                <InputLabel id="leave-type-label">Type de congé</InputLabel>
                <Select
                    labelId="leave-type-label"
                    label="Type de congé"
                    value={leaveType}
                    onChange={(p_event: SelectChangeEvent<LeaveType>) =>
                        setLeaveType(p_event.target.value as LeaveType)
                    }
                >
                    {LEAVE_TYPE_OPTIONS.map((p_option: LeaveType) => (
                        <MenuItem key={p_option} value={p_option}>
                            {LEAVE_TYPE_LABELS[p_option]}
                        </MenuItem>
                    ))}
                </Select>
            </FormControl>
            <TextField
                fullWidth
                label="Date de début"
                type="date"
                value={startDate}
                onChange={(p_event) => setStartDate(p_event.target.value)}
                InputLabelProps={{ shrink: true }}
                sx={{ mb: 2 }}
                required
                error={!!errors.startDate}
                helperText={errors.startDate}
            />
            <TextField
                fullWidth
                label="Date de fin"
                type="date"
                value={endDate}
                onChange={(p_event) => setEndDate(p_event.target.value)}
                InputLabelProps={{ shrink: true }}
                sx={{ mb: 2 }}
                required
                error={!!errors.endDate}
                helperText={errors.endDate}
            />
            <TextField
                fullWidth
                label="Motif (optionnel)"
                value={reason}
                onChange={(p_event) => setReason(p_event.target.value)}
                rows={3}
                multiline
                sx={{ mb: 2 }}
            />
        </FormModal>
    );
}
