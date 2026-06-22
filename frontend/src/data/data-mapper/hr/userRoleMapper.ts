import type { DynamicUserRole, DynamicUserRoleApiDTO } from "../../types/hr/dynamicUserRole";
import { createDataMapper } from "../dataMapper";

export const userRoleMapper = createDataMapper<DynamicUserRoleApiDTO, DynamicUserRole>({
    toDomain: (dto: DynamicUserRoleApiDTO): DynamicUserRole => ({
        id: dto.id,
        name: dto.name,
        isPreset: dto.isPreset ?? false,
        permissions: dto.permissions ?? [],
    }),

    toApi: (domain: DynamicUserRole): DynamicUserRoleApiDTO => ({
        id: domain.id,
        name: domain.name,
        isPreset: domain.isPreset,
        permissions: domain.permissions,
    })
});