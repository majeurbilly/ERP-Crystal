import {
    Alert,
    Box,
    Button,
    Checkbox,
    Chip,
    Divider,
    FormControl,
    FormControlLabel,
    FormHelperText,
    FormLabel,
    IconButton,
    InputLabel,
    ListItemText,
    MenuItem,
    Paper,
    Radio,
    RadioGroup,
    Select,
    Stack,
    TextField,
    Typography,
} from "@mui/material";
import DeleteIcon from "@mui/icons-material/Delete";
import AddIcon from "@mui/icons-material/Add";
import { useEffect, useMemo, useState } from "react";
import type { DynamicUserRole, LocationScope, PermissionRule } from "../../../data/types/hr/dynamicUserRole";
import type { Location } from "../../../data/types/inventory/location";
import { FormModal } from "../FormModal";
import { notifySuccessMessage } from "../../../data/utils/popupMessageManager";
import { useUserRoleMutations } from "../../../api/mutations/hr/useUserRoleMutations";
import { displayErrorMessage } from "../../../data/utils/extractApiErrorMessage";
import { getDefaultRoleById } from "../../../permissions/defaultRolePermissions";
import { PRESET_ROLE_IDS, userRoleLabels } from "../../../data/types/hr/userRoles";
import { CRUD_OPERATIONS } from "../../../permissions/permissions";
import permissionEntityService from "../../../api/services/hr/permissionEntityService";
import locationService from "../../../api/services/inventory/locationService";
import type { PermissionEntity } from "../../../data/types/permissionEntity";
import {
    formatPermissionSentence,
    getActionDescription,
    getActionLabel,
    getEntityLabel,
    isInventoryPermission,
    LOCATION_SCOPES,
} from "../../../permissions/permissionLabels";
import PermissionListView from "../../hr-components/PermissionListView";

interface UserRoleFormProps {
    showUserRoleForm: boolean;
    setShowUserRoleForm: (p_value: boolean) => void;
    editUserRole: DynamicUserRole | null;
    setEditUserRole?: (p_value: DynamicUserRole | null) => void;
}

const ACTION_OPTIONS = [
    CRUD_OPERATIONS.READ,
    CRUD_OPERATIONS.CREATE,
    CRUD_OPERATIONS.UPDATE,
    CRUD_OPERATIONS.DELETE,
    CRUD_OPERATIONS.SUBMIT,
    CRUD_OPERATIONS.APPROVE,
    CRUD_OPERATIONS.MANAGE,
];

const PRESET_BUTTONS = [
    PRESET_ROLE_IDS.ADMIN,
    PRESET_ROLE_IDS.GERANT,
    PRESET_ROLE_IDS.ASSISTANT,
    PRESET_ROLE_IDS.EMPLOYE,
] as const;

