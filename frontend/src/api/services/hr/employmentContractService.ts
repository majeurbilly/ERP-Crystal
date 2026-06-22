import { BaseService } from "../baseService";
import { API_CONTRACTS_URL } from "../../apiBaseUrl";
import type {
    EmploymentContract,
    EmploymentContractApiDto,
    EmploymentContractFormData,
} from "../../../data/types/hr/employmentContract";
import { CONTRACT_TYPES, WAGE_TYPES } from "../../../data/types/hr/employmentContract";
import {
    employmentContractMapper,
    mapEmploymentContractFormToApi,
    type EmploymentContractApiPayload,
} from "../../../data/data-mapper/hr/employmentContractMapper";

class EmploymentContractService {
    private m_api = new BaseService<EmploymentContractApiDto, EmploymentContractApiPayload>(
        API_CONTRACTS_URL
    );

    async getAll(): Promise<EmploymentContract[]> {
        const rawData: EmploymentContractApiDto[] = await this.m_api.getAll();
        return employmentContractMapper.mapCollectionToDomain(rawData);
    }

    async getById(p_id: string): Promise<EmploymentContract> {
        const rawData: EmploymentContractApiDto = await this.m_api.getById(p_id);
        return employmentContractMapper.mapToDomain(rawData);
    }

    async getByEmployeeId(p_employeeProfileId: number): Promise<EmploymentContract[]> {
        const allContracts: EmploymentContract[] = await this.getAll();
        return allContracts.filter(
            (p_contract: EmploymentContract) => p_contract.employeeProfileId === p_employeeProfileId
        );
    }

    async add(p_data: EmploymentContractFormData): Promise<EmploymentContract> {
        const payload: EmploymentContractApiPayload = mapEmploymentContractFormToApi(p_data);
        const response: EmploymentContractApiDto = await this.m_api.add(payload);
        return employmentContractMapper.mapToDomain(response);
    }

    async update(
        p_id: string,
        p_data: Partial<EmploymentContractFormData>
    ): Promise<EmploymentContract> {
        const payload: EmploymentContractApiPayload = mapEmploymentContractFormToApi({
            employeeProfileId: p_data.employeeProfileId ?? 0,
            contractType: p_data.contractType ?? CONTRACT_TYPES.FullTime,
            wageType: p_data.wageType ?? WAGE_TYPES.Monthly,
            baseRate: p_data.baseRate ?? 0,
            startDate: p_data.startDate ?? "",
            endDate: p_data.endDate ?? null,
        });
        const response: EmploymentContractApiDto = await this.m_api.update(
            p_id,
            payload as unknown as Partial<EmploymentContractApiDto>
        );
        return employmentContractMapper.mapToDomain(response);
    }

    async delete(p_id: string): Promise<void> {
        await this.m_api.delete(p_id);
    }
}

const employmentContractService = new EmploymentContractService();
export default employmentContractService;
