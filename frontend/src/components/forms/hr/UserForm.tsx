import { TextField, Select, FormControl, FormHelperText, InputLabel, MenuItem } from "@mui/material";
import { useEffect, useState } from "react";
import {
    DEFAULT_ASSIGNED_ROLE_ID,
    userAccessFieldHelpers,
    userAccessFieldLabels,
} from "../../../data/types/hr/userRoles";
import type { User } from "../../../data/types/hr/user";
import { useUserMutations } from "../../../api/mutations/hr/useUserMutations";
import { notifySuccessMessage } from "../../../data/utils/popupMessageManager";
import { useFormValidation } from "../useFormValidation";
import { FormModal } from "../FormModal";
import { useAuth } from "../../../context/AuthContext";
import { displayErrorMessage } from "../../../data/utils/extractApiErrorMessage";
import userRoleService from "../../../api/services/hr/userRoleService";
import type { DynamicUserRole } from "../../../data/types/hr/dynamicUserRole";
import { usePermissions } from "../../../permissions/usePermissions";
import { ENTITY_TYPES } from "../../../permissions/permissions";

interface Props {
    showUserForm: boolean;
    setShowUserForm: (v: boolean) => void;
    editUser: User | null;
    setEditUser?: (u: User | null) => void;
}

export default function UserForm({ showUserForm, setShowUserForm, editUser, setEditUser }: Props) {
    const handleClose = () => setShowUserForm(false);
    const { addUser, isAddingUser: isAdding, updateUser, isUpdatingUser: isUpdating, updateMe, isUpdatingMe } = useUserMutations();

    const { user } = useAuth();
    const myId = user?.id;
    const isMe = editUser ? editUser.id === myId : false;
    const isEditMode = editUser !== null;
    const { canCreate, canUpdate } = usePermissions(ENTITY_TYPES.USER);
    const canModifyRole = isEditMode ? canUpdate : canCreate;
    const [email, setEmail] = useState("");
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [assignedRoleId, setAssignedRoleId] = useState<string>(DEFAULT_ASSIGNED_ROLE_ID);
    const [availableRoles, setAvailableRoles] = useState<DynamicUserRole[]>([]);

    const { errors, setErrors, clearErrors } = useFormValidation({
        email: "",
        username: "",
        password: "",
        assignedRoleId: "",
    });

    useEffect(() => {
        if (showUserForm && canModifyRole && !isMe) {
            userRoleService.getAll().then(setAvailableRoles).catch(() => setAvailableRoles([]));
        }
    }, [showUserForm, canModifyRole, isMe]);

    useEffect(() => {
        if (showUserForm) {
            if (editUser) {
                setEmail(editUser.email);
                setUsername(editUser.userName);
                setAssignedRoleId(editUser.dynamicRoleId ?? DEFAULT_ASSIGNED_ROLE_ID);
            } else {
                setEmail("");
                setUsername("");
                setAssignedRoleId(DEFAULT_ASSIGNED_ROLE_ID);
            }
            setPassword("");
            clearErrors();
        }
    }, [editUser, showUserForm, clearErrors]);

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    const validate = () => {
        const newErrors = { id: "", email: "", username: "", password: "", assignedRoleId: "" };
        let isValid = true;

        if (!email.trim()) {
            newErrors.email = "Email requis";
            isValid = false;
        } else if (!emailRegex.test(email)) {
            newErrors.email = "Email invalide";
            isValid = false;
        }
        if (!username.trim() || username.trim().length < 3) {
            newErrors.username = "Nom d'utilisateur d'au moins 3 caractères réquis.";
            isValid = false;
        }
        const requiresPassword: boolean = !isEditMode;
        if (requiresPassword && (!password.trim() || password.trim().length < 8)) {
            newErrors.password = "Mot de passe d'au moins 8 caractères réquis.";
            isValid = false;
        } else if (isEditMode && password.trim() && password.trim().length < 8) {
            newErrors.password = "Le nouveau mot de passe doit contenir au moins 8 caractères.";
            isValid = false;
        }
        if (canModifyRole && !isMe && !assignedRoleId.trim()) {
            newErrors.assignedRoleId = "Un rôle est requis.";
            isValid = false;
        }

        setErrors(newErrors);
        return isValid;
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!validate()) return;

        const newUser = {
            id: editUser?.id || "",
            userName: username,
            email: email,
            dynamicRoleId: assignedRoleId,
            ...(password.trim() ? { password: password.trim() } : {}),
        };

        try {
            if (!isEditMode) {
                await addUser(newUser);
                notifySuccessMessage(`Utilisateur ${newUser.userName} ajouté avec succès!`);
            } else {
                if (isMe) {
                    await updateMe(newUser);
                } else {
                    await updateUser({ id: editUser!.id, data: newUser });
                }
                if (setEditUser) setEditUser(null);
                notifySuccessMessage(`Utilisateur ${newUser.userName} modifié avec succès!`);
            }
            handleClose();
        } catch (error: unknown) {
            displayErrorMessage(error);
        }
    };

    return (
        <FormModal
            open={showUserForm}
            onClose={handleClose}
            title={!isEditMode ? "Ajouter un utilisateur" : isMe ? "Modifier mon profil" : "Modifier un utilisateur"}
            onSubmit={handleSubmit}
            isSubmitting={!isEditMode ? isAdding : isMe ? isUpdatingMe : isUpdating}
        >
            <TextField fullWidth label="Email" value={email} onChange={(e) => setEmail(e.target.value)} error={!!errors.email} helperText={errors.email} sx={{ mb: 2 }} />
            <TextField fullWidth label="Username" value={username} onChange={(e) => setUsername(e.target.value)} error={!!errors.username} helperText={errors.username} sx={{ mb: 2 }} />
            {canModifyRole && !isMe && (
                <FormControl fullWidth sx={{ mb: 2 }} error={!!errors.assignedRoleId}>
                    <InputLabel id="assigned-role-label">{userAccessFieldLabels.assignedRole}</InputLabel>
                    <Select
                        labelId="assigned-role-label"
                        value={assignedRoleId}
                        label={userAccessFieldLabels.assignedRole}
                        onChange={(e) => setAssignedRoleId(e.target.value)}
                    >
                        {availableRoles.map((p_role) => (
                            <MenuItem key={p_role.id} value={p_role.id}>
                                {p_role.name}
                            </MenuItem>
                        ))}
                    </Select>
                    <FormHelperText>{errors.assignedRoleId || userAccessFieldHelpers.assignedRole}</FormHelperText>
                </FormControl>
            )}
            <TextField
                fullWidth
                label={isEditMode ? "Nouveau mot de passe (optionnel)" : "Mot de passe"}
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                error={!!errors.password}
                helperText={errors.password || (isEditMode ? "Laissez vide pour conserver le mot de passe actuel." : undefined)}
                sx={{ mb: 2 }}
            />
        </FormModal>
    );
}
