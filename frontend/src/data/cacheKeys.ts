export const HR_METRICS_CACHE_KEY = ["hrMetrics", "dashboard"] as const;

export const hrMetricsCacheKey = {
    all: ["hrMetrics"] as const,
    dashboard: () => HR_METRICS_CACHE_KEY,
};

export const jobPositionsCacheKey = {
    all: ["jobPositions"] as const,
    list: () => [...jobPositionsCacheKey.all, "list"] as const,
    details: (p_id: string) => [...jobPositionsCacheKey.all, "details", p_id] as const,
};

export const employeeProfilesCacheKey = {
    all: ["employeeProfiles"] as const,
    list: () => [...employeeProfilesCacheKey.all, "list"] as const,
    me: () => [...employeeProfilesCacheKey.all, "me"] as const,
    details: (p_id: string) => [...employeeProfilesCacheKey.all, "details", p_id] as const,
    byApplicationUserId: (p_userId: string) =>
        [...employeeProfilesCacheKey.all, "byApplicationUser", p_userId] as const,
};

export const scheduledShiftsCacheKey = {
    all: ["scheduledShifts"] as const,
    list: () => [...scheduledShiftsCacheKey.all, "list"] as const,
    teamList: () => [...scheduledShiftsCacheKey.all, "team"] as const,
    details: (p_id: string) => [...scheduledShiftsCacheKey.all, "details", p_id] as const,
};

export const timeEntriesCacheKey = {
    all: ["timeEntries"] as const,
    list: () => [...timeEntriesCacheKey.all, "list"] as const,
    active: () => [...timeEntriesCacheKey.all, "me", "active"] as const,
    punchEligibility: () => [...timeEntriesCacheKey.all, "me", "punch-eligibility"] as const,
    details: (p_id: string) => [...timeEntriesCacheKey.all, "details", p_id] as const,
};

export const timesheetsCacheKey = {
    all: ["timesheets"] as const,
    list: () => [...timesheetsCacheKey.all, "list"] as const,
    details: (p_id: string) => [...timesheetsCacheKey.all, "details", p_id] as const,
};

export const payStubsCacheKey = {
    all: ["payStubs"] as const,
    list: () => [...payStubsCacheKey.all, "list"] as const,
};

export const payPeriodsCacheKey = {
    all: ["payPeriods"] as const,
    list: () => [...payPeriodsCacheKey.all, "list"] as const,
};

export const leaveRequestsCacheKey = {
    all: ["leaveRequests"] as const,
    list: () => [...leaveRequestsCacheKey.all, "list"] as const,
    details: (p_id: string) => [...leaveRequestsCacheKey.all, "details", p_id] as const,
};

export const employmentContractsCacheKey = {
    all: ["employmentContracts"] as const,
    list: () => [...employmentContractsCacheKey.all, "list"] as const,
    byEmployee: (p_employeeId: string) =>
        [...employmentContractsCacheKey.all, "employee", p_employeeId] as const,
    details: (p_id: string) => [...employmentContractsCacheKey.all, "details", p_id] as const,
};

export const usersCacheKey = {
    all: ['users'] as const,
    list: () => [...usersCacheKey.all, 'list'] as const,
    details: (id: string) => [...usersCacheKey.all, 'details', id] as const,
    me: () => [...usersCacheKey.all, 'me'] as const,
};

export interface ItemListFilters {
    categoryIds?: number[];
    authorId?: number;
}

export const itemsCacheKey = {
    all: ['items'] as const,
    list: (p_filters?: ItemListFilters) => [...itemsCacheKey.all, 'list', p_filters ?? {}] as const,
    details: (id: string) => [...itemsCacheKey.all, 'details', id] as const,
};

export const locationsCacheKey = {
    all: ['locations'] as const,
    list: () => [...locationsCacheKey.all, 'list'] as const,
    details: (id: string) => [...locationsCacheKey.all, 'details', id] as const,
};

export const categoriesCachekey = {
    all: ['categories'] as const,
    list: () => [...categoriesCachekey.all, 'list'] as const,
    details: (id: string) => [...categoriesCachekey.all, 'details', id] as const,
};

export const inventoryQuantityCacheKey = {
    all: ['inventory'] as const,
    locationLines: (locationId: number) => [...inventoryQuantityCacheKey.all, 'location', locationId, 'lines'] as const,
    locationGrid: (locationId: number) => [...inventoryQuantityCacheKey.all, 'location', locationId, 'grid'] as const,
    itemLines: (itemId: number) => [...inventoryQuantityCacheKey.all, 'item', itemId, 'lines'] as const,
};

export const authorsCachekey = {
    all: ['authors'] as const,
    list: () => [...authorsCachekey.all, 'list'] as const,
    details: (id: string) => [...authorsCachekey.all, 'details', id] as const,
}
export const userRolesCacheKey = {
    all: ["userRoles"] as const,
    list: () => [...userRolesCacheKey.all, "list"] as const,
    details: (id: string) => [...userRolesCacheKey.all, "details", id] as const,
};

export const permissionEntitiesCacheKey = {
    all: ["permissionEntities"] as const,
    list: () => [...permissionEntitiesCacheKey.all, "list"] as const,
    details: (id: string) => [...permissionEntitiesCacheKey.all, "details", id] as const,
};
