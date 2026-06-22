import { type LocationApiDTO, type Location } from "../../types/inventory/location";
import { createDataMapper } from "../dataMapper";

export const locationMapper = createDataMapper<LocationApiDTO, Location>({
    toDomain: (dto: LocationApiDTO) => ({
        id: dto.id,
        title: dto.title,
        address: dto.address,
        description: dto.description,
    }) as Location,
    toApi: (domain: Location) => ({
        id: domain.id,
        title: domain.title,
        address: domain.address,
        description: domain.description,
    }) as LocationApiDTO,
})