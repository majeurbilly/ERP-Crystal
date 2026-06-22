import type { JobPosition, JobPositionApiDto, JobPositionFormData } from "../../types/hr/jobPosition";
import { createDataMapper } from "../dataMapper";

export const jobPositionMapper = createDataMapper<JobPositionApiDto, JobPosition>({
    toDomain: (p_dto: JobPositionApiDto): JobPosition => ({
        id: p_dto.id,
        name: p_dto.name,
        description: p_dto.description,
        color: p_dto.color ?? "#3B82F6",
        isDeleted: false,
    }),
    toApi: (p_domain: JobPosition): JobPositionApiDto => ({
        id: p_domain.id,
        name: p_domain.name,
        description: p_domain.description,
        color: p_domain.color,
    }),
});

export function mapJobPositionFormToApi(p_form: JobPositionFormData): JobPositionFormData {
    return {
        name: p_form.name.trim(),
        description: p_form.description.trim(),
        color: (p_form.color ?? "#3B82F6").trim(),
    };
}
