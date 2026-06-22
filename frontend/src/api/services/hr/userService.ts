import { API_URL } from "../../apiBaseUrl";
import type { CreatedUserApiDTO, User, UserApiDTO, UserFormData } from "../../../data/types/hr/user";
import { BaseService } from "../baseService";
import apiClient from "../../apiClient";
import { userMapper } from "../../../data/data-mapper/hr/userMapper";

const USER_API_URL: string = `${API_URL}/users`

class UserService {

    private api = new BaseService<UserApiDTO, CreatedUserApiDTO>(USER_API_URL);

    async getAll(): Promise<User[]> {
        const rawData = await this.api.getAll();
        return userMapper.mapCollectionToDomain(rawData);
    }

    async getUserById(id: string): Promise<User> {
        const rawData = await this.api.getById(id);
        return userMapper.mapToDomain(rawData);
    }

    async add(user: UserFormData): Promise<User> {
        const payload = userMapper.mapToApi(user) as CreatedUserApiDTO;
        const response = await this.api.add(payload);
        return userMapper.mapToDomain(response);
    }

    async update(id: string, user: Partial<UserFormData>): Promise<User> {
        const payload = userMapper.mapToApi(user as UserFormData);
        const response = await this.api.update(id, payload as Partial<UserApiDTO>);
        return userMapper.mapToDomain(response);
    }

    async delete(id: string): Promise<void> {
        await this.api.delete(id);
    }

    async getMe(): Promise<User> {
        const response = await apiClient.get<UserApiDTO>("/users/me");
        return userMapper.mapToDomain(response.data);
    }

    async updateMe(user: Partial<UserFormData>): Promise<User> {
        const response = await apiClient.put<UserApiDTO>("/users/me", {
            email: user.email,
            userName: user.userName,
            ...(user.password ? { password: user.password } : {}),
        });
        return userMapper.mapToDomain(response.data);
    }
}

const userService = new UserService();
export default userService;