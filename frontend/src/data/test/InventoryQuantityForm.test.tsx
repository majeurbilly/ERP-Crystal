import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ThemeProvider } from "@mui/material/styles";
import InventoryQuantityForm from "../../components/forms/inventory/InventoryQuantityForm";
import { lightTheme } from "../themes";

vi.mock("../../api/services/inventory/itemService", () => ({
    default: {
        getAll: vi.fn(),
    },
}));

vi.mock("../../api/services/inventory/inventoryQuantityService", () => ({
    default: {
        getLinesByLocation: vi.fn(),
        getLinesByItem: vi.fn(),
    },
}));

vi.mock("../../permissions/useScopedPermissions", () => ({
    useScopedPermissions: () => ({
        canUpdateInventoryOnLocation: () => true,
        canUpdateInventoryAnywhere: true,
        canPerformOnLocation: () => true,
        isSuperAdmin: true,
    }),
}));

import itemService from "../../api/services/inventory/itemService";
import inventoryQuantityService from "../../api/services/inventory/inventoryQuantityService";

describe("InventoryQuantityForm", () => {
    it("renders add mode without crashing", async () => {
        vi.mocked(itemService.getAll).mockResolvedValue([
            {
                id: 99,
                name: "Article test",
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
        vi.mocked(inventoryQuantityService.getLinesByLocation).mockResolvedValue([]);

        const queryClient = new QueryClient({
            defaultOptions: { queries: { retry: false } },
        });

        render(
            <QueryClientProvider client={queryClient}>
                <ThemeProvider theme={lightTheme}>
                    <InventoryQuantityForm
                        showForm={true}
                        setShowForm={() => { }}
                        editQuantity={{
                            mode: "add",
                            fixedLocationId: 1,
                            locationId: 1,
                            locationName: "Succursale Québec",
                        }}
                    />
                </ThemeProvider>
            </QueryClientProvider>
        );

        expect(await screen.findByText(/Ajouter un article au stock/i)).toBeInTheDocument();
    });
});