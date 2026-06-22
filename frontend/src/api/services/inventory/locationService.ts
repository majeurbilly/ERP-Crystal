import { API_LOCATIONS_URL } from "../../apiBaseUrl";
import { type LocationApiDTO, type Location } from "../../../data/types/inventory/location";
import { BaseService } from "../baseService";
import { locationMapper } from "../../../data/data-mapper/inventory/locationMapper";

const LOCATION_API_URL = API_LOCATIONS_URL;

class LocationService {
    private api = new BaseService<LocationApiDTO>(LOCATION_API_URL);

    async getAll(): Promise<Location[]> {
        const rawData = await this.api.getAll();
        return locationMapper.mapCollectionToDomain(rawData);
    }

    async getById(id: string): Promise<Location> {
        const rawData = await this.api.getById(id);
        return locationMapper.mapToDomain(rawData);
    }

    async add(data: Location): Promise<Location> {
        const response = await this.api.add(data);
        return locationMapper.mapToDomain(response);
    }

    async update(id: string, data: Partial<Location>): Promise<Location> {
        const response = await this.api.update(id, data as Partial<LocationApiDTO>);
        return locationMapper.mapToDomain(response);
    }

    async delete(id: string): Promise<void> {
        await this.api.delete(id);
    }
}

const locationService = new LocationService();
export default locationService;
