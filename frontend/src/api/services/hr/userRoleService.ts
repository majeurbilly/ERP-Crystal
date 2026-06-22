import { userRoleMapper } from "../../../data/data-mapper/hr/userRoleMapper";
import type { CreateDynamicUserRolePayload, DynamicUserRole, DynamicUserRoleApiDTO } from "../../../data/types/hr/dynamicUserRole";
import { getDefaultRoleById } from "../../../permissions/defaultRolePermissions";
import { BaseService } from "../baseService";
import { API_URL } from "../../apiBaseUrl";
import apiClient from "../../apiClient";

const ROLES_API_URL: string = `${API_URL}/roles`;

class UserRoleService {
    private api = new BaseService<DynamicUserRoleApiDTO, DynamicUserRole>(ROLES_API_URL);

    async getAll(): Promise<DynamicUserRole[]> {
        const rawData = await this.api.getAll();
        return userRoleMapper.mapCollectionToDomain(rawData);
    }

    async getById(id: string): Promise<DynamicUserRole> {
        try {
            const rawData = await this.api.getById(id);
            return userRoleMapper.mapToDomain(rawData);
        } catch {
            const preset = getDefaultRoleById(id);
            if (preset) {
                return preset;
            }
            throw new Error(`Rôle introuvable : ${id}`);
        }
    }

    async add(userRole: DynamicUserRole, presetId?: string): Promise<DynamicUserRole> {
        const payload: CreateDynamicUserRolePayload = {
            name: userRole.name,
            permissions: userRole.permissions,
            ...(presetId ? { presetId } : {}),
        };
        const response = await apiClient.post<DynamicUserRoleApiDTO>(ROLES_API_URL, payload);
        return userRoleMapper.mapToDomain(response.data);
    }

    async update(id: string, userRole: Partial<DynamicUserRole>): Promise<DynamicUserRole> {
        const payload = {
            name: userRole.name,
            permissions: userRole.permissions ?? [],
        };
        const response = await apiClient.put<DynamicUserRoleApiDTO>(`${ROLES_API_URL}/${id}`, payload);
        return userRoleMapper.mapToDomain(response.data);
    }

    async delete(id: string): Promise<void> {
        await this.api.delete(id);
    }
}

const userRoleService = new UserRoleService();
export default userRoleService;
