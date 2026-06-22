import type { InventoryQuantity, InventoryQuantityApiDTO } from "../../types/inventory/inventoryQuantity";
import { createDataMapper } from "../dataMapper";

export const quantityMapper = createDataMapper<InventoryQuantityApiDTO, InventoryQuantity>({
    toDomain: (dto: InventoryQuantityApiDTO) => ({
        id: `${dto.locationId}-${dto.itemId}`,
        itemId: dto.itemId,
        locationId: dto.locationId,
        quantity: dto.quantity,
    }) as InventoryQuantity,
    toApi: (domain: InventoryQuantity) => ({
        itemId: domain.itemId,
        locationId: domain.locationId,
        quantity: domain.quantity,
    }) as InventoryQuantityApiDTO,
});