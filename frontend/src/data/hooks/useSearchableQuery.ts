import { useQuery } from "@tanstack/react-query";
import { useSearch } from "./useSearch";

interface UseSearchableQueryOptions<T> {
    queryKey: ReadonlyArray<unknown>;
    queryFn: () => Promise<T[]>;
    filterFn: (item: T, searchTerm: string) => boolean;
    enabled?: boolean;
}

export function useSearchableQuery<T>({
    queryKey,
    queryFn,
    filterFn,
    enabled = true
}: UseSearchableQueryOptions<T>) {

    const { searchValue, setSearchValue, debouncedSearchTerm } = useSearch();

    const query = useQuery({
        queryKey,
        queryFn,
        placeholderData: (previousData) => previousData,
        enabled,
    });

    const filteredData = (query.data ?? []).filter((item) =>
        filterFn(item, debouncedSearchTerm)
    );

    return {
        ...query,
        filteredData,
        searchProps: {
            searchValue,
            onSearchChange: setSearchValue,
            showSearch: true,
        },
    };
}