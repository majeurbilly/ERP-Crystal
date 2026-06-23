export const ROUTE_ROOT: string = "/";
export const ROUTE_DASHBOARD: string = "/dashboard";
export const ROUTE_HR: string = "/rh";
export const ROUTE_IR: string = "/ir";
export const ROUTE_MY_PROFILE: string = "/monprofil"
export const ROUTE_MON_ESPACE: string = "/mon-espace";
export const ROUTE_LIST_USERS: string = `${ROUTE_HR}/utilisateurs`
export const ROUTE_JOB_POSITIONS: string = `${ROUTE_HR}/referentiels/postes`;
export const ROUTE_EMPLOYEE_PROFILES: string = `${ROUTE_HR}/employes`;
export const ROUTE_EMPLOYEE_PROFILE_DETAILS: string = `${ROUTE_EMPLOYEE_PROFILES}/:id`;
export const ROUTE_EMPLOYMENT_CONTRACTS: string = `${ROUTE_HR}/contrats-de-travail`;
export const ROUTE_LEAVE_REQUESTS: string = `${ROUTE_HR}/absences`;
export const ROUTE_LEAVE_REQUEST_DETAILS: string = `${ROUTE_LEAVE_REQUESTS}/:id`;

export function buildLeaveRequestDetailsPath(p_id: number | string): string {
    return ROUTE_LEAVE_REQUEST_DETAILS.replace(":id", String(p_id));
}
export const ROUTE_SCHEDULES: string = `${ROUTE_HR}/planning`;
export const ROUTE_TIME_ENTRIES: string = `${ROUTE_HR}/pointages`;
export const ROUTE_TIMESHEETS: string = `${ROUTE_HR}/feuilles-de-temps`;
export const ROUTE_TIMESHEET_DETAILS: string = `${ROUTE_TIMESHEETS}/:id`;
export const ROUTE_PAYROLL: string = `${ROUTE_HR}/paie`;
export const ROUTE_USER_PROFILE: string = `${ROUTE_LIST_USERS}/:id`;
export const ROUTE_CATALOGUE: string = `/catalogue`;
export const ROUTE_ITEM_DETAILS: string = `${ROUTE_CATALOGUE}/:id`;
export const ROUTE_CATEGORY: string = "/livres/categories";
export const ROUTE_CATEGORY_DETAILS: string = `${ROUTE_CATEGORY}/:id`;
export const ROUTE_LOCATIONS: string = "/succursales";
export const ROUTE_LOCATION_DETAILS: string = `${ROUTE_LOCATIONS}/:id`;
export const ROUTE_LOCATION_INVENTORY: string = `${ROUTE_LOCATION_DETAILS}/inventaire`;

export function buildLocationInventoryPath(p_id: number | string): string {
    return ROUTE_LOCATION_INVENTORY.replace(":id", String(p_id));
}
export const ROUTE_LIST_AUTHORS: string = '/auteurs'
export const ROUTE_LIST_USER_ROLES: string = "/roles";
export const ROUTE_USER_ROLE_DETAILS: string = `${ROUTE_LIST_USER_ROLES}/:id`
export const ROUTE_LIST_PERMISSION_ENTITIES: string = `/permission-entities`
export const ROUTE_AUTHOR_DETAILS: string = `${ROUTE_LIST_AUTHORS}/:id`