import { useNavigate } from "react-router-dom";
import { CustomDataGrid } from "./CustomDataGrid";
import { FORM_TYPES, useFormContainer } from "../../context/FormContext";
import { itemInventoryColumns, type ItemInventoryRow } from "../../data/gridColumns";
import { ROUTE_LOCATION_DETAILS } from "../../data/routeNames";
import type { InventoryLine, InventoryQuantityFormData } from "../../data/types/inventory/inventoryQuantity";
import { useScopedPermissions } from "../../permissions/useScopedPermissions";
import { Typography } from "@mui/material";

interface ItemQuantitiesTableProps {
    inventoryLines: InventoryLine[];
    itemId: string | number;
    itemName: string;
    onAddClick?: () => void;
}

export default function ItemInventoryQuantitiesTable({
    inventoryLines,
    itemId,
    itemName,
    onAddClick,
}: ItemQuantitiesTableProps) {
    const navigate = useNavigate();
    const { openForm } = useFormContainer();
    const { canUpdateInventoryOnLocation } = useScopedPermissions();

    const gridRows: ItemInventoryRow[] = inventoryLines.map((line) => ({
        id: line.locationId,
        title: line.locationTitle,
        quantity: line.quantity,
    }));

    return (
        <>
            {inventoryLines.length === 0 && (
                <Typography variant="body2" color="text.secondary" sx={{ mb: 2, textAlign: "left" }}>
                    Cet article n'est en stock dans aucune succursale.
                </Typography>
            )}
            <CustomDataGrid
                rows={gridRows}
                columns={itemInventoryColumns}
                onRowClick={(params) => {
                    navigate(ROUTE_LOCATION_DETAILS.replace(":id", String(params.id)));
                }}
                onEditClick={(row) => {
                    if (!canUpdateInventoryOnLocation(Number(row.id))) {
                        return;
                    }

                    const quantityRecordId = `${String(row.id)}-${String(itemId)}`;
                    openForm(FORM_TYPES.QUANTITY, {
                        mode: "edit",
                        id: quantityRecordId,
                        itemId,
                        locationId: Number(row.id),
                        quantity: row.quantity,
                        itemName,
                        locationName: row.title,
                    } satisfies InventoryQuantityFormData);
                }}
                isEditDisabledForRow={(row) => !canUpdateInventoryOnLocation(Number(row.id))}
                onAddClick={onAddClick}
                addLabel="Ajouter au stock"
            />
        </>
    );
}
