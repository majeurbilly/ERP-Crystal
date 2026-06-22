export interface InventoryQuantity {
    id: string;
    itemId: number | string;
    locationId: number | string;
    quantity: number;
}

export interface InventoryQuantityApiDTO {
    locationId: string | number;
    itemId: string | number;
    quantity: number;
}

export interface InventoryLine {
    locationId: number;
    locationTitle: string;
    itemId: number;
    itemName: string;
    quantity: number;
}

export interface InventoryLineApiDTO {
    locationId: number;
    locationTitle: string;
    itemId: number;
    itemName: string;
    quantity: number;
}

export interface ReceivedInventoryQuantityApiDTO extends InventoryQuantityApiDTO {
    locationTitle: string;
    itemName: string;
}

export interface InventoryQuantityFormData {
    mode?: "add" | "edit";
    id?: string;
    itemId?: number | string;
    locationId?: number | string;
    quantity?: number;
    itemName?: string;
    locationName?: string;
    fixedLocationId?: number;
    fixedItemId?: number;
}