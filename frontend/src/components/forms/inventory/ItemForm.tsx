import React, { useEffect, useState } from "react";
import {
    Autocomplete,
    FormControl,
    FormControlLabel,
    FormHelperText,
    Radio,
    RadioGroup,
    TextField,
    Typography
} from "@mui/material";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useItemMutations } from "../../../api/mutations/inventory/useItemMutations";
import categoryService from "../../../api/services/inventory/categoryService";
import { categoriesCachekey, itemsCacheKey } from "../../../data/cacheKeys";
import { notifySuccessMessage } from "../../../data/utils/popupMessageManager";
import type { Category } from "../../../data/types/inventory/category";
import type { CreateBookRequest, CreateItemRequest, Item } from "../../../data/types/inventory/item";
import { FormModal } from "../FormModal";
import { useFormValidation } from "../useFormValidation";
import ImageUploader from "../../images/ImageUploader";
import { imageService } from "../../../api/services/imageService";
import { displayErrorMessage } from "../../../data/utils/extractApiErrorMessage";

const initialErrors = {
    name: "",
    price: "",
    alertQuantity: "",
    isbn: "",
    authors: "",
    publicationDate: "",
    categories: ""
};

const textFieldSx = {
    mb: 2,
    "& .MuiInputBase-root": {
        color: "secondary.main"
    },
    "& .MuiInputLabel-root": {
        color: "secondary.main",
        "&.Mui-focused": { color: "secondary.main" },
        "&.Mui-error": { color: "error.main" }
    },
    "& .MuiOutlinedInput-notchedOutline": {
        borderColor: "secondary.main"
    },
    "& .MuiOutlinedInput-root:hover .MuiOutlinedInput-notchedOutline": {
        borderColor: "secondary.main"
    },
    "& .MuiOutlinedInput-root.Mui-focused .MuiOutlinedInput-notchedOutline": {
        borderColor: "secondary.main",
        borderWidth: 2
    },
    "& .MuiFormHelperText-root": {
        color: "secondary.main"
    },
    "& .MuiFormHelperText-root.Mui-error": {
        color: "error.main"
    },
    "& .MuiSvgIcon-root": {
        color: "secondary.main"
    },
};

interface Props {
    showItemForm: boolean;
    setShowItemForm: (v: boolean) => void;
    editItem: Item | null;
    setEditItem?: (i: Item | null) => void;
}

function parseAuthorsInput(p_value: string): string[] {
    return p_value
        .split(",")
        .map((authorName) => authorName.trim())
        .filter(Boolean);
}

