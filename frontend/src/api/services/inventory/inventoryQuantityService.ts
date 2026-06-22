import type {
    InventoryLine,
    InventoryLineApiDTO,
    InventoryQuantity,
    InventoryQuantityApiDTO,
} from "../../../data/types/inventory/inventoryQuantity";
import { API_URL } from "../../apiBaseUrl";
import { BaseService } from "../baseService";
import { quantityMapper } from "../../../data/data-mapper/inventory/quantityMapper";
import apiClient from "../../apiClient";

export type LocationQuantityMap = Record<number | string, number>;

const QUANTITY_API_URL: string = `${API_URL}/inventory`;
const QUANTITY_UPDATE_URL: string = `${API_URL}/inventory/quantity`;

function mapLineToDomain(p_dto: InventoryLineApiDTO): InventoryLine {
    return {
        locationId: p_dto.locationId,
        locationTitle: p_dto.locationTitle,
        itemId: p_dto.itemId,
        itemName: p_dto.itemName,
        quantity: p_dto.quantity,
    };
}

class InventoryQuantityService {
    private api = new BaseService<InventoryQuantityApiDTO>(QUANTITY_API_URL);

    async getLines(p_params?: { locationId?: number; itemId?: number }): Promise<InventoryLine[]> {
        const response = await apiClient.get<InventoryLineApiDTO[]>(QUANTITY_API_URL, {
            params: {
                p_locationId: p_params?.locationId,
                p_itemId: p_params?.itemId,
            },
        });

        return response.data.map(mapLineToDomain);
    }

    async getLinesByLocation(p_locationId: number): Promise<InventoryLine[]> {
        return this.getLines({ locationId: p_locationId });
    }

    async getLinesByItem(p_itemId: number): Promise<InventoryLine[]> {
        return this.getLines({ itemId: p_itemId });
    }

    async getAll(): Promise<InventoryQuantity[]> {
        const lines = await this.getLines();
        return lines.map((p_line) => quantityMapper.mapToDomain({
            locationId: p_line.locationId,
            itemId: p_line.itemId,
            quantity: p_line.quantity,
        }));
    }

    async getByCompositeId(p_compositeId: string): Promise<InventoryQuantity> {
        const [locationId, itemId] = p_compositeId.split("-");

        if (!locationId || !itemId) {
            throw new Error(`Format d'ID composite invalide : ${p_compositeId}. Attendu : 'locationId-itemId'`);
        }

        const response = await apiClient.get<InventoryQuantityApiDTO>(
            `${API_URL}/inventory/locations/${locationId}/items/${itemId}`
        );

        return quantityMapper.mapToDomain(response.data);
    }

    async getByLocation(p_locationId: number): Promise<InventoryQuantity[]> {
        const lines = await this.getLinesByLocation(p_locationId);
        return lines.map((p_line) => quantityMapper.mapToDomain({
            locationId: p_line.locationId,
            itemId: p_line.itemId,
            quantity: p_line.quantity,
        }));
    }

    async upsert(p_payload: InventoryQuantityApiDTO): Promise<void> {
        await apiClient.put(QUANTITY_UPDATE_URL, p_payload);
    }

    async add(p_quantity: InventoryQuantity): Promise<InventoryQuantityApiDTO> {
        const payload = quantityMapper.mapToApi(p_quantity) as InventoryQuantityApiDTO;
        const response = await this.api.add(payload);
        return quantityMapper.mapToDomain(response);
    }

    async delete(p_id: string): Promise<void> {
        const [locationId, itemId] = p_id.split("-");

        await apiClient.delete(QUANTITY_API_URL, { params: { locationId, itemId } });
    }

    async update(p_id: string, p_quantityData: Partial<Omit<InventoryQuantity, "id">>): Promise<InventoryQuantity> {
        const [locationIdStr, itemIdStr] = p_id.split("-");
        const locationId = Number(locationIdStr);
        const itemId = Number(itemIdStr);
        const fullDomainData = {
            id: p_id,
            locationId,
            itemId,
            ...p_quantityData,
        } as InventoryQuantity;
        const payload = quantityMapper.mapToApi(fullDomainData);

        await this.upsert(payload);
        return quantityMapper.mapToDomain(payload);
    }
}

const inventoryQuantityService = new InventoryQuantityService();
export default inventoryQuantityService;
