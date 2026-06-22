import { useMemo } from "react";
import { useAppPermissionContext } from "./AppPermissionContext";
import { CRUD_OPERATIONS, ENTITY_TYPES, type Entity } from "./permissions";

export function usePermissions(entity: Entity) {
    const ability = useAppPermissionContext();

    const isSuperAdmin = useMemo(() => {
        return ability.can(CRUD_OPERATIONS.MANAGE, ENTITY_TYPES.ALL);
    }, [ability]);

    const basePermissions = useMemo(() => {
        return {
            canCreate: isSuperAdmin || ability.can(CRUD_OPERATIONS.CREATE, entity) || ability.can(CRUD_OPERATIONS.MANAGE, entity),
            canRead: isSuperAdmin || ability.can(CRUD_OPERATIONS.READ, entity) || ability.can(CRUD_OPERATIONS.MANAGE, entity),
            canUpdate: isSuperAdmin || ability.can(CRUD_OPERATIONS.UPDATE, entity) || ability.can(CRUD_OPERATIONS.MANAGE, entity),
            canDelete: isSuperAdmin || ability.can(CRUD_OPERATIONS.DELETE, entity) || ability.can(CRUD_OPERATIONS.MANAGE, entity),
        };
    }, [ability, entity, isSuperAdmin]);

    return {
        ability,
        isSuperAdmin,
        ...basePermissions,
    };
}
