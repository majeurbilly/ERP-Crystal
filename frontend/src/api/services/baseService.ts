import apiClient from "../apiClient";

export class BaseService<T, CreateDTO = T> {
    protected endpoint: string;

    constructor(endpoint: string) {
        this.endpoint = endpoint;
    }

    async getAll(): Promise<T[]> {
        const response = await apiClient.get<T[]>(this.endpoint);
        return response.data;
    }

    async getById(id: string): Promise<T> {
        const response = await apiClient.get<T>(`${this.endpoint}/${id}`);
        return response.data;
    }

    async add(data: CreateDTO): Promise<T> {
        const response = await apiClient.post<T>(this.endpoint, data);
        return response.data;
    }

    async delete(id: string): Promise<void> {
        await apiClient.delete(`${this.endpoint}/${id}`);
    }

    async update(id: string, data: Partial<T>): Promise<T> {
        const response = await apiClient.put<T>(`${this.endpoint}/${id}`, data);
        return response.data;
    }
}