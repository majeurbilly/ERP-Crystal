import { authorsCachekey } from "../../../data/cacheKeys";
import type { Author } from "../../../data/types/inventory/author";
import authorService from "../../services/inventory/authorService";
import { useGenericMutations } from "../useGenericMutation";

export const useAuthorMutations = () => {
    const mutations = useGenericMutations<Author>(
        authorService, authorsCachekey.list(), (variables) => [authorsCachekey.details(variables.id)]
    );

    return {
        addAuthor: mutations.add,
        isAddingAuthor: mutations.isAdding,
        addAuthorError: mutations.addError,
        deleteAuthor: mutations.delete,
        isDeletingAuthor: mutations.isDeleting,
        deleteAuthorError: mutations.deleteError,
        updateAuthor: mutations.update,
        isUpdatingAuthor: mutations.isUpdating,
        updateAuthorError: mutations.updateError
    }
}