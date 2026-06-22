export interface PayStubResponseDto {
    id: number;
    payPeriodId: number;
    employeeProfileId: number;
    employeeFirstName: string;
    employeeLastName: string;
    periodStartDate: string;
    periodEndDate: string;
    totalHours: number;
    grossPay: number;
    isPublished: boolean;
}

export interface PayStub extends PayStubResponseDto {
    isDeleted: boolean;
}

export interface GeneratePayrollRequest {
    payPeriodId: number;
    employeeProfileId: number;
}

export interface GeneratePayrollForPeriodRequest {
    payPeriodId: number;
    locationId?: number | null;
}

export interface GeneratePayrollForPeriodResponseDto {
    payPeriodId: number;
    periodStartDate: string;
    periodEndDate: string;
    locationId?: number | null;
    createdCount: number;
    existingCount: number;
    skippedCount: number;
    payStubs: PayStubResponseDto[];
}

export interface GeneratePayrollForPeriodResult extends GeneratePayrollForPeriodResponseDto {
    payStubs: PayStub[];
}
