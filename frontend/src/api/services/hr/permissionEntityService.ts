import type { PermissionEntity, PermissionEntityApiDTO } from "../../../data/types/permissionEntity";
import { ENTITY_TYPES } from "../../../permissions/permissions";
import { BaseService } from "../baseService";
import { API_URL } from "../../apiBaseUrl";

const PERMISSION_ENTITIES_API_URL: string = `${API_URL}/permission-entities`;

const FALLBACK_ENTITIES: PermissionEntity[] = Object.values(ENTITY_TYPES)
    .filter((p_value) => p_value !== ENTITY_TYPES.ALL && p_value !== ENTITY_TYPES.ME)
    .map((p_id) => ({ id: p_id }));

class PermissionEntityService {
    private api = new BaseService<PermissionEntityApiDTO, PermissionEntity>(PERMISSION_ENTITIES_API_URL);

    async getAll(): Promise<PermissionEntity[]> {
        try {
            const rawData = await this.api.getAll();
            return rawData.map((p_dto) => ({ id: p_dto.id }));
        } catch {
            return FALLBACK_ENTITIES;
        }
    }

    async getById(id: string): Promise<PermissionEntity> {
        const entities = await this.getAll();
        const entity = entities.find((p_item) => p_item.id === id);
        if (!entity) {
            throw new Error(`Entité de permission introuvable : ${id}`);
        }
        return entity;
    }
}

const permissionEntityService = new PermissionEntityService();
export default permissionEntityService;
