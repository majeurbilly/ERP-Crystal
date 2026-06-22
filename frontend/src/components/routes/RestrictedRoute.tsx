import { Navigate, Outlet } from "react-router-dom"
import { useAuth } from "../../context/AuthContext"
import {
    ROUTE_DASHBOARD,
    ROUTE_ROOT
} from "../../data/routeNames";
import { usePermissions } from "../../permissions/usePermissions";
import { ENTITY_TYPES } from "../../permissions/permissions";

interface RestrictedRouteProps {
    userHasAccess: boolean;
    routeTo: string;
}

function RestrictedRoute({ userHasAccess, routeTo }: RestrictedRouteProps) {
    const { isAuthenticated, loading } = useAuth();

    if (loading) return null;

    if (!isAuthenticated) {
        return <Navigate to={ROUTE_ROOT} replace />
    }

    if (userHasAccess) {
        return <Outlet />
    } else {
        return <Navigate to={routeTo} replace />
    }
}

export function AuthenticatedExclusiveRoute() {
    const { isAuthenticated, loading } = useAuth();

    if (loading) return null;

    return <RestrictedRoute userHasAccess={isAuthenticated} routeTo={ROUTE_ROOT} />
}

export function HrDashboardExclusiveRoute() {
    const { canRead: userHasAccess } = usePermissions(ENTITY_TYPES.HR_DASHBOARD);
    return <RestrictedRoute userHasAccess={userHasAccess} routeTo={ROUTE_DASHBOARD} />
}

export function EmploymentContractExclusiveRoute() {
    const { canRead: userHasAccess } = usePermissions(ENTITY_TYPES.EMPLOYMENT_CONTRACT);
    return <RestrictedRoute userHasAccess={userHasAccess} routeTo={ROUTE_DASHBOARD} />
}

export function PayrollExclusiveRoute() {
    const { canRead: userHasAccess } = usePermissions(ENTITY_TYPES.PAYROLL);
    return <RestrictedRoute userHasAccess={userHasAccess} routeTo={ROUTE_DASHBOARD} />
}

export function TimesheetExclusiveRoute() {
    const { canRead: userHasAccess } = usePermissions(ENTITY_TYPES.TIMESHEET);
    return <RestrictedRoute userHasAccess={userHasAccess} routeTo={ROUTE_DASHBOARD} />
}
