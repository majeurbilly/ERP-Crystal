import { useMutation, useQueryClient } from "@tanstack/react-query";
import { inventoryQuantityCacheKey, itemsCacheKey } from "../../../data/cacheKeys";
import type { InventoryQuantityApiDTO } from "../../../data/types/inventory/inventoryQuantity";
import inventoryQuantityService from "../../services/inventory/inventoryQuantityService";

function invalidateInventoryQueries(
    queryClient: ReturnType<typeof useQueryClient>,
    locationId?: number,
    itemId?: number
) {
    queryClient.invalidateQueries({ queryKey: inventoryQuantityCacheKey.all });

    if (locationId) {
        queryClient.invalidateQueries({
            queryKey: [...inventoryQuantityCacheKey.all, 'location', locationId],
        });
    }

    if (itemId) {
        queryClient.invalidateQueries({
            queryKey: [...inventoryQuantityCacheKey.all, 'item', itemId],
        });
        queryClient.invalidateQueries({ queryKey: itemsCacheKey.details(String(itemId)) });
    }

    queryClient.invalidateQueries({ queryKey: itemsCacheKey.all });
}

export const useInventoryQuantityMutations = () => {
    const queryClient = useQueryClient();

    const updateMutation = useMutation({
        mutationFn: ({ id, data }: { id: string; data: Partial<InventoryQuantityApiDTO> }) =>
            inventoryQuantityService.update(id, data),
        onSuccess: (_data, variables) => {
            invalidateInventoryQueries(
                queryClient,
                Number(variables.data.locationId),
                Number(variables.data.itemId)
            );
        },
    });

    return {
        updateInventoryQuantity: updateMutation.mutate,
        isUpdatingInventoryQuantity: updateMutation.isPending,
        updateInventoryQuantityError: updateMutation.error,
    };
};
