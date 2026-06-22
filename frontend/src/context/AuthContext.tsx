import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import { extractUserIdFromJwt } from '../data/utils/authJwt';
import type { DynamicUserRole } from '../data/types/hr/dynamicUserRole';
import type { EmployeeProfile } from '../data/types/hr/employeeProfile';
import { EntityProvider } from './EntityContext';
import employeeProfileService from '../api/services/hr/employeeProfileService';
import permissionService from '../api/services/hr/permissionService';
import userService from '../api/services/hr/userService';
import {
    clearSessionExpiredHandler,
    isInvalidSessionError,
    registerSessionExpiredHandler,
} from '../api/sessionUtils';

export interface SessionUser {
    id: string;
    dynamicRole: DynamicUserRole | null;
    employeeProfile: EmployeeProfile | undefined;
    userName: string;
    email: string;
}

interface AuthContextType {
    token: string | null;
    user: SessionUser | null;
    login: (token: string) => void;
    logout: () => void;
    isAuthenticated: boolean;
    loading: boolean;
}

const tokenLocalStorage: string = "token";

const getIdFromToken = (token: string | null): string | null => {
    if (!token) return null;
    try {
        return extractUserIdFromJwt(token);
    } catch {
        return null;
    }
};

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
    const [token, setToken] = useState<string | null>(localStorage.getItem(tokenLocalStorage));

    const [loading, setLoading] = useState<boolean>(!!localStorage.getItem(tokenLocalStorage));

    const [asyncUserData, setAsyncUserData] = useState<{
        profile: EmployeeProfile | undefined;
        dynamicRole: DynamicUserRole | null;
        userName: string;
        email: string;
    }>({ profile: undefined, dynamicRole: null, userName: "", email: "" });

    const logout = useCallback(() => {
        localStorage.removeItem(tokenLocalStorage);
        setToken(null);

        localStorage.removeItem('sidebar_inventory_open');
        localStorage.removeItem('sidebar_hr_open');
    }, []);

    useEffect(() => {
        registerSessionExpiredHandler(logout);
        return () => clearSessionExpiredHandler();
    }, [logout]);

    const id = getIdFromToken(token);

    const user = useMemo<SessionUser | null>(() => {
        if (!id) return null;
        return {
            id,
            dynamicRole: asyncUserData.dynamicRole,
            employeeProfile: asyncUserData.profile,
            userName: asyncUserData.userName || id,
            email: asyncUserData.email,
        };
    }, [id, asyncUserData]);

    useEffect(() => {
        let isCurrentRequest = true;

        if (id) {
            setLoading(true);

            Promise.allSettled([
                employeeProfileService.getMe(),
                permissionService.getMyPermissions(),
                userService.getMe(),
            ]).then((results) => {
                if (!isCurrentRequest) {
                    return;
                }

                const profileResult = results[0];
                const permissionsResult = results[1];
                const meResult = results[2];

                if (
                    (meResult.status === "rejected" && isInvalidSessionError(meResult.reason))
                    || (permissionsResult.status === "rejected" && isInvalidSessionError(permissionsResult.reason))
                ) {
                    logout();
                    setLoading(false);
                    return;
                }

                const profile: EmployeeProfile | undefined =
                    profileResult.status === "fulfilled" ? profileResult.value : undefined;

                const myPermissions =
                    permissionsResult.status === "fulfilled" ? permissionsResult.value : null;

                const me = meResult.status === "fulfilled" ? meResult.value : null;

                const dynamicRole: DynamicUserRole | null = myPermissions
                    ? {
                        id: myPermissions.roleId,
                        name: myPermissions.roleName,
                        permissions: myPermissions.permissions,
                    }
                    : null;

                setAsyncUserData({
                    profile,
                    dynamicRole,
                    userName: me?.userName ?? "",
                    email: me?.email ?? "",
                });

                setLoading(false);
            });
        } else {
            setAsyncUserData({ profile: undefined, dynamicRole: null, userName: "", email: "" });
            setLoading(false);
        }

        return () => {
            isCurrentRequest = false;
        };
    }, [id, logout]);

    const login = (newToken: string) => {
        localStorage.setItem(tokenLocalStorage, newToken);
        setToken(newToken);
    };

    const isAuthenticated = !!token;

    return (
        <EntityProvider>
            <AuthContext.Provider value={{ token, user, login, logout, isAuthenticated, loading }}>
                {children}
            </AuthContext.Provider>
        </EntityProvider>
    );
}

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (!context) throw new Error("useAuth must be used within AuthProvider");
    return context;
}