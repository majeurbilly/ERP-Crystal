import type { Location } from "../../../data/types/inventory/location";
import { locationsCacheKey } from "../../../data/cacheKeys";
import locationService from "../../services/inventory/locationService";
import { useGenericMutations } from "../useGenericMutation";

export const useLocationMutations = () => {
    const mutations = useGenericMutations<Location>(
        locationService,
        locationsCacheKey.all,
        (variables) => [locationsCacheKey.details(variables.id)]
    );

    return {
        addLocation: mutations.add,
        isAddingLocation: mutations.isAdding,
        addLocationError: mutations.addError,

        deleteLocation: mutations.delete,
        isDeletingLocation: mutations.isDeleting,
        deleteLocationError: mutations.deleteError,

        updateLocation: mutations.update,
        isUpdatingLocation: mutations.isUpdating,
        updateLocationError: mutations.updateError,
    };
};
