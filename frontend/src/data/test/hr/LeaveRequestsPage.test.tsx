import { cleanup, screen, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import LeaveRequestsPage from "../../../pages/hr/leave-requests/LeaveRequestsPage";
import type { LeaveRequest } from "../../types/hr/leaveRequest";
import { LEAVE_REQUEST_STATUSES, LEAVE_TYPES } from "../../types/hr/leaveRequest";
import { renderWithHrProviders } from "./testUtils";

const mockLeaveRequests: LeaveRequest[] = [
    {
        id: 1,
        employeeProfileId: 10,
        employeeFirstName: "Alice",
        employeeLastName: "Martin",
        leaveType: LEAVE_TYPES.Vacation,
        status: LEAVE_REQUEST_STATUSES.Pending,
        startDate: "2025-07-01",
        endDate: "2025-07-10",
        reason: "Vacances d'été",
        isDeleted: false,
    },
    {
        id: 2,
        employeeProfileId: 11,
        employeeFirstName: "Bob",
        employeeLastName: "Dupont",
        leaveType: LEAVE_TYPES.Sick,
        status: LEAVE_REQUEST_STATUSES.Approved,
        startDate: "2025-03-01",
        endDate: "2025-03-03",
        reason: null,
        isDeleted: false,
    },
];

vi.mock("../../../api/services/hr/leaveRequestService", () => ({
    default: {
        getAll: vi.fn(),
        getById: vi.fn(),
        add: vi.fn(),
        updateStatus: vi.fn(),
        delete: vi.fn(),
    },
}));

vi.mock("../../../api/mutations/useLeaveRequestMutations", () => ({
    useLeaveRequestMutations: () => ({
        addLeaveRequest: vi.fn(),
        isAddingLeaveRequest: false,
        deleteLeaveRequest: vi.fn(),
        isDeletingLeaveRequest: false,
        updateLeaveRequestStatus: vi.fn(),
        isUpdatingLeaveRequestStatus: false,
    }),
}));

vi.mock("../../../context/AuthContext", () => ({
    useAuth: () => ({
        token: "test-token",
        role: "Admin",
        id: "admin-id",
        login: vi.fn(),
        logout: vi.fn(),
        isAuthenticated: true,
    }),
}));

import leaveRequestService from "../../../api/services/hr/leaveRequestService";

describe("LeaveRequestsPage", () => {
    beforeEach(() => {
        vi.mocked(leaveRequestService.getAll).mockResolvedValue(mockLeaveRequests);
    });

    afterEach(() => {
        cleanup();
        vi.clearAllMocks();
    });

    it("should render leave requests in the data grid after loading", async () => {
        renderWithHrProviders(<LeaveRequestsPage />);

        expect(await screen.findByText("Congés")).toBeInTheDocument();
        expect(leaveRequestService.getAll).toHaveBeenCalledTimes(1);

        await waitFor(() => {
            expect(screen.getByText("Alice Martin")).toBeInTheDocument();
            expect(screen.getByText("Bob Dupont")).toBeInTheDocument();
            expect(screen.getByText("Vacances d'été")).toBeInTheDocument();
            expect(screen.getByText("En attente")).toBeInTheDocument();
            expect(screen.getByText("Approuvée")).toBeInTheDocument();
        });
    });

    it("should show approve and reject buttons for pending requests when user can manage", async () => {
        renderWithHrProviders(<LeaveRequestsPage />);

        await screen.findByText("Alice Martin");

        expect(screen.getByRole("button", { name: "Approuver" })).toBeInTheDocument();
        expect(screen.getByRole("button", { name: "Refuser" })).toBeInTheDocument();
        expect(screen.getAllByRole("button", { name: "Approuver" })).toHaveLength(1);
        expect(screen.getAllByRole("button", { name: "Refuser" })).toHaveLength(1);
    });
});
