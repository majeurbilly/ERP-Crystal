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
import employeeProfileService from "../../../api/services/hr/employeeProfileService";
import { employeeProfilesCacheKey } from "../../../data/cacheKeys";
import type { EmployeeProfile } from "../../../data/types/hr/employeeProfile";
import type {
    EmploymentContract,
    EmploymentContractFormData,
} from "../../../data/types/hr/employmentContract";
import {
    CONTRACT_TYPES,
    CONTRACT_TYPE_LABELS,
    WAGE_TYPES,
    WAGE_TYPE_LABELS,
    getBaseRateHelper,
    getBaseRateLabel,
    type ContractType,
    type WageType,
} from "../../../data/types/hr/employmentContract";
import { FormModal } from "../FormModal";
import { notifyErrorMessage, notifySuccessMessage } from "../../../data/utils/popupMessageManager";
import { useEmploymentContractMutations } from "../../../api/mutations/hr/useEmploymentContractMutations";
import { extractApiErrorMessage } from "../../../data/utils/extractApiErrorMessage";

const NO_END_DATE_VALUE: string = "";

const CONTRACT_TYPE_OPTIONS: ContractType[] = [
    CONTRACT_TYPES.FullTime,
    CONTRACT_TYPES.PartTime,
    CONTRACT_TYPES.SelfEmployed,
    CONTRACT_TYPES.Internship,
];

const WAGE_TYPE_OPTIONS: WageType[] = [WAGE_TYPES.Monthly, WAGE_TYPES.Fixed];

interface EmploymentContractFormProps {
    employeeProfileId?: number;
    showEmploymentContractForm: boolean;
    setShowEmploymentContractForm: (p_value: boolean) => void;
    editEmploymentContract: EmploymentContract | null;
    setEditEmploymentContract?: (p_value: EmploymentContract | null) => void;
}

interface EmploymentContractFormErrors {
    employeeProfileId: string;
    contractType: string;
    wageType: string;
    baseRate: string;
    startDate: string;
    endDate: string;
}

