import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, type RenderOptions } from "@testing-library/react";
import type { ReactElement, ReactNode } from "react";
import { MemoryRouter } from "react-router-dom";
import { DeleteDialogProvider } from "../../../context/DeleteDialogContext";
import { FormProvider } from "../../../context/FormContext";
import { PRESET_ROLE_IDS, type PresetRoleId } from "../../types/hr/userRoles";
import { PermissionContext } from "../../../permissions/AppPermissionContext";
import { createMongoAbility } from "@casl/ability";
import { CRUD_OPERATIONS, ENTITY_TYPES, type AppAbility } from "../../../permissions/permissions";

export function createTestQueryClient(): QueryClient {
    return new QueryClient({
        defaultOptions: {
            queries: { retry: false },
            mutations: { retry: false },
        },
    });
}

interface RenderWithProvidersOptions extends Omit<RenderOptions, "wrapper"> {
    initialRoute?: string;
    role?: PresetRoleId;
}

const createTestAbility = (): AppAbility => {
    const ability = createMongoAbility<AppAbility>();

    ability.update([{ action: CRUD_OPERATIONS.MANAGE, subject: ENTITY_TYPES.ALL }]);

    return ability;
};

export function renderWithHrProviders(
    ui: ReactElement,
    options: RenderWithProvidersOptions = {}
): ReturnType<typeof render> {
    const { initialRoute = "/", ...renderOptions } = options;
    const queryClient: QueryClient = createTestQueryClient();

    function Wrapper({ children }: { children: ReactNode }): ReactElement {
        const testAbility = createTestAbility();
        return (
            <QueryClientProvider client={queryClient}>
                <MemoryRouter initialEntries={[initialRoute]}>
                    <DeleteDialogProvider>
                        <PermissionContext.Provider value={testAbility}>
                            <FormProvider>{children}</FormProvider>
                        </PermissionContext.Provider>

                    </DeleteDialogProvider>
                </MemoryRouter>
            </QueryClientProvider>
        );
    }

    return render(ui, { wrapper: Wrapper, ...renderOptions });
}

export const adminAuthMock = {
    token: "test-token",
    role: PRESET_ROLE_IDS.ADMIN,
    id: "admin-user-id",
    login: (): void => { },
    logout: (): void => { },
    isAuthenticated: true,
};
