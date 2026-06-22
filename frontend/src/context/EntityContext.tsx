import { createContext, useContext, useEffect, useState, type ReactNode } from "react";
import permissionEntityService from "../api/services/hr/permissionEntityService";

interface EntityContextType {
    activeEntities: string[];
    isLoading: boolean;
    error: Error | null;
}

const EntityContext = createContext<EntityContextType | undefined>(undefined);

export function EntityProvider({ children }: { children: ReactNode }) {
    const [activeEntities, setActiveEntities] = useState<string[]>([]);
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const [error, setError] = useState<Error | null>(null);

    useEffect(() => {
        let isCurrentRequest = true;

        permissionEntityService.getAll()
            .then((entities) => {
                if (isCurrentRequest) {
                    const entityKeys = entities.map((e) => e.id);
                    setActiveEntities(entityKeys);
                    setIsLoading(false);
                }
            })
            .catch((err) => {
                if (isCurrentRequest) {
                    console.error("Failed to load permission entities:", err);
                    setError(err instanceof Error ? err : new Error("Unknown error"));
                    setIsLoading(false);
                }
            });

        return () => {
            isCurrentRequest = false;
        };
    }, []);

    return (
        <EntityContext.Provider value={{ activeEntities, isLoading, error }}>
            {children}
        </EntityContext.Provider>
    );
}

export const useActiveEntities = () => {
    const context = useContext(EntityContext);
    if (!context) {
        throw new Error("useActiveEntities must be used within an EntityProvider");
    }
    return context;
};