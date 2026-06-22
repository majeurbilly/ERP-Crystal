import { useEffect, useMemo, useState } from "react";
import {
    Alert,
    Box,
    Button,
    Card,
    CardContent,
    Chip,
    CircularProgress,
    Link,
    Typography,
} from "@mui/material";
import AccessTimeIcon from "@mui/icons-material/AccessTime";
import LoginIcon from "@mui/icons-material/Login";
import LogoutIcon from "@mui/icons-material/Logout";
import EventBusyIcon from "@mui/icons-material/EventBusy";
import { FormModal } from "../forms/FormModal";
import { Link as RouterLink } from "react-router-dom";
import { usePunchEligibility } from "../../api/queries/usePunchEligibility";
import { usePunchClockMutations } from "../../api/mutations/hr/usePunchClockMutations";
import { useAuth } from "../../context/AuthContext";
import { usePermissions } from "../../permissions/usePermissions";
import { ENTITY_TYPES } from "../../permissions/permissions";
import { ROUTE_MON_ESPACE } from "../../data/routeNames";
import { notifyErrorMessage, notifySuccessMessage } from "../../data/utils/popupMessageManager";
import { extractApiErrorMessage } from "../../data/utils/extractApiErrorMessage";
import {
    buildPunchClockDisplay,
    PUNCH_ELIGIBILITY_BLOCK_CODES,
    type PunchEligibility,
} from "../../data/types/hr/punchEligibility";

function buildElapsedLabel(p_startTime: string, p_now: Date): string {
    const [hours, minutes] = p_startTime.split(":").map(Number);
    const start = new Date(p_now);
    start.setHours(hours, minutes, 0, 0);

    const diffMs = Math.max(0, p_now.getTime() - start.getTime());
    const totalMinutes = Math.floor(diffMs / 60_000);
    const elapsedHours = Math.floor(totalMinutes / 60);
    const elapsedMinutes = totalMinutes % 60;

    if (elapsedHours > 0) {
        return `${elapsedHours} h ${elapsedMinutes.toString().padStart(2, "0")} min`;
    }

    return `${elapsedMinutes} min`;
}

interface PunchClockContentProps {
    eligibility: PunchEligibility | undefined;
    isLoading: boolean;
    isPunchingIn: boolean;
    isPunchingOut: boolean;
    onPunchIn: () => void;
    onPunchOut: () => void;
}

function PunchClockContent({
    eligibility,
    isLoading,
    isPunchingIn,
    isPunchingOut,
    onPunchIn,
    onPunchOut,
}: PunchClockContentProps) {
    const [now, setNow] = useState<Date>(() => new Date());
    const hasActiveEntry = eligibility?.activeEntryId !== null && eligibility?.activeEntryId !== undefined;

    useEffect(() => {
        if (!hasActiveEntry) {
            return undefined;
        }

        const intervalId = window.setInterval(() => {
            setNow(new Date());
        }, 60_000);

        return () => window.clearInterval(intervalId);
    }, [hasActiveEntry]);

    const elapsedLabel = useMemo(() => {
        if (!eligibility?.activeEntryStartTime) {
            return null;
        }
        return buildElapsedLabel(eligibility.activeEntryStartTime, now);
    }, [eligibility?.activeEntryStartTime, now]);

    const display = eligibility ? buildPunchClockDisplay(eligibility) : null;
    const isBusy = isLoading || isPunchingIn || isPunchingOut;
    const canPunchIn = eligibility?.canPunchIn ?? false;
    const canPunchOut = eligibility?.canPunchOut ?? false;
    const monEspaceScheduleUrl = `${ROUTE_MON_ESPACE}?tab=horaire`;
    const monEspacePointagesUrl = `${ROUTE_MON_ESPACE}?tab=pointages`;

    return (
        <Card variant="outlined" sx={{ height: "100%" }}>
            <CardContent>
                <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", gap: 2, mb: 2 }}>
                    <Box sx={{ flex: 1 }}>
                        <Typography variant="overline" color="text.secondary" sx={{ letterSpacing: 1 }}>
                            Horloge de pointage
                        </Typography>
                        {isLoading ? (
                            <CircularProgress size={24} sx={{ mt: 1 }} />
                        ) : display ? (
                            <>
                                <Typography variant="h5" fontWeight={700} sx={{ mt: 0.5 }}>
                                    {display.headline}
                                </Typography>
                                {hasActiveEntry && elapsedLabel && (
                                    <Chip
                                        label={`Durée : ${elapsedLabel}`}
                                        color="success"
                                        size="small"
                                        sx={{ mt: 1, mb: 1 }}
                                    />
                                )}
                                {display.shiftLabel && (
                                    <Chip
                                        icon={<AccessTimeIcon />}
                                        label={display.shiftLabel}
                                        variant="outlined"
                                        size="small"
                                        sx={{ mt: 1, mb: 1, mr: 1 }}
                                    />
                                )}
                                {display.alertSeverity && (
                                    <Alert severity={display.alertSeverity} sx={{ mt: 1.5 }}>
                                        {display.detail}
                                    </Alert>
                                )}
                            </>
                        ) : (
                            <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                                Chargement de votre statut…
                            </Typography>
                        )}
                    </Box>
                    <AccessTimeIcon color="primary" fontSize="large" />
                </Box>

                <Box sx={{ display: "flex", flexWrap: "wrap", gap: 1, alignItems: "center" }}>
                    {hasActiveEntry ? (
                        <Button
                            variant="contained"
                            color="error"
                            size="large"
                            startIcon={isPunchingOut ? <CircularProgress size={18} color="inherit" /> : <LogoutIcon />}
                            disabled={isBusy || !canPunchOut}
                            onClick={onPunchOut}
                        >
                            Terminer mon quart
                        </Button>
                    ) : (
                        <Button
                            variant="contained"
                            color="success"
                            size="large"
                            startIcon={isPunchingIn ? <CircularProgress size={18} color="inherit" /> : <LoginIcon />}
                            disabled={isBusy || !canPunchIn}
                            onClick={onPunchIn}
                        >
                            Commencer mon quart
                        </Button>
                    )}

                    {eligibility?.blockCode === PUNCH_ELIGIBILITY_BLOCK_CODES.NO_SHIFT && (
                        <Button
                            component={RouterLink}
                            to={monEspaceScheduleUrl}
                            variant="outlined"
                            color="warning"
                            startIcon={<EventBusyIcon />}
                        >
                            Voir mon horaire
                        </Button>
                    )}

                    <Link component={RouterLink} to={monEspacePointagesUrl} variant="body2">
                        Historique des pointages
                    </Link>
                </Box>
            </CardContent>
        </Card>
    );
}

