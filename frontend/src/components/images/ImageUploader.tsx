import { Box, Button, Typography, Avatar, FormHelperText } from "@mui/material";
import CloudUploadIcon from "@mui/icons-material/CloudUpload";
import DeleteIcon from "@mui/icons-material/Delete";
import { useState, useMemo, useCallback, type DragEvent } from "react";
import Zoom from "react-medium-image-zoom";
import { resolveAssetUrl } from "../../data/utils/resolveAssetUrl";

interface ImageUploaderProps {
    value?: File | string | null;
    onChange?: (file: File | null) => void;
    maxSizeInMB?: number;
    customStyles?: { mb?: number };
    label?: string;
}

const VALID_IMAGE_TYPES = ["image/jpeg", "image/jpg", "image/png"];

export default function ImageUploader({
    value,
    onChange,
    maxSizeInMB = 2,
    customStyles,
    label = "Image de l'article",
}: ImageUploaderProps) {
    const [error, setError] = useState<string | null>(null);
    const [isDragging, setIsDragging] = useState(false);

    const previewUrl = useMemo(() => {
        if (!value) {
            return undefined;
        }

        if (typeof value === "string") {
            return resolveAssetUrl(value) ?? undefined;
        }

        return URL.createObjectURL(value);
    }, [value]);

    const validateAndSetFile = useCallback((file: File | undefined): void => {
        if (!file) {
            return;
        }

        if (!VALID_IMAGE_TYPES.includes(file.type)) {
            setError("Seuls les formats .jpg, .jpeg et .png sont acceptés.");
            return;
        }

        const maxByteSize = maxSizeInMB * 1024 * 1024;
        if (file.size > maxByteSize) {
            setError(`L'image est trop lourde. Limite max: ${maxSizeInMB} Mo.`);
            return;
        }

        setError(null);
        onChange?.(file);
    }, [maxSizeInMB, onChange]);

    const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>): void => {
        validateAndSetFile(event.target.files?.[0]);
    };

    const handleDragOver = (event: DragEvent<HTMLDivElement>): void => {
        event.preventDefault();
        setIsDragging(true);
    };

    const handleDragLeave = (event: DragEvent<HTMLDivElement>): void => {
        event.preventDefault();
        setIsDragging(false);
    };

    const handleDrop = (event: DragEvent<HTMLDivElement>): void => {
        event.preventDefault();
        setIsDragging(false);
        validateAndSetFile(event.dataTransfer.files?.[0]);
    };

    const handleRemoveImage = (): void => {
        onChange?.(null);
        setError(null);
    };

    return (
        <Box sx={{ display: "flex", flexDirection: "column", gap: 1, alignItems: "flex-start", mb: customStyles?.mb ?? 2 }}>
            <Typography variant="subtitle2" color="text.secondary">
                {label}
            </Typography>

            <Box
                onDragOver={handleDragOver}
                onDragLeave={handleDragLeave}
                onDrop={handleDrop}
                sx={{
                    display: "flex",
                    alignItems: "center",
                    gap: 3,
                    width: "100%",
                    p: 2,
                    borderRadius: 2,
                    border: "2px dashed",
                    borderColor: isDragging ? "primary.main" : "divider",
                    bgcolor: isDragging ? "action.hover" : "background.paper",
                    transition: "border-color 0.2s, background-color 0.2s",
                }}
            >
                <Zoom>
                    <Avatar
                        src={previewUrl}
                        variant="rounded"
                        sx={{ width: 80, height: 80, bgcolor: "grey.200", border: "1px dashed grey", flexShrink: 0 }}
                    >
                        {!value && "?"}
                    </Avatar>
                </Zoom>

                <Box sx={{ display: "flex", flexDirection: "column", gap: 1, flex: 1 }}>
                    <Typography variant="body2" color="text.secondary">
                        Glissez une image ici ou cliquez pour parcourir
                    </Typography>
                    <Box sx={{ display: "flex", gap: 1, flexWrap: "wrap" }}>
                        <Button
                            component="label"
                            variant="contained"
                            startIcon={<CloudUploadIcon />}
                            size="small"
                        >
                            Parcourir
                            <input
                                type="file"
                                accept=".jpg,.jpeg,.png,image/jpeg,image/png"
                                hidden
                                onChange={handleFileChange}
                            />
                        </Button>

                        {value && (
                            <Button
                                variant="text"
                                color="error"
                                startIcon={<DeleteIcon />}
                                size="small"
                                onClick={handleRemoveImage}
                            >
                                Supprimer
                            </Button>
                        )}
                    </Box>
                    <Typography variant="caption" color="text.secondary">
                        JPG ou PNG acceptés (max {maxSizeInMB} Mo)
                    </Typography>
                </Box>
            </Box>

            {error && <FormHelperText error>{error}</FormHelperText>}
        </Box>
    );
}
