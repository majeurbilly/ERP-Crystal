import { cleanup, fireEvent, screen, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import SchedulesPage from "../../../pages/hr/SchedulesPage";
import { renderWithHrProviders } from "./testUtils";
import type { ScheduledShift } from "../../types/hr/scheduledShift";
import type { EmployeeProfile } from "../../types/hr/employeeProfile";
import type { Location } from "../../types/inventory/location";
import scheduledShiftService from "../../../api/services/hr/scheduledShiftService";
import employeeProfileService from "../../../api/services/hr/employeeProfileService";
import locationService from "../../../api/services/inventory/locationService";

const updateScheduledShiftMock = vi.fn();

vi.mock("../../../api/services/hr/scheduledShiftService", () => ({
    default: {
        getAll: vi.fn(),
        getById: vi.fn(),
        add: vi.fn(),
        update: vi.fn(),
        delete: vi.fn(),
    },
}));

vi.mock("../../../api/services/hr/employeeProfileService", () => ({
    default: {
        getAll: vi.fn(),
    },
}));

vi.mock("../../../api/services/inventory/locationService", () => ({
    default: {
        getAll: vi.fn(),
    },
}));

vi.mock("../../../api/mutations/hr/useScheduledShiftMutations", () => ({
    useScheduledShiftMutations: () => ({
        addScheduledShift: vi.fn(),
        isAddingScheduledShift: false,
        deleteScheduledShift: vi.fn(),
        isDeletingScheduledShift: false,
        updateScheduledShift: updateScheduledShiftMock,
        isUpdatingScheduledShift: false,
    }),
}));

vi.mock("../../../context/AuthContext", () => ({
    useAuth: () => ({
        token: "test-token",
        role: "Admin",
        isAuthenticated: true,
    }),
}));

function formatDateKey(p_date: Date): string {
    return p_date.toISOString().split("T")[0];
}

const today = new Date();
const tomorrow = new Date(today);
tomorrow.setDate(today.getDate() + 1);
const todayDate = formatDateKey(today);
const tomorrowDate = formatDateKey(tomorrow);

const mockScheduledShifts: ScheduledShift[] = [
    {
        id: 1,
        employeeProfileId: 10,
        employeeFirstName: "Alice",
        employeeLastName: "Martin",
        jobPositionId: 2,
        jobPositionName: "Caissier",
        locationId: 1,
        locationTitle: "Succursale Centre",
        date: todayDate,
        startTime: "09:00",
        endTime: "17:00",
        isDeleted: false,
    },
    {
        id: 2,
        employeeProfileId: 11,
        employeeFirstName: "Bob",
        employeeLastName: "Dupont",
        jobPositionId: 3,
        jobPositionName: "Gérant",
        locationId: 2,
        locationTitle: "Succursale Nord",
        date: tomorrowDate,
        startTime: "08:30",
        endTime: "16:30",
        isDeleted: false,
    },
];

const mockEmployees: EmployeeProfile[] = [
    {
        id: 10,
        firstName: "Alice",
        lastName: "Martin",
        email: "alice@test.ca",
        hiringDate: "2025-01-01",
        jobPositionId: 2,
        jobPositionName: "Caissier",
        salary: 42000,
        status: "Active",
        isDeleted: false,
        applicationUserId: null
    },
    {
        id: 11,
        firstName: "Bob",
        lastName: "Dupont",
        email: "bob@test.ca",
        hiringDate: "2025-01-01",
        jobPositionId: 3,
        jobPositionName: "Gérant",
        salary: 52000,
        status: "Active",
        isDeleted: false,
        locationId: 2,
        applicationUserId: null
    },
];

const mockLocations: Location[] = [
    { id: 1, title: "Succursale Centre", address: "1 rue A", description: "Centre" },
    { id: 2, title: "Succursale Nord", address: "2 rue B", description: "Nord" },
];

describe("SchedulesPage", () => {
    beforeEach(() => {
        vi.mocked(scheduledShiftService.getAll).mockResolvedValue(mockScheduledShifts);
        vi.mocked(employeeProfileService.getAll).mockResolvedValue(mockEmployees);
        vi.mocked(locationService.getAll).mockResolvedValue(mockLocations);
        updateScheduledShiftMock.mockResolvedValue(undefined);
    });

    afterEach(() => {
        cleanup();
        vi.clearAllMocks();
    });

    it("should render scheduled shifts in the calendar after loading", async () => {
        renderWithHrProviders(<SchedulesPage />);
        expect(await screen.findByText("Planification")).toBeInTheDocument();
        expect(scheduledShiftService.getAll).toHaveBeenCalledTimes(1);
        expect(employeeProfileService.getAll).toHaveBeenCalledTimes(1);
        expect(locationService.getAll).toHaveBeenCalledTimes(1);

        await waitFor(() => {
            expect(screen.getByText("Alice Martin")).toBeInTheDocument();
            expect(screen.getByText("Bob Dupont")).toBeInTheDocument();
            expect(screen.getByText("09:00-17:00")).toBeInTheDocument();
            expect(screen.getByText("08:30-16:30")).toBeInTheDocument();
        });
    });

    it("should filter shifts by selected location", async () => {
        renderWithHrProviders(<SchedulesPage />);

        expect(await screen.findByText("Alice Martin")).toBeInTheDocument();

        fireEvent.mouseDown(screen.getByRole("combobox", { name: /succursale/i }));
        fireEvent.click(await screen.findByRole("option", { name: "Succursale Centre" }));

        await waitFor(() => {
            expect(screen.getByText("Alice Martin")).toBeInTheDocument();
            expect(screen.queryByText("Bob Dupont")).not.toBeInTheDocument();
        });
    });

    it("should show shift details when clicking a shift", async () => {
        renderWithHrProviders(<SchedulesPage />);

        fireEvent.click(await screen.findByText("Alice Martin"));

        expect(await screen.findByText("Quart de travail")).toBeInTheDocument();
        expect(screen.getByText(/Caissier/)).toBeInTheDocument();
        expect(screen.getByText(/Succursale Centre/)).toBeInTheDocument();
    });

    it("should highlight the current day in the calendar", async () => {
        renderWithHrProviders(<SchedulesPage />);

        expect(await screen.findByTestId("today-calendar-day")).toBeInTheDocument();
        expect(screen.getByText("Aujourd'hui")).toBeInTheDocument();
    });
    it("should change visible shifts when selecting a day or week view", async () => {
        vi.mocked(scheduledShiftService.getAll).mockResolvedValue([
            { ...mockScheduledShifts[0], date: "2026-06-10" },
            { ...mockScheduledShifts[1], date: "2026-06-11" },
        ]);

        renderWithHrProviders(<SchedulesPage />);

        expect(await screen.findByText("Alice Martin")).toBeInTheDocument();
        expect(screen.getByText("Bob Dupont")).toBeInTheDocument();

        fireEvent.click(screen.getByRole("button", { name: "Jour" }));

        const periodInput = screen.getByLabelText(/riode/i);
        await waitFor(() => {
            expect(periodInput).toHaveAttribute("type", "date");
        });
        fireEvent.change(periodInput, { target: { value: "2026-06-10" } });

        await waitFor(() => {
            expect(screen.getByText("Alice Martin")).toBeInTheDocument();
            expect(screen.queryByText("Bob Dupont")).not.toBeInTheDocument();
        });

        fireEvent.click(screen.getByRole("button", { name: "Semaine" }));
        await waitFor(() => {
            expect(screen.getByText("Alice Martin")).toBeInTheDocument();
            expect(screen.getByText("Bob Dupont")).toBeInTheDocument();
        });
    });

    it("should update the shift date when a shift is dragged to another day", async () => {
        renderWithHrProviders(<SchedulesPage />);

        const sourceShift = await screen.findByText("Alice Martin");
        const sourceButton = sourceShift.closest("[role='button']") as HTMLElement;
        const targetDay = await screen.findByTestId(`calendar-day-${tomorrowDate}`);

        const dataTransferMock = {
            data: new Map<string, string>(),
            effectAllowed: 'all',
            dropEffect: 'none',
            setData: vi.fn((key: string, value: string) => {
                dataTransferMock.data.set(key, value);
            }),
            getData: vi.fn((key: string) => dataTransferMock.data.get(key)),
        };

        fireEvent.dragStart(sourceButton, { dataTransfer: dataTransferMock });

        fireEvent.drop(targetDay, { dataTransfer: dataTransferMock });

        await waitFor(() => {
            expect(updateScheduledShiftMock).toHaveBeenCalledWith(
                expect.objectContaining({
                    id: "1",
                    data: expect.objectContaining({
                        date: tomorrowDate,
                        locationId: 1,
                        jobPositionId: 2,
                    }),
                })
            );
        });
    });
});
