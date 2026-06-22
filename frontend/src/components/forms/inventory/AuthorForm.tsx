import { useState, useEffect } from "react";
import { useAuthorMutations } from "../../../api/mutations/inventory/useAuthorMutations";
import { notifySuccessMessage } from "../../../data/utils/popupMessageManager";
import type { Author } from "../../../data/types/inventory/author";
import { displayErrorMessage } from "../../../data/utils/extractApiErrorMessage";
import { FormModal } from "../FormModal";
import { TextField } from "@mui/material";

interface Props {
    showAuthorForm: boolean;
    setShowAuthorForm: (v: boolean) => void;
    editAuthor: Author | null;
    setEditAuthor?: (c: Author | null) => void;
}

export default function AuthorForm({ showAuthorForm, setShowAuthorForm, editAuthor, setEditAuthor }: Props) {
    const handleClose = () => setShowAuthorForm(false);
    const { addAuthor, isAddingAuthor, updateAuthor, isUpdatingAuthor } = useAuthorMutations();
    const isEditMode = editAuthor !== null;
    const [name, setName] = useState("");
    const [errors, setErrors] = useState({ name: "", });
    useEffect(() => {
        if (showAuthorForm) {
            if (editAuthor) {
                setName(editAuthor.name);
            }
            else {
                setName("");
            }
        }
    }, [editAuthor, showAuthorForm])
    const validate = () => {
        let isValid = true;
        const newErrors = { name: "" };
        if (!name.trim()) {
            newErrors.name = "Nom requis";
            isValid = false;
        } else if (name.trim().length < 3) {
            newErrors.name = "Le nom doit contenir au moins 3 caractères";
            isValid = false;
        }
        setErrors(newErrors);
        return isValid;
    };
    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!validate()) return;
        const authorData: Partial<Author> = {
            name: name.trim(),
        };
        try {
            if (isEditMode) {
                await updateAuthor({
                    id: String(editAuthor!.id),
                    data: authorData,
                });
                notifySuccessMessage(`L'auteur ${authorData.name} a été modifiée avec succès!`);
                if (setEditAuthor) setEditAuthor(null);
                handleClose();
            }
            else {
                await addAuthor({
                    id: 0,
                    name: authorData.name!,
                });
                notifySuccessMessage(`L'auteur ${authorData.name} a été ajoutée avec succès!`);
                handleClose();
            }

        } catch (error: unknown) {
            displayErrorMessage(error);
        }

    }
    return (
        <>
            <FormModal open={showAuthorForm} onClose={handleClose} title={isEditMode ? "Modifier un auteur" : "Ajouter un auteur"} onSubmit={handleSubmit} isSubmitting={isEditMode ? isUpdatingAuthor : isAddingAuthor}>
                <TextField fullWidth label="Nom" value={name} onChange={(e) => setName(e.target.value)} sx={{ mb: 2 }} required error={!!errors.name} helperText={errors.name} />
            </FormModal>
        </>
    )
}