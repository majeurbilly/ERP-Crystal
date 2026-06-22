import { Box, Modal, Typography, useTheme, CircularProgress } from "@mui/material";
import { CancelButton, ConfirmButton } from "../buttons/AddEditDeleteButtons";

interface FormModalProps {
    open: boolean;
    onClose: () => void;
    title: string;
    children?: React.ReactNode;
    onSubmit?: (e: React.FormEvent) => void;
    onConfirmClick?: () => void;
    isSubmitting?: boolean;
    confirmLabel?: string;
    confirmIcon?: React.ReactNode;
    confirmProps?: Record<string, unknown>;
    hideConfirmButton?: boolean;
    maxWidth?: number;
}

export function FormModal({
    open,
    onClose,
    title,
    children,
    onSubmit,
    onConfirmClick,
    isSubmitting,
    confirmLabel,
    confirmIcon,
    confirmProps,
    hideConfirmButton = false,
    maxWidth = 420,
}: FormModalProps) {
    const theme = useTheme();
    const submitLabel = confirmLabel ?? (title.startsWith("Ajouter") ? "Ajouter" : "Enregistrer");

    const renderContent = () => (
        <>
            {children}
            <Box sx={{ mt: 3, display: "flex", justifyContent: "flex-end", gap: 2 }}>
                <CancelButton
                    label={hideConfirmButton ? "Fermer" : "Annuler"}
                    onClick={onClose}
                    disabled={isSubmitting}
                    sx={{ fontWeight: "bold", textTransform: "none", px: 3 }}
                />
                {!hideConfirmButton && (
                    <ConfirmButton
                        label={submitLabel}
                        type={onSubmit ? "submit" : "button"}
                        onClick={onSubmit ? undefined : onConfirmClick}
                        disabled={isSubmitting}
                        startIcon={isSubmitting ? <CircularProgress size={20} color="inherit" /> : confirmIcon}
                        sx={{ fontWeight: "bold", textTransform: "none", px: 3, ...confirmProps }}
                    />
                )}
            </Box>
        </>
    );

    return (
        <Modal open={open} onClose={isSubmitting ? undefined : onClose}>
            <Box sx={{
                position: "absolute",
                top: "50%",
                left: "50%",
                transform: "translate(-50%, -50%)",
                width: { xs: "calc(100vw - 32px)", sm: maxWidth },
                maxHeight: "calc(100vh - 32px)",
                overflowY: "auto",
                boxSizing: "border-box",
                bgcolor: theme.palette.background.paper || "#fff",
                borderRadius: 3,
                boxShadow: 24,
                p: 4,
                border: "1px solid #000000",
            }}>
                <Typography variant="h6" sx={{ mb: 2 }}>{title}</Typography>

                {onSubmit ? (
                    <form onSubmit={onSubmit}>
                        {renderContent()}
                    </form>
                ) : (
                    renderContent()
                )}
            </Box>
        </Modal>
    );
}
