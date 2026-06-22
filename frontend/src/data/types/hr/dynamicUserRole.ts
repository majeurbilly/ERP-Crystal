export interface PermissionRule {
    action: string;
    subject: string;
    locationScope?: LocationScope | null;
    locationIds?: number[];
}

export type LocationScope = "all" | "specific";

export interface DynamicUserRole {
    id: string;
    name: string;
    isPreset?: boolean;
    permissions: PermissionRule[];
}

export interface DynamicUserRoleApiDTO {
    id: string;
    name: string;
    isPreset?: boolean;
    permissions: PermissionRule[];
}

export interface CreateDynamicUserRolePayload {
    name: string;
    permissions: PermissionRule[];
    presetId?: string;
}