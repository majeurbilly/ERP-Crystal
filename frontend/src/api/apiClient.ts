import axios, {
	type AxiosInstance,
	type AxiosResponse,
	type InternalAxiosRequestConfig
} from 'axios';
import { MOCK_URL } from './apiBaseUrl';

const apiClient: AxiosInstance = axios.create({
	baseURL: MOCK_URL,
	timeout: 1000,
	headers: {
		'Content-Type': 'application/json',
	},
});

apiClient.interceptors.request.use(
	(config: InternalAxiosRequestConfig) => {
		const token = localStorage.getItem("token");
		if (token && config.headers) {
			config.headers.Authorization = `Bearer ${token}`;
		}
		return config
	},
	(error) => Promise.reject(error)
);

apiClient.interceptors.response.use(
	(response: AxiosResponse) => response,
	(error) => {
		if (error.response?.status === 401) {
			console.error(`Unauthorized, logging out...`);
		}
		return Promise.reject(error);
	}
);

export default apiClient;