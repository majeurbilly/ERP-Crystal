import { cleanup, fireEvent, render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import LeaveRequestForm from "../../../components/forms/hr/LeaveRequestForm";
import type { EmployeeProfile } from "../../types/hr/employeeProfile";
import type { LeaveRequestFormData } from "../../types/hr/leaveRequest";
import { LEAVE_TYPES } from "../../types/hr/leaveRequest";

const mockEmployees: EmployeeProfile[] = [
    {
        id: 5,
        firstName: "Claire",
        lastName: "Bernard",
        email: "claire@test.ca",
        hiringDate: "2023-01-15",
        jobPositionId: 1,
        jobPositionName: "Analyste",
        applicationUserId: null,
        salary: 50000,
        status: "Active",
        isDeleted: false,
    },
];

const addLeaveRequestMock = vi.fn().mockResolvedValue(undefined);
const setShowLeaveRequestFormMock = vi.fn();

vi.mock("../../../api/services/hr/employeeProfileService", () => ({
    default: { getAll: vi.fn() },
}));

vi.mock("../../../api/mutations/hr/useLeaveRequestMutations", () => ({
    useLeaveRequestMutations: () => ({
        addLeaveRequest: addLeaveRequestMock,
        isAddingLeaveRequest: false,
        deleteLeaveRequest: vi.fn(),
        isDeletingLeaveRequest: false,
        updateLeaveRequestStatus: vi.fn(),
        isUpdatingLeaveRequestStatus: false,
    }),
}));

vi.mock("../../../data/utils/popupMessageManager", () => ({
    notifySuccessMessage: vi.fn(),
    notifyErrorMessage: vi.fn(),
}));

import employeeProfileService from "../../../api/services/hr/employeeProfileService";

function renderLeaveRequestForm(): ReturnType<typeof render> {
    const queryClient: QueryClient = new QueryClient({
        defaultOptions: {
            queries: { retry: false },
            mutations: { retry: false },
        },
    });

    return render(
        <QueryClientProvider client={queryClient}>
            <LeaveRequestForm
                showLeaveRequestForm={true}
                setShowLeaveRequestForm={setShowLeaveRequestFormMock}
            />
        </QueryClientProvider>
    );
}

describe("LeaveRequestForm", () => {
    beforeEach(() => {
        vi.mocked(employeeProfileService.getAll).mockResolvedValue(mockEmployees);
        addLeaveRequestMock.mockClear();
        setShowLeaveRequestFormMock.mockClear();
    });

    afterEach(() => {
        cleanup();
        vi.clearAllMocks();
    });

    it("should render employee options from mocked query", async () => {
        const user = userEvent.setup();
        renderLeaveRequestForm();

        expect(await screen.findByText("Ajouter une demande de congé")).toBeInTheDocument();
        expect(employeeProfileService.getAll).toHaveBeenCalled();

        await user.click(await screen.findByRole("combobox", { name: /employé/i }));
        expect(await screen.findByRole("option", { name: "Claire Bernard" })).toBeInTheDocument();
    });

    it("should call addLeaveRequest with correct payload on submit", async () => {
        const user = userEvent.setup();
        renderLeaveRequestForm();

        await screen.findByText("Ajouter une demande de congé");

        await user.click(await screen.findByRole("combobox", { name: /employé/i }));
        await user.click(await screen.findByRole("option", { name: "Claire Bernard" }));

        await user.click(await screen.findByRole("combobox", { name: /type de congé/i }));
        await user.click(await screen.findByRole("option", { name: "Maladie" }));

        await user.type(await screen.findByLabelText(/date de début/i), "2025-06-01");
        await user.type(await screen.findByLabelText(/date de fin/i), "2025-06-05");
        await user.type(await screen.findByLabelText(/motif/i), "Rendez-vous médical");

        const formElement: HTMLFormElement = document.querySelector("form") as HTMLFormElement;
        fireEvent.submit(formElement);

        const expectedPayload: LeaveRequestFormData = {
            employeeProfileId: 5,
            leaveType: LEAVE_TYPES.Sick,
            startDate: "2025-06-01",
            endDate: "2025-06-05",
            reason: "Rendez-vous médical",
        };

        await waitFor(() => {
            expect(addLeaveRequestMock).toHaveBeenCalledTimes(1);
            expect(addLeaveRequestMock).toHaveBeenCalledWith(expectedPayload);
            expect(setShowLeaveRequestFormMock).toHaveBeenCalledWith(false);
        });
    });
});