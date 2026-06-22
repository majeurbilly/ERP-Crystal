import { cleanup, fireEvent, render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import EmployeeProfileForm from "../../../components/forms/hr/EmployeeProfileForm";
import type { EmployeeProfileFormData } from "../../types/hr/employeeProfile";
import type { User } from "../../types/hr/user";
import type { Location } from "../../types/inventory/location";
import { PRESET_ROLE_IDS } from "../../types/hr/userRoles";

const mockLocations: Location[] = [
    {
        id: 2,
        title: "Succursale Sainte-Foy",
        address: "123 rue Test",
        description: "",
    },
];

const mockUsers: User[] = [
    {
        id: "user-abc",
        userName: "jdoe",
        email: "jdoe@test.ca",
        dynamicRoleId: PRESET_ROLE_IDS.ADMIN,
        dynamicRoleName: "Administrateur",
    },
];

const addEmployeeProfileMock = vi.fn().mockResolvedValue(undefined);
const setShowEmployeeProfileFormMock = vi.fn();

vi.mock("../../../api/services/hr/userService", () => ({
    default: { getAll: vi.fn() },
}));

vi.mock("../../../api/services/inventory/locationService", () => ({
    default: { getAll: vi.fn() },
}));

vi.mock("../../../api/mutations/hr/useEmployeeProfileMutations", () => ({
    useEmployeeProfileMutations: () => ({
        addEmployeeProfile: addEmployeeProfileMock,
        isAddingEmployeeProfile: false,
        updateEmployeeProfile: vi.fn(),
        isUpdatingEmployeeProfile: false,
    }),
}));

vi.mock("../../../data/popupMessageManager", () => ({
    notifySuccessMessage: vi.fn(),
    notifyErrorMessage: vi.fn(),
}));

import userService from "../../../api/services/hr/userService";
import locationService from "../../../api/services/inventory/locationService";

function renderEmployeeProfileForm(): ReturnType<typeof render> {
    const queryClient: QueryClient = new QueryClient({
        defaultOptions: {
            queries: { retry: false },
            mutations: { retry: false },
        },
    });

    return render(
        <QueryClientProvider client={queryClient}>
            <EmployeeProfileForm
                showEmployeeProfileForm={true}
                setShowEmployeeProfileForm={setShowEmployeeProfileFormMock}
                editEmployeeProfile={null}
            />
        </QueryClientProvider>
    );
}

describe("EmployeeProfileForm", () => {
    beforeEach(() => {
        vi.mocked(userService.getAll).mockResolvedValue(mockUsers);
        vi.mocked(locationService.getAll).mockResolvedValue(mockLocations);
        addEmployeeProfileMock.mockClear();
        setShowEmployeeProfileFormMock.mockClear();
    });

    afterEach(() => {
        cleanup();
        vi.clearAllMocks();
    });

    it("should render the form with dropdown options from mocked queries", async () => {
        const user = userEvent.setup();
        renderEmployeeProfileForm();

        expect(await screen.findByText("Ajouter un employé")).toBeInTheDocument();
        expect(userService.getAll).toHaveBeenCalled();
        expect(locationService.getAll).toHaveBeenCalled();

        const userCombobox = await screen.findByRole("combobox", { name: /utilisateur système \(optionnel\)/i });
        await user.click(userCombobox);

        expect(await screen.findByRole("option", { name: /jdoe/i })).toBeInTheDocument();
        expect(screen.getByRole("option", { name: "Aucun utilisateur lié" })).toBeInTheDocument();
    });

    it("should call addEmployeeProfile with form data when submitting a valid new employee", async () => {
        const user = userEvent.setup();
        renderEmployeeProfileForm();

        await screen.findByText("Ajouter un employé");

        await user.type(await screen.findByRole("textbox", { name: /prénom/i }), "Alice");
        await user.type(await screen.findByRole("textbox", { name: /^nom$/i }), "Martin");
        await user.type(await screen.findByRole("textbox", { name: /courriel/i }), "alice.martin@test.ca");

        const hiringDateInput = await screen.findByLabelText(/date d'embauche/i);
        fireEvent.change(hiringDateInput, { target: { value: "2024-05-15" } });

        await user.type(await screen.findByRole("spinbutton"), "62000");

        const statusCombobox = await screen.findByRole("combobox", { name: /^statut$/i });
        await user.click(statusCombobox);
        const statusOption = await screen.findByRole("option", { name: "Active" });
        await user.click(statusOption);

        const locationCombobox = await screen.findByRole("combobox", { name: /succursale \(optionnel\)/i });
        await user.click(locationCombobox);
        const locationOption = await screen.findByRole("option", { name: "Succursale Sainte-Foy" });
        await user.click(locationOption);

        const formElement: HTMLFormElement = document.querySelector("form") as HTMLFormElement;
        fireEvent.submit(formElement);

        const expectedPayload: EmployeeProfileFormData = {
            firstName: "Alice",
            lastName: "Martin",
            email: "alice.martin@test.ca",
            applicationUserId: null,
            salary: 62000,
            status: "Active",
            hiringDate: "2024-05-15",
            locationId: 2,
        };

        await waitFor(() => {
            expect(addEmployeeProfileMock).toHaveBeenCalledTimes(1);
            expect(addEmployeeProfileMock).toHaveBeenCalledWith(expectedPayload);
            expect(setShowEmployeeProfileFormMock).toHaveBeenCalledWith(false);
        });
    });

    it("should not submit when required fields are missing", async () => {
        renderEmployeeProfileForm();

        await screen.findByText("Ajouter un employé");
        const formElement: HTMLFormElement = document.querySelector("form") as HTMLFormElement;
        fireEvent.submit(formElement);

        expect(addEmployeeProfileMock).not.toHaveBeenCalled();
        expect(await screen.findByText("Le prénom est requis.")).toBeInTheDocument();
        expect(screen.getByText("Le nom est requis.")).toBeInTheDocument();
    });
});