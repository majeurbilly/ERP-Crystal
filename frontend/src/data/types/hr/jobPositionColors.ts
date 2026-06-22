export interface JobPositionColorOption {
    hex: string;
    label: string;
}

export const JOB_POSITION_COLOR_PALETTE: JobPositionColorOption[] = [
    { hex: "#3B82F6", label: "Bleu" },
    { hex: "#6366F1", label: "Indigo" },
    { hex: "#8B5CF6", label: "Violet" },
    { hex: "#EC4899", label: "Rose" },
    { hex: "#EF4444", label: "Rouge" },
    { hex: "#F97316", label: "Orange" },
    { hex: "#F59E0B", label: "Ambre" },
    { hex: "#22C55E", label: "Vert" },
    { hex: "#10B981", label: "Émeraude" },
    { hex: "#14B8A6", label: "Sarcelle" },
    { hex: "#06B6D4", label: "Cyan" },
    { hex: "#6B7280", label: "Gris" },
];

export const DEFAULT_JOB_POSITION_COLOR = JOB_POSITION_COLOR_PALETTE[0].hex;

export function resolveJobPositionColor(p_color: string | undefined | null): string {
    if (!p_color?.trim()) {
        return DEFAULT_JOB_POSITION_COLOR;
    }

    const normalized = p_color.trim().toUpperCase();
    const match = JOB_POSITION_COLOR_PALETTE.find(
        (option) => option.hex.toUpperCase() === normalized
    );

    return match?.hex ?? DEFAULT_JOB_POSITION_COLOR;
}
