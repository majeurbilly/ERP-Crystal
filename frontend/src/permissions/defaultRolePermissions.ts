import type { DynamicUserRole, PermissionRule } from "../data/types/hr/dynamicUserRole";
import { PRESET_ROLE_IDS, type PresetRoleId } from "../data/types/hr/userRoles";
import { CRUD_OPERATIONS, ENTITY_TYPES } from "./permissions";

const adminPermissions: PermissionRule[] = [
    { action: CRUD_OPERATIONS.MANAGE, subject: ENTITY_TYPES.ALL },
];

const selfAccountPermissions: PermissionRule[] = [
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.ME },
    { action: CRUD_OPERATIONS.UPDATE, subject: ENTITY_TYPES.ME },
];

const gerantPermissions: PermissionRule[] = [
    ...selfAccountPermissions,
    { action: CRUD_OPERATIONS.MANAGE, subject: ENTITY_TYPES.USER },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.HR_DASHBOARD },
    { action: CRUD_OPERATIONS.MANAGE, subject: ENTITY_TYPES.EMPLOYEE_PROFILE },
    { action: CRUD_OPERATIONS.MANAGE, subject: ENTITY_TYPES.LEAVE_REQUEST },
    { action: CRUD_OPERATIONS.MANAGE, subject: ENTITY_TYPES.SCHEDULED_SHIFT },
    { action: CRUD_OPERATIONS.MANAGE, subject: ENTITY_TYPES.TIME_ENTRY },
    { action: CRUD_OPERATIONS.MANAGE, subject: ENTITY_TYPES.TIMESHEET },
    { action: CRUD_OPERATIONS.APPROVE, subject: ENTITY_TYPES.TIMESHEET },
    { action: CRUD_OPERATIONS.MANAGE, subject: ENTITY_TYPES.PAYROLL },
    { action: CRUD_OPERATIONS.MANAGE, subject: ENTITY_TYPES.EMPLOYMENT_CONTRACT },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.LOCATION },
    { action: CRUD_OPERATIONS.UPDATE, subject: ENTITY_TYPES.LOCATION },
    { action: CRUD_OPERATIONS.MANAGE, subject: ENTITY_TYPES.ITEM },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.INVENTORY_QUANTITY, locationScope: "all" },
    { action: CRUD_OPERATIONS.UPDATE, subject: ENTITY_TYPES.INVENTORY_QUANTITY, locationScope: "all" },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.CATEGORY },
    { action: CRUD_OPERATIONS.CREATE, subject: ENTITY_TYPES.CATEGORY },
    { action: CRUD_OPERATIONS.UPDATE, subject: ENTITY_TYPES.CATEGORY },
    { action: CRUD_OPERATIONS.MANAGE, subject: ENTITY_TYPES.JOB_POSITION },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.AUTHOR },
    { action: CRUD_OPERATIONS.CREATE, subject: ENTITY_TYPES.AUTHOR },
    { action: CRUD_OPERATIONS.UPDATE, subject: ENTITY_TYPES.AUTHOR },
];

const assistantPermissions: PermissionRule[] = [
    ...selfAccountPermissions,
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.HR_DASHBOARD },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.EMPLOYEE_PROFILE },
    { action: CRUD_OPERATIONS.CREATE, subject: ENTITY_TYPES.LEAVE_REQUEST },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.LEAVE_REQUEST },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.SCHEDULED_SHIFT },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.TIME_ENTRY },
    { action: CRUD_OPERATIONS.CREATE, subject: ENTITY_TYPES.TIME_ENTRY },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.TIMESHEET },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.PAYROLL },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.EMPLOYMENT_CONTRACT },
    { action: CRUD_OPERATIONS.SUBMIT, subject: ENTITY_TYPES.TIMESHEET },
    { action: CRUD_OPERATIONS.CREATE, subject: ENTITY_TYPES.TIMESHEET },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.ITEM },
    { action: CRUD_OPERATIONS.CREATE, subject: ENTITY_TYPES.ITEM },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.INVENTORY_QUANTITY, locationScope: "all" },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.LOCATION },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.CATEGORY },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.JOB_POSITION },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.AUTHOR },
];

const employeePermissions: PermissionRule[] = [
    ...selfAccountPermissions,
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.EMPLOYEE_PROFILE },
    { action: CRUD_OPERATIONS.CREATE, subject: ENTITY_TYPES.LEAVE_REQUEST },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.LEAVE_REQUEST },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.SCHEDULED_SHIFT },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.TIME_ENTRY },
    { action: CRUD_OPERATIONS.CREATE, subject: ENTITY_TYPES.TIME_ENTRY },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.TIMESHEET },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.PAYROLL },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.EMPLOYMENT_CONTRACT },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.ITEM },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.LOCATION },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.CATEGORY },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.JOB_POSITION },
    { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.AUTHOR },
];

const ROLE_LABELS: Record<PresetRoleId, string> = {
    Admin: "Administrateur",
    Gerant: "Gérant de succursale",
    Assistant: "Assistant",
    Employee: "Employé",
};

const PRESET_ROLES: Record<PresetRoleId, DynamicUserRole> = {
    [PRESET_ROLE_IDS.ADMIN]: {
        id: PRESET_ROLE_IDS.ADMIN,
        name: ROLE_LABELS.Admin,
        permissions: adminPermissions,
    },
    [PRESET_ROLE_IDS.GERANT]: {
        id: PRESET_ROLE_IDS.GERANT,
        name: ROLE_LABELS.Gerant,
        permissions: gerantPermissions,
    },
    [PRESET_ROLE_IDS.ASSISTANT]: {
        id: PRESET_ROLE_IDS.ASSISTANT,
        name: ROLE_LABELS.Assistant,
        permissions: assistantPermissions,
    },
    [PRESET_ROLE_IDS.EMPLOYE]: {
        id: PRESET_ROLE_IDS.EMPLOYE,
        name: ROLE_LABELS.Employee,
        permissions: employeePermissions,
    },
};

export function getDefaultRoleById(p_id: string): DynamicUserRole | null {
    if (p_id in PRESET_ROLES) {
        return PRESET_ROLES[p_id as PresetRoleId];
    }
    return null;
}

export function getAllDefaultRoles(): DynamicUserRole[] {
    return Object.values(PRESET_ROLES);
}
