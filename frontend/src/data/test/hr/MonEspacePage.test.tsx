import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { cleanup, render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { createMongoAbility } from "@casl/ability";
import MySpacePage from "../../../pages/MySpacePage";
import { PermissionContext } from "../../../permissions/AppPermissionContext";
import { CRUD_OPERATIONS, ENTITY_TYPES, type AppAbility } from "../../../permissions/permissions";

const testData = vi.hoisted(() => ({
    profile: {
        id: 7,
        firstName: "Emilie",
        lastName: "Employee",
        email: "employee@crystal.local",
        applicationUserId: "employee-user-id",
        hiringDate: "2024-01-15",
        salary: 43000,
        status: "Active",
        jobPositionId: 2,
        jobPositionName: "Caissier",
        isDeleted: false,
        locationId: 1,
        locationTitle: "Succursale Quebec",
    },
}));

vi.mock("../../../context/AuthContext", () => ({
    useAuth: () => ({
        user: {
            id: "employee-user-id",
            email: "employee@crystal.local",
            employeeProfile: { id: 7, locationId: 1 },
        },
        isAuthenticated: true,
    }),
}));

vi.mock("../../../api/services/hr/scheduledShiftService", () => ({
    default: { getAll: vi.fn().mockResolvedValue([]) },
}));

vi.mock("../../../api/services/hr/leaveRequestService", () => ({
    default: { getAll: vi.fn().mockResolvedValue([]) },
}));

vi.mock("../../../api/services/hr/timeEntryService", () => ({
    default: { getAll: vi.fn().mockResolvedValue([]) },
}));

vi.mock("../../../api/services/hr/timesheetService", () => ({
    default: { getAll: vi.fn().mockResolvedValue([]) },
}));

vi.mock("../../../api/services/hr/employeeProfileService", () => ({
    default: { getMe: vi.fn().mockResolvedValue(testData.profile) },
}));

function renderMonEspace(): ReturnType<typeof render> {
    const queryClient = new QueryClient({
        defaultOptions: {
            queries: { retry: false },
            mutations: { retry: false },
        },
    });
    const ability = createMongoAbility<AppAbility>([
        { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.EMPLOYMENT_CONTRACT },
        { action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.PAYROLL },
    ]);

    return render(
        <QueryClientProvider client={queryClient}>
            <PermissionContext.Provider value={ability}>
                <MemoryRouter initialEntries={["/mon-espace?tab=fiche"]}>
                    <MySpacePage />
                </MemoryRouter>
            </PermissionContext.Provider>
        </QueryClientProvider>
    );
}

describe("MySpacePage", () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    afterEach(() => {
        cleanup();
    });

    it("affiche les accès aux contrats et aux fiches de paie dans Ma fiche", async () => {
        renderMonEspace();

        expect(await screen.findByRole("link", { name: /Mes contrats/i })).toHaveAttribute(
            "href",
            "/rh/contrats-de-travail"
        );
        expect(screen.getByRole("link", { name: /Mes fiches de paie/i })).toHaveAttribute(
            "href",
            "/rh/paie"
        );
    });
});
