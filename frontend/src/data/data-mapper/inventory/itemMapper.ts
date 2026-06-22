import { type ItemApiDTO, type Item } from "../../types/inventory/item";
import { createDataMapper } from "../dataMapper";
import { resolveAssetUrl } from "../../utils/resolveAssetUrl";

export const itemMapper = createDataMapper<ItemApiDTO, Item>({
    toDomain: (dto: ItemApiDTO): Item => ({
        id: dto.id,
        name: dto.name,
        description: dto.description,
        imageUrl: resolveAssetUrl(dto.imageUrl),
        price: dto.price,
        totalQuantity: dto.totalQuantity,
        alertQuantity: dto.alertQuantity,
        isLowStock: dto.isLowStock,
        lastUpdate: dto.lastUpdate,
        isBook: dto.isBook,
        isActive: dto.isActive,
        distributor: dto.distributor ?? null,
        isbn: dto.isbn ?? null,
        publicationDate: dto.publicationDate ?? null,
        authors: dto.authors ?? [],
        authorIds: dto.authorIds ?? [],
        publishers: dto.publishers ?? [],
        categories: dto.categories ?? [],
        categoryIds: dto.categoryIds ?? [],
    }),
    toApi: (domain: Item): ItemApiDTO => ({ ...domain }),
});
