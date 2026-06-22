import { describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ThemeProvider } from "@mui/material/styles";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { lightTheme } from "../themes";
import { FormProvider, useFormContainer, FORM_TYPES } from "../../context/FormContext";
import FormRoot from "../../components/forms/FormRoot";
import LocationInventoryQuantityPage from "../../pages/inventory/LocationInventoryQuantityPage";
import { PermissionContext } from "../../permissions/AppPermissionContext";
import { createMongoAbility } from "@casl/ability";
import { CRUD_OPERATIONS, ENTITY_TYPES, type AppAbility } from "../../permissions/permissions";

vi.mock("../../api/services/inventory/itemService", () => ({
    default: { getAll: vi.fn() }
}));

vi.mock("../../api/services/inventory/inventoryQuantityService", () => ({
    default: { getLinesByLocation: vi.fn() },
}));

vi.mock("../../api/services/inventory/locationService", () => ({
    default: { getById: vi.fn() }, // FIX 2 : Déclarer getById comme vi.fn() pour pouvoir utiliser mockResolvedValue
}));

vi.mock("../../context/AuthContext", () => ({
    useAuth: () => ({ user: { employeeProfile: { locationId: 1 } } }),
}));

import itemService from "../../api/services/inventory/itemService";
import inventoryQuantityService from "../../api/services/inventory/inventoryQuantityService";
import locationService from "../../api/services/inventory/locationService";

function OpenAddStockButton() {
    const { openForm } = useFormContainer();
    return (
        <button
            type="button"
            onClick={() => openForm(FORM_TYPES.QUANTITY, {
                mode: "add",
                fixedLocationId: 1,
                locationId: 1,
                locationName: "Succursale Québec",
            })}
        >
            Ouvrir ajout stock
        </button>
    );
}

describe("Location inventory add stock flow", () => {
    it("opens modal without crashing the page", async () => {
        vi.mocked(locationService.getById).mockResolvedValue({
            id: 1,
            title: "Succursale Québec",
            address: "123 Rue Saint-Jean",
            description: "Succursale principale",
        });
        vi.mocked(inventoryQuantityService.getLinesByLocation).mockResolvedValue([
            {
                locationId: 1,
                locationTitle: "Succursale Québec",
                itemId: 1,
                itemName: "Clean Code",
                quantity: 8,
            },
        ]);
        vi.mocked(itemService.getAll).mockResolvedValue([
            {
                id: 1,
                name: "Clean Code",
                isBook: true,
                description: "",
                distributor: "",
                imageUrl: null,
                price: 10,
                alertQuantity: 2,
                totalQuantity: 8,
                isLowStock: false,
                lastUpdate: "",
                isActive: true,
                isbn: null,
                publicationDate: null,
                authors: [],
                publishers: [],
                categories: [],
                categoryIds: [],
            },
            {
                id: 99,
                name: "Nouveau produit",
                isBook: false,
                description: "",
                distributor: "",
                imageUrl: null,
                price: 10,
                alertQuantity: 2,
                totalQuantity: 0,
                isLowStock: false,
                lastUpdate: "",
                isActive: true,
                isbn: null,
                publicationDate: null,
                authors: [],
                publishers: [],
                categories: [],
                categoryIds: [],
            },
        ]);

        const queryClient = new QueryClient({
            defaultOptions: { queries: { retry: false } },
        });
        const ability = createMongoAbility<AppAbility>();
        ability.update([{ action: CRUD_OPERATIONS.MANAGE, subject: ENTITY_TYPES.ALL }]);

        render(
            <QueryClientProvider client={queryClient}>
                <ThemeProvider theme={lightTheme}>
                    <PermissionContext.Provider value={ability}>
                        <FormProvider>
                            <MemoryRouter initialEntries={["/succursales/1/inventaire"]}>
                                <Routes>
                                    <Route path="/succursales/:id/inventaire" element={<LocationInventoryQuantityPage />} />
                                </Routes>
                                <FormRoot />
                                <OpenAddStockButton />
                            </MemoryRouter>
                        </FormProvider>
                    </PermissionContext.Provider>
                </ThemeProvider>
            </QueryClientProvider>
        );

        expect(await screen.findByText(/Inventaire de Succursale Québec/i)).toBeInTheDocument();

        await userEvent.click(screen.getByRole("button", { name: /Ouvrir ajout stock/i }));

        expect(await screen.findByText(/Ajouter un article au stock/i)).toBeInTheDocument();
        await waitFor(() => {
            expect(screen.getByText(/Inventaire de Succursale Québec/i)).toBeInTheDocument();
        });
    });
});