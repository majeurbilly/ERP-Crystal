import { BaseService } from "../baseService";
import type { Author, AuthorApiDTO } from "../../../data/types/inventory/author";
import { API_AUTHORS_URL } from "../../apiBaseUrl";
import { authorMapper } from "../../../data/data-mapper/inventory/authorMapper";

class AuthorService {
    private api = new BaseService<AuthorApiDTO>(API_AUTHORS_URL);

    async getAll(): Promise<Author[]> {
        const rawData = await this.api.getAll();
        return authorMapper.mapCollectionToDomain(rawData);
    }

    async getById(id: string): Promise<Author> {
        const rawData = await this.api.getById(id);
        return authorMapper.mapToDomain(rawData);
    }

    async add(author: Author): Promise<Author> {
        const payload = authorMapper.mapToApi(author);
        const response = await this.api.add(payload);
        return authorMapper.mapToDomain(response);
    }

    async update(id: string, author: Partial<Author>): Promise<Author> {
        const payload = authorMapper.mapToApi(author as Author);
        const response = await this.api.update(id, payload as Partial<AuthorApiDTO>);
        return authorMapper.mapToDomain(response);
    }

    async delete(id: string): Promise<void> {
        await this.api.delete(id);
    }
}

const authorService = new AuthorService();
export default authorService;
