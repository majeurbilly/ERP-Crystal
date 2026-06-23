import { useEffect, useState } from "react";
import {
    Alert,
    Divider,
    FormControl,
    FormControlLabel,
    FormHelperText,
    InputLabel,
    MenuItem,
    Radio,
    RadioGroup,
    Select,
    Switch,
    TextField,
    Typography,
    type SelectChangeEvent,
} from "@mui/material";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import WizardModal from "./WizardModal";
import ColorPalettePicker from "../../forms/ColorPalettePicker";
import { DEFAULT_JOB_POSITION_COLOR } from "../../../data/types/hr/jobPositionColors";
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

import {
    DEFAULT_ASSIGNED_ROLE_ID,
    getAssignedRoleDisplayName,
    userAccessFieldHelpers,
    userAccessFieldLabels,
} from "../../../data/types/hr/userRoles";

import type { JobPosition } from "../../../data/types/hr/jobPosition";
import type { Location } from "../../../data/types/inventory/location";
import type { DynamicUserRole } from "../../../data/types/hr/dynamicUserRole";
import jobPositionService from "../../../api/services/hr/jobPositionService";
import locationService from "../../../api/services/inventory/locationService";
import userService from "../../../api/services/hr/userService";
import userRoleService from "../../../api/services/hr/userRoleService";
import employeeProfileService from "../../../api/services/hr/employeeProfileService";
import employmentContractService from "../../../api/services/hr/employmentContractService";
import {
    employeeProfilesCacheKey,
    employmentContractsCacheKey,
    hrMetricsCacheKey,
    jobPositionsCacheKey,
    locationsCacheKey,
    usersCacheKey,
} from "../../../data/cacheKeys";
import { notifyErrorMessage, notifySuccessMessage } from "../../../data/utils/popupMessageManager";
import { extractApiErrorMessage } from "../../../data/utils/extractApiErrorMessage";
import { usePermissions } from "../../../permissions/usePermissions";
import { ENTITY_TYPES } from "../../../permissions/permissions";

const WIZARD_STEPS: string[] = ["Poste", "Profil", "Accès", "Contrat", "Confirmation"];

const NO_LOCATION_VALUE = "__none_location__";
const NO_END_DATE_VALUE = "";
const EMPLOYEE_STATUS_OPTIONS: Array<{ value: string; label: string }> = [
    { value: "Active", label: "Actif" },
    { value: "Inactive", label: "Inactif" },
    { value: "OnLeave", label: "En congé" },
];

type JobPositionMode = "existing" | "new";

interface EmployeeOnboardingWizardProps {
    open: boolean;
    onClose: () => void;
}