export default function EmploymentContractForm({
    employeeProfileId,
    showEmploymentContractForm,
    setShowEmploymentContractForm,
    editEmploymentContract,
    setEditEmploymentContract,
}: EmploymentContractFormProps) {
    const handleClose = (): void => setShowEmploymentContractForm(false);
    const {
        addEmploymentContract,
        isAddingEmploymentContract,
        updateEmploymentContract,
        isUpdatingEmploymentContract,
    } = useEmploymentContractMutations(
        employeeProfileId !== undefined ? String(employeeProfileId) : undefined
    );

    const isEditMode: boolean = editEmploymentContract !== null;
    const requiresEmployeeSelection: boolean = employeeProfileId === undefined && !isEditMode;

    const [selectedEmployeeId, setSelectedEmployeeId] = useState<string>("");
    const [contractType, setContractType] = useState<ContractType>(CONTRACT_TYPES.FullTime);
    const [wageType, setWageType] = useState<WageType>(WAGE_TYPES.Monthly);
    const [baseRate, setBaseRate] = useState<string>("");
    const [startDate, setStartDate] = useState<string>("");
    const [endDate, setEndDate] = useState<string>(NO_END_DATE_VALUE);
    const [errors, setErrors] = useState<EmploymentContractFormErrors>({
        employeeProfileId: "",
        contractType: "",
        wageType: "",
        baseRate: "",
        startDate: "",
        endDate: "",
    });

    const employeesQuery = useQuery<EmployeeProfile[], Error>({
        queryKey: employeeProfilesCacheKey.list(),
        queryFn: () => employeeProfileService.getAll(),
        enabled: showEmploymentContractForm && requiresEmployeeSelection,
    });

    useEffect(() => {
        if (showEmploymentContractForm) {
            if (editEmploymentContract) {
                setSelectedEmployeeId(String(editEmploymentContract.employeeProfileId));
                setContractType(editEmploymentContract.contractType);
                setWageType(editEmploymentContract.wageType);
                setBaseRate(String(editEmploymentContract.baseRate));
                setStartDate(editEmploymentContract.startDate);
                setEndDate(editEmploymentContract.endDate ?? NO_END_DATE_VALUE);
            } else {
                setSelectedEmployeeId(employeeProfileId !== undefined ? String(employeeProfileId) : "");
                setContractType(CONTRACT_TYPES.FullTime);
                setWageType(WAGE_TYPES.Monthly);
                setBaseRate("");
                setStartDate("");
                setEndDate(NO_END_DATE_VALUE);
            }
            setErrors({
                employeeProfileId: "",
                contractType: "",
                wageType: "",
                baseRate: "",
                startDate: "",
                endDate: "",
            });
        }
    }, [editEmploymentContract, employeeProfileId, showEmploymentContractForm]);

    const validate = (): boolean => {
        let isValid: boolean = true;
        const newErrors: EmploymentContractFormErrors = {
            employeeProfileId: "",
            contractType: "",
            wageType: "",
            baseRate: "",
            startDate: "",
            endDate: "",
        };

        const resolvedEmployeeId: number = employeeProfileId ?? Number(selectedEmployeeId);
        if (!resolvedEmployeeId || Number.isNaN(resolvedEmployeeId)) {
            newErrors.employeeProfileId = "L'employé est requis.";
            isValid = false;
        }

        if (!startDate) {
            newErrors.startDate = "La date de début est requise.";
            isValid = false;
        }

        if (endDate && startDate && endDate < startDate) {
            newErrors.endDate = "La date de fin doit être postérieure ou égale à la date de début.";
            isValid = false;
        }

        const parsedBaseRate: number = Number(baseRate);
        if (!baseRate.trim() || Number.isNaN(parsedBaseRate) || parsedBaseRate < 0) {
            newErrors.baseRate = "Le taux de base doit être un nombre positif ou nul.";
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

        const resolvedEmployeeId: number = employeeProfileId ?? Number(selectedEmployeeId);

        const formData: EmploymentContractFormData = {
            employeeProfileId: resolvedEmployeeId,
            contractType,
            wageType,
            baseRate: Number(baseRate),
            startDate,
            endDate: endDate === NO_END_DATE_VALUE ? null : endDate,
        };

        try {
            if (isEditMode && editEmploymentContract) {
                await updateEmploymentContract({
                    id: String(editEmploymentContract.id),
                    data: formData,
                });
                notifySuccessMessage("Le contrat a été modifié avec succès.");
                if (setEditEmploymentContract) {
                    setEditEmploymentContract(null);
                }
            } else {
                await addEmploymentContract(formData);
                notifySuccessMessage("Le contrat a été ajouté avec succès.");
            }
            handleClose();
        } catch (error: unknown) {
            notifyErrorMessage(extractApiErrorMessage(error));
        }
    };

    return (
        <FormModal
            open={showEmploymentContractForm}
            onClose={handleClose}
            title={isEditMode ? "Modifier un contrat" : "Ajouter un contrat"}
            onSubmit={handleSubmit}
            isSubmitting={isEditMode ? isUpdatingEmploymentContract : isAddingEmploymentContract}
        >
            {requiresEmployeeSelection && (
                <FormControl fullWidth sx={{ mb: 2 }} required error={!!errors.employeeProfileId}>
                    <InputLabel id="contract-employee-label">Employé</InputLabel>
                    <Select
                        labelId="contract-employee-label"
                        label="Employé"
                        value={selectedEmployeeId}
                        onChange={(p_event: SelectChangeEvent<string>) =>
                            setSelectedEmployeeId(p_event.target.value)
                        }
                        disabled={employeesQuery.isLoading}
                    >
                        {(employeesQuery.data ?? []).map((p_employee: EmployeeProfile) => (
                            <MenuItem key={p_employee.id} value={String(p_employee.id)}>
                                {`${p_employee.firstName} ${p_employee.lastName}`}
                            </MenuItem>
                        ))}
                    </Select>
                </FormControl>
            )}
            <FormControl fullWidth sx={{ mb: 2 }} required error={!!errors.contractType}>
                <InputLabel id="contract-type-label">Type de contrat</InputLabel>
                <Select
                    labelId="contract-type-label"
                    label="Type de contrat"
                    value={contractType}
                    onChange={(p_event: SelectChangeEvent<ContractType>) =>
                        setContractType(p_event.target.value as ContractType)
                    }
                >
                    {CONTRACT_TYPE_OPTIONS.map((p_option: ContractType) => (
                        <MenuItem key={p_option} value={p_option}>
                            {CONTRACT_TYPE_LABELS[p_option]}
                        </MenuItem>
                    ))}
                </Select>
            </FormControl>
            <FormControl fullWidth sx={{ mb: 2 }} required error={!!errors.wageType}>
                <InputLabel id="wage-type-label">Type de rémunération</InputLabel>
                <Select
                    labelId="wage-type-label"
                    label="Type de rémunération"
                    value={wageType}
                    onChange={(p_event: SelectChangeEvent<WageType>) =>
                        setWageType(p_event.target.value as WageType)
                    }
                >
                    {WAGE_TYPE_OPTIONS.map((p_option: WageType) => (
                        <MenuItem key={p_option} value={p_option}>
                            {WAGE_TYPE_LABELS[p_option]}
                        </MenuItem>
                    ))}
                </Select>
            </FormControl>
            <TextField
                fullWidth
                label={getBaseRateLabel(wageType)}
                type="number"
                inputProps={{ min: 0, step: "0.01" }}
                value={baseRate}
                onChange={(p_event) => setBaseRate(p_event.target.value)}
                sx={{ mb: 2 }}
                required
                error={!!errors.baseRate}
                helperText={errors.baseRate || getBaseRateHelper(wageType)}
            />
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
                label="Date de fin (optionnel)"
                type="date"
                value={endDate}
                onChange={(p_event) => setEndDate(p_event.target.value)}
                InputLabelProps={{ shrink: true }}
                sx={{ mb: 2 }}
                error={!!errors.endDate}
                helperText={errors.endDate || "Laissez vide pour un contrat sans date de fin."}
            />
        </FormModal>
    );
}
