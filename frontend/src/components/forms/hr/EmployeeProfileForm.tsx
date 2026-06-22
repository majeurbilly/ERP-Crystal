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
import type { EmployeeProfile, EmployeeProfileFormData } from "../../../data/types/hr/employeeProfile";
import { FormModal } from "../FormModal";
import { notifyErrorMessage, notifySuccessMessage } from "../../../data/utils/popupMessageManager";
import { useEmployeeProfileMutations } from "../../../api/mutations/hr/useEmployeeProfileMutations";
import { extractApiErrorMessage } from "../../../data/utils/extractApiErrorMessage";
import userService from "../../../api/services/hr/userService";
import locationService from "../../../api/services/inventory/locationService";
import { usersCacheKey } from "../../../data/cacheKeys";
import { locationsCacheKey } from "../../../data/cacheKeys";
import type { User } from "../../../data/types/hr/user";
import type { Location } from "../../../data/types/inventory/location";

const NO_LINKED_USER_VALUE = "__none_user__";
const NO_LOCATION_VALUE = "__none_location__";

interface EmployeeProfileFormProps {
    showEmployeeProfileForm: boolean;
    setShowEmployeeProfileForm: (p_value: boolean) => void;
    editEmployeeProfile: EmployeeProfile | null;
    setEditEmployeeProfile?: (p_value: EmployeeProfile | null) => void;
}

interface EmployeeProfileFormErrors {
    firstName: string;
    lastName: string;
    email: string;
    salary: string;
    status: string;
    hiringDate: string;
}

const EMPLOYEE_STATUS_OPTIONS: string[] = ["Active", "Inactive", "OnLeave"];

