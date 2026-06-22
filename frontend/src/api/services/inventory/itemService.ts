import { API_URL } from "../../apiBaseUrl";
import type { ItemApiDTO, CreateItemRequest, CreateBookRequest, Item } from "../../../data/types/inventory/item";
import { BaseService } from "../baseService";
import { itemMapper } from "../../../data/data-mapper/inventory/itemMapper";
import apiClient from "../../apiClient";

const ITEMS_API_URL: string = `${API_URL}/items`
const BOOKS_API_URL: string = `${ITEMS_API_URL}/books`;

export interface ItemQueryParams {
    search?: string;
    publisherId?: number;
    categoryIds?: number[];
    authorId?: number;
    isBook?: boolean;
}

class ItemService {
    private api = new BaseService<ItemApiDTO, CreateItemRequest | CreateBookRequest>(ITEMS_API_URL);
    private bookApi = new BaseService<ItemApiDTO, CreateBookRequest>(BOOKS_API_URL);

    async getAll(p_params?: ItemQueryParams): Promise<Item[]> {
        const response = await apiClient.get<ItemApiDTO[]>(ITEMS_API_URL, {
            params: {
                p_search: p_params?.search,
                p_publisherId: p_params?.publisherId,
                p_categoryIds: p_params?.categoryIds,
                p_authorId: p_params?.authorId,
                p_isBook: p_params?.isBook,
            },
            paramsSerializer: {
                indexes: null,
            },
        });
        return itemMapper.mapCollectionToDomain(response.data);
    }

    async getById(id: string): Promise<Item> {
        const rawData = await this.api.getById(id);
        return itemMapper.mapToDomain(rawData);
    }

    async add(item: Item | CreateItemRequest | CreateBookRequest): Promise<Item> {
        if ("publicationDate" in item || ("isBook" in item && item.isBook)) {
            const response = await this.bookApi.add(item as CreateBookRequest);
            return itemMapper.mapToDomain(response);
        }

        const response = await this.api.add(item as CreateItemRequest);
        return itemMapper.mapToDomain(response);
    }

    async update(id: string, item: Partial<Item>): Promise<Item> {
        const response = await this.api.update(id, item as Partial<ItemApiDTO>);
        return itemMapper.mapToDomain(response);
    }

    async delete(id: string): Promise<void> {
        await this.api.delete(id);
    }
}

const itemService = new ItemService();
export default itemService;
