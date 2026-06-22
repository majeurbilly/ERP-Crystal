import { cleanup, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Route, Routes } from "react-router-dom";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import TimesheetDetailsPage from "../../../pages/hr/timesheets/TimesheetDetailsPage";
import { ROUTE_TIMESHEET_DETAILS } from "../../routeNames";
import type { Timesheet } from "../../types/hr/timesheet";
import { TIMESHEET_STATUSES } from "../../types/hr/timesheet";
import { renderWithHrProviders } from "./testUtils";

const updateTimesheetStatusMock = vi.fn().mockResolvedValue(undefined);
const deleteTimesheetMock = vi.fn().mockResolvedValue(undefined);
const reloadTimesheetTimeEntriesMock = vi.fn().mockResolvedValue(undefined);
const updateTimesheetPaidMock = vi.fn().mockResolvedValue(undefined);

const mockTimesheet: Timesheet = {
    id: 42,
    employeeProfileId: 7,
    employeeFirstName: "Sophie",
    employeeLastName: "Lavoie",
    periodStart: "2026-05-01",
    periodEnd: "2026-05-31",
    status: TIMESHEET_STATUSES.Submitted,
    isPaid: false,
    timeEntries: [
        {
            id: 101,
            employeeProfileId: 7,
            employeeFirstName: "Sophie",
            employeeLastName: "Lavoie",
            scheduledShiftId: 12,
            date: "2026-05-10",
            startTime: "09:00",
            endTime: "17:00",
            isDeleted: false,
        },
        {
            id: 102,
            employeeProfileId: 7,
            employeeFirstName: "Sophie",
            employeeLastName: "Lavoie",
            scheduledShiftId: null,
            date: "2026-05-11",
            startTime: "08:30",
            endTime: null,
            isDeleted: false,
        },
    ],
    isDeleted: false,
};

vi.mock("../../../api/services/hr/timesheetService", () => ({
    default: {
        getAll: vi.fn(),
        getById: vi.fn(),
        add: vi.fn(),
        delete: vi.fn(),
        update: vi.fn(),
        updateStatus: vi.fn(),
    },
}));

vi.mock("../../../api/mutations/hr/useTimesheetMutations", () => ({
    useTimesheetMutations: () => ({
        addTimesheet: vi.fn(),
        isAddingTimesheet: false,
        deleteTimesheet: deleteTimesheetMock,
        isDeletingTimesheet: false,
        reloadTimesheetTimeEntries: reloadTimesheetTimeEntriesMock,
        isReloadingTimesheetTimeEntries: false,
        updateTimesheet: vi.fn(),
        isUpdatingTimesheet: false,
        updateTimesheetStatus: updateTimesheetStatusMock,
        isUpdatingTimesheetStatus: false,
        updateTimesheetPaid: updateTimesheetPaidMock,
        isUpdatingTimesheetPaid: false,
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

vi.mock("../../../data/popupMessageManager", () => ({
    notifySuccessMessage: vi.fn(),
    notifyErrorMessage: vi.fn(),
}));

import timesheetService from "../../../api/services/hr/timesheetService";

describe("TimesheetDetailsPage", () => {
    beforeEach(() => {
        vi.mocked(timesheetService.getById).mockResolvedValue(mockTimesheet);
        updateTimesheetStatusMock.mockClear();
        deleteTimesheetMock.mockClear();
        reloadTimesheetTimeEntriesMock.mockClear();
        updateTimesheetPaidMock.mockClear();
    });

    afterEach(() => {
        cleanup();
        vi.clearAllMocks();
    });

    it("should render timesheet header, linked time entries sub-grid, and approval buttons", async () => {
        renderWithHrProviders(
            <Routes>
                <Route path={ROUTE_TIMESHEET_DETAILS} element={<TimesheetDetailsPage />} />
            </Routes>,
            { initialRoute: "/rh/feuilles-de-temps/42" }
        );

        expect(await screen.findByText((content, element) => {
            return element?.tagName.toLowerCase() === 'h1' && content.includes('D') && content.includes('tail');
        })).toBeInTheDocument();
        expect(timesheetService.getById).toHaveBeenCalledWith("42");

        await waitFor(() => {
            expect(screen.getByRole("heading", { name: "Sophie Lavoie" })).toBeInTheDocument();
            expect(screen.getByText("Soumise")).toBeInTheDocument();
            expect(screen.getByText("Paiement : Non payée")).toBeInTheDocument();
            expect(screen.getByText((content, _element) => {
                return content.toLowerCase().includes('pointages li');
            })).toBeInTheDocument();
            expect(screen.getByText("09:00")).toBeInTheDocument();
            expect(screen.getByText("17:00")).toBeInTheDocument();
            expect(screen.getByText("08:30")).toBeInTheDocument();
            expect(screen.getAllByText("—").length).toBeGreaterThanOrEqual(1);
            expect(screen.getByRole("button", { name: "Recharger" })).toBeInTheDocument();
            expect(screen.getByRole("button", { name: "Supprimer" })).toBeInTheDocument();
            expect(screen.getByRole("button", { name: "Approuver" })).toBeInTheDocument();
            expect(screen.getByRole("button", { name: "Rejeter" })).toBeInTheDocument();
        });
    });

    it("should mark the timesheet as paid", async () => {
        const user = userEvent.setup();
        renderWithHrProviders(
            <Routes>
                <Route path={ROUTE_TIMESHEET_DETAILS} element={<TimesheetDetailsPage />} />
            </Routes>,
            { initialRoute: "/rh/feuilles-de-temps/42" }
        );

        await user.click(await screen.findByRole("button", { name: "Marquer payée" }));

        expect(updateTimesheetPaidMock).toHaveBeenCalledWith({ id: 42, isPaid: true });
    });

    it("should call updateTimesheetStatus with Approved when approve is clicked", async () => {
        const user = userEvent.setup();
        renderWithHrProviders(
            <Routes>
                <Route path={ROUTE_TIMESHEET_DETAILS} element={<TimesheetDetailsPage />} />
            </Routes>,
            { initialRoute: "/rh/feuilles-de-temps/42" }
        );

        await screen.findByRole("button", { name: "Approuver" });
        await user.click(screen.getByRole("button", { name: "Approuver" }));

        await waitFor(() => {
            expect(updateTimesheetStatusMock).toHaveBeenCalledTimes(1);
            expect(updateTimesheetStatusMock).toHaveBeenCalledWith({
                id: 42,
                status: TIMESHEET_STATUSES.Approved,
            });
        });
    });

    it("should reload linked time entries for a draft timesheet", async () => {
        const user = userEvent.setup();
        vi.mocked(timesheetService.getById).mockResolvedValue({
            ...mockTimesheet,
            status: TIMESHEET_STATUSES.Draft,
        });

        renderWithHrProviders(
            <Routes>
                <Route path={ROUTE_TIMESHEET_DETAILS} element={<TimesheetDetailsPage />} />
            </Routes>,
            { initialRoute: "/rh/feuilles-de-temps/42" }
        );

        const reloadButton = await screen.findByRole("button", { name: "Recharger" });
        await user.click(reloadButton);

        await waitFor(() => {
            expect(reloadTimesheetTimeEntriesMock).toHaveBeenCalledWith(42);
        });
    });
});
