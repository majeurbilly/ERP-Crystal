export interface Mapper<ApiDTO, DomainType> {
    toDomain: (raw: ApiDTO) => DomainType;
    toApi: (domain: DomainType) => ApiDTO;
}

export const createDataMapper = <ApiDTO, DomainType>(
    mapper: Mapper<ApiDTO, DomainType>
) => ({
    mapToDomain: (data: ApiDTO): DomainType => mapper.toDomain(data),
    mapToApi: (data: DomainType): ApiDTO => mapper.toApi(data),
    mapCollectionToDomain: (data: ApiDTO[]): DomainType[] => data.map(mapper.toDomain),
    mapCollectionToApi: (data: DomainType[]): ApiDTO[] => data.map(mapper.toApi),
})