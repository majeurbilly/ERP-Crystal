import apiClient from "../../apiClient";
import type { PermissionRule } from "../../../data/types/hr/dynamicUserRole";

export interface UserPermissionsApiDTO {
    roleId: string;
    roleName: string;
    permissions: PermissionRule[];
}

export interface UserPermissions {
    roleId: string;
    roleName: string;
    permissions: PermissionRule[];
}

class PermissionService {
    async getMyPermissions(): Promise<UserPermissions> {
        const response = await apiClient.get<UserPermissionsApiDTO>("/users/me/permissions");
        return {
            roleId: response.data.roleId,
            roleName: response.data.roleName,
            permissions: response.data.permissions ?? [],
        };
    }
}

const permissionService = new PermissionService();
export default permissionService;
