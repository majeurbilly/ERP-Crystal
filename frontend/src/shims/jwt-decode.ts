/**
 * Sous-ensemble compatible avec l’API publique de `jwt-decode` (décodage du payload, sans vérification de signature).
 * Lorsque `npm install jwt-decode` fonctionne, vous pouvez retirer l’alias `jwt-decode` dans `vite.config.ts` / `tsconfig.app.json`
 * et utiliser le package npm à la place.
 */
export function jwtDecode<T = unknown>(token: string): T {
	if (typeof token !== "string" || !token) {
		throw new Error("Invalid token specified: must be a non-empty string");
	}
	const parts = token.split(".");
	if (parts.length < 2) {
		throw new Error("Invalid token: not enough segments");
	}
	const base64Url = parts[1];
	const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
	const pad = (4 - (base64.length % 4)) % 4;
	const padded = base64 + "=".repeat(pad);
	const binary = atob(padded);
	const jsonPayload = decodeUtf8FromBinary(binary);
	return JSON.parse(jsonPayload) as T;
}

function decodeUtf8FromBinary(binary: string): string {
	try {
		return decodeURIComponent(
			Array.from(binary, (c) => {
				const code = c.charCodeAt(0);
				return `%${`00${code.toString(16)}`.slice(-2)}`;
			}).join(""),
		);
	} catch {
		return binary;
	}
}
