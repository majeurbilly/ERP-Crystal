import { cleanup, fireEvent, render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import TimesheetForm from "../../../components/forms/hr/TimesheetForm";
import type { EmployeeProfile } from "../../types/hr/employeeProfile";
import type { TimesheetFormData } from "../../types/hr/timesheet";

const mockEmployees: EmployeeProfile[] = [
    {
        id: 7,
        firstName: "Sophie",
        lastName: "Lavoie",
        email: "sophie@test.ca",
        hiringDate: "2024-02-01",
        jobPositionId: 1,
        jobPositionName: "Vendeur",
        applicationUserId: null,
        salary: 45000,
        status: "Active",
        isDeleted: false,
    },
];

const addTimesheetMock = vi.fn().mockResolvedValue(undefined);
const setShowTimesheetFormMock = vi.fn();

vi.mock("../../../api/mutations/hr/useTimesheetMutations", () => ({
    useTimesheetMutations: () => ({
        addTimesheet: addTimesheetMock,
        isAddingTimesheet: false,
        updateTimesheet: vi.fn(),
        isUpdatingTimesheet: false,
        updateTimesheetStatus: vi.fn(),
        isUpdatingTimesheetStatus: false,
    }),
}));

vi.mock("../../../api/services/hr/employeeProfileService", () => ({
    default: { getAll: vi.fn() },
}));

vi.mock("../../../data/utils/popupMessageManager", () => ({
    notifySuccessMessage: vi.fn(),
    notifyErrorMessage: vi.fn(),
}));

import employeeProfileService from "../../../api/services/hr/employeeProfileService";

function renderTimesheetForm(): ReturnType<typeof render> {
    const queryClient: QueryClient = new QueryClient({
        defaultOptions: {
            queries: { retry: false },
            mutations: { retry: false },
        },
    });

    return render(
        <QueryClientProvider client={queryClient}>
            <TimesheetForm
                showTimesheetForm={true}
                setShowTimesheetForm={setShowTimesheetFormMock}
            />
        </QueryClientProvider>
    );
}

describe("TimesheetForm", () => {
    beforeEach(() => {
        vi.mocked(employeeProfileService.getAll).mockResolvedValue(mockEmployees);
        addTimesheetMock.mockClear();
        setShowTimesheetFormMock.mockClear();
    });

    afterEach(() => {
        cleanup();
        vi.clearAllMocks();
    });

    it("should render employee options from mocked query", async () => {
        const user = userEvent.setup();
        renderTimesheetForm();

        expect(await screen.findByText("Ajouter une feuille de temps")).toBeInTheDocument();
        expect(employeeProfileService.getAll).toHaveBeenCalled();

        await user.click(await screen.findByRole("combobox", { name: /employé/i }));
        expect(await screen.findByRole("option", { name: "Sophie Lavoie" })).toBeInTheDocument();
    });

    it("should call addTimesheet with correct payload on submit", async () => {
        const user = userEvent.setup();
        renderTimesheetForm();

        await screen.findByText("Ajouter une feuille de temps");

        await user.click(await screen.findByRole("combobox", { name: /employé/i }));
        await user.click(await screen.findByRole("option", { name: "Sophie Lavoie" }));

        const debutInput = screen.getByLabelText(/début de période/i);
        const finInput = screen.getByLabelText(/fin de période/i);

        fireEvent.change(debutInput, { target: { value: "2026-05-01" } });
        fireEvent.change(finInput, { target: { value: "2026-05-31" } });

        const formElement = document.querySelector("form");
        expect(formElement).not.null;
        fireEvent.submit(formElement!);

        const expectedPayload: TimesheetFormData = {
            employeeProfileId: 7,
            periodStart: "2026-05-01",
            periodEnd: "2026-05-31",
            timeEntryIds: [],
        };

        await waitFor(() => {
            expect(addTimesheetMock).toHaveBeenCalledTimes(1);
            expect(addTimesheetMock).toHaveBeenCalledWith(expectedPayload);
            expect(setShowTimesheetFormMock).toHaveBeenCalledWith(false);
        });
    });

    it("should not submit when period end is before period start", async () => {
        const user = userEvent.setup();
        renderTimesheetForm();

        await screen.findByText("Ajouter une feuille de temps");

        await user.click(await screen.findByRole("combobox", { name: /employé/i }));
        await user.click(await screen.findByRole("option", { name: "Sophie Lavoie" }));

        const debutInput = screen.getByLabelText(/début de période/i);
        const finInput = screen.getByLabelText(/fin de période/i);

        fireEvent.change(debutInput, { target: { value: "2026-05-31" } });
        fireEvent.change(finInput, { target: { value: "2026-05-01" } });

        const formElement = document.querySelector("form");
        expect(formElement).not.null;
        fireEvent.submit(formElement!);

        expect(addTimesheetMock).not.toHaveBeenCalled();
        expect(
            await screen.findByText(
                "La date de fin doit être postérieure ou égale à la date de début."
            )
        ).toBeInTheDocument();
    });
});