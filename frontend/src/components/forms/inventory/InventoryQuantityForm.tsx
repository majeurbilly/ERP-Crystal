import { useEffect, useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Autocomplete, TextField } from "@mui/material";
import { useInventoryQuantityMutations } from "../../../api/mutations/inventory/useInventoryQuantityMutation";
import itemService from "../../../api/services/inventory/itemService";
import locationService from "../../../api/services/inventory/locationService";
import inventoryQuantityService from "../../../api/services/inventory/inventoryQuantityService";
import type { InventoryQuantityFormData } from "../../../data/types/inventory/inventoryQuantity";
import { useFormValidation } from "../useFormValidation";
import { notifySuccessMessage } from "../../../data/utils/popupMessageManager";
import { FormModal } from "../FormModal";
import { displayErrorMessage } from "../../../data/utils/extractApiErrorMessage";
import { itemsCacheKey, inventoryQuantityCacheKey, locationsCacheKey } from "../../../data/cacheKeys";
import type { Item } from "../../../data/types/inventory/item";
import type { Location } from "../../../data/types/inventory/location";
import { useScopedPermissions } from "../../../permissions/useScopedPermissions";

const INITIAL_FORM_ERRORS = {
    quantity: "",
    itemId: "",
    locationId: "",
};

interface InventoryQuantityFormProps {
    showForm: boolean;
    setShowForm: (v: boolean) => void;
    editQuantity: InventoryQuantityFormData | null;
    setEditInventoryQuantity?: (quantity: InventoryQuantityFormData | null) => void;
    locationId?: number;
}

