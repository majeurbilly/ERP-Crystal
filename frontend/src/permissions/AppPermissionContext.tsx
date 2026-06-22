import React, { createContext, useContext, useState, useEffect } from 'react';
import { AbilityProvider as CaslProvider, Can } from '@casl/react';
import { defineAbilityFor } from './permissions';

import type { AppAbility } from './permissions';
import type { SessionUser } from '../context/AuthContext';
import type { PermissionRule } from '../data/types/hr/dynamicUserRole';
import permissionService from '../api/services/hr/permissionService';

export const PermissionContext = createContext<AppAbility | undefined>(undefined);
export const PermissionRulesContext = createContext<PermissionRule[]>([]);

export const AppCan = Can;

interface AbilityProviderProps {
    user: SessionUser | null;
    children: React.ReactNode;
}

export const PermissionProvider: React.FC<AbilityProviderProps> = ({ user, children }) => {
    const [permission, setPermission] = useState<AppAbility>(() => defineAbilityFor(user, []));
    const [permissionRules, setPermissionRules] = useState<PermissionRule[]>([]);

    const permissionUserKey = user
        ? `${user.id}:${user.dynamicRole?.id ?? ""}`
        : "";

    useEffect(() => {
        let isMounted = true;

        async function fetchAndSetPermissions() {
            if (!user) {
                if (isMounted) {
                    setPermission(defineAbilityFor(null, []));
                    setPermissionRules([]);
                }
                return;
            }

            if (user.dynamicRole?.permissions) {
                if (isMounted) {
                    setPermissionRules(user.dynamicRole.permissions);
                    setPermission(defineAbilityFor(user, user.dynamicRole.permissions));
                }
                return;
            }

            try {
                const myPermissions = await permissionService.getMyPermissions();

                if (isMounted) {
                    setPermissionRules(myPermissions.permissions);
                    setPermission(defineAbilityFor(user, myPermissions.permissions));
                }
            } catch (error) {
                console.error("Failed to fetch user permissions:", error);
                if (isMounted) {
                    setPermissionRules([]);
                    setPermission(defineAbilityFor(user, []));
                }
            }
        }

        fetchAndSetPermissions();

        return () => {
            isMounted = false;
        };
    }, [permissionUserKey, user]);

    return (
        <PermissionRulesContext.Provider value={permissionRules}>
            <PermissionContext.Provider value={permission}>
                <CaslProvider value={permission}>
                    {children}
                </CaslProvider>
            </PermissionContext.Provider>
        </PermissionRulesContext.Provider>
    );
};

export const useAppPermissionContext = () => {
    const context = useContext(PermissionContext);
    if (!context) throw new Error('useAppPermission must be used within a PermissionProvider');
    return context;
};

export const usePermissionRules = () => useContext(PermissionRulesContext);
