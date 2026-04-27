import type { UserRole } from "../userRoles";

export interface User {
    id: string;
    userName: string;
    email: string;
    role: UserRole
};