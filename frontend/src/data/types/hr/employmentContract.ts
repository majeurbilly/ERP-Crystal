export const CONTRACT_TYPES = {
    FullTime: "FullTime",
    PartTime: "PartTime",
    SelfEmployed: "SelfEmployed",
    Internship: "Internship",
} as const;

export const WAGE_TYPES = {
    Monthly: "Monthly",
    Fixed: "Fixed",
} as const;

export type ContractType = (typeof CONTRACT_TYPES)[keyof typeof CONTRACT_TYPES];
export type WageType = (typeof WAGE_TYPES)[keyof typeof WAGE_TYPES];

export const CONTRACT_TYPE_LABELS: Record<ContractType, string> = {
    [CONTRACT_TYPES.FullTime]: "Temps plein",
    [CONTRACT_TYPES.PartTime]: "Temps partiel",
    [CONTRACT_TYPES.SelfEmployed]: "Travailleur autonome",
    [CONTRACT_TYPES.Internship]: "Stage",
};

export const WAGE_TYPE_LABELS: Record<WageType, string> = {
    [WAGE_TYPES.Monthly]: "Taux horaire",
    [WAGE_TYPES.Fixed]: "Montant fixe",
};

export function getBaseRateLabel(p_wageType: WageType): string {
    return p_wageType === WAGE_TYPES.Monthly ? "Taux horaire ($/h)" : "Montant fixe ($)";
}

export function getBaseRateHelper(p_wageType: WageType): string {
    return p_wageType === WAGE_TYPES.Monthly
        ? "Salaire calculé à l'heure selon les heures pointées."
        : "Montant fixe par période (ex. : 40 $ pour une livraison).";
}

export interface EmploymentContract {
    id: number;
    employeeProfileId: number;
    employeeFirstName: string;
    employeeLastName: string;
    contractType: ContractType;
    wageType: WageType;
    baseRate: number;
    startDate: string;
    endDate: string | null;
    isDeleted: boolean;
}

export interface EmploymentContractApiDto {
    id: number;
    employeeProfileId: number;
    employeeFirstName: string;
    employeeLastName: string;
    contractType: string;
    wageType: string;
    baseRate: number;
    startDate: string;
    endDate: string | null;
}

export interface EmploymentContractFormData {
    employeeProfileId: number;
    contractType: ContractType;
    wageType: WageType;
    baseRate: number;
    startDate: string;
    endDate: string | null;
}
