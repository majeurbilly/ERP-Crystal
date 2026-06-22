import { Box } from "@mui/material";
import MenuBookIcon from "@mui/icons-material/MenuBook";
import InventoryIcon from "@mui/icons-material/Inventory";
import Zoom from "react-medium-image-zoom";
import "react-medium-image-zoom/dist/styles.css";

interface ItemImageDisplayProps {
    imageUrl?: string | null;
    name: string;
    isBook: boolean;
}

export default function ItemImageDisplay({ imageUrl, name, isBook }: ItemImageDisplayProps) {
    return (
        <Box
            sx={{
                width: { xs: 140, md: 160 },
                height: { xs: 180, md: 210 },
                borderRadius: 1,
                overflow: "hidden",
                backgroundColor: "action.hover",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                border: "1px solid",
                borderColor: "divider",
                flexShrink: 0,
            }}
        >
            {imageUrl ? (
                <Zoom>
                    <Box
                        component="img"
                        src={imageUrl}
                        alt={name}
                        sx={{
                            width: "100%",
                            height: "100%",
                            objectFit: "cover",
                        }}
                    />
                </Zoom>
            ) : (
                isBook ? (
                    <MenuBookIcon sx={{ fontSize: 60, color: "text.disabled" }} />
                ) : (
                    <InventoryIcon sx={{ fontSize: 60, color: "text.disabled" }} />
                )
            )}
        </Box>
    );
}