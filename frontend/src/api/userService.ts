import apiClient from "./apiClient";
import { MOCK_URL } from "./apiBaseUrl";
import type { User } from "../data/types/user";

const userApiUrl: string = `${MOCK_URL}/users`

export const getUsers = async (): Promise<User[]> => {
    const response = await apiClient.get<User[]>(userApiUrl);
    return response.data;
}

export const getUserById = async (id: string): Promise<User> => {
    const response = await apiClient.get<User>(`${userApiUrl}/${id}`);
    return response.data;
}