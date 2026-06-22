import { createContext, useContext, useState, type ReactNode } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import { FormModal } from "../components/forms/FormModal";
import DeleteIcon from '@mui/icons-material/Delete';
import { Typography } from "@mui/material";

interface DeleteConfig {
    id: string | number;
    displayLabel: string;
    onDelete: (id: any) => Promise<void>;
    isDeleting?: boolean;
    redirectUrl?: string;
    onSuccess?: () => void;
}

interface DeleteDialogContextType {
    openConfirmDeleteWindow: (config: DeleteConfig) => void;
}

const DeleteDialogContext = createContext<DeleteDialogContextType | undefined>(undefined);

export function DeleteDialogProvider({ children }: { children: ReactNode }) {
    const navigate = useNavigate();
    const [isOpen, setIsOpen] = useState(false);
    const [config, setConfig] = useState<DeleteConfig | null>(null);
    const [isPending, setIsPending] = useState(false);

    const confirmDelete = (newConfig: DeleteConfig) => {
        setConfig(newConfig);
        setIsOpen(true);
    };

    const handleClose = () => {
        setIsOpen(false);
        setConfig(null);
        setIsPending(false);
    };

    const handleConfirm = async () => {
        if (!config) return;

        const { id, displayLabel, onDelete, onSuccess, redirectUrl } = config;

        try {
            setIsPending(true);
            await onDelete(String(id));
            toast.success(`${displayLabel} a été supprimé avec succès.`);

            if (onSuccess) onSuccess();
            if (redirectUrl) navigate(redirectUrl);

            handleClose();
        } catch (error: any) {
            const serverMessage = error.response?.data?.message || error.response?.data || error.message;
            toast.error(serverMessage || `Impossible de supprimer ${displayLabel}.`);
            console.error(error);

            handleClose();
        }
    };

    const entityName = config?.displayLabel || "cet élément";

    return (
        <DeleteDialogContext.Provider value={{ openConfirmDeleteWindow: confirmDelete }}>
            {children}
            <FormModal
                open={isOpen}
                title={`Supprimer ${entityName}?`}
                onClose={handleClose}
                onConfirmClick={handleConfirm}
                isSubmitting={isPending || config?.isDeleting}
                confirmLabel={(isPending || config?.isDeleting) ? "Suppression..." : "Oui"}
                confirmIcon={<DeleteIcon />}
            >
                <Typography variant="body1" sx={{ color: "text.secondary" }}>
                    Cette action est définitive et irréversible.
                </Typography>
            </FormModal>
        </DeleteDialogContext.Provider>
    );
}

export function useDeleteDialog() {
    const context = useContext(DeleteDialogContext);
    if (!context) {
        throw new Error("useDeleteDialog must be used within a DeleteDialogProvider");
    }
    return context;
}
