import { AbilityBuilder, createMongoAbility } from '@casl/ability';
import type { MongoAbility } from '@casl/ability';
import type { SessionUser } from '../context/AuthContext';

export const ENTITY_TYPES = {
    ME: "me",
    ALL: "all",
    LOCATION: "location",
    ITEM: "item",
    USER: "user",
    CATEGORY: "category",
    INVENTORY_QUANTITY: "inventory_quantity",
    HR_DASHBOARD: "hr_dashboard",
    JOB_POSITION: "job_position",
    EMPLOYEE_PROFILE: "employee_profile",
    EMPLOYMENT_CONTRACT: "employment_contract",
    LEAVE_REQUEST: "leave_request",
    SCHEDULED_SHIFT: "scheduled_shift",
    TIME_ENTRY: "time_entry",
    TIMESHEET: "timesheet",
    PAYROLL: "payroll",
    AUTHOR: "author",
    PERMISSION: "permission",
    USER_ROLE: "user_role",
} as const;

export type Entity = (typeof ENTITY_TYPES)[keyof typeof ENTITY_TYPES];

export const CRUD_OPERATIONS = {
    CREATE: "create",
    READ: "read",
    UPDATE: "update",
    DELETE: "delete",
    SUBMIT: "submit",
    APPROVE: "approve",
    MANAGE: "manage",
} as const;

export type CRUDOperation = (typeof CRUD_OPERATIONS)[keyof typeof CRUD_OPERATIONS];

export type AppAbility = MongoAbility<[CRUDOperation, Entity]>;


export function defineAbilityFor(
    user: SessionUser | null,
    rolePermissions: Array<{ action: string; subject: string }>
): AppAbility {
    const { can, cannot, build } = new AbilityBuilder<AppAbility>(createMongoAbility);

    if (!user || !rolePermissions) {
        cannot(CRUD_OPERATIONS.MANAGE, ENTITY_TYPES.ALL);
        return build();
    }

    rolePermissions.forEach(({ action, subject }) => {
        can(action as CRUDOperation, subject as Entity);
    });

    return build();
}
