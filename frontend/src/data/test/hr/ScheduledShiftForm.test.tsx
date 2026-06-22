import { cleanup, fireEvent, render, screen, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import ScheduledShiftForm from "../../../components/forms/hr/ScheduledShiftForm";
import type { EmployeeProfile } from "../../types/hr/employeeProfile";
import type { JobPosition } from "../../types/hr/jobPosition";
import type { Location } from "../../types/inventory/location";
import type { ScheduledShift, ScheduledShiftFormData } from "../../types/hr/scheduledShift";

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
        locationId: 2,
    },
    {
        id: 8,
        firstName: "Marc",
        lastName: "Gagnon",
        email: "marc@test.ca",
        hiringDate: "2024-03-01",
        jobPositionId: 1,
        jobPositionName: "Vendeur",
        applicationUserId: null,
        salary: 44000,
        status: "Active",
        isDeleted: false,
        locationId: 2,
    },
    {
        id: 9,
        firstName: "Alice",
        lastName: "Martin",
        email: "alice@test.ca",
        hiringDate: "2024-04-01",
        jobPositionId: 1,
        jobPositionName: "Vendeur",
        applicationUserId: null,
        salary: 44000,
        status: "Active",
        isDeleted: false,
        locationId: 3,
    },
];

const mockScheduledShifts: ScheduledShift[] = [
    {
        id: 20,
        employeeProfileId: 8,
        employeeFirstName: "Marc",
        employeeLastName: "Gagnon",
        jobPositionId: 1,
        jobPositionName: "Vendeur",
        locationId: 2,
        locationTitle: "Succursale Sainte-Foy",
        date: "2026-07-15",
        startTime: "10:00",
        endTime: "14:00",
        isDeleted: false,
    },
];

const mockJobPositions: JobPosition[] = [
    {
        id: 1,
        name: "Gérant de succursale",
        description: "Supervision",
        isDeleted: false,
    },
];

const mockLocations: Location[] = [
    {
        id: 2,
        title: "Succursale Sainte-Foy",
        address: "2450 chemin Sainte-Foy",
        description: "Sainte-Foy",
    },
];

const addScheduledShiftMock = vi.fn().mockResolvedValue(undefined);
const setShowScheduledShiftFormMock = vi.fn();

vi.mock("../../../api/services/hr/employeeProfileService", () => ({
    default: { getAll: vi.fn() },
}));

vi.mock("../../../api/services/hr/jobPositionService", () => ({
    default: { getAll: vi.fn() },
}));

vi.mock("../../../api/services/inventory/locationService", () => ({
    default: {
        getAll: vi.fn(),
    },
}));

vi.mock("../../../api/services/hr/scheduledShiftService", () => ({
    default: { getAll: vi.fn() },
}));

vi.mock("../../../api/mutations/hr/useScheduledShiftMutations", () => ({
    useScheduledShiftMutations: () => ({
        addScheduledShift: addScheduledShiftMock,
        isAddingScheduledShift: false,
        updateScheduledShift: vi.fn(),
        isUpdatingScheduledShift: false,
    }),
}));

vi.mock("../../../data/popupMessageManager", () => ({
    notifySuccessMessage: vi.fn(),
    notifyErrorMessage: vi.fn(),
}));

import employeeProfileService from "../../../api/services/hr/employeeProfileService";
import jobPositionService from "../../../api/services/hr/jobPositionService";
import locationService from "../../../api/services/inventory/locationService";
import scheduledShiftService from "../../../api/services/hr/scheduledShiftService";

async function openSelectAndChoose(p_name: RegExp, p_option: string): Promise<void> {
    fireEvent.mouseDown(await screen.findByRole("combobox", { name: p_name }));
    const option = await screen.findByRole("option", { name: p_option });
    fireEvent.click(option);
}

function selectTimeByLabel(
    p_label: RegExp,
    p_time: string,
): void {
    const [hour, minute] = p_time.split(":").map(Number);
    const clockHour = hour % 12 || 12;

    fireEvent.click(screen.getAllByLabelText(p_label)[0]);
    fireEvent.click(screen.getByRole("button", { name: hour >= 12 ? "PM" : "AM" }));
    fireEvent.click(screen.getByRole("button", { name: `${clockHour} heure` }));
    fireEvent.click(screen.getByRole("button", {
        name: `:${minute.toString().padStart(2, "0")}`,
    }));
    fireEvent.click(screen.getByRole("button", { name: "OK" }));
}

function renderScheduledShiftForm(p_defaultLocationId: number | null = null): ReturnType<typeof render> {
    const queryClient: QueryClient = new QueryClient({
        defaultOptions: {
            queries: { retry: false },
            mutations: { retry: false },
        },
    });

    return render(
        <QueryClientProvider client={queryClient}>
            <ScheduledShiftForm
                showScheduledShiftForm={true}
                setShowScheduledShiftForm={setShowScheduledShiftFormMock}
                editScheduledShift={null}
                defaultLocationId={p_defaultLocationId}
            />
        </QueryClientProvider>
    );
}

