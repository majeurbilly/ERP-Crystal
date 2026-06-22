export const PUNCH_ELIGIBILITY_BLOCK_CODES = {
    NO_PROFILE: "no_profile",
    NO_SHIFT: "no_shift",
    TOO_EARLY: "too_early",
    TOO_LATE: "too_late",
} as const;

export type PunchEligibilityBlockCode =
    (typeof PUNCH_ELIGIBILITY_BLOCK_CODES)[keyof typeof PUNCH_ELIGIBILITY_BLOCK_CODES];

export interface PunchEligibilityApiDto {
    canPunchIn: boolean;
    canPunchOut: boolean;
    blockedReason: string | null;
    blockCode: string | null;
    activeEntryId: number | null;
    activeEntryStartTime: string | null;
    scheduledShiftId: number | null;
    shiftDate: string | null;
    shiftStartTime: string | null;
    shiftEndTime: string | null;
    earliestPunchInTime: string | null;
}

export interface PunchEligibility {
    canPunchIn: boolean;
    canPunchOut: boolean;
    blockedReason: string | null;
    blockCode: string | null;
    activeEntryId: number | null;
    activeEntryStartTime: string | null;
    scheduledShiftId: number | null;
    shiftDate: string | null;
    shiftStartTime: string | null;
    shiftEndTime: string | null;
    earliestPunchInTime: string | null;
}

function normalizeTime(p_time: string | null): string | null {
    if (p_time === null || p_time.trim().length === 0) {
        return null;
    }
    return p_time.length >= 5 ? p_time.substring(0, 5) : p_time;
}

function normalizeDate(p_date: string | null): string | null {
    if (p_date === null || p_date.trim().length === 0) {
        return null;
    }
    return p_date.length >= 10 ? p_date.substring(0, 10) : p_date;
}

export function mapPunchEligibilityToDomain(p_dto: PunchEligibilityApiDto): PunchEligibility {
    return {
        canPunchIn: p_dto.canPunchIn,
        canPunchOut: p_dto.canPunchOut,
        blockedReason: p_dto.blockedReason,
        blockCode: p_dto.blockCode,
        activeEntryId: p_dto.activeEntryId,
        activeEntryStartTime: normalizeTime(p_dto.activeEntryStartTime),
        scheduledShiftId: p_dto.scheduledShiftId,
        shiftDate: normalizeDate(p_dto.shiftDate),
        shiftStartTime: normalizeTime(p_dto.shiftStartTime),
        shiftEndTime: normalizeTime(p_dto.shiftEndTime),
        earliestPunchInTime: normalizeTime(p_dto.earliestPunchInTime),
    };
}

export interface PunchClockDisplay {
    headline: string;
    detail: string;
    alertSeverity: "success" | "info" | "warning" | "error" | null;
    shiftLabel: string | null;
}

function formatClockTime(p_time: string): string {
    const [hours, minutes] = p_time.split(":");
    return `${hours} h ${minutes}`;
}

function buildShiftLabel(p_eligibility: PunchEligibility): string | null {
    if (!p_eligibility.shiftStartTime || !p_eligibility.shiftEndTime) {
        return null;
    }
    return `Quart du jour : ${formatClockTime(p_eligibility.shiftStartTime)} – ${formatClockTime(p_eligibility.shiftEndTime)}`;
}

export function buildPunchClockDisplay(p_eligibility: PunchEligibility): PunchClockDisplay {
    const hasActiveEntry =
        p_eligibility.activeEntryId !== null && p_eligibility.activeEntryId !== undefined;
    const shiftLabel = buildShiftLabel(p_eligibility);

    if (hasActiveEntry) {
        const startLabel = p_eligibility.activeEntryStartTime
            ? formatClockTime(p_eligibility.activeEntryStartTime)
            : "—";
        return {
            headline: "Vous êtes en service",
            detail: `Entrée enregistrée à ${startLabel}. N'oubliez pas de pointer votre sortie en fin de quart.`,
            alertSeverity: "success",
            shiftLabel,
        };
    }

    if (p_eligibility.blockCode === PUNCH_ELIGIBILITY_BLOCK_CODES.NO_SHIFT) {
        return {
            headline: "Aucun quart aujourd'hui",
            detail:
                "Vous n'avez pas de quart planifié aujourd'hui. Contactez votre gérant si vous devez travailler.",
            alertSeverity: "warning",
            shiftLabel: null,
        };
    }

    if (p_eligibility.blockCode === PUNCH_ELIGIBILITY_BLOCK_CODES.TOO_EARLY) {
        const openTime = p_eligibility.earliestPunchInTime
            ? formatClockTime(p_eligibility.earliestPunchInTime)
            : null;
        const shiftStart = p_eligibility.shiftStartTime
            ? formatClockTime(p_eligibility.shiftStartTime)
            : null;
        const detail =
            openTime && shiftStart
                ? `Le pointage ouvre à ${openTime}. Votre quart débute à ${shiftStart}.`
                : "Revenez plus tard pour pointer votre entrée.";

        return {
            headline: "Pointage pas encore ouvert",
            detail,
            alertSeverity: "info",
            shiftLabel,
        };
    }

    if (p_eligibility.blockCode === PUNCH_ELIGIBILITY_BLOCK_CODES.TOO_LATE) {
        const shiftEnd = p_eligibility.shiftEndTime
            ? formatClockTime(p_eligibility.shiftEndTime)
            : null;
        const detail = shiftEnd
            ? `Votre quart est terminé depuis ${shiftEnd}. Contactez votre gérant si vous devez corriger un oubli.`
            : "La fenêtre de pointage pour aujourd'hui est fermée.";

        return {
            headline: "Pointage fermé",
            detail,
            alertSeverity: "warning",
            shiftLabel,
        };
    }

    if (p_eligibility.canPunchIn) {
        const openHint = p_eligibility.earliestPunchInTime
            ? `Vous pouvez pointer dès ${formatClockTime(p_eligibility.earliestPunchInTime)}.`
            : "Vous pouvez pointer votre entrée maintenant.";
        return {
            headline: "Prêt à commencer",
            detail: openHint,
            alertSeverity: "success",
            shiftLabel,
        };
    }

    return {
        headline: "Pointage indisponible",
        detail: "Le pointage d'entrée n'est pas autorisé pour le moment.",
        alertSeverity: "warning",
        shiftLabel,
    };
}
