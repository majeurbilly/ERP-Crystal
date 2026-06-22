import apiClient from "../../apiClient";
import { API_PAYROLL_URL } from "../../apiBaseUrl";
import type {
    GeneratePayrollForPeriodRequest,
    GeneratePayrollForPeriodResponseDto,
    GeneratePayrollForPeriodResult,
    GeneratePayrollRequest,
    PayStub,
    PayStubResponseDto,
} from "../../../data/types/hr/payStub";
import type { CreatePayPeriodRequest, PayPeriod, PayPeriodApiDto } from "../../../data/types/hr/payPeriod";
import {
    mapPayStubCollectionToDomain,
    mapPayStubDtoToDomain
} from "../../../data/data-mapper/hr/payStubMapper";
import {
    mapPayPeriodCollectionToDomain,
    mapPayPeriodDtoToDomain
} from "../../../data/data-mapper/hr/payPeriodMapper";

class PayrollService {
    async getStubs(): Promise<PayStub[]> {
        const response = await apiClient.get<PayStubResponseDto[]>(`${API_PAYROLL_URL}/stubs`);
        return mapPayStubCollectionToDomain(response.data);
    }

    async getPeriods(): Promise<PayPeriod[]> {
        const response = await apiClient.get<PayPeriodApiDto[]>(`${API_PAYROLL_URL}/periods`);
        return mapPayPeriodCollectionToDomain(response.data);
    }

    async createPeriod(p_data: CreatePayPeriodRequest): Promise<PayPeriod> {
        const response = await apiClient.post<PayPeriodApiDto>(`${API_PAYROLL_URL}/periods`, p_data);
        return mapPayPeriodDtoToDomain(response.data);
    }

    async generatePayStub(p_data: GeneratePayrollRequest): Promise<PayStub> {
        const response = await apiClient.post<PayStubResponseDto>(
            `${API_PAYROLL_URL}/generate`,
            p_data
        );
        return mapPayStubDtoToDomain(response.data);
    }

    async generatePayrollForPeriod(
        p_data: GeneratePayrollForPeriodRequest
    ): Promise<GeneratePayrollForPeriodResult> {
        const response = await apiClient.post<GeneratePayrollForPeriodResponseDto>(
            `${API_PAYROLL_URL}/generate-period`,
            p_data
        );

        return {
            ...response.data,
            payStubs: mapPayStubCollectionToDomain(response.data.payStubs),
        };
    }

    async publishPayStub(p_id: number): Promise<PayStub> {
        const response = await apiClient.post<PayStubResponseDto>(
            `${API_PAYROLL_URL}/stubs/${p_id}/publish`
        );
        return mapPayStubDtoToDomain(response.data);
    }
}

const payrollService = new PayrollService();
export default payrollService;