export default function UserRoleForm({
    showUserRoleForm,
    setShowUserRoleForm,
    editUserRole,
    setEditUserRole,
}: UserRoleFormProps) {
    const handleClose = (): void => setShowUserRoleForm(false);
    const {
        addUserRole,
        isAddingUserRole,
        updateUserRole,
        isUpdatingUserRole,
    } = useUserRoleMutations();

    const isEditMode = editUserRole !== null;
    const isPresetReadOnly = isEditMode && editUserRole?.isPreset === true;
    const [name, setName] = useState<string>("");
    const [permissions, setPermissions] = useState<PermissionRule[]>([]);
    const [entities, setEntities] = useState<PermissionEntity[]>([]);
    const [locations, setLocations] = useState<Location[]>([]);
    const [selectedSubject, setSelectedSubject] = useState<string>("");
    const [selectedAction, setSelectedAction] = useState<string>(CRUD_OPERATIONS.READ);
    const [locationScope, setLocationScope] = useState<LocationScope>(LOCATION_SCOPES.ALL);
    const [selectedLocationIds, setSelectedLocationIds] = useState<number[]>([]);
    const [nameError, setNameError] = useState<string>("");
    const [permissionsError, setPermissionsError] = useState<string>("");
    const [scopeError, setScopeError] = useState<string>("");

    const isInventorySubjectSelected = isInventoryPermission(selectedSubject);

    const locationTitlesById = useMemo((): Record<number, string> => {
        const titles: Record<number, string> = {};
        locations.forEach((p_location: Location) => {
            titles[p_location.id] = p_location.title;
        });
        return titles;
    }, [locations]);

    useEffect(() => {
        if (showUserRoleForm) {
            permissionEntityService.getAll().then(setEntities).catch(() => setEntities([]));
            locationService.getAll().then(setLocations).catch(() => setLocations([]));
        }
    }, [showUserRoleForm]);

    useEffect(() => {
        if (showUserRoleForm) {
            if (editUserRole) {
                setName(editUserRole.name);
                setPermissions([...editUserRole.permissions]);
            } else {
                setName("");
                setPermissions([]);
            }
            setNameError("");
            setPermissionsError("");
            setScopeError("");
            setSelectedSubject("");
            setSelectedAction(CRUD_OPERATIONS.READ);
            setLocationScope(LOCATION_SCOPES.ALL);
            setSelectedLocationIds([]);
        }
    }, [editUserRole, showUserRoleForm]);

    const resetScopeFields = (): void => {
        setLocationScope(LOCATION_SCOPES.ALL);
        setSelectedLocationIds([]);
        setScopeError("");
    };

    const handleSubjectChange = (p_subject: string): void => {
        setSelectedSubject(p_subject);
        resetScopeFields();
    };

    const applyPreset = (p_presetId: string): void => {
        const preset = getDefaultRoleById(p_presetId);
        if (!preset) {
            return;
        }
        if (!isEditMode) {
            setName(`${preset.name} (copie)`);
        }
        setPermissions([...preset.permissions]);
        setPermissionsError("");
    };

    const buildPermissionRule = (): PermissionRule | null => {
        if (!selectedSubject || !selectedAction) {
            return null;
        }

        const rule: PermissionRule = {
            subject: selectedSubject,
            action: selectedAction,
        };

        if (isInventoryPermission(selectedSubject)) {
            rule.locationScope = locationScope;
            rule.locationIds = locationScope === LOCATION_SCOPES.SPECIFIC
                ? [...selectedLocationIds]
                : [];
        }

        return rule;
    };

    const addPermission = (): void => {
        if (!selectedSubject || !selectedAction) {
            return;
        }

        if (isInventorySubjectSelected) {
            if (locationScope === LOCATION_SCOPES.SPECIFIC && selectedLocationIds.length === 0) {
                setScopeError("Sélectionnez au moins une succursale pour un périmètre précis.");
                return;
            }
        }

        const exists = permissions.some(
            (p_rule: PermissionRule) =>
                p_rule.subject === selectedSubject && p_rule.action === selectedAction,
        );
        if (exists) {
            return;
        }

        const newRule = buildPermissionRule();
        if (!newRule) {
            return;
        }

        setPermissions((p_prev: PermissionRule[]) => [...p_prev, newRule]);
        setPermissionsError("");
        setScopeError("");
    };

    const removePermission = (p_index: number): void => {
        setPermissions((p_prev: PermissionRule[]) => p_prev.filter((_, p_i: number) => p_i !== p_index));
    };

    const validateInventoryPermissions = (): boolean => {
        for (const rule of permissions) {
            if (!isInventoryPermission(rule.subject)) {
                continue;
            }

            if (!rule.locationScope) {
                setPermissionsError(
                    "Chaque droit sur l'inventaire doit préciser un périmètre de succursales.",
                );
                return false;
            }

            if (
                rule.locationScope === LOCATION_SCOPES.SPECIFIC
                && (!rule.locationIds || rule.locationIds.length === 0)
            ) {
                setPermissionsError(
                    "Chaque droit sur l'inventaire en périmètre précis doit inclure au moins une succursale.",
                );
                return false;
            }
        }

        return true;
    };

    const validate = (): boolean => {
        let isValid = true;
        setNameError("");
        setPermissionsError("");

        if (!name.trim()) {
            setNameError("Le nom est requis.");
            isValid = false;
        } else if (name.trim().length < 2) {
            setNameError("Le nom doit contenir au moins 2 caractères.");
            isValid = false;
        }

        if (permissions.length === 0) {
            setPermissionsError("Ajoutez au moins un droit d'accès pour ce rôle.");
            isValid = false;
        }

        if (isValid && !validateInventoryPermissions()) {
            isValid = false;
        }

        return isValid;
    };

    const handleSubmit = async (p_event: React.FormEvent): Promise<void> => {
        p_event.preventDefault();
        if (!validate()) {
            return;
        }

        const roleData: DynamicUserRole = {
            id: editUserRole?.id ?? "",
            name: name.trim(),
            permissions,
            isPreset: editUserRole?.isPreset,
        };

        try {
            if (isEditMode && editUserRole) {
                await updateUserRole({ id: editUserRole.id, data: roleData });
                notifySuccessMessage(`Le rôle « ${roleData.name} » a été modifié avec succès.`);
                if (setEditUserRole) {
                    setEditUserRole(null);
                }
            } else {
                await addUserRole(roleData);
                notifySuccessMessage(`Le rôle « ${roleData.name} » a été ajouté avec succès.`);
            }
            handleClose();
        } catch (error: unknown) {
            displayErrorMessage(error);
        }
    };

    const renderLocationScopeValue = (p_selected: number[]): string => {
        return p_selected
            .map((p_id: number) => locationTitlesById[p_id] ?? `Succursale #${p_id}`)
            .join(", ");
    };

    return (
        <FormModal
            open={showUserRoleForm}
            onClose={handleClose}
            title={isPresetReadOnly ? "Consulter un rôle prédéfini" : isEditMode ? "Modifier un rôle" : "Ajouter un rôle"}
            onSubmit={isPresetReadOnly ? undefined : handleSubmit}
            isSubmitting={isEditMode ? isUpdatingUserRole : isAddingUserRole}
            hideConfirmButton={isPresetReadOnly}
            maxWidth={680}
        >
            {isPresetReadOnly && (
                <Alert severity="info" sx={{ mb: 2 }}>
                    Ce rôle est fourni par défaut et ne peut pas être modifié. Pour personnaliser les droits,
                    créez un nouveau rôle à partir d&apos;un modèle.
                </Alert>
            )}

            <TextField
                fullWidth
                label="Nom du rôle"
                placeholder="Ex. : Employé (Saint-Foy)"
                value={name}
                onChange={(p_event) => setName(p_event.target.value)}
                required
                error={!!nameError}
                helperText={nameError || "Un nom clair aide les autres à comprendre à qui attribuer ce rôle."}
                disabled={isPresetReadOnly}
                sx={{ mb: 3 }}
            />

            {!isPresetReadOnly && (
                <>
                    <Typography variant="subtitle1" fontWeight={700} sx={{ mb: 0.5 }}>
                        Partir d&apos;un modèle
                    </Typography>
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 1.5 }}>
                        Cliquez sur un modèle pour préremplir les droits, puis ajustez si nécessaire.
                    </Typography>
                    <Stack direction="row" flexWrap="wrap" gap={1} sx={{ mb: 3 }}>
                        {PRESET_BUTTONS.map((p_presetId) => (
                            <Chip
                                key={p_presetId}
                                label={userRoleLabels[p_presetId]}
                                onClick={() => applyPreset(p_presetId)}
                                clickable
                                color="primary"
                                variant="outlined"
                            />
                        ))}
                    </Stack>

                    <Divider sx={{ mb: 3 }} />

                    <Typography variant="subtitle1" fontWeight={700} sx={{ mb: 0.5 }}>
                        Ajouter un droit d&apos;accès
                    </Typography>
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                        Choisissez une section de l&apos;application et ce que la personne pourra y faire.
                    </Typography>

                    <Paper variant="outlined" sx={{ p: 2, mb: 2, borderRadius: 2, bgcolor: "background.default" }}>
                        <Stack spacing={2}>
                            <FormControl fullWidth>
                                <InputLabel id="permission-subject-label">Section de l&apos;application</InputLabel>
                                <Select
                                    labelId="permission-subject-label"
                                    value={selectedSubject}
                                    label="Section de l'application"
                                    onChange={(p_event) => handleSubjectChange(p_event.target.value)}
                                >
                                    {entities.map((p_entity: PermissionEntity) => (
                                        <MenuItem key={p_entity.id} value={p_entity.id}>
                                            {getEntityLabel(p_entity.id)}
                                        </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>

                            <FormControl fullWidth>
                                <InputLabel id="permission-action-label">Droit accordé</InputLabel>
                                <Select
                                    labelId="permission-action-label"
                                    value={selectedAction}
                                    label="Droit accordé"
                                    onChange={(p_event) => setSelectedAction(p_event.target.value)}
                                >
                                    {ACTION_OPTIONS.map((p_action: string) => (
                                        <MenuItem key={p_action} value={p_action}>
                                            <Box>
                                                <Typography variant="body2" fontWeight={600}>
                                                    {getActionLabel(p_action)}
                                                </Typography>
                                                <Typography variant="caption" color="text.secondary">
                                                    {getActionDescription(p_action)}
                                                </Typography>
                                            </Box>
                                        </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>

                            {isInventorySubjectSelected && (
                                <Box
                                    sx={{
                                        p: 2,
                                        borderRadius: 2,
                                        border: "1px solid",
                                        borderColor: "divider",
                                        bgcolor: "background.paper",
                                    }}
                                >
                                    <FormControl component="fieldset" fullWidth>
                                        <FormLabel component="legend" sx={{ fontWeight: 700, mb: 1 }}>
                                            Périmètre d&apos;application
                                        </FormLabel>
                                        <Typography variant="body2" color="text.secondary" sx={{ mb: 1.5 }}>
                                            Indiquez sur quelles succursales ce droit d&apos;inventaire s&apos;applique.
                                        </Typography>
                                        <RadioGroup
                                            value={locationScope}
                                            onChange={(p_event) => {
                                                setLocationScope(p_event.target.value as LocationScope);
                                                setScopeError("");
                                                if (p_event.target.value === LOCATION_SCOPES.ALL) {
                                                    setSelectedLocationIds([]);
                                                }
                                            }}
                                        >
                                            <FormControlLabel
                                                value={LOCATION_SCOPES.ALL}
                                                control={<Radio />}
                                                label="Toutes les succursales (actuelles et futures)"
                                            />
                                            <FormControlLabel
                                                value={LOCATION_SCOPES.SPECIFIC}
                                                control={<Radio />}
                                                label="Succursales précises"
                                            />
                                        </RadioGroup>
                                    </FormControl>

                                    {locationScope === LOCATION_SCOPES.SPECIFIC && (
                                        <FormControl fullWidth sx={{ mt: 2 }} error={!!scopeError}>
                                            <InputLabel id="permission-locations-label">
                                                Succursales concernées
                                            </InputLabel>
                                            <Select
                                                labelId="permission-locations-label"
                                                multiple
                                                value={selectedLocationIds}
                                                label="Succursales concernées"
                                                onChange={(p_event) => {
                                                    const value = p_event.target.value;
                                                    setSelectedLocationIds(
                                                        typeof value === "string"
                                                            ? value.split(",").map(Number)
                                                            : (value as number[]),
                                                    );
                                                    setScopeError("");
                                                }}
                                                renderValue={renderLocationScopeValue}
                                            >
                                                {locations.map((p_location: Location) => (
                                                    <MenuItem key={p_location.id} value={p_location.id}>
                                                        <Checkbox
                                                            checked={selectedLocationIds.includes(p_location.id)}
                                                        />
                                                        <ListItemText
                                                            primary={p_location.title}
                                                            secondary={p_location.address}
                                                        />
                                                    </MenuItem>
                                                ))}
                                            </Select>
                                            {scopeError ? (
                                                <FormHelperText>{scopeError}</FormHelperText>
                                            ) : (
                                                <FormHelperText>
                                                    Choisissez une ou plusieurs succursales pour limiter ce droit.
                                                </FormHelperText>
                                            )}
                                        </FormControl>
                                    )}
                                </Box>
                            )}

                            <Button
                                variant="contained"
                                startIcon={<AddIcon />}
                                onClick={addPermission}
                                disabled={!selectedSubject}
                                sx={{ alignSelf: "flex-start" }}
                            >
                                Ajouter ce droit
                            </Button>
                        </Stack>
                    </Paper>
                </>
            )}

            <Typography variant="subtitle1" fontWeight={700} sx={{ mb: 1 }}>
                Droits accordés ({permissions.length})
            </Typography>

            {permissionsError && (
                <Alert severity="error" sx={{ mb: 2 }}>
                    {permissionsError}
                </Alert>
            )}

            {isPresetReadOnly ? (
                <PermissionListView permissions={permissions} locationTitlesById={locationTitlesById} />
            ) : (
                <Stack spacing={1} sx={{ maxHeight: 280, overflowY: "auto" }}>
                    {permissions.length === 0 ? (
                        <Paper
                            variant="outlined"
                            sx={{ p: 3, textAlign: "center", borderRadius: 2, bgcolor: "background.default" }}
                        >
                            <Typography color="text.secondary">
                                Aucun droit pour l&apos;instant. Utilisez un modèle ou ajoutez un droit ci-dessus.
                            </Typography>
                        </Paper>
                    ) : (
                        permissions.map((p_rule: PermissionRule, p_index: number) => (
                            <Paper
                                key={`${p_rule.subject}_${p_rule.action}_${p_index}`}
                                variant="outlined"
                                sx={{
                                    px: 2,
                                    py: 1,
                                    borderRadius: 2,
                                    display: "flex",
                                    alignItems: "center",
                                    justifyContent: "space-between",
                                    bgcolor: "background.default",
                                }}
                            >
                                <Box>
                                    <Typography variant="body2" fontWeight={600}>
                                        {formatPermissionSentence(p_rule, locationTitlesById)}
                                    </Typography>
                                    <Typography variant="caption" color="text.secondary">
                                        {getEntityLabel(p_rule.subject)} · {getActionLabel(p_rule.action)}
                                    </Typography>
                                </Box>
                                <IconButton
                                    edge="end"
                                    onClick={() => removePermission(p_index)}
                                    size="small"
                                    aria-label="Retirer ce droit"
                                >
                                    <DeleteIcon fontSize="small" />
                                </IconButton>
                            </Paper>
                        ))
                    )}
                </Stack>
            )}
        </FormModal>
    );
}
