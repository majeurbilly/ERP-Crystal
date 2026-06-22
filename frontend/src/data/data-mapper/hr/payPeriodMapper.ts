import type { PayPeriod, PayPeriodApiDto } from "../../types/hr/payPeriod";

function normalizeDate(p_date: string | null | undefined): string {
    if (!p_date || p_date.trim().length === 0) {
        return "";
    }
    return p_date.length >= 10 ? p_date.substring(0, 10) : p_date;
}

export function mapPayPeriodDtoToDomain(p_dto: PayPeriodApiDto): PayPeriod {
    return {
        id: p_dto.id,
        startDate: normalizeDate(p_dto.startDate),
        endDate: normalizeDate(p_dto.endDate),
        isProcessed: p_dto.isProcessed,
    };
}

export function mapPayPeriodCollectionToDomain(p_dtos: PayPeriodApiDto[]): PayPeriod[] {
    return p_dtos.map(mapPayPeriodDtoToDomain);
}
