import type { DynamicUserRole } from "./dynamicUserRole";

export interface User {
    id: string;
    userName: string;
    email: string;
    dynamicRoleId?: string | null;
    dynamicRoleName?: string | null;
    dynamicRole?: DynamicUserRole;
};

export type UserFormData = User & { password?: string };

export interface UserApiDTO {
    id: string;
    email: string;
    userName: string;
    dynamicRoleId?: string | null;
    dynamicRoleName?: string | null;
}

export interface CreatedUserApiDTO extends UserApiDTO {
    password: string;
}
