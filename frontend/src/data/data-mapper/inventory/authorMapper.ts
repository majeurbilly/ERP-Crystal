import type { Author, AuthorApiDTO } from "../../types/inventory/author";
import { createDataMapper } from "../dataMapper";

export const authorMapper = createDataMapper<AuthorApiDTO, Author>({
    toDomain: (dto: AuthorApiDTO) => ({
        id: dto.id,
        name: dto.name,
    }) as Author,
    toApi: (domain: Author) => ({
        id: domain.id,
        name: domain.name,
    })
})