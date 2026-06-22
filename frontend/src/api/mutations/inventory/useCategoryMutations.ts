import { categoriesCachekey } from "../../../data/cacheKeys";
import type { Category } from "../../../data/types/inventory/category";
import categoryService from "../../services/inventory/categoryService";
import { useGenericMutations } from "../useGenericMutation";

export const useCategoryMutations = () => {
    const mutations = useGenericMutations<Category>(
        categoryService,
        categoriesCachekey.list(),
        (variables) => [categoriesCachekey.details(variables.id)]
    );

    return {
        addCategory: mutations.add,
        isAddingCategory: mutations.isAdding,
        addCategoryError: mutations.addError,

        deleteCategory: mutations.delete,
        isDeletingCategory: mutations.isDeleting,
        deleteCategoryError: mutations.deleteError,

        updateCategory: mutations.update,
        isUpdatingCategory: mutations.isUpdating,
        updateCategoryError: mutations.updateError,
    }
}