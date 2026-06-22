export const PRESET_ROLE_IDS = {
    ADMIN: "Admin",
    GERANT: "Gerant",
    ASSISTANT: "Assistant",
    EMPLOYE: "Employee",
} as const;

export type PresetRoleId = (typeof PRESET_ROLE_IDS)[keyof typeof PRESET_ROLE_IDS];

export const userRoleLabels: Record<PresetRoleId, string> = {
    Gerant: "Gérant",
    Assistant: "Assistant",
    Employee: "Employé",
    Admin: "Administrateur",
};

export const DEFAULT_ASSIGNED_ROLE_ID: PresetRoleId = PRESET_ROLE_IDS.EMPLOYE;

export const userAccessFieldLabels = {
    assignedRole: "Rôle",
} as const;

export const userAccessFieldHelpers = {
    assignedRole: "Choisissez le profil d'accès depuis la liste des rôles.",
} as const;

export function resolvePresetRoleFromAssignedRole(
    p_assignedRoleId: string | null | undefined,
): PresetRoleId {
    const presetValues: PresetRoleId[] = Object.values(PRESET_ROLE_IDS);
    if (p_assignedRoleId && presetValues.includes(p_assignedRoleId as PresetRoleId)) {
        return p_assignedRoleId as PresetRoleId;
    }
    return PRESET_ROLE_IDS.EMPLOYE;
}

export const ROLE_CHIP_COLORS: Record<PresetRoleId, "error" | "warning" | "info" | "default"> = {
    Admin: "error",
    Gerant: "warning",
    Assistant: "info",
    Employee: "default",
};

export function getRoleChipColorForAssignedRole(
    p_assignedRoleId: string | null | undefined,
): "error" | "warning" | "info" | "default" {
    const presetRole: PresetRoleId = resolvePresetRoleFromAssignedRole(p_assignedRoleId);
    return ROLE_CHIP_COLORS[presetRole];
}

export function getAssignedRoleDisplayName(p_user: {
    dynamicRoleName?: string | null;
    dynamicRoleId?: string | null;
}): string {
    if (p_user.dynamicRoleName) {
        return p_user.dynamicRoleName;
    }
    const presetRole: PresetRoleId = resolvePresetRoleFromAssignedRole(p_user.dynamicRoleId);
    return userRoleLabels[presetRole] ?? "—";
}

export function isPresetRoleId(p_value: string): p_value is PresetRoleId {
    return Object.values(PRESET_ROLE_IDS).includes(p_value as PresetRoleId);
}