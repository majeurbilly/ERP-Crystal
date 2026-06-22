import type {
    EmploymentContract,
    EmploymentContractApiDto,
    EmploymentContractFormData,
} from "../../types/hr/employmentContract";
import {
    CONTRACT_TYPES,
    WAGE_TYPES,
    type ContractType,
    type WageType,
} from "../../types/hr/employmentContract";
import { createDataMapper } from "../dataMapper";

const CONTRACT_TYPE_API_VALUES: Record<ContractType, number> = {
    [CONTRACT_TYPES.FullTime]: 0,
    [CONTRACT_TYPES.PartTime]: 1,
    [CONTRACT_TYPES.Internship]: 2,
    [CONTRACT_TYPES.SelfEmployed]: 3,
};

const WAGE_TYPE_API_VALUES: Record<WageType, number> = {
    [WAGE_TYPES.Monthly]: 0,
    [WAGE_TYPES.Fixed]: 1,
};

const LEGACY_CONTRACT_TYPES: Record<string, ContractType> = {
    CDI: CONTRACT_TYPES.FullTime,
    CDD: CONTRACT_TYPES.PartTime,
    Stage: CONTRACT_TYPES.Internship,
    Freelance: CONTRACT_TYPES.SelfEmployed,
};

const LEGACY_WAGE_TYPES: Record<string, WageType> = {
    Hourly: WAGE_TYPES.Monthly,
    Annual: WAGE_TYPES.Fixed,
};

function normalizeDate(p_date: string | null | undefined): string | null {
    if (p_date === null || p_date === undefined || p_date.trim().length === 0) {
        return null;
    }
    return p_date.length >= 10 ? p_date.substring(0, 10) : p_date;
}

function parseContractType(p_value: string): ContractType {
    const values: ContractType[] = Object.values(CONTRACT_TYPES);
    const match: ContractType | undefined = values.find((p_type: ContractType) => p_type === p_value);
    if (match) {
        return match;
    }
    return LEGACY_CONTRACT_TYPES[p_value] ?? CONTRACT_TYPES.FullTime;
}

function parseWageType(p_value: string): WageType {
    const values: WageType[] = Object.values(WAGE_TYPES);
    const match: WageType | undefined = values.find((p_type: WageType) => p_type === p_value);
    if (match) {
        return match;
    }
    return LEGACY_WAGE_TYPES[p_value] ?? WAGE_TYPES.Fixed;
}

export const employmentContractMapper = createDataMapper<EmploymentContractApiDto, EmploymentContract>({
    toDomain: (p_dto: EmploymentContractApiDto): EmploymentContract => ({
        id: p_dto.id,
        employeeProfileId: p_dto.employeeProfileId,
        employeeFirstName: p_dto.employeeFirstName,
        employeeLastName: p_dto.employeeLastName,
        contractType: parseContractType(p_dto.contractType),
        wageType: parseWageType(p_dto.wageType),
        baseRate: p_dto.baseRate,
        startDate: normalizeDate(p_dto.startDate) ?? "",
        endDate: normalizeDate(p_dto.endDate),
        isDeleted: false,
    }),
    toApi: (p_domain: EmploymentContract): EmploymentContractApiDto => ({
        id: p_domain.id,
        employeeProfileId: p_domain.employeeProfileId,
        employeeFirstName: p_domain.employeeFirstName,
        employeeLastName: p_domain.employeeLastName,
        contractType: p_domain.contractType,
        wageType: p_domain.wageType,
        baseRate: p_domain.baseRate,
        startDate: normalizeDate(p_domain.startDate) ?? "",
        endDate: normalizeDate(p_domain.endDate),
    }),
});

export interface EmploymentContractApiPayload {
    employeeProfileId: number;
    contractType: number;
    wageType: number;
    baseRate: number;
    startDate: string;
    endDate: string | null;
}

export function mapEmploymentContractFormToApi(
    p_form: EmploymentContractFormData
): EmploymentContractApiPayload {
    return {
        employeeProfileId: p_form.employeeProfileId,
        contractType: CONTRACT_TYPE_API_VALUES[p_form.contractType],
        wageType: WAGE_TYPE_API_VALUES[p_form.wageType],
        baseRate: p_form.baseRate,
        startDate: normalizeDate(p_form.startDate) ?? "",
        endDate: normalizeDate(p_form.endDate),
    };
}
