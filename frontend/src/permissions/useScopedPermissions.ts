import { useCallback, useMemo } from "react";
import { useAppPermissionContext, usePermissionRules } from "./AppPermissionContext";
import {
    canUpdateInventoryOnAnyLocation,
    rulesGrantPermissionForLocation,
} from "./scopedPermissionRules";
import { CRUD_OPERATIONS, ENTITY_TYPES } from "./permissions";

export function useScopedPermissions() {
    const ability = useAppPermissionContext();
    const permissionRules = usePermissionRules();

    const isSuperAdmin = useMemo(
        () => ability.can(CRUD_OPERATIONS.MANAGE, ENTITY_TYPES.ALL),
        [ability]
    );

    const canPerformOnLocation = useCallback(
        (p_action: string, p_subject: string, p_locationId: number): boolean => {
            if (isSuperAdmin) {
                return true;
            }

            return rulesGrantPermissionForLocation(
                permissionRules,
                p_action,
                p_subject,
                p_locationId
            );
        },
        [isSuperAdmin, permissionRules]
    );

    const canUpdateInventoryOnLocation = useCallback(
        (p_locationId: number): boolean => canPerformOnLocation(
            CRUD_OPERATIONS.UPDATE,
            ENTITY_TYPES.INVENTORY_QUANTITY,
            p_locationId
        ),
        [canPerformOnLocation]
    );

    const canUpdateInventoryAnywhere = useMemo(
        () => canUpdateInventoryOnAnyLocation(permissionRules, isSuperAdmin),
        [permissionRules, isSuperAdmin]
    );

    return {
        canPerformOnLocation,
        canUpdateInventoryOnLocation,
        canUpdateInventoryAnywhere,
        isSuperAdmin,
    };
}
