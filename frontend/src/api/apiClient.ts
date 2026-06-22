import axios, {
	type AxiosInstance,
	type AxiosResponse,
	type InternalAxiosRequestConfig
} from 'axios';
import { API_URL } from './apiBaseUrl';
import { notifySessionExpired } from './sessionUtils';

const apiClient: AxiosInstance = axios.create({
	baseURL: API_URL,
	timeout: 10000,
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

		if (config.data instanceof FormData && config.headers) {
			delete config.headers["Content-Type"];
		}

		return config
	},
	(error) => Promise.reject(error)
);

apiClient.interceptors.response.use(
	(response: AxiosResponse) => response,
	(error) => {
		if (error.response?.status === 401) {
			notifySessionExpired();
		}
		return Promise.reject(error);
	}
);

export default apiClient;
