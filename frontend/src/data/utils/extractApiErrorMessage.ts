import axios from "axios";
import { notifyErrorMessage } from "./popupMessageManager";

export function extractApiErrorMessage(p_error: unknown): string {
    if (axios.isAxiosError(p_error)) {
        const data: unknown = p_error.response?.data;

        if (typeof data === "string" && data.trim().length > 0) {
            return data;
        }

        if (data !== null && typeof data === "object" && "message" in data) {
            const message: unknown = (data as { message: unknown }).message;
            if (typeof message === "string" && message.trim().length > 0) {
                return message;
            }
        }

        if (typeof p_error.message === "string" && p_error.message.trim().length > 0) {
            return p_error.message;
        }
    }

    if (p_error instanceof Error && p_error.message.trim().length > 0) {
        return p_error.message;
    }

    return "Une erreur est survenue.";
}

export function displayErrorMessage(error: any) {
    const messageToShow = extractApiErrorMessage(error);
    notifyErrorMessage(messageToShow);
    console.error(messageToShow);
}
