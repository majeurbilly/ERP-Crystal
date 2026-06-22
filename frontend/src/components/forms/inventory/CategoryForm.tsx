import { useEffect, useState } from "react";
import { TextField } from "@mui/material";
import type { Category } from "../../../data/types/inventory/category";
import { FormModal } from "../FormModal";
import { notifySuccessMessage } from "../../../data/utils/popupMessageManager";
import { useCategoryMutations } from "../../../api/mutations/inventory/useCategoryMutations";
import { displayErrorMessage } from "../../../data/utils/extractApiErrorMessage";

interface Props {
    showCategoryForm: boolean;
    setShowCategoryForm: (v: boolean) => void;
    editCategory: Category | null;
    setEditCategory?: (c: Category | null) => void;
}

export default function CategoryForm({ showCategoryForm, setShowCategoryForm, editCategory, setEditCategory }: Props) {
    const handleClose = () => setShowCategoryForm(false);
    const { addCategory, isAddingCategory, updateCategory, isUpdatingCategory } = useCategoryMutations();
    const isEditMode = editCategory !== null;
    const [name, setName] = useState("");
    const [errors, setErrors] = useState({ name: "", });

    useEffect(() => {
        if (showCategoryForm) {
            if (editCategory) {
                setName(editCategory.name);
            }
            else {
                setName("");
            }
        }
    }, [editCategory, showCategoryForm])

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
        const categoryData: Partial<Category> = {
            name: name.trim(),
        };
        try {
            if (isEditMode) {
                await updateCategory({
                    id: String(editCategory!.id),
                    data: categoryData,
                });
                notifySuccessMessage(`Catégorie ${categoryData.name} a été modifiée avec succès!`);
                if (setEditCategory) setEditCategory(null);
                handleClose();
            }
            else {
                await addCategory({
                    id: 0,
                    name: categoryData.name!,
                });
                notifySuccessMessage(`Catégorie ${categoryData.name} a été ajoutée avec succès!`);
                handleClose();
            }
        } catch (error: unknown) {
            displayErrorMessage(error);
        }

    }
    return (
        <>
            <FormModal open={showCategoryForm} onClose={handleClose} title={isEditMode ? "Modifier une catégorie" : "Ajouter une catégorie"} onSubmit={handleSubmit} isSubmitting={isEditMode ? isUpdatingCategory : isAddingCategory}>
                <TextField fullWidth label="Nom" value={name} onChange={(e) => setName(e.target.value)} sx={{ mb: 2 }} required error={!!errors.name} helperText={errors.name} />
            </FormModal>
        </>
    )
}