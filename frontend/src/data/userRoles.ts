/** Rôles alignés sur les segments d’URL `/dashboard/:role`. */
export type UserRole = "gerant" | "assistant" | "employee" | "admin";

export const roleLabels: Record<string, string> = {
    gerant: "Gérant",
    assistant: "Assistant",
    employee: "Employé",
    admin: "Administrateur",
};