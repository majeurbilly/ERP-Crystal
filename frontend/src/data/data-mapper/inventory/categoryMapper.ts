import type { Category, CategoryApiDTO } from "../../types/inventory/category";
import { createDataMapper } from "../dataMapper";

export const categoryMapper = createDataMapper<CategoryApiDTO, Category>({
    toDomain: (dto: CategoryApiDTO) => ({
        id: dto.id,
        name: dto.name,
    }) as Category,
    toApi: (domain: Category) => ({
        id: domain.id,
        name: domain.name,
    }) as CategoryApiDTO,
});