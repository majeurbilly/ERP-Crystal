import { useCallback, useMemo } from "react";
import { useAuth } from "../context/AuthContext";
import { useAppPermissionContext } from "./AppPermissionContext";
import { CRUD_OPERATIONS, ENTITY_TYPES } from "./permissions";

/**
 * Restriction héritée pour la gestion des succursales :
 * un employé non administrateur ne peut modifier que sa propre succursale.
 */
export function useEmployeeBranchRowGuard() {
    const { user } = useAuth();
    const ability = useAppPermissionContext();

    const isSuperAdmin = useMemo(
        () => ability.can(CRUD_OPERATIONS.MANAGE, ENTITY_TYPES.ALL),
        [ability]
    );

    const employeeLocationId = user?.employeeProfile?.locationId;

    const isOtherBranch = useCallback((p_rowLocationId?: string | number): boolean => {
        if (isSuperAdmin) {
            return false;
        }

        if (!employeeLocationId || !p_rowLocationId) {
            return false;
        }

        return String(employeeLocationId) !== String(p_rowLocationId);
    }, [isSuperAdmin, employeeLocationId]);

    return { isOtherBranch };
}
