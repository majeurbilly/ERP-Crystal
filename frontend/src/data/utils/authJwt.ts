import { jwtDecode } from "jwt-decode";

export const ASPNET_ID_CLAIM =
	"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

export function extractUserIdFromJwt(token: string): string {
	const decoded = jwtDecode<Record<string, unknown>>(token);
	const asp = decoded[ASPNET_ID_CLAIM];
	const sub = decoded.sub;
	const raw = asp !== undefined && asp !== null ? asp : sub;

	if (typeof raw === "string") {
		return raw;
	}
	if (raw !== undefined && raw !== null) {
		return String(raw);
	}
	return "";
}
