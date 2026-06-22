import { cleanup, fireEvent, render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import LeaveRequestForm from "../../../components/forms/hr/LeaveRequestForm";
import { LEAVE_TYPES } from "../../types/hr/leaveRequest";

const addLeaveRequestMock = vi.fn().mockResolvedValue(undefined);
const setShowLeaveRequestFormMock = vi.fn();

vi.mock("../../../api/services/hr/employeeProfileService", () => ({
    default: { getAll: vi.fn() },
}));

vi.mock("../../../api/mutations/hr/useLeaveRequestMutations", () => ({
    useLeaveRequestMutations: () => ({
        addLeaveRequest: addLeaveRequestMock,
        isAddingLeaveRequest: false,
    }),
}));

vi.mock("../../../data/utils/popupMessageManager", () => ({
    notifySuccessMessage: vi.fn(),
    notifyErrorMessage: vi.fn(),
}));

import employeeProfileService from "../../../api/services/hr/employeeProfileService";

describe("Phase 3 — LeaveRequestForm selfMode", () => {
    beforeEach(() => {
        addLeaveRequestMock.mockClear();
        setShowLeaveRequestFormMock.mockClear();
    });

    afterEach(() => {
        cleanup();
        vi.clearAllMocks();
    });

    it("n'appelle pas getAll et masque le sélecteur employé", async () => {
        const queryClient = new QueryClient({
            defaultOptions: { queries: { retry: false } },
        });

        render(
            <QueryClientProvider client={queryClient}>
                <LeaveRequestForm
                    showLeaveRequestForm={true}
                    setShowLeaveRequestForm={setShowLeaveRequestFormMock}
                    selfMode
                    defaultEmployeeProfileId={42}
                />
            </QueryClientProvider>
        );

        expect(await screen.findByText("Ajouter une demande de congé")).toBeInTheDocument();
        expect(employeeProfileService.getAll).not.toHaveBeenCalled();
        expect(screen.queryByRole("combobox", { name: /employé/i })).not.toBeInTheDocument();
    });

    it("soumet avec l'employeeProfileId par défaut en selfMode", async () => {
        const user = userEvent.setup();
        const queryClient = new QueryClient({
            defaultOptions: { queries: { retry: false } },
        });

        render(
            <QueryClientProvider client={queryClient}>
                <LeaveRequestForm
                    showLeaveRequestForm={true}
                    setShowLeaveRequestForm={setShowLeaveRequestFormMock}
                    selfMode
                    defaultEmployeeProfileId={42}
                />
            </QueryClientProvider>
        );

        await screen.findByText("Ajouter une demande de congé");

        await user.click(await screen.findByRole("combobox", { name: /type de congé/i }));
        await user.click(await screen.findByRole("option", { name: "Vacances" }));
        const startDateInput = await screen.findByLabelText(/date de début/i);
        const endDateInput = await screen.findByLabelText(/date de fin/i);

        fireEvent.change(startDateInput, { target: { value: "2026-07-01" } });
        fireEvent.change(endDateInput, { target: { value: "2026-07-05" } });

        const formElement = document.querySelector("form") as HTMLFormElement;
        formElement.requestSubmit();

        await waitFor(() => {
            expect(addLeaveRequestMock).toHaveBeenCalledWith({
                employeeProfileId: 42,
                leaveType: LEAVE_TYPES.Vacation,
                startDate: "2026-07-01",
                endDate: "2026-07-05",
                reason: null,
            });
        });
    });
});