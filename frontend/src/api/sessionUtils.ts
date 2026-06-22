import { isAxiosError } from "axios";

let sessionExpiredHandler: (() => void) | null = null;

export function registerSessionExpiredHandler(p_handler: () => void): void {
    sessionExpiredHandler = p_handler;
}

export function clearSessionExpiredHandler(): void {
    sessionExpiredHandler = null;
}

export function notifySessionExpired(): void {
    sessionExpiredHandler?.();
}

export function isInvalidSessionError(p_error: unknown): boolean {
    if (!isAxiosError(p_error)) {
        return false;
    }

    const status: number | undefined = p_error.response?.status;
    return status === 401 || status === 404;
}
