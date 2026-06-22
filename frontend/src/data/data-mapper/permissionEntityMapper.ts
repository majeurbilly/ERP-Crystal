import type { PermissionEntityApiDTO, PermissionEntity } from "../types/permissionEntity";
import { createDataMapper } from "./dataMapper";

export const permissionEntityMapper = createDataMapper<PermissionEntityApiDTO, PermissionEntity>({
    toDomain: (dto: PermissionEntityApiDTO) => ({
        id: dto.id
    }) as PermissionEntity,
    toApi: (domain: PermissionEntity) => ({
        id: domain.id
    }) as PermissionEntityApiDTO,
});