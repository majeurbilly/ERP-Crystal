import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { notifyErrorMessage, notifySuccessMessage } from "../utils/popupMessageManager";

interface DeletableEntity {
    id: string | number;
    name?: string;
    userName?: string;
}

interface DeleteService {
    delete: (id: string) => Promise<any>;
}

export function useDeleteEntity(
    entity: DeletableEntity | null,
    service: DeleteService,
    redirectRoute: string
) {
    const [isDeleteDialogOpen, setIsDialogOpen] = useState(false);
    const navigate = useNavigate();

    const entityName = entity?.name || entity?.userName || "l'élément";

    const handleDelete = async () => {
        if (!entity) return;

        try {
            await service.delete(entity.id.toString());
            notifySuccessMessage(`${entityName} supprimé avec succès !`);
            setIsDialogOpen(false);
            navigate(redirectRoute);
        } catch (error: any) {
            const serverMessage = error.response?.data || error.message || "Une erreur est survenue.";
            notifyErrorMessage(serverMessage);
        }
    };

    return {
        isDeleteDialogOpen,
        openDeleteDialog: () => setIsDialogOpen(true),
        closeDeleteDialog: () => setIsDialogOpen(false),
        handleDelete,
        entityName
    };
}