export default function EmployeeOnboardingWizard({ open, onClose }: EmployeeOnboardingWizardProps) {
    const queryClient = useQueryClient();
    const { canCreate: canModifyRole } = usePermissions(ENTITY_TYPES.USER);

    const [activeStep, setActiveStep] = useState<number>(0);
    const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
    const [stepError, setStepError] = useState<string>("");

    const [jobPositionMode, setJobPositionMode] = useState<JobPositionMode>("existing");
    const [selectedJobPositionId, setSelectedJobPositionId] = useState<string>("");
    const [newPositionName, setNewPositionName] = useState<string>("");
    const [newPositionDescription, setNewPositionDescription] = useState<string>("");
    const [newPositionColor, setNewPositionColor] = useState<string>(DEFAULT_JOB_POSITION_COLOR);

    const [firstName, setFirstName] = useState<string>("");
    const [lastName, setLastName] = useState<string>("");
    const [email, setEmail] = useState<string>("");
    const [hiringDate, setHiringDate] = useState<string>("");
    const [salary, setSalary] = useState<string>("");
    const [status, setStatus] = useState<string>("Active");
    const [locationId, setLocationId] = useState<string>(NO_LOCATION_VALUE);

    const [createUserAccount, setCreateUserAccount] = useState<boolean>(true);
    const [userName, setUserName] = useState<string>("");
    const [password, setPassword] = useState<string>("");
    const [assignedRoleId, setAssignedRoleId] = useState<string>(DEFAULT_ASSIGNED_ROLE_ID);

    const [addContract, setAddContract] = useState<boolean>(true);
    const [contractType, setContractType] = useState<ContractType>(CONTRACT_TYPES.FullTime);
    const [wageType, setWageType] = useState<WageType>(WAGE_TYPES.Monthly);
    const [baseRate, setBaseRate] = useState<string>("");
    const [contractStartDate, setContractStartDate] = useState<string>("");
    const [contractEndDate, setContractEndDate] = useState<string>(NO_END_DATE_VALUE);

    const jobPositionsQuery = useQuery<JobPosition[], Error>({
        queryKey: jobPositionsCacheKey.list(),
        queryFn: () => jobPositionService.getAll(),
        enabled: open,
    });

    const locationsQuery = useQuery<Location[], Error>({
        queryKey: locationsCacheKey.list(),
        queryFn: () => locationService.getAll(),
        enabled: open,
    });

    const dynamicRolesQuery = useQuery<DynamicUserRole[], Error>({
        queryKey: ["userRoles", "list"],
        queryFn: () => userRoleService.getAll(),
        enabled: open && createUserAccount,
    });

    const jobPositions: JobPosition[] = jobPositionsQuery.data ?? [];
    const locations: Location[] = locationsQuery.data ?? [];

    useEffect(() => {
        if (!open) {
            return;
        }

        setActiveStep(0);
        setIsSubmitting(false);
        setStepError("");
        setNewPositionName("");
        setNewPositionDescription("");
        setNewPositionColor(DEFAULT_JOB_POSITION_COLOR);
        setFirstName("");
        setLastName("");
        setEmail("");
        setHiringDate("");
        setSalary("");
        setStatus("Active");
        setLocationId(NO_LOCATION_VALUE);
        setCreateUserAccount(true);
        setUserName("");
        setPassword("");
        setAssignedRoleId(DEFAULT_ASSIGNED_ROLE_ID);
        setAddContract(true);
        setContractType(CONTRACT_TYPES.FullTime);
        setWageType(WAGE_TYPES.Monthly);
        setBaseRate("");
        setContractStartDate("");
        setContractEndDate(NO_END_DATE_VALUE);
    }, [open]);

    useEffect(() => {
        if (!open || jobPositionsQuery.isLoading) {
            return;
        }

        if (jobPositions.length === 0) {
            setJobPositionMode("new");
            setSelectedJobPositionId("");
            return;
        }

        setJobPositionMode("existing");
        setSelectedJobPositionId(String(jobPositions[0].id));
    }, [open, jobPositions, jobPositionsQuery.isLoading]);

    useEffect(() => {
        if (email.trim() && !userName.trim()) {
            const localPart: string = email.trim().split("@")[0] ?? "";
            setUserName(localPart.replace(/[^a-zA-Z0-9._-]/g, "").slice(0, 32));
        }
    }, [email, userName]);

    useEffect(() => {
        if (hiringDate && !contractStartDate) {
            setContractStartDate(hiringDate);
        }
    }, [hiringDate, contractStartDate]);

    const validateStep = (p_step: number): boolean => {
        setStepError("");

        if (p_step === 0) {
            if (jobPositionMode === "existing") {
                if (!selectedJobPositionId) {
                    setStepError("Sélectionnez un poste existant.");
                    return false;
                }
            } else {
                if (!newPositionName.trim()) {
                    setStepError("Le nom du poste est requis.");
                    return false;
                }
                if (!newPositionDescription.trim()) {
                    setStepError("La description du poste est requise.");
                    return false;
                }
            }
        }

        if (p_step === 1) {
            if (!firstName.trim() || !lastName.trim()) {
                setStepError("Le prénom et le nom sont requis.");
                return false;
            }
            if (!email.trim() || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())) {
                setStepError("Un courriel valide est requis.");
                return false;
            }
            if (!hiringDate) {
                setStepError("La date d'embauche est requise.");
                return false;
            }
            const parsedSalary: number = Number(salary);
            if (!salary.trim() || Number.isNaN(parsedSalary) || parsedSalary < 0) {
                setStepError("Le salaire de référence doit être un nombre positif ou nul.");
                return false;
            }
        }

        if (p_step === 2 && createUserAccount) {
            if (!userName.trim() || userName.trim().length < 3) {
                setStepError("Le nom d'utilisateur doit contenir au moins 3 caractères.");
                return false;
            }
            if (!password.trim() || password.trim().length < 8) {
                setStepError("Le mot de passe doit contenir au moins 8 caractères.");
                return false;
            }
        }

        if (p_step === 3 && addContract) {
            if (!contractStartDate) {
                setStepError("La date de début du contrat est requise.");
                return false;
            }
            if (contractEndDate && contractEndDate < contractStartDate) {
                setStepError("La date de fin du contrat doit être postérieure ou égale à la date de début.");
                return false;
            }
            const parsedBaseRate: number = Number(baseRate);
            if (!baseRate.trim() || Number.isNaN(parsedBaseRate) || parsedBaseRate < 0) {
                setStepError("Le montant de rémunération doit être un nombre positif ou nul.");
                return false;
            }
        }

        return true;
    };

    const handleNext = async (): Promise<void> => {
        if (!validateStep(activeStep)) {
            return;
        }

        if (activeStep < WIZARD_STEPS.length - 1) {
            setActiveStep((p_prev) => p_prev + 1);
            return;
        }

        await handleFinish();
    };

    const handleFinish = async (): Promise<void> => {
        setIsSubmitting(true);
        try {
            let jobPositionId: number;
            if (jobPositionMode === "new") {
                const createdPosition = await jobPositionService.add({
                    name: newPositionName.trim(),
                    description: newPositionDescription.trim(),
                    color: newPositionColor,
                });
                jobPositionId = createdPosition.id;
            } else {
                jobPositionId = Number(selectedJobPositionId);
            }

            let applicationUserId: string | null = null;
            if (createUserAccount) {
                const createdUser = await userService.add({
                    id: "",
                    userName: userName.trim(),
                    email: email.trim(),
                    password: password.trim(),
                    dynamicRoleId: canModifyRole ? assignedRoleId : DEFAULT_ASSIGNED_ROLE_ID,
                });
                applicationUserId = createdUser.id;
            }

            const createdEmployee = await employeeProfileService.add({
                firstName: firstName.trim(),
                lastName: lastName.trim(),
                email: email.trim(),
                hiringDate,
                salary: Number(salary),
                status,
                jobPositionId,
                applicationUserId,
                locationId: locationId === NO_LOCATION_VALUE ? undefined : Number(locationId),
            });

            if (addContract) {
                await employmentContractService.add({
                    employeeProfileId: createdEmployee.id,
                    contractType,
                    wageType,
                    baseRate: Number(baseRate),
                    startDate: contractStartDate,
                    endDate: contractEndDate === NO_END_DATE_VALUE ? null : contractEndDate,
                });
            }

            await Promise.all([
                queryClient.invalidateQueries({ queryKey: jobPositionsCacheKey.list() }),
                queryClient.invalidateQueries({ queryKey: usersCacheKey.list() }),
                queryClient.invalidateQueries({ queryKey: employeeProfilesCacheKey.list() }),
                queryClient.invalidateQueries({ queryKey: employmentContractsCacheKey.list() }),
                queryClient.invalidateQueries({ queryKey: hrMetricsCacheKey.dashboard() }),
            ]);

            notifySuccessMessage(
                `L'employé « ${createdEmployee.firstName} ${createdEmployee.lastName} » a été créé avec succès.`
            );
            onClose();
        } catch (error: unknown) {
            notifyErrorMessage(extractApiErrorMessage(error));
        } finally {
            setIsSubmitting(false);
        }
    };

    const selectedPositionLabel: string =
        jobPositionMode === "new"
            ? newPositionName.trim() || "Nouveau poste"
            : jobPositions.find((p) => String(p.id) === selectedJobPositionId)?.name ?? "â€”";

    const selectedLocationLabel: string =
        locationId === NO_LOCATION_VALUE
            ? "Aucune"
            : locations.find((p) => String(p.id) === locationId)?.title ?? "â€”";

    const renderStepContent = (): React.ReactNode => {
        switch (activeStep) {
            case 0:
                return (
                    <>
                        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                            Choisissez le poste occupé par cet employé. Vous pouvez en créer un nouveau si
                            nécessaire.
                        </Typography>
                        <FormControl component="fieldset" sx={{ mb: 2 }}>
                            <RadioGroup
                                value={jobPositionMode}
                                onChange={(p_event) =>
                                    setJobPositionMode(p_event.target.value as JobPositionMode)
                                }
                            >
                                <FormControlLabel
                                    value="existing"
                                    control={<Radio />}
                                    label="Utiliser un poste existant"
                                    disabled={jobPositions.length === 0}
                                />
                                <FormControlLabel
                                    value="new"
                                    control={<Radio />}
                                    label="Créer un nouveau poste"
                                />
                            </RadioGroup>
                        </FormControl>
                        {jobPositionMode === "existing" ? (
                            <FormControl fullWidth sx={{ mb: 2 }} required>
                                <InputLabel id="onboarding-position-label">Poste</InputLabel>
                                <Select
                                    labelId="onboarding-position-label"
                                    label="Poste"
                                    value={selectedJobPositionId}
                                    onChange={(p_event: SelectChangeEvent<string>) =>
                                        setSelectedJobPositionId(p_event.target.value)
                                    }
                                    disabled={jobPositionsQuery.isLoading}
                                >
                                    {jobPositions.map((p_position: JobPosition) => (
                                        <MenuItem key={p_position.id} value={String(p_position.id)}>
                                            {p_position.name}
                                        </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                        ) : (
                            <>
                                <TextField
                                    fullWidth
                                    label="Nom du poste"
                                    value={newPositionName}
                                    onChange={(p_event) => setNewPositionName(p_event.target.value)}
                                    sx={{ mb: 2 }}
                                    required
                                />
                                <TextField
                                    fullWidth
                                    label="Description"
                                    value={newPositionDescription}
                                    onChange={(p_event) => setNewPositionDescription(p_event.target.value)}
                                    sx={{ mb: 2 }}
                                    required
                                />
                                <ColorPalettePicker
                                    value={newPositionColor}
                                    onChange={setNewPositionColor}
                                />
                            </>
                        )}
                    </>
                );
            case 1:
                return (
                    <>
                        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                            Renseignez les informations du dossier employé.
                        </Typography>
                        <TextField
                            fullWidth
                            label="Prénom"
                            value={firstName}
                            onChange={(p_event) => setFirstName(p_event.target.value)}
                            sx={{ mb: 2 }}
                            required
                        />
                        <TextField
                            fullWidth
                            label="Nom"
                            value={lastName}
                            onChange={(p_event) => setLastName(p_event.target.value)}
                            sx={{ mb: 2 }}
                            required
                        />
                        <TextField
                            fullWidth
                            label="Courriel"
                            type="email"
                            value={email}
                            onChange={(p_event) => setEmail(p_event.target.value)}
                            sx={{ mb: 2 }}
                            required
                        />
                        <TextField
                            fullWidth
                            label="Date d'embauche"
                            type="date"
                            value={hiringDate}
                            onChange={(p_event) => setHiringDate(p_event.target.value)}
                            InputLabelProps={{ shrink: true }}
                            sx={{ mb: 2 }}
                            required
                        />
                        <TextField
                            fullWidth
                            label="Salaire de référence"
                            type="number"
                            inputProps={{ min: 0, step: "0.01" }}
                            value={salary}
                            onChange={(p_event) => setSalary(p_event.target.value)}
                            helperText="Montant indicatif dans le dossier employé (distinct du contrat)."
                            sx={{ mb: 2 }}
                            required
                        />
                        <FormControl fullWidth sx={{ mb: 2 }} required>
                            <InputLabel id="onboarding-status-label">Statut</InputLabel>
                            <Select
                                labelId="onboarding-status-label"
                                label="Statut"
                                value={status}
                                onChange={(p_event: SelectChangeEvent<string>) =>
                                    setStatus(p_event.target.value)
                                }
                            >
                                {EMPLOYEE_STATUS_OPTIONS.map((p_option) => (
                                    <MenuItem key={p_option.value} value={p_option.value}>
                                        {p_option.label}
                                    </MenuItem>
                                ))}
                            </Select>
                        </FormControl>
                        <FormControl fullWidth sx={{ mb: 2 }}>
                            <InputLabel id="onboarding-location-label">Succursale (optionnel)</InputLabel>
                            <Select
                                labelId="onboarding-location-label"
                                label="Succursale (optionnel)"
                                value={locationId}
                                onChange={(p_event: SelectChangeEvent<string>) =>
                                    setLocationId(p_event.target.value)
                                }
                                disabled={locationsQuery.isLoading}
                            >
                                <MenuItem value={NO_LOCATION_VALUE}>Aucune succursale</MenuItem>
                                {locations.map((p_location: Location) => (
                                    <MenuItem key={p_location.id} value={String(p_location.id)}>
                                        {p_location.title}
                                    </MenuItem>
                                ))}
                            </Select>
                        </FormControl>
                    </>
                );
            case 2:
                return (
                    <>
                        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                            Liez un compte utilisateur pour permettre la connexion à l'application.
                        </Typography>
                        <FormControlLabel
                            control={
                                <Switch
                                    checked={createUserAccount}
                                    onChange={(p_event) => setCreateUserAccount(p_event.target.checked)}
                                />
                            }
                            label="Créer un compte utilisateur"
                            sx={{ mb: 2, display: "block" }}
                        />
                        {createUserAccount ? (
                            <>
                                <TextField
                                    fullWidth
                                    label="Nom d'utilisateur"
                                    value={userName}
                                    onChange={(p_event) => setUserName(p_event.target.value)}
                                    sx={{ mb: 2 }}
                                    required
                                />
                                <TextField
                                    fullWidth
                                    label="Mot de passe"
                                    type="password"
                                    value={password}
                                    onChange={(p_event) => setPassword(p_event.target.value)}
                                    sx={{ mb: 2 }}
                                    required
                                />
                                <FormControl fullWidth sx={{ mb: 2 }}>
                                    <InputLabel id="onboarding-assigned-role-label">
                                        {userAccessFieldLabels.assignedRole}
                                    </InputLabel>
                                    <Select
                                        labelId="onboarding-assigned-role-label"
                                        label={userAccessFieldLabels.assignedRole}
                                        value={assignedRoleId}
                                        onChange={(p_event: SelectChangeEvent<string>) =>
                                            setAssignedRoleId(p_event.target.value)
                                        }
                                        disabled={!canModifyRole}
                                    >
                                        {(dynamicRolesQuery.data ?? []).map((p_dynamicRole) => (
                                            <MenuItem key={p_dynamicRole.id} value={p_dynamicRole.id}>
                                                {p_dynamicRole.name}
                                            </MenuItem>
                                        ))}
                                    </Select>
                                    <FormHelperText>{userAccessFieldHelpers.assignedRole}</FormHelperText>
                                </FormControl>
                            </>
                        ) : (
                            <Alert severity="info">
                                L'employé sera créé sans compte de connexion. Vous pourrez en ajouter un
                                plus tard depuis la fiche employé.
                            </Alert>
                        )}
                    </>
                );
            case 3:
                return (
                    <>
                        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                            Définissez les conditions de rémunération de l'employé.
                        </Typography>
                        <FormControlLabel
                            control={
                                <Switch
                                    checked={addContract}
                                    onChange={(p_event) => setAddContract(p_event.target.checked)}
                                />
                            }
                            label="Ajouter un contrat de travail"
                            sx={{ mb: 2, display: "block" }}
                        />
                        {addContract ? (
                            <>
                                <FormControl fullWidth sx={{ mb: 2 }} required>
                                    <InputLabel id="onboarding-contract-type-label">Type de contrat</InputLabel>
                                    <Select
                                        labelId="onboarding-contract-type-label"
                                        label="Type de contrat"
                                        value={contractType}
                                        onChange={(p_event: SelectChangeEvent<ContractType>) =>
                                            setContractType(p_event.target.value as ContractType)
                                        }
                                    >
                                        {Object.values(CONTRACT_TYPES).map((p_type: ContractType) => (
                                            <MenuItem key={p_type} value={p_type}>
                                                {CONTRACT_TYPE_LABELS[p_type]}
                                            </MenuItem>
                                        ))}
                                    </Select>
                                </FormControl>
                                <FormControl fullWidth sx={{ mb: 2 }} required>
                                    <InputLabel id="onboarding-wage-type-label">Type de rémunération</InputLabel>
                                    <Select
                                        labelId="onboarding-wage-type-label"
                                        label="Type de rémunération"
                                        value={wageType}
                                        onChange={(p_event: SelectChangeEvent<WageType>) =>
                                            setWageType(p_event.target.value as WageType)
                                        }
                                    >
                                        {Object.values(WAGE_TYPES).map((p_type: WageType) => (
                                            <MenuItem key={p_type} value={p_type}>
                                                {WAGE_TYPE_LABELS[p_type]}
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
                                    helperText={getBaseRateHelper(wageType)}
                                    sx={{ mb: 2 }}
                                    required
                                />
                                <TextField
                                    fullWidth
                                    label="Date de début"
                                    type="date"
                                    value={contractStartDate}
                                    onChange={(p_event) => setContractStartDate(p_event.target.value)}
                                    InputLabelProps={{ shrink: true }}
                                    sx={{ mb: 2 }}
                                    required
                                />
                                <TextField
                                    fullWidth
                                    label="Date de fin (optionnel)"
                                    type="date"
                                    value={contractEndDate}
                                    onChange={(p_event) => setContractEndDate(p_event.target.value)}
                                    InputLabelProps={{ shrink: true }}
                                    helperText="Laissez vide pour un contrat sans date de fin."
                                    sx={{ mb: 2 }}
                                />
                            </>
                        ) : (
                            <Alert severity="info">
                                Vous pourrez ajouter un contrat plus tard depuis la fiche employé ou la page
                                Contrats de travail.
                            </Alert>
                        )}
                    </>
                );
            default:
                return (
                    <>
                        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                            Vérifiez les informations avant de finaliser la création.
                        </Typography>
                        <Typography variant="subtitle2">Poste</Typography>
                        <Typography variant="body2" sx={{ mb: 1 }}>
                            {selectedPositionLabel}
                            {jobPositionMode === "new" ? " (nouveau)" : ""}
                        </Typography>
                        <Divider sx={{ my: 1.5 }} />
                        <Typography variant="subtitle2">Profil employé</Typography>
                        <Typography variant="body2">
                            {firstName} {lastName} — {email}
                        </Typography>
                        <Typography variant="body2">
                            Embauche : {hiringDate} · Salaire de référence : {salary} $ · Statut :{" "}
                            {EMPLOYEE_STATUS_OPTIONS.find((p) => p.value === status)?.label ?? status}
                        </Typography>
                        <Typography variant="body2" sx={{ mb: 1 }}>
                            Succursale : {selectedLocationLabel}
                        </Typography>
                        <Divider sx={{ my: 1.5 }} />
                        <Typography variant="subtitle2">Accès système</Typography>
                        <Typography variant="body2" sx={{ mb: 1 }}>
                            {createUserAccount
                                ? `Compte « ${userName} » (${(dynamicRolesQuery.data ?? []).find((p_role) => p_role.id === assignedRoleId)?.name ?? getAssignedRoleDisplayName({ dynamicRoleId: assignedRoleId })})`
                                : "Aucun compte utilisateur"}
                        </Typography>
                        <Divider sx={{ my: 1.5 }} />
                        <Typography variant="subtitle2">Contrat</Typography>
                        <Typography variant="body2">
                            {addContract
                                ? `${CONTRACT_TYPE_LABELS[contractType]} · ${WAGE_TYPE_LABELS[wageType]} · ${baseRate} $`
                                : "Aucun contrat (à ajouter plus tard)"}
                        </Typography>
                    </>
                );
        }
    };

    return (
        <WizardModal
            open={open}
            onClose={onClose}
            title="Assistant — Nouvel employé"
            steps={WIZARD_STEPS}
            activeStep={activeStep}
            onBack={() => {
                setStepError("");
                setActiveStep((p_prev) => Math.max(0, p_prev - 1));
            }}
            onNext={() => void handleNext()}
            isSubmitting={isSubmitting}
        >
            {stepError && (
                <Alert severity="error" sx={{ mb: 2 }}>
                    {stepError}
                </Alert>
            )}
            {renderStepContent()}
        </WizardModal>
    );
}