describe("ScheduledShiftForm", () => {
    beforeEach(() => {
        vi.mocked(employeeProfileService.getAll).mockResolvedValue(mockEmployees);
        vi.mocked(jobPositionService.getAll).mockResolvedValue(mockJobPositions);
        vi.mocked(locationService.getAll).mockResolvedValue(mockLocations);
        vi.mocked(scheduledShiftService.getAll).mockResolvedValue(mockScheduledShifts);
        addScheduledShiftMock.mockClear();
        setShowScheduledShiftFormMock.mockClear();
    });

    afterEach(() => {
        cleanup();
        vi.clearAllMocks();
    });

    it("should render location, employee and job position options from mocked queries", async () => {
        renderScheduledShiftForm();

        expect(await screen.findByText("Ajouter un quart planifié")).toBeInTheDocument();
        expect(locationService.getAll).toHaveBeenCalled();
        expect(employeeProfileService.getAll).toHaveBeenCalled();
        expect(jobPositionService.getAll).toHaveBeenCalled();

        await openSelectAndChoose(/succursale/i, "Succursale Sainte-Foy");
        await openSelectAndChoose(/employé/i, "Sophie Lavoie");
        await openSelectAndChoose(/^poste$/i, "Gérant de succursale");
    });

    it("should preselect the default location when creating from a filtered schedule", async () => {
        renderScheduledShiftForm(2);

        expect(await screen.findByText(/Ajouter un quart/)).toBeInTheDocument();
        await waitFor(() => {
            expect(screen.getByRole("combobox", { name: /succursale/i })).toHaveTextContent(
                "Succursale Sainte-Foy"
            );
        });
    });

    it("should show only employees from the selected location who are available for the shift", async () => {
        renderScheduledShiftForm();

        await screen.findByText(/Ajouter un quart/);
        await openSelectAndChoose(/succursale/i, "Succursale Sainte-Foy");

        const dateInput: HTMLInputElement = document.querySelector(
            'input[type="date"]'
        ) as HTMLInputElement;
        fireEvent.change(dateInput, { target: { value: "2026-07-15" } });

        selectTimeByLabel(/heure de d.but/i, "09:00");
        selectTimeByLabel(/heure de fin/i, "12:00");

        await waitFor(async () => {
            fireEvent.mouseDown(await screen.findByRole("combobox", { name: /employ./i }));
        });

        expect(await screen.findByRole("option", { name: "Sophie Lavoie" })).toBeInTheDocument();
        expect(screen.queryByRole("option", { name: "Marc Gagnon" })).not.toBeInTheDocument();
        expect(screen.queryByRole("option", { name: "Alice Martin" })).not.toBeInTheDocument();
    }, 10000);

    it("should call addScheduledShift with correct payload including HH:mm times on submit", async () => {
        renderScheduledShiftForm();

        await screen.findByText("Ajouter un quart planifié");

        await openSelectAndChoose(/succursale/i, "Succursale Sainte-Foy");

        const dateInput: HTMLInputElement = document.querySelector(
            'input[type="date"]'
        ) as HTMLInputElement;
        fireEvent.change(dateInput, { target: { value: "2026-07-15" } });

        selectTimeByLabel(/heure de début/i, "09:00");
        selectTimeByLabel(/heure de fin/i, "17:30");

        await waitFor(async () => {
            await openSelectAndChoose(/employé/i, "Sophie Lavoie");
        });

        await openSelectAndChoose(/^poste$/i, "Gérant de succursale");

        const formElement: HTMLFormElement = document.querySelector("form") as HTMLFormElement;
        fireEvent.submit(formElement);

        const expectedPayload: ScheduledShiftFormData = {
            locationId: 2,
            employeeProfileId: 7,
            jobPositionId: 1,
            date: "2026-07-15",
            startTime: "09:00",
            endTime: "17:30",
        };

        await waitFor(() => {
            expect(addScheduledShiftMock).toHaveBeenCalledTimes(1);
            expect(addScheduledShiftMock).toHaveBeenCalledWith(expectedPayload);
            expect(setShowScheduledShiftFormMock).toHaveBeenCalledWith(false);
        });
    }, 10000);

    it("should not submit when end time is before start time", async () => {
        renderScheduledShiftForm();

        await screen.findByText("Ajouter un quart planifié");

        await openSelectAndChoose(/succursale/i, "Succursale Sainte-Foy");

        selectTimeByLabel(/heure de début/i, "17:00");
        selectTimeByLabel(/heure de fin/i, "09:00");

        const formElement: HTMLFormElement = document.querySelector("form") as HTMLFormElement;
        fireEvent.submit(formElement);

        expect(addScheduledShiftMock).not.toHaveBeenCalled();
        expect(
            await screen.findByText("L'heure de fin doit être postérieure à l'heure de début.")
        ).toBeInTheDocument();
    }, 10000);
});