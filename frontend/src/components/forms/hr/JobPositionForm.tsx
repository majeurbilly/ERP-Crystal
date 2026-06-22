import { useEffect, useState } from "react";
import { TextField } from "@mui/material";
import type { JobPosition, JobPositionFormData } from "../../../data/types/hr/jobPosition";
import { DEFAULT_JOB_POSITION_COLOR, resolveJobPositionColor } from "../../../data/types/hr/jobPositionColors";
import { FormModal } from "../FormModal";
import ColorPalettePicker from "../ColorPalettePicker";
import { notifySuccessMessage } from "../../../data/utils/popupMessageManager";
import { useJobPositionMutations } from "../../../api/mutations/hr/useJobPositionMutations";
import { displayErrorMessage } from "../../../data/utils/extractApiErrorMessage";

interface JobPositionFormProps {
    showJobPositionForm: boolean;
    setShowJobPositionForm: (p_value: boolean) => void;
    editJobPosition: JobPosition | null;
    setEditJobPosition?: (p_value: JobPosition | null) => void;
}

interface JobPositionFormErrors {
    name: string;
    description: string;
    color: string;
}

export default function JobPositionForm({
    showJobPositionForm,
    setShowJobPositionForm,
    editJobPosition,
    setEditJobPosition,
}: JobPositionFormProps) {
    const handleClose = (): void => setShowJobPositionForm(false);
    const { addJobPosition, isAddingJobPosition, updateJobPosition, isUpdatingJobPosition } =
        useJobPositionMutations();

    const isEditMode: boolean = editJobPosition !== null;
    const [name, setName] = useState<string>("");
    const [description, setDescription] = useState<string>("");
    const [color, setColor] = useState<string>(DEFAULT_JOB_POSITION_COLOR);
    const [errors, setErrors] = useState<JobPositionFormErrors>({
        name: "",
        description: "",
        color: "",
    });

    useEffect(() => {
        if (showJobPositionForm) {
            if (editJobPosition) {
                setName(editJobPosition.name);
                setDescription(editJobPosition.description);
                setColor(resolveJobPositionColor(editJobPosition.color));
            } else {
                setName("");
                setDescription("");
                setColor(DEFAULT_JOB_POSITION_COLOR);
            }
            setErrors({ name: "", description: "", color: "" });
        }
    }, [editJobPosition, showJobPositionForm]);

    const validate = (): boolean => {
        let isValid: boolean = true;
        const newErrors: JobPositionFormErrors = {
            name: "",
            description: "",
            color: "",
        };

        if (!name.trim()) {
            newErrors.name = "Le nom est requis.";
            isValid = false;
        }

        if (!description.trim()) {
            newErrors.description = "La description est requise.";
            isValid = false;
        }


        setErrors(newErrors);
        return isValid;
    };

    const handleSubmit = async (p_event: React.FormEvent): Promise<void> => {
        p_event.preventDefault();
        if (!validate()) {
            return;
        }

        const formData: JobPositionFormData = {
            name: name.trim(),
            description: description.trim(),
            color: color.trim(),
        };

        try {
            if (isEditMode && editJobPosition) {
                await updateJobPosition({
                    id: String(editJobPosition.id),
                    data: formData,
                });
                notifySuccessMessage(`Le poste Â« ${formData.name} Â» a Ã©tÃ© modifiÃ© avec succÃ¨s.`);
                if (setEditJobPosition) {
                    setEditJobPosition(null);
                }
            } else {
                await addJobPosition(formData);
                notifySuccessMessage(`Le poste Â« ${formData.name} Â» a Ã©tÃ© ajoutÃ© avec succÃ¨s.`);
            }
            handleClose();
        } catch (error: unknown) {
            displayErrorMessage(error);
        }
    };

    return (
        <FormModal
            open={showJobPositionForm}
            onClose={handleClose}
            title={isEditMode ? "Modifier un poste" : "Ajouter un poste"}
            onSubmit={handleSubmit}
            isSubmitting={isEditMode ? isUpdatingJobPosition : isAddingJobPosition}
        >
            <TextField
                fullWidth
                label="Nom"
                value={name}
                onChange={(p_event) => setName(p_event.target.value)}
                sx={{ mb: 2 }}
                required
                error={!!errors.name}
                helperText={errors.name}
            />
            <TextField
                fullWidth
                label="Description"
                value={description}
                onChange={(p_event) => setDescription(p_event.target.value)}
                rows={3}
                sx={{ mb: 2 }}
                required
                error={!!errors.description}
                helperText={errors.description}
            />
            <ColorPalettePicker
                value={color}
                onChange={setColor}
                error={errors.color}
            />
        </FormModal>
    );
}
