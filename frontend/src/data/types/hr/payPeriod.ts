export interface PayPeriod {
    id: number;
    startDate: string;
    endDate: string;
    isProcessed: boolean;
}

export interface PayPeriodApiDto {
    id: number;
    startDate: string;
    endDate: string;
    isProcessed: boolean;
}

export interface CreatePayPeriodRequest {
    startDate: string;
    endDate: string;
}

const payPeriodDateFormatter = new Intl.DateTimeFormat("fr-CA", {
    year: "numeric",
    month: "short",
    day: "numeric",
});

function formatPayPeriodDate(p_date: string): string {
    return payPeriodDateFormatter.format(new Date(`${p_date}T00:00:00`));
}

export function formatPayPeriodLabel(p_period: Pick<PayPeriod, "startDate" | "endDate">): string {
    return `${formatPayPeriodDate(p_period.startDate)} – ${formatPayPeriodDate(p_period.endDate)}`;
}
