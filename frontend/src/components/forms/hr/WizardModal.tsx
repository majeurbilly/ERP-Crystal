import {
    Box,
    Modal,
    Step,
    StepLabel,
    Stepper,
    Typography,
    useTheme,
    CircularProgress,
} from "@mui/material";
import { CancelButton, ConfirmButton, PreviousButton } from "../../buttons/AddEditDeleteButtons";
import ArrowForwardIcon from '@mui/icons-material/ArrowForward';
import CheckIcon from '@mui/icons-material/Check';

interface WizardModalProps {
    open: boolean;
    onClose: () => void;
    title: string;
    steps: string[];
    activeStep: number;
    onBack: () => void;
    onNext: () => void;
    isSubmitting?: boolean;
    nextLabel?: string;
    children?: React.ReactNode;
}

export default function WizardModal({
    open,
    onClose,
    title,
    steps,
    activeStep,
    onBack,
    onNext,
    isSubmitting = false,
    nextLabel,
    children,
}: WizardModalProps) {
    const theme = useTheme();
    const isLastStep: boolean = activeStep === steps.length - 1;
    const actionLabel: string = nextLabel ?? (isLastStep ? "Terminer" : "Suivant");

    return (
        <Modal open={open} onClose={isSubmitting ? undefined : onClose}>
            <Box
                sx={{
                    position: "absolute",
                    top: "50%",
                    left: "50%",
                    transform: "translate(-50%, -50%)",
                    width: { xs: "calc(100vw - 32px)", sm: 560 },
                    maxHeight: "calc(100vh - 32px)",
                    overflowY: "auto",
                    boxSizing: "border-box",
                    bgcolor: theme.palette.background.paper || "#fff",
                    borderRadius: 3,
                    boxShadow: 24,
                    p: 4,
                    border: "2px solid",
                    borderColor: "primary.main",
                }}
            >
                <Typography variant="h6" sx={{ mb: 2 }}>
                    {title}
                </Typography>

                <Stepper activeStep={activeStep} alternativeLabel sx={{ mb: 3 }}>
                    {steps.map((p_label: string) => (
                        <Step key={p_label}>
                            <StepLabel>{p_label}</StepLabel>
                        </Step>
                    ))}
                </Stepper>

                <Box sx={{ mb: 3 }}>{children}</Box>

                <Box sx={{ display: "flex", justifyContent: "space-between", gap: 2 }}>
                    <CancelButton
                        label="Annuler"
                        onClick={onClose}
                        disabled={isSubmitting}
                        sx={{ fontWeight: "bold", textTransform: "none", px: 3 }}
                    />
                    <Box sx={{ display: "flex", gap: 2 }}>
                        {activeStep > 0 && (
                            <PreviousButton
                                label="Précédent"
                                onClick={onBack}
                                disabled={isSubmitting}
                                sx={{ fontWeight: "bold", textTransform: "none", px: 3 }}
                            />
                        )}
                        <ConfirmButton
                            label={actionLabel}
                            type="button"
                            onClick={onNext}
                            disabled={isSubmitting}
                            icon={isLastStep ? <CheckIcon /> : <ArrowForwardIcon />}
                            startIcon={isSubmitting ? <CircularProgress size={20} color="inherit" /> : undefined}
                            sx={{ fontWeight: "bold", textTransform: "none", px: 3 }}
                        />
                    </Box>
                </Box>
            </Box>
        </Modal>
    );
}
