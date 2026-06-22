import { cleanup, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Route, Routes } from "react-router-dom";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import LeaveRequestDetailsPage from "../../../pages/hr/leave-requests/LeaveRequestDetailsPage";
import { ROUTE_LEAVE_REQUEST_DETAILS } from "../../routeNames";
import { type LeaveRequest } from "../../types/hr/leaveRequest";
import { renderWithHrProviders } from "./testUtils";
import leaveRequestService from "../../../api/services/hr/leaveRequestService";

const updateLeaveRequestStatusMock = vi.fn().mockResolvedValue(undefined);

vi.mock("../../../api/services/hr/leaveRequestService", () => ({
    default: {
        getById: vi.fn(),
    },
}));

vi.mock("../../../api/mutations/hr/useLeaveRequestMutations", () => ({
    useLeaveRequestMutations: () => ({
        updateLeaveRequestStatus: updateLeaveRequestStatusMock,
        isUpdatingLeaveRequestStatus: false,
    }),
}));

vi.mock("../../../permissions/usePermissions", () => ({
    usePermissions: () => ({
        canUpdate: true,
        canRead: true,
    }),
}));

describe("LeaveRequestDetailsPage", () => {
    beforeEach(() => {
        vi.mocked(leaveRequestService.getById).mockResolvedValue({
            id: 7,
            employeeFirstName: "Alice",
            employeeLastName: "Martin",
            status: "Pending",
            startDate: "2026-07-01",
            endDate: "2026-07-10",
        } as LeaveRequest);
    });

    afterEach(() => {
        cleanup();
        vi.clearAllMocks();
    });

    it("should approve a pending leave request from the detail page", async () => {
        const user = userEvent.setup();

        renderWithHrProviders(
            <Routes>
                <Route path={ROUTE_LEAVE_REQUEST_DETAILS} element={<LeaveRequestDetailsPage />} />
            </Routes>,
            { initialRoute: "/rh/absences/7" }
        );

        const approveButton = await screen.findByRole("button", {
            name: /approuver/i
        });

        await user.click(approveButton);

        await waitFor(() => {
            expect(updateLeaveRequestStatusMock).toHaveBeenCalledWith(
                expect.objectContaining({
                    id: 7,
                    status: "Approved",
                })
            );
        });
    });
});