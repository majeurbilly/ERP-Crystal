export type LanguageCode = 'fr' | 'en';
export const DEFAULT_LANGUAGE: LanguageCode = 'fr';

interface AuthTranslations {
    enterDetails: string;
    forgotPassword: string;
    loginButton: string;
    noAccount: string;
    signUp: string;
    contact: string;
    email: string;
    password: string;
    welcomeBack: string;
    thirtyDays: string;
}

interface DashboardTranslations {
    nextShift: string,
    noScheduledShift: string,
    pendingLeavesCountTitleAdmin: string,
    pendingLeavesCountTitleUser: string,
    pendingLeavesSubtitle: string,
    inventoryTitle: string,
    inventoryValue: string,
    inventorySubtitle: string,
    pendingTimesheetsTitle: string,
    catalogAlertsTitle: string,
    catalogAlertsValue: string,
    catalogAlertsSubtitle: string,
    rolesPermissionsTitle: string,
    rolesPermissionsValue: string,
    connectedAs: string
}

type TranslationSchema = Record<LanguageCode, {
    auth: AuthTranslations;
    dashboard: DashboardTranslations;
}>;

export const TRANSLATIONS = {
    fr: {
        auth: {
            enterDetails: "Veuillez saisir vos informations",
            forgotPassword: "Mot de passe oublié ?",
            loginButton: "Se connecter",
            noAccount: "Vous n'avez pas de compte ?",
            signUp: "S'inscrire",
            contact: "Nous contacter",
            email: "Courriel",
            password: "Mot de passe",
            welcomeBack: "Heureux de vous revoir !",
            thirtyDays: "Mémoriser pendant 30 jours"
        },
        dashboard: {
            nextShift: "Next Shift",
            noScheduledShift: "No shifts scheduled",
            pendingLeavesCountTitleAdmin: "Pending Leave Requests",
            pendingLeavesCountTitleUser: "My Pending Leaves",
            pendingLeavesSubtitle: "Requests processing or pending",
            inventoryTitle: "Inventory",
            inventoryValue: "View",
            inventorySubtitle: "Quantities per location",
            pendingTimesheetsTitle: "Pending Timesheets",
            catalogAlertsTitle: "Catalog Alerts",
            catalogAlertsValue: "View",
            catalogAlertsSubtitle: "Items and stock",
            rolesPermissionsTitle: "Roles & Permissions",
            rolesPermissionsValue: "Manage",
            connectedAs: "Logged in:"
        }
    },
    en: {
        auth: {
            enterDetails: "Please enter your details",
            forgotPassword: "Forgot password?",
            loginButton: "Log in",
            noAccount: "Don't have an account?",
            signUp: "Sign up",
            contact: "Contact",
            email: "Email",
            password: "Password",
            welcomeBack: "Welcome back!",
            thirtyDays: "Remember for 30 days"
        },
        dashboard: {
            nextShift: "Next Shift",
            noScheduledShift: "No shifts scheduled",
            pendingLeavesCountTitleAdmin: "Pending Leave Requests",
            pendingLeavesCountTitleUser: "My Pending Leaves",
            pendingLeavesSubtitle: "Requests processing or pending",
            inventoryTitle: "Inventory",
            inventoryValue: "View",
            inventorySubtitle: "Quantities per location",
            pendingTimesheetsTitle: "Pending Timesheets",
            catalogAlertsTitle: "Catalog Alerts",
            catalogAlertsValue: "View",
            catalogAlertsSubtitle: "Items and stock",
            rolesPermissionsTitle: "Roles & Permissions",
            rolesPermissionsValue: "Manage",
            connectedAs: "Logged in:"
        }
    }
} as const satisfies TranslationSchema;