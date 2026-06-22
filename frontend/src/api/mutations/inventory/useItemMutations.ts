import { useGenericMutations } from "../useGenericMutation"
import type { CreateBookRequest, CreateItemRequest, Item } from "../../../data/types/inventory/item";
import itemService from "../../services/inventory/itemService";
import { itemsCacheKey } from "../../../data/cacheKeys";

export const useItemMutations = () => {
    const mutations = useGenericMutations<Item, Item | CreateItemRequest | CreateBookRequest>(
        itemService,
        itemsCacheKey.list(),
        (variables) => [itemsCacheKey.details(variables.id)]
    );



    return {
        additem: mutations.add,
        isAddingItem: mutations.isAdding,
        addItemError: mutations.addError,

        deleteItem: mutations.delete,
        isDeletingItem: mutations.isDeleting,
        deleteItemError: mutations.deleteError,

        updateItem: mutations.update,
        isUpdatingItem: mutations.isUpdating,
        updateItemError: mutations.updateError,
    }
}
