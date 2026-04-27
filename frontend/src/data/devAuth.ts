import { type UserRole } from "./userRoles";

const SERVER_ROLE_TO_FRONTEND: Record<string, UserRole> = {
	admin: "admin",
	manager: "gerant",
	assistant: "assistant",
	employee: "employee",
};

/**
 * Traduit le libellé de rôle renvoyé par l’API / le JWT vers le segment d’URL frontend.
 * Correspondances : Admin → admin, Manager → gerant, Assistant → assistant, Employee → employe.
 */
export function mapServerRoleToFrontend(serverRole: string): UserRole | null {
	const trimmed = serverRole.trim();
	if (!trimmed) {
		return null;
	}
	const lower = trimmed.toLowerCase();
	for (const [serverKey, frontRole] of Object.entries(
		SERVER_ROLE_TO_FRONTEND,
	)) {
		if (serverKey.toLowerCase() === lower) {
			return frontRole;
		}
	}
	return null;
}

export function isUserRole(value: string): value is UserRole {
	return (
		value === "gerant" ||
		value === "assistant" ||
		value === "employee" ||
		value === "admin"
	);
}
