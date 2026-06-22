import { cleanup, screen, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import TimeEntriesPage from "../../../pages/hr/TimeEntriesPage";
import type { TimeEntry } from "../../types/hr/timeEntry";
import { renderWithHrProviders } from "./testUtils";

const mockTimeEntries: TimeEntry[] = [
    {
        id: 1,
        employeeProfileId: 10,
        employeeFirstName: "Alice",
        employeeLastName: "Martin",
        scheduledShiftId: 5,
        date: "2026-06-10",
        startTime: "09:00",
        endTime: "17:00",
        isDeleted: false,
    },
    {
        id: 2,
        employeeProfileId: 11,
        employeeFirstName: "Bob",
        employeeLastName: "Dupont",
        scheduledShiftId: null,
        date: "2026-06-11",
        startTime: "08:30",
        endTime: null,
        isDeleted: false,
    },
];

vi.mock("../../../api/services/hr/timeEntryService", () => ({
    default: {
        getAll: vi.fn(),
        getById: vi.fn(),
        add: vi.fn(),
        update: vi.fn(),
        delete: vi.fn(),
    },
}));

vi.mock("../../../api/mutations/useTimeEntryMutations", () => ({
    useTimeEntryMutations: () => ({
        addTimeEntry: vi.fn(),
        isAddingTimeEntry: false,
        deleteTimeEntry: vi.fn(),
        isDeletingTimeEntry: false,
        updateTimeEntry: vi.fn(),
        isUpdatingTimeEntry: false,
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

import timeEntryService from "../../../api/services/hr/timeEntryService";

describe("TimeEntriesPage", () => {
    beforeEach(() => {
        vi.mocked(timeEntryService.getAll).mockResolvedValue(mockTimeEntries);
    });

    afterEach(() => {
        cleanup();
        vi.clearAllMocks();
    });

    it("should render time entries in the data grid after loading", async () => {
        renderWithHrProviders(<TimeEntriesPage />);

        expect(await screen.findByText("Pointages")).toBeInTheDocument();
        expect(timeEntryService.getAll).toHaveBeenCalledTimes(1);

        await waitFor(() => {
            expect(screen.getByText("Alice Martin")).toBeInTheDocument();
            expect(screen.getByText("Bob Dupont")).toBeInTheDocument();
            expect(screen.getByText("09:00")).toBeInTheDocument();
            expect(screen.getByText("17:00")).toBeInTheDocument();
            expect(screen.getByText("08:30")).toBeInTheDocument();
            expect(screen.getAllByText("—").length).toBeGreaterThanOrEqual(1);
            expect(screen.getByText("8,00 h")).toBeInTheDocument();
        });
    });
});