export default function EmployeeProfileForm({
    showEmployeeProfileForm,
    setShowEmployeeProfileForm,
    editEmployeeProfile,
    setEditEmployeeProfile,
}: EmployeeProfileFormProps) {
    const handleClose = (): void => setShowEmployeeProfileForm(false);
    const {
        addEmployeeProfile,
        isAddingEmployeeProfile,
        updateEmployeeProfile,
        isUpdatingEmployeeProfile,
    } = useEmployeeProfileMutations();

    const isEditMode: boolean = editEmployeeProfile !== null;

    const [firstName, setFirstName] = useState<string>("");
    const [lastName, setLastName] = useState<string>("");
    const [email, setEmail] = useState<string>("");
    const [hiringDate, setHiringDate] = useState<string>("");
    const [salary, setSalary] = useState<string>("");
    const [status, setStatus] = useState<string>("Active");
    const [applicationUserId, setApplicationUserId] = useState<string>(NO_LINKED_USER_VALUE);
    const [locationId, setLocationId] = useState<string>(NO_LOCATION_VALUE);
    const [errors, setErrors] = useState<EmployeeProfileFormErrors>({
        firstName: "",
        lastName: "",
        email: "",
        salary: "",
        status: "",
        hiringDate: "",
    });

    const usersQuery = useQuery<User[], Error>({
        queryKey: usersCacheKey.list(),
        queryFn: () => userService.getAll(),
        enabled: showEmployeeProfileForm,
    });

    const locationsQuery = useQuery<Location[], Error>({
        queryKey: locationsCacheKey.list(),
        queryFn: () => locationService.getAll(),
        enabled: showEmployeeProfileForm,
    });

    useEffect(() => {
        if (showEmployeeProfileForm) {
            if (editEmployeeProfile) {
                setFirstName(editEmployeeProfile.firstName);
                setLastName(editEmployeeProfile.lastName);
                setEmail(editEmployeeProfile.email);
                setHiringDate(editEmployeeProfile.hiringDate);
                setSalary(String(editEmployeeProfile.salary));
                setStatus(editEmployeeProfile.status);
                setApplicationUserId(editEmployeeProfile.applicationUserId ?? NO_LINKED_USER_VALUE);
                setLocationId(
                    editEmployeeProfile.locationId
                        ? String(editEmployeeProfile.locationId)
                        : NO_LOCATION_VALUE
                );
            } else {
                setFirstName("");
                setLastName("");
                setEmail("");
                setHiringDate("");
                setSalary("");
                setStatus("Active");
                setApplicationUserId(NO_LINKED_USER_VALUE);
                setLocationId(NO_LOCATION_VALUE);
            }
            setErrors({
                firstName: "",
                lastName: "",
                email: "",
                salary: "",
                status: "",
                hiringDate: "",
            });
        }
    }, [editEmployeeProfile, showEmployeeProfileForm]);

    const validate = (): boolean => {
        let isValid: boolean = true;
        const newErrors: EmployeeProfileFormErrors = {
            firstName: "",
            lastName: "",
            email: "",
            salary: "",
            status: "",
            hiringDate: "",
        };

        if (!firstName.trim()) {
            newErrors.firstName = "Le prénom est requis.";
            isValid = false;
        }

        if (!lastName.trim()) {
            newErrors.lastName = "Le nom est requis.";
            isValid = false;
        }

        if (!email.trim()) {
            newErrors.email = "Le courriel est requis.";
            isValid = false;
        } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())) {
            newErrors.email = "Le format du courriel est invalide.";
            isValid = false;
        }

        if (!hiringDate) {
            newErrors.hiringDate = "La date d'embauche est requise.";
            isValid = false;
        }

        const parsedSalary: number = Number(salary);
        if (!salary.trim() || Number.isNaN(parsedSalary) || parsedSalary < 0) {
            newErrors.salary = "Le salaire doit être un nombre positif ou nul.";
            isValid = false;
        }

        if (!status.trim()) {
            newErrors.status = "Le statut est requis.";
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

        const formData: EmployeeProfileFormData = {
            firstName: firstName.trim(),
            lastName: lastName.trim(),
            email: email.trim(),
            applicationUserId:
                applicationUserId === NO_LINKED_USER_VALUE ? null : applicationUserId,
            salary: Number(salary),
            status: status.trim(),
            hiringDate,
            locationId: locationId === NO_LOCATION_VALUE ? undefined : Number(locationId),
        };

        try {
            if (isEditMode && editEmployeeProfile) {
                await updateEmployeeProfile({
                    id: String(editEmployeeProfile.id),
                    data: formData,
                });
                notifySuccessMessage(
                    `L'employé « ${formData.firstName} ${formData.lastName} » a été modifié avec succès.`
                );
                if (setEditEmployeeProfile) {
                    setEditEmployeeProfile(null);
                }
            } else {
                await addEmployeeProfile(formData);
                notifySuccessMessage(
                    `L'employé « ${formData.firstName} ${formData.lastName} » a été ajouté avec succès.`
                );
            }
            handleClose();
        } catch (error: unknown) {
            notifyErrorMessage(extractApiErrorMessage(error));
        }
    };

    const users: User[] = usersQuery.data ?? [];
    const locations: Location[] = locationsQuery.data ?? [];
    const selectedLocationValue =
        locationId === NO_LOCATION_VALUE
            ? NO_LOCATION_VALUE
            : locations.some((p_location) => String(p_location.id) === locationId)
                ? locationId
                : NO_LOCATION_VALUE;

    const selectedApplicationUserValue =
        applicationUserId === NO_LINKED_USER_VALUE
            ? NO_LINKED_USER_VALUE
            : users.some((p_user) => p_user.id === applicationUserId)
                ? applicationUserId
                : NO_LINKED_USER_VALUE;

    return (
        <FormModal
            open={showEmployeeProfileForm}
            onClose={handleClose}
            title={isEditMode ? "Modifier un employé" : "Ajouter un employé"}
            onSubmit={handleSubmit}
            isSubmitting={isEditMode ? isUpdatingEmployeeProfile : isAddingEmployeeProfile}
        >
            <TextField
                fullWidth
                label="Prénom"
                value={firstName}
                onChange={(p_event) => setFirstName(p_event.target.value)}
                sx={{ mb: 2 }}
                required
                error={!!errors.firstName}
                helperText={errors.firstName}
            />
            <TextField
                fullWidth
                label="Nom"
                value={lastName}
                onChange={(p_event) => setLastName(p_event.target.value)}
                sx={{ mb: 2 }}
                required
                error={!!errors.lastName}
                helperText={errors.lastName}
            />
            <TextField
                fullWidth
                label="Courriel"
                type="email"
                value={email}
                onChange={(p_event) => setEmail(p_event.target.value)}
                sx={{ mb: 2 }}
                required
                error={!!errors.email}
                helperText={errors.email}
            />
            <TextField
                fullWidth
                label="Date d'embauche"
                type="date"
                value={hiringDate}
                onChange={(p_event) => setHiringDate(p_event.target.value)}
                InputLabelProps={{ shrink: true }}
                required
                error={!!errors.hiringDate}
                helperText={errors.hiringDate}
                sx={{ mb: 2 }}
            />
            <TextField
                fullWidth
                label="Salaire"
                type="number"
                inputProps={{ min: 0, step: "0.01" }}
                value={salary}
                onChange={(p_event) => setSalary(p_event.target.value)}
                sx={{ mb: 2 }}
                required
                error={!!errors.salary}
                helperText={errors.salary}
            />
            <FormControl fullWidth sx={{ mb: 2 }} required error={!!errors.status}>
                <InputLabel id="employee-status-label">Statut</InputLabel>
                <Select
                    labelId="employee-status-label"
                    label="Statut"
                    value={status}
                    onChange={(p_event: SelectChangeEvent<string>) => setStatus(p_event.target.value)}
                >
                    {EMPLOYEE_STATUS_OPTIONS.map((p_option: string) => (
                        <MenuItem key={p_option} value={p_option}>
                            {p_option}
                        </MenuItem>
                    ))}
                </Select>
            </FormControl>
            <FormControl fullWidth sx={{ mb: 2 }}>
                <InputLabel id="employee-location-label">Succursale (optionnel)</InputLabel>
                <Select
                    labelId="employee-location-label"
                    label="Succursale (optionnel)"
                    value={selectedLocationValue}
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
            <FormControl fullWidth sx={{ mb: 2 }}>
                <InputLabel id="application-user-label">Utilisateur système (optionnel)</InputLabel>
                <Select
                    labelId="application-user-label"
                    label="Utilisateur système (optionnel)"
                    value={selectedApplicationUserValue}
                    onChange={(p_event: SelectChangeEvent<string>) =>
                        setApplicationUserId(p_event.target.value)
                    }
                    disabled={usersQuery.isLoading}
                >
                    <MenuItem value={NO_LINKED_USER_VALUE}>Aucun utilisateur lié</MenuItem>
                    {users.map((p_user: User) => (
                        <MenuItem key={p_user.id} value={p_user.id}>
                            {`${p_user.userName} (${p_user.email})`}
                        </MenuItem>
                    ))}
                </Select>
            </FormControl>
        </FormModal>
    );
}
