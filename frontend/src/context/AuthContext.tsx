import { createContext, useContext, useState, type ReactNode } from 'react';
import { type UserRole } from '../data/userRoles';
import { isUserRole } from '../data/devAuth';
import { extractServerRoleFromJwt, extractUserIdFromJwt } from '../data/authJwt';


/**
 * CODE TEMPORAIRE DE WILL, CECI EXISTE POUR EMPECHER LE ID DES UTILISATEURS D'ETRE DIFFERENT CHAQUE FOIS
 */
const ROLE_IDS: Record<string, string> = {
    "admin@crystal.local": "1",
    "employee@crystal.local": "2",
    "assistant@crystal.local": "3",
    "gerant@crystal.local": "4",
};
/**
 * FIN CODE WILL
 */

interface AuthContextType {
    token: string | null;
    role: UserRole | null;
    id: string | null;
    login: (token: string, email?: string) => void;
    logout: () => void;
    isAuthenticated: boolean;
}

const getRoleFromToken = (token: string | null): UserRole | null => {
    if (!token) return null;
    try {
        const rawRole = extractServerRoleFromJwt(token);
        const normalized = rawRole.toLowerCase();
        return isUserRole(normalized) ? normalized : null;
    } catch {
        return null;
    }
};

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
    const [token, setToken] = useState<string | null>(localStorage.getItem("token"));


    const [id, setId] = useState<string | null>(() => {
        const savedId = localStorage.getItem("user_id");
        if (savedId) return savedId;

        const token = localStorage.getItem("token");
        return token ? extractUserIdFromJwt(token) : null;
    });

    const [role, setRole] = useState<UserRole | null>(() =>
        getRoleFromToken(localStorage.getItem("token"))
    );

    const login = (newToken: string, email?: string) => {
        const newRole = getRoleFromToken(newToken);

        let newId;
        if (email) {
            newId = ROLE_IDS[email];
        } else {
            newId = extractUserIdFromJwt(newToken);
        }

        localStorage.setItem("token", newToken);
        localStorage.setItem("user_id", newId); //CODE TEMPORAIRE

        setToken(newToken);
        setRole(newRole);
        setId(newId);
    };

    const logout = () => {
        localStorage.removeItem("token");
        setToken(null);
        setRole(null);
        setRole(null);
    };

    const isAuthenticated = !!token;

    return (
        <AuthContext.Provider value={{ token, role, id, login, logout, isAuthenticated }}>
            {children}
        </AuthContext.Provider>
    );
}

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (!context) throw new Error("useAuth must be used within AuthProvider");
    return context;
}