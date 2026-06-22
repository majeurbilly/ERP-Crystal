export interface JobPosition {
    id: number;
    name: string;
    description: string;
    color?: string;
    isDeleted: boolean;
}

export interface JobPositionApiDto {
    id: number;
    name: string;
    description: string;
    color?: string;
}

export interface JobPositionFormData {
    name: string;
    description: string;
    color?: string;
}
