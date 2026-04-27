// import { Navigate, Outlet } from "react-router-dom"
// import { useAuth } from "../../context/AuthContext"
// import { ROUTE_DASHBOARD } from "../../data/routeNames";
// import { ROLES } from "../../data/userRoles";

// interface RestrictedRouteProps {
//     userHasAccess: boolean;
// }

// function RestrictedRoute({ userHasAccess }: RestrictedRouteProps) {

//     if (userHasAccess) {
//         return <Outlet />
//     } else {
//         return <Navigate to={ROUTE_DASHBOARD} replace />
//     }
// }

// export function AdminOrManagerExclusiveRoute() {
//     const { role } = useAuth();
//     const userHasAccess = (role === ROLES.ADMIN || role === ROLES.GERANT);
//     return <RestrictedRoute userHasAccess={userHasAccess} />
// }