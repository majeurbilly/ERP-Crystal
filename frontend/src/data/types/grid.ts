import type { SearchableProps } from "./search";

export interface StandardGridProps<T = any> extends SearchableProps {
    addLabel?: string;
    onAddClick?: () => void;
    onEditClick?: (item: T) => void;
    onDeleteClick?: (item: T) => void;
}