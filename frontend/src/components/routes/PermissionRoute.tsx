import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "../../context/AuthContext";
import { usePermissions } from "../../permissions/usePermissions";
import { ROUTE_DASHBOARD, ROUTE_ROOT } from "../../data/routeNames";
import type { Entity } from "../../permissions/permissions";

interface PermissionRouteProps {
    entityType: Entity;
}

export function PermissionRoute({ entityType }: PermissionRouteProps) {
    const { isAuthenticated, loading } = useAuth();
    const { canRead } = usePermissions(entityType);

    if (loading) return null;
    if (!isAuthenticated) return <Navigate to={ROUTE_ROOT} replace />;

    return canRead ? (
        <Outlet />
    ) : (
        <Navigate
            to={ROUTE_DASHBOARD}
            replace
            state={{ unauthorized: true }}
        />
    );
}