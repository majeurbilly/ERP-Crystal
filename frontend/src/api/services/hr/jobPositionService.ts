import { BaseService } from "../baseService";
import { API_JOB_POSITIONS_URL } from "../../apiBaseUrl";
import type { JobPosition, JobPositionApiDto, JobPositionFormData } from "../../../data/types/hr/jobPosition";
import { jobPositionMapper } from "../../../data/data-mapper/hr/jobPositionMapper";

class JobPositionService {
    private m_api = new BaseService<JobPositionApiDto, JobPositionFormData>(API_JOB_POSITIONS_URL);

    async getAll(): Promise<JobPosition[]> {
        const rawData: JobPositionApiDto[] = await this.m_api.getAll();
        return jobPositionMapper.mapCollectionToDomain(rawData);
    }

    async getById(p_id: string): Promise<JobPosition> {
        const rawData: JobPositionApiDto = await this.m_api.getById(p_id);
        return jobPositionMapper.mapToDomain(rawData);
    }

    async add(p_data: JobPositionFormData): Promise<JobPosition> {
        const response: JobPositionApiDto = await this.m_api.add(p_data);
        return jobPositionMapper.mapToDomain(response);
    }

    async update(p_id: string, p_data: Partial<JobPositionFormData>): Promise<JobPosition> {
        const response: JobPositionApiDto = await this.m_api.update(p_id, p_data);
        return jobPositionMapper.mapToDomain(response);
    }

    async delete(p_id: string): Promise<void> {
        await this.m_api.delete(p_id);
    }
}

const jobPositionService = new JobPositionService();
export default jobPositionService;
