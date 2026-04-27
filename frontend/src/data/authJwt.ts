import { jwtDecode } from "jwt-decode";

/** Claim de rôle typique des JWT émis par ASP.NET Core. */
export const ASPNET_ROLE_CLAIM =
	"http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

/**
 * Lit le rôle « métier » tel que renvoyé par le serveur (Admin, Manager, …)
 * depuis le payload du JWT, sans faire confiance à la signature (comme jwt-decode).
 */
export function extractServerRoleFromJwt(token: string): string {
	const decoded = jwtDecode<Record<string, unknown>>(token);
	const asp = decoded[ASPNET_ROLE_CLAIM];
	const simple = decoded.role;
	const raw = asp !== undefined && asp !== null ? asp : simple;

	if (Array.isArray(raw)) {
		const first = raw[0];
		if (typeof first === "string") {
			return first;
		}
		return first !== undefined && first !== null ? String(first) : "";
	}
	if (typeof raw === "string") {
		return raw;
	}
	return "";
}

/** Claim d'ID typique des JWT émis par ASP.NET Core. */
export const ASPNET_ID_CLAIM =
	"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

export function extractUserIdFromJwt(token: string): string {
	const decoded = jwtDecode<Record<string, unknown>>(token);
	const asp = decoded[ASPNET_ID_CLAIM];
	const simple = decoded.role;
	const raw = asp !== undefined && asp !== null ? asp : simple;

	if (typeof raw === "string") {
		return raw;
	}
	if (raw !== undefined && raw !== null) {
		return String(raw);
	}
	return "";
}
