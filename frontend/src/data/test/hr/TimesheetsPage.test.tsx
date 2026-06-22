import { cleanup, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import TimesheetsPage from "../../../pages/hr/timesheets/TimesheetsPage";
import type { GenerateWeeklyTimesheetsResult, Timesheet } from "../../types/hr/timesheet";
import { TIMESHEET_STATUSES } from "../../types/hr/timesheet";
import { renderWithHrProviders } from "./testUtils";

const generateWeeklyTimesheetsMock = vi.fn();
const deleteTimesheetMock = vi.fn();

const mockTimesheets: Timesheet[] = [
    {
        id: 1,
        employeeProfileId: 10,
        employeeFirstName: "Alice",
        employeeLastName: "Martin",
        periodStart: "2026-05-01",
        periodEnd: "2026-05-31",
        status: TIMESHEET_STATUSES.Draft,
        isPaid: false,
        timeEntries: [],
        isDeleted: false,
    },
    {
        id: 2,
        employeeProfileId: 11,
        employeeFirstName: "Bob",
        employeeLastName: "Dupont",
        periodStart: "2026-06-01",
        periodEnd: "2026-06-15",
        status: TIMESHEET_STATUSES.Submitted,
        isPaid: true,
        timeEntries: [],
        isDeleted: false,
    },
];

vi.mock("../../../api/services/hr/timesheetService", () => ({
    default: {
        getAll: vi.fn(),
        getById: vi.fn(),
        add: vi.fn(),
        update: vi.fn(),
        updateStatus: vi.fn(),
        generateWeekly: vi.fn(),
    },
}));

vi.mock("../../../api/mutations/hr/useTimesheetMutations", () => ({
    useTimesheetMutations: () => ({
        addTimesheet: vi.fn(),
        isAddingTimesheet: false,
        deleteTimesheet: deleteTimesheetMock,
        isDeletingTimesheet: false,
        updateTimesheet: vi.fn(),
        isUpdatingTimesheet: false,
        updateTimesheetStatus: vi.fn(),
        isUpdatingTimesheetStatus: false,
        generateWeeklyTimesheets: generateWeeklyTimesheetsMock,
        isGeneratingWeeklyTimesheets: false,
    }),
}));

vi.mock("../../../api/services/locationService", () => ({
    default: {
        getAll: vi.fn().mockResolvedValue([]),
    },
}));

vi.mock("../../../data/popupMessageManager", () => ({
    notifySuccessMessage: vi.fn(),
    notifyErrorMessage: vi.fn(),
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

import timesheetService from "../../../api/services/hr/timesheetService";

describe("TimesheetsPage", () => {
    beforeEach(() => {
        vi.mocked(timesheetService.getAll).mockResolvedValue(mockTimesheets);
        const generationResult: GenerateWeeklyTimesheetsResult = {
            periodStart: "2026-06-15",
            periodEnd: "2026-06-21",
            locationId: null,
            createdCount: 2,
            existingCount: 1,
            linkedTimeEntryCount: 5,
            timesheets: [],
        };
        generateWeeklyTimesheetsMock.mockResolvedValue(generationResult);
        deleteTimesheetMock.mockResolvedValue(undefined);
    });

    afterEach(() => {
        cleanup();
        vi.clearAllMocks();
    });

    it("should render timesheets in the data grid after loading", async () => {
        renderWithHrProviders(<TimesheetsPage />);

        expect(await screen.findByText("Feuilles de temps")).toBeInTheDocument();
        expect(timesheetService.getAll).toHaveBeenCalledTimes(1);
        expect(
            screen.getByRole("button", { name: "Ajouter une feuille de temps" })
        ).toBeInTheDocument();

        await waitFor(() => {
            expect(screen.getByText("Alice Martin")).toBeInTheDocument();
            expect(screen.getByText("Bob Dupont")).toBeInTheDocument();
            expect(screen.getByText("Brouillon")).toBeInTheDocument();
            expect(screen.getByText("Soumise")).toBeInTheDocument();
            expect(screen.getByText("Non payée")).toBeInTheDocument();
            expect(screen.getByText("Payée")).toBeInTheDocument();
        });
    });

    it("should generate weekly timesheets from the admin tool", async () => {
        const user = userEvent.setup();
        renderWithHrProviders(<TimesheetsPage />);

        await user.click(await screen.findByRole("button", { name: "Générer une semaine" }));
        await user.click(screen.getByRole("button", { name: "Générer" }));

        await waitFor(() => {
            expect(generateWeeklyTimesheetsMock).toHaveBeenCalledWith({
                periodStart: expect.stringMatching(/^\d{4}-\d{2}-\d{2}$/),
                locationId: null,
            });
        });

        expect(await screen.findByText(/2 créée\(s\)/)).toBeInTheDocument();
    });

    it("should restrict weekly generation date selection to mondays", async () => {
        const user = userEvent.setup();
        renderWithHrProviders(<TimesheetsPage />);

        await user.click(await screen.findByRole("button", { name: "Générer une semaine" }));
        const periodStartInput = screen.getByLabelText(/Début de semaine/);

        expect(periodStartInput).toHaveAttribute("step", "7");
        expect(periodStartInput).toHaveAttribute("min", "1970-01-05");
        expect(generateWeeklyTimesheetsMock).not.toHaveBeenCalled();
    });
});
