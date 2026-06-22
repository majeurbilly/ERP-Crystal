import apiClient from "../apiClient";
import { API_URL } from "../apiBaseUrl";
import { resolveAssetUrl } from "../../data/utils/resolveAssetUrl";

const IMAGE_UPLOAD_URL: string = `${API_URL}/items/:itemId/image`;

interface ItemImageUploadResponse {
    imageUrl?: string | null;
    ImageUrl?: string | null;
}

function extractImageUrl(p_data: ItemImageUploadResponse): string | null {
    return p_data.imageUrl ?? p_data.ImageUrl ?? null;
}

class ImageService {
    async upload(file: File, itemId: number | string): Promise<string> {
        const formData = new FormData();
        formData.append("p_file", file);

        const url: string = IMAGE_UPLOAD_URL.replace(":itemId", String(itemId));
        const response = await apiClient.post<ItemImageUploadResponse>(url, formData);

        const imageUrl = resolveAssetUrl(extractImageUrl(response.data));
        if (!imageUrl) {
            throw new Error("Réponse serveur sans URL d'image.");
        }

        return imageUrl;
    }
}

export const imageService = new ImageService();
