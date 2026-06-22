import { useState } from "react";
import { useDebounce } from "use-debounce";

export function useSearch(initialValue: string = "", delay: number = 150) {
    const [searchValue, setSearchValue] = useState(initialValue);
    const [debouncedSearchTerm] = useDebounce(searchValue, delay);

    return {
        searchValue,
        setSearchValue,
        debouncedSearchTerm
    }
}