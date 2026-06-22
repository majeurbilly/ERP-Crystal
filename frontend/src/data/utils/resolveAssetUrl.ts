import { AUTH_URL } from "../../api/apiBaseUrl";

export function resolveAssetUrl(p_url: string | null | undefined): string | null {
    if (!p_url) {
        return null;
    }

    if (p_url.startsWith("http://") || p_url.startsWith("https://") || p_url.startsWith("blob:")) {
        return p_url;
    }

    const normalizedPath = p_url.startsWith("/") ? p_url : `/${p_url}`;
    return `${AUTH_URL}${normalizedPath}`;
}