export default function PunchClockWidget() {
    const { user } = useAuth();
    const { canCreate } = usePermissions(ENTITY_TYPES.TIME_ENTRY);
    const hasEmployeeProfile = user?.employeeProfile !== undefined;

    const [isConfirmOpen, setIsConfirmOpen] = useState(false);

    const eligibilityQuery = usePunchEligibility({
        enabled: canCreate && hasEmployeeProfile,
    });

    const { punchIn, isPunchingIn, punchOut, isPunchingOut } = usePunchClockMutations();

    if (!canCreate) {
        return null;
    }

    if (!hasEmployeeProfile) {
        return (
            <Card variant="outlined">
                <CardContent>
                    <Typography variant="overline" color="text.secondary">
                        Horloge de pointage
                    </Typography>
                    <Alert severity="error" sx={{ mt: 1 }}>
                        Votre compte n&apos;est pas lié à un profil employé. Contactez un administrateur pour activer le
                        pointage.
                    </Alert>
                </CardContent>
            </Card>
        );
    }

    const handlePunchIn = async (): Promise<void> => {
        try {
            await punchIn();
            notifySuccessMessage("Votre entrée a été enregistrée. Bon quart !");
        } catch (p_error) {
            notifyErrorMessage(extractApiErrorMessage(p_error));
        }
    };

    const executePunchOut = async (): Promise<void> => {
        try {
            await punchOut();
            notifySuccessMessage("Votre sortie a été enregistrée. Bonne fin de journée !");
            setIsConfirmOpen(false);
        } catch (p_error) {
            notifyErrorMessage(extractApiErrorMessage(p_error));
            setIsConfirmOpen(false);
        }
    };

    return (
        <>
            <PunchClockContent
                eligibility={eligibilityQuery.data}
                isLoading={eligibilityQuery.isLoading}
                isPunchingIn={isPunchingIn}
                isPunchingOut={isPunchingOut}
                onPunchIn={() => void handlePunchIn()}
                onPunchOut={() => setIsConfirmOpen(true)}
            />

            <FormModal
                open={isConfirmOpen}
                title="Terminer le quart de travail?"
                onClose={() => setIsConfirmOpen(false)}
                onConfirmClick={() => void executePunchOut()}
                isSubmitting={isPunchingOut}
                confirmLabel={isPunchingOut ? "Enregistrement..." : "Oui"}
                confirmIcon={<LogoutIcon />}
            >
                <Typography variant="body1" sx={{ color: "text.secondary" }}>
                    Confirmer la fin de votre quart de travail ?
                </Typography>
            </FormModal>
        </>
    );
}