export default function InventoryQuantityForm({
    showForm,
    setShowForm,
    editQuantity,
    setEditInventoryQuantity,
}: InventoryQuantityFormProps) {
    const handleClose = () => setShowForm(false);

    const { updateInventoryQuantity, isUpdatingInventoryQuantity } = useInventoryQuantityMutations();
    const { canUpdateInventoryOnLocation } = useScopedPermissions();

    const isAddMode = editQuantity?.mode === "add";
    const fixedLocationId = editQuantity?.fixedLocationId;
    const fixedItemId = editQuantity?.fixedItemId;

    const [quantity, setQuantity] = useState<string>("0");
    const [selectedItem, setSelectedItem] = useState<Item | null>(null);
    const [selectedLocation, setSelectedLocation] = useState<Location | null>(null);

    const { errors, setErrors, clearErrors } = useFormValidation(INITIAL_FORM_ERRORS);

    const itemsQuery = useQuery({
        queryKey: itemsCacheKey.list(),
        queryFn: () => itemService.getAll(),
        enabled: showForm && isAddMode && !!fixedLocationId,
    });

    const locationsQuery = useQuery({
        queryKey: locationsCacheKey.list(),
        queryFn: () => locationService.getAll(),
        enabled: showForm && isAddMode && !!fixedItemId,
    });

    const existingLocationLinesQuery = useQuery({
        queryKey: inventoryQuantityCacheKey.locationLines(fixedLocationId ?? 0),
        queryFn: () => inventoryQuantityService.getLinesByLocation(fixedLocationId!),
        enabled: showForm && isAddMode && !!fixedLocationId,
    });

    const existingItemLinesQuery = useQuery({
        queryKey: inventoryQuantityCacheKey.itemLines(fixedItemId ?? 0),
        queryFn: () => inventoryQuantityService.getLinesByItem(fixedItemId!),
        enabled: showForm && isAddMode && !!fixedItemId,
    });

    const availableItems = useMemo(() => {
        if (!itemsQuery.data) return [];
        const stockedItemIds = new Set(
            (existingLocationLinesQuery.data ?? []).map((line) => line.itemId)
        );
        return itemsQuery.data.filter((item) => !stockedItemIds.has(Number(item.id)));
    }, [itemsQuery.data, existingLocationLinesQuery.data]);

    const availableLocations = useMemo(() => {
        if (!locationsQuery.data) return [];
        const stockedLocationIds = new Set(
            (existingItemLinesQuery.data ?? []).map((line) => line.locationId)
        );
        return locationsQuery.data
            .filter((location) => !stockedLocationIds.has(Number(location.id)))
            .filter((location) => canUpdateInventoryOnLocation(Number(location.id)));
    }, [locationsQuery.data, existingItemLinesQuery.data, canUpdateInventoryOnLocation]);

    const itemName = editQuantity?.itemName ?? selectedItem?.name ?? "l'article";
    const locationName = editQuantity?.locationName ?? selectedLocation?.title ?? "la succursale";

    const formTitle = isAddMode
        ? fixedLocationId
            ? `Ajouter un article au stock de ${locationName}`
            : `Ajouter ${itemName} au stock d'une succursale`
        : `Modifier la quantité de ${itemName} dans ${locationName}`;

    useEffect(() => {
        if (!showForm) return;

        if (editQuantity?.mode === "edit") {
            setQuantity(String(editQuantity.quantity ?? 0));
        } else {
            setQuantity("1");
        }

        setSelectedItem(null);
        setSelectedLocation(null);
        clearErrors();
    }, [editQuantity, showForm]);

    const validate = (): boolean => {
        const newErrors = { quantity: "", itemId: "", locationId: "" };
        let isValid = true;
        const numericValue = Number(quantity);

        if (quantity.trim() === "") {
            newErrors.quantity = "Quantité requise";
            isValid = false;
        } else if (isNaN(numericValue) || numericValue < 0 || !Number.isInteger(numericValue)) {
            newErrors.quantity = "La quantité doit être un nombre entier valide";
            isValid = false;
        }

        if (isAddMode && fixedLocationId && !selectedItem) {
            newErrors.itemId = "Sélectionnez un article";
            isValid = false;
        }

        if (isAddMode && fixedItemId && !selectedLocation) {
            newErrors.locationId = "Sélectionnez une succursale";
            isValid = false;
        }

        setErrors(newErrors);
        return isValid;
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (!validate()) return;

        const itemId = isAddMode
            ? fixedItemId ?? Number(selectedItem?.id)
            : Number(editQuantity?.itemId);
        const locationId = isAddMode
            ? fixedLocationId ?? Number(selectedLocation?.id)
            : Number(editQuantity?.locationId);

        if (!itemId || !locationId) return;

        const virtualId = `${locationId}-${itemId}`;
        const updatePayload = {
            itemId,
            locationId,
            quantity: Number(quantity),
        };

        updateInventoryQuantity(
            {
                id: virtualId,
                data: updatePayload,
            },
            {
                onSuccess: () => {
                    notifySuccessMessage(
                        isAddMode ? "Article ajouté au stock avec succès !" : "Stock mis à jour avec succès !"
                    );
                    if (setEditInventoryQuantity) setEditInventoryQuantity(null);
                    handleClose();
                },
                onError: (error: unknown) => {
                    displayErrorMessage(error);
                },
            }
        );
    };

    const isLoadingOptions =
        (isAddMode && !!fixedLocationId && (itemsQuery.isLoading || existingLocationLinesQuery.isLoading))
        || (isAddMode && !!fixedItemId && (locationsQuery.isLoading || existingItemLinesQuery.isLoading));

    return (
        <FormModal
            open={showForm}
            onClose={handleClose}
            title={formTitle}
            onSubmit={handleSubmit}
            isSubmitting={isUpdatingInventoryQuantity || isLoadingOptions}
        >
            {isAddMode && fixedLocationId && (
                <Autocomplete
                    options={availableItems}
                    getOptionLabel={(option) => option.name}
                    isOptionEqualToValue={(option, value) => String(option.id) === String(value.id)}
                    value={selectedItem}
                    onChange={(_event, value) => setSelectedItem(value)}
                    renderInput={(params) => (
                        <TextField
                            {...params}
                            label="Article"
                            error={!!errors.itemId}
                            helperText={errors.itemId || (availableItems.length === 0 ? "Tous les articles sont déjà en stock ici." : "")}
                        />
                    )}
                    sx={{ mt: 1 }}
                    disabled={availableItems.length === 0}
                />
            )}

            {isAddMode && fixedItemId && (
                <Autocomplete
                    options={availableLocations}
                    getOptionLabel={(option) => option.title}
                    isOptionEqualToValue={(option, value) => String(option.id) === String(value.id)}
                    value={selectedLocation}
                    onChange={(_event, value) => setSelectedLocation(value)}
                    renderInput={(params) => (
                        <TextField
                            {...params}
                            label="Succursale"
                            error={!!errors.locationId}
                            helperText={errors.locationId || (availableLocations.length === 0 ? "Cet article est déjà en stock partout." : "")}
                        />
                    )}
                    sx={{ mt: 1 }}
                    disabled={availableLocations.length === 0}
                />
            )}

            <TextField
                fullWidth
                label="Quantité"
                type="number"
                value={quantity}
                onChange={(e) => setQuantity(e.target.value)}
                error={!!errors.quantity}
                helperText={errors.quantity}
                slotProps={{
                    htmlInput: { min: 0, step: 1 },
                }}
                sx={{ mt: 2 }}
            />
        </FormModal>
    );
}
