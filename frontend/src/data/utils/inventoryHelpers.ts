import type { Theme } from '@mui/material/styles'

export const getInventoryDisplayColor = (quantity: number, theme: Theme): string => {
    if (quantity === 0) return theme.palette.inventory.empty;
    if (quantity <= 10) return theme.palette.inventory.low;
    return theme.palette.inventory.good;
}