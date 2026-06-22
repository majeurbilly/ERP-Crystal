import type { PayStub, PayStubResponseDto } from "../../types/hr/payStub";

function normalizeDate(p_date: string | null | undefined): string {
    if (p_date === null || p_date === undefined || p_date.trim().length === 0) {
        return "";
    }
    return p_date.length >= 10 ? p_date.substring(0, 10) : p_date;
}

export function mapPayStubDtoToDomain(p_dto: PayStubResponseDto): PayStub {
    return {
        id: p_dto.id,
        payPeriodId: p_dto.payPeriodId,
        employeeProfileId: p_dto.employeeProfileId,
        employeeFirstName: p_dto.employeeFirstName,
        employeeLastName: p_dto.employeeLastName,
        periodStartDate: normalizeDate(p_dto.periodStartDate),
        periodEndDate: normalizeDate(p_dto.periodEndDate),
        totalHours: p_dto.totalHours,
        grossPay: p_dto.grossPay,
        isPublished: p_dto.isPublished ?? false,
        isDeleted: false,
    };
}

export function mapPayStubCollectionToDomain(p_dtos: PayStubResponseDto[]): PayStub[] {
    return p_dtos.map(mapPayStubDtoToDomain);
}
