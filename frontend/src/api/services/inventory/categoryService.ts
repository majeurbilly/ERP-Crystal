import { BaseService } from "../baseService";
import type { Category, CategoryApiDTO } from "../../../data/types/inventory/category";
import { API_URL } from "../../apiBaseUrl";
import { categoryMapper } from "../../../data/data-mapper/inventory/categoryMapper";

const CATEGORY_API_URL: string = `${API_URL}/categories`;

class CategoryService {
    private api = new BaseService<CategoryApiDTO>(CATEGORY_API_URL);

    async getAll(): Promise<Category[]> {
        const rawData = await this.api.getAll();
        return categoryMapper.mapCollectionToDomain(rawData);
    }

    async getById(id: string): Promise<Category> {
        const rawData = await this.api.getById(id);
        return categoryMapper.mapToDomain(rawData);
    }

    async add(category: Category): Promise<Category> {
        const payload = categoryMapper.mapToApi(category);
        const response = await this.api.add(payload);
        return categoryMapper.mapToDomain(response);
    }

    async update(id: string, category: Partial<Category>): Promise<Category> {
        const payload = categoryMapper.mapToApi(category as Category);
        const response = await this.api.update(id, payload as Partial<CategoryApiDTO>);
        return categoryMapper.mapToDomain(response);
    }

    async delete(id: string): Promise<void> {
        await this.api.delete(id);
    }
}
const categoryService = new CategoryService();
export default categoryService;