export default function ItemForm({ showItemForm, setShowItemForm, editItem, setEditItem }: Props) {
    const handleClose = () => setShowItemForm(false);
    const queryClient = useQueryClient();
    const { additem, isAddingItem, updateItem, isUpdatingItem } = useItemMutations();
    const isEditMode = editItem !== null;
    const [itemType, setItemType] = useState<"product" | "book">("product");
    const [name, setName] = useState<string>("");
    const [description, setDescription] = useState("");
    const [distributor, setDistributor] = useState("");
    const [publisher, setPublisher] = useState("");
    const [isbn, setIsbn] = useState("");
    const [publicationDate, setPublicationDate] = useState("");
    const [price, setPrice] = useState("");
    const [alertQuantity, setAlertQuantity] = useState("");
    const [authorsInput, setAuthorsInput] = useState("");
    const [selectedCategories, setSelectedCategories] = useState<Category[]>([]);
    const [image, setImage] = useState<File | string | null>(null);

    const { data: categories = [], isLoading: isLoadingCategories } = useQuery({
        queryKey: categoriesCachekey.list(),
        queryFn: () => categoryService.getAll(),
        enabled: showItemForm && itemType === "book"
    });

    const { errors, setErrors, clearErrors } = useFormValidation(initialErrors);

    const refreshItemQueries = async (p_itemId?: number): Promise<void> => {
        await queryClient.invalidateQueries({ queryKey: itemsCacheKey.list() });
        if (p_itemId) {
            await queryClient.invalidateQueries({ queryKey: itemsCacheKey.details(String(p_itemId)) });
        }
    };

    const uploadItemImage = async (p_file: File, p_itemId: number): Promise<void> => {
        await imageService.upload(p_file, p_itemId);
        await refreshItemQueries(p_itemId);
    };

    useEffect(() => {
        if (showItemForm) {
            if (editItem) {
                setItemType(editItem.isBook ? "book" : "product");
                setName(editItem.name);
                setDescription(editItem.description ?? "");
                setDistributor(editItem.isBook ? "" : editItem.distributor ?? "");
                setPublisher(editItem.isBook ? editItem.publishers?.[0] ?? "" : "");
                setIsbn(editItem.isBook ? editItem.isbn ?? "" : "");
                setPublicationDate(editItem.isBook ? editItem.publicationDate?.slice(0, 10) ?? "" : "");
                setPrice(editItem.price.toString());
                setAlertQuantity(editItem.alertQuantity.toString());
                setAuthorsInput(editItem.isBook ? editItem.authors.join(", ") : "");
                setImage(editItem.imageUrl ?? null);
            } else {
                setItemType("product");
                setName("");
                setDescription("");
                setDistributor("");
                setPublisher("");
                setIsbn("");
                setPublicationDate("");
                setPrice("");
                setAlertQuantity("");
                setAuthorsInput("");
                setImage(null);
            }

            setSelectedCategories([]);
            clearErrors();
        }
    }, [editItem, showItemForm]);

    const validate = (): boolean => {
        const newErrors = { name: "", price: "", alertQuantity: "", isbn: "", authors: "", publicationDate: "", categories: "" };
        let isValid = true;
        const numericPrice = Number(price);
        const numericAlertQuantity = Number(alertQuantity);

        if (!name.trim()) {
            newErrors.name = "Nom requis";
            isValid = false;
        }

        if (price.trim() === "" || Number.isNaN(numericPrice)) {
            newErrors.price = "Prix requis";
            isValid = false;
        } else if (numericPrice < 0) {
            newErrors.price = "Le prix ne peut pas etre negatif.";
            isValid = false;
        }

        if (alertQuantity.trim() === "" || Number.isNaN(numericAlertQuantity)) {
            newErrors.alertQuantity = "Quantite stock alerte requise";
            isValid = false;
        } else if (!Number.isInteger(numericAlertQuantity) || numericAlertQuantity < 0) {
            newErrors.alertQuantity = "La quantite stock alerte doit etre un entier positif ou zero.";
            isValid = false;
        }

        if (itemType === "book" && parseAuthorsInput(authorsInput).length === 0) {
            newErrors.authors = "Au moins un auteur est requis";
            isValid = false;
        }

        if (itemType === "book" && !isbn.trim()) {
            newErrors.isbn = "ISBN requis";
            isValid = false;
        }

        if (itemType === "book" && !publicationDate) {
            newErrors.publicationDate = "Date de publication requise";
            isValid = false;
        }

        setErrors(newErrors);
        return isValid;
    };

    const formatPrice = () => {
        if (price.trim() === "") return;

        const numericPrice = Number(price);
        if (!Number.isNaN(numericPrice)) {
            setPrice(numericPrice.toFixed(2));
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!validate()) return;

        const trimmedName = name.trim();
        const cleanedDescription = description.trim() || null;
        const cleanedDistributor = distributor.trim() || null;
        const cleanedPublisher = publisher.trim();
        const cleanedIsbn = isbn.trim();
        const cleanedAuthors = parseAuthorsInput(authorsInput);
        const numericPrice = Number(price);
        const numericAlertQuantity = Number(alertQuantity);

        try {
            if (!isEditMode) {
                let createPayload: CreateItemRequest | CreateBookRequest;

                if (itemType === "book") {
                    const bookPayload: CreateBookRequest = {
                        name: trimmedName,
                        description: cleanedDescription || null,
                        price: numericPrice,
                        alertQuantity: numericAlertQuantity,
                        imageUrl: null,
                        isbn: cleanedIsbn,
                        publicationDate,
                        authors: cleanedAuthors,
                        publishers: cleanedPublisher ? [cleanedPublisher] : [],
                        authorIds: [],
                        categoryIds: selectedCategories.map((c) => Number(c.id)),
                        publisherIds: [],
                    };
                    createPayload = bookPayload;
                } else {
                    const itemPayload: CreateItemRequest = {
                        name: trimmedName,
                        description: cleanedDescription || null,
                        distributor: cleanedDistributor || null,
                        price: numericPrice,
                        alertQuantity: numericAlertQuantity,
                        imageUrl: null,
                    };
                    createPayload = itemPayload;
                }

                const savedItem = await additem(createPayload);

                if (image instanceof File && savedItem?.id) {
                    await uploadItemImage(image, savedItem.id);
                    notifySuccessMessage(`Item "${trimmedName}" ajouté avec image !`);
                } else {
                    notifySuccessMessage(`Item "${trimmedName}" ajouté avec succès !`);
                }

            } else {
                const updatePayload: Partial<Item> = {
                    name: trimmedName,
                    description: cleanedDescription || null,
                    price: numericPrice,
                    alertQuantity: numericAlertQuantity,
                    isActive: editItem.isActive,
                    categoryIds: selectedCategories.map((c) => Number(c.id)),
                    imageUrl: typeof image === "string" ? image : editItem.imageUrl,
                    ...(editItem.isBook ? {
                        isbn: cleanedIsbn,
                        publicationDate,
                        authors: cleanedAuthors,
                        publishers: cleanedPublisher ? [cleanedPublisher] : [],
                    } : {
                        distributor: cleanedDistributor || null,
                    })
                };

                await updateItem({ id: editItem.id.toString(), data: updatePayload });

                if (image instanceof File) {
                    await uploadItemImage(image, editItem.id);
                    notifySuccessMessage(`Item "${trimmedName}" modifié avec image !`);
                } else {
                    notifySuccessMessage(`Item "${trimmedName}" modifié avec succès !`);
                }

                setEditItem?.(null);
            }

            handleClose();
        } catch (error: unknown) {
            displayErrorMessage(error);
        }
    };

    return (
        <FormModal open={showItemForm} onClose={handleClose} title={isEditMode ? "Modifier un item" : "Ajouter un item"} onSubmit={handleSubmit} isSubmitting={isEditMode ? isUpdatingItem : isAddingItem}>
            <FormControl component="fieldset" disabled={isEditMode} sx={{ mb: 2 }}>
                <Typography component="legend" sx={{ color: "secondary.main", fontWeight: 600, mb: 0.5 }}>
                    Type d'item
                </Typography>
                <RadioGroup
                    row
                    value={itemType}
                    onChange={(e) => {
                        const nextItemType = e.target.value as "product" | "book";
                        setItemType(nextItemType);
                        setDistributor("");
                        setPublisher("");
                        setIsbn("");
                        setPublicationDate("");
                        setAuthorsInput("");
                        setSelectedCategories([]);
                    }}
                >
                    <FormControlLabel
                        value="product"
                        control={<Radio sx={{ color: "secondary.main", "&.Mui-checked": { color: "secondary.main" } }} />}
                        label="Produit"
                        sx={{ color: "secondary.main", "& .MuiFormControlLabel-label": { color: "secondary.main" } }}
                    />
                    <FormControlLabel
                        value="book"
                        control={<Radio sx={{ color: "secondary.main", "&.Mui-checked": { color: "secondary.main" } }} />}
                        label="Livre"
                        sx={{ color: "secondary.main", "& .MuiFormControlLabel-label": { color: "secondary.main" } }}
                    />
                </RadioGroup>
                {isEditMode && <FormHelperText>Le type ne peut pas etre modifie.</FormHelperText>}
            </FormControl>

            <ImageUploader
                value={image}
                onChange={(newFile) => setImage(newFile)}
            />

            <TextField fullWidth required label="Nom de l'item" value={name} onChange={(e) => setName(e.target.value)} error={!!errors.name} helperText={errors.name} sx={textFieldSx} />
            <TextField fullWidth required label="Prix" type="number" value={price} onChange={(e) => setPrice(e.target.value)} onBlur={formatPrice} error={!!errors.price} helperText={errors.price} slotProps={{ htmlInput: { step: "0.01", min: "0" } }} sx={textFieldSx} />
            <TextField fullWidth required label="Quantite stock alerte" type="number" value={alertQuantity} onChange={(e) => setAlertQuantity(e.target.value)} error={!!errors.alertQuantity} helperText={errors.alertQuantity} slotProps={{ htmlInput: { step: "1", min: "0" } }} sx={textFieldSx} />
            {itemType === "product" && (
                <TextField fullWidth label="Distributeur" value={distributor} onChange={(e) => setDistributor(e.target.value)} sx={textFieldSx} />
            )}
            <TextField fullWidth multiline minRows={3} label="Description" value={description} onChange={(e) => setDescription(e.target.value)} sx={textFieldSx} />

            {itemType === "book" && (
                <>
                    <TextField fullWidth required label="ISBN" value={isbn} onChange={(e) => setIsbn(e.target.value)} error={!!errors.isbn} helperText={errors.isbn} sx={textFieldSx} />
                    <TextField fullWidth label="Editeur" value={publisher} onChange={(e) => setPublisher(e.target.value)} sx={textFieldSx} />
                    <TextField
                        fullWidth
                        required
                        label="Auteur(s)"
                        value={authorsInput}
                        onChange={(e) => setAuthorsInput(e.target.value)}
                        error={!!errors.authors}
                        helperText={errors.authors || "Séparez plusieurs auteurs par une virgule"}
                        sx={textFieldSx}
                    />
                    <TextField fullWidth required label="Date de publication" type="date" value={publicationDate} onChange={(e) => setPublicationDate(e.target.value)} error={!!errors.publicationDate} helperText={errors.publicationDate} slotProps={{ inputLabel: { shrink: true } }} sx={textFieldSx} />
                    <Autocomplete
                        multiple
                        options={categories}
                        loading={isLoadingCategories}
                        loadingText="Chargement des categories..."
                        noOptionsText="Aucune categorie disponible"
                        value={selectedCategories}
                        getOptionLabel={(option) => option.name}
                        isOptionEqualToValue={(option, value) => Number(option.id) === Number(value.id)}
                        onChange={(_event, value) => setSelectedCategories(value)}
                        renderInput={(params) => (
                            <TextField
                                {...params}
                                label="Categories"
                                error={!!errors.categories}
                                helperText={errors.categories || "Optionnel"}
                                sx={textFieldSx}
                            />
                        )}
                    />
                </>
            )}
        </FormModal>
    );
}
