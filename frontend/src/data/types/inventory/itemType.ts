export const ITEM_TYPES = {
    BOOK: "livre",
    PRODUCT: "produit"
} as const;

export type ItemType = (typeof ITEM_TYPES)[keyof typeof ITEM_TYPES];