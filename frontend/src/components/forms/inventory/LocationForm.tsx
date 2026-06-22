import { TextField } from "@mui/material";
import { useEffect, useState } from "react";
import { useLocationMutations } from "../../../api/mutations/inventory/useLocationMutations";
import { notifySuccessMessage } from "../../../data/utils/popupMessageManager";
import type { Location } from "../../../data/types/inventory/location";
import { FormModal } from "../FormModal";
import { useFormValidation } from "../useFormValidation";
import { displayErrorMessage } from "../../../data/utils/extractApiErrorMessage";

interface Props {
    showLocationForm: boolean;
    setShowLocationForm: (v: boolean) => void;
    editLocation: Location | null;
    setEditLocation?: (location: Location | null) => void;
}

export default function LocationForm({
    showLocationForm,
    setShowLocationForm,
    editLocation,
    setEditLocation,
}: Props) {
    const handleClose = () => setShowLocationForm(false);
    const { addLocation, isAddingLocation, updateLocation, isUpdatingLocation } = useLocationMutations();
    const isEditMode = editLocation !== null;
    const [title, setTitle] = useState("");
    const [address, setAddress] = useState("");
    const [description, setDescription] = useState("");

    const { errors, setErrors, clearErrors } = useFormValidation({
        title: "",
        address: "",
    });

    useEffect(() => {
        if (showLocationForm) {
            if (editLocation) {
                setTitle(editLocation.title);
                setAddress(editLocation.address);
                setDescription(editLocation.description);
            } else {
                setTitle("");
                setAddress("");
                setDescription("");
            }
            clearErrors();
        }
    }, [editLocation, showLocationForm]);

    const validate = (): boolean => {
        const newErrors = { title: "", address: "" };
        let isValid = true;

        if (!title.trim()) {
            newErrors.title = "Nom requis";
            isValid = false;
        }

        if (!address.trim()) {
            newErrors.address = "Adresse requise";
            isValid = false;
        }

        setErrors(newErrors);
        return isValid;
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!validate()) return;

        const locationData: Location = {
            id: editLocation ? editLocation.id : 0,
            title,
            address,
            description,
        };

        try {
            if (isEditMode) {
                await updateLocation({ id: editLocation.id.toString(), data: locationData });
                notifySuccessMessage(`Succursale ${locationData.title} modifiée avec succès !`);
                if (setEditLocation) setEditLocation(null);
            } else {
                await addLocation(locationData);
                notifySuccessMessage(`Succursale ${locationData.title} ajoutée avec succès !`);
            }

            handleClose();
        } catch (error: unknown) {
            displayErrorMessage(error);
        }
    };

    return (
        <FormModal
            open={showLocationForm}
            onClose={handleClose}
            title={isEditMode ? "Modifier une succursale" : "Ajouter une succursale"}
            onSubmit={handleSubmit}
            isSubmitting={isEditMode ? isUpdatingLocation : isAddingLocation}
        >
            <TextField fullWidth label="Nom" value={title} onChange={(e) => setTitle(e.target.value)} error={!!errors.title} helperText={errors.title} sx={{ mb: 2 }} />
            <TextField fullWidth label="Adresse" value={address} onChange={(e) => setAddress(e.target.value)} error={!!errors.address} helperText={errors.address} sx={{ mb: 2 }} />
            <TextField fullWidth label="Description" value={description} onChange={(e) => setDescription(e.target.value)} multiline minRows={3} />
        </FormModal>
    );
}
