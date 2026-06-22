import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { createMongoAbility } from "@casl/ability";
import { describe, expect, it, vi, afterEach } from "vitest";
import ScheduleCalendarPanel from "../../../components/hr-components/ScheduleCalendarPanel";
import { PermissionContext } from "../../../permissions/AppPermissionContext";
import { CRUD_OPERATIONS, ENTITY_TYPES, type AppAbility } from "../../../permissions/permissions";
import type { ScheduledShift } from "../../types/hr/scheduledShift";

function formatDateKey(p_date: Date): string {
    const year = p_date.getFullYear();
    const month = String(p_date.getMonth() + 1).padStart(2, "0");
    const day = String(p_date.getDate()).padStart(2, "0");
    return `${year}-${month}-${day}`;
}

const todayDate = formatDateKey(new Date());

const ownShift: ScheduledShift = {
    id: 1,
    employeeProfileId: 10,
    employeeFirstName: "Alice",
    employeeLastName: "Employe",
    jobPositionId: 2,
    jobPositionName: "Caissier",
    locationId: 1,
    locationTitle: "Succursale Centre",
    date: todayDate,
    startTime: "09:00",
    endTime: "17:00",
    isDeleted: false,
};

const teamShift: ScheduledShift = {
    id: 2,
    employeeProfileId: 11,
    employeeFirstName: "Bob",
    employeeLastName: "Equipe",
    jobPositionId: 3,
    jobPositionName: "Vendeur",
    locationId: 1,
    locationTitle: "Succursale Centre",
    date: todayDate,
    startTime: "12:00",
    endTime: "20:00",
    isDeleted: false,
};

vi.mock("../../../api/services/hr/scheduledShiftService", () => ({
    default: {
        getAll: vi.fn(),
        getTeamSchedule: vi.fn(),
    },
}));

import scheduledShiftService from "../../../api/services/hr/scheduledShiftService";

function renderPanel(): void {
    const queryClient = new QueryClient({
        defaultOptions: {
            queries: { retry: false },
        },
    });
    const ability = createMongoAbility<AppAbility>();
    ability.update([{ action: CRUD_OPERATIONS.READ, subject: ENTITY_TYPES.SCHEDULED_SHIFT }]);

    render(
        <QueryClientProvider client={queryClient}>
            <PermissionContext.Provider value={ability}>
                <ScheduleCalendarPanel ownEmployeeProfileId={10} />
            </PermissionContext.Provider>
        </QueryClientProvider>
    );
}

describe("ScheduleCalendarPanel", () => {
    afterEach(() => {
        vi.clearAllMocks();
    });

    it("charge l'horaire d'equipe et affiche les details d'un quart", async () => {
        vi.mocked(scheduledShiftService.getAll).mockResolvedValue([ownShift]);
        vi.mocked(scheduledShiftService.getTeamSchedule).mockResolvedValue([ownShift, teamShift]);

        renderPanel();

        expect(await screen.findByText("Alice Employe")).toBeInTheDocument();

        fireEvent.click(screen.getByRole("button", { name: /quipe/i }));

        await waitFor(() => {
            expect(scheduledShiftService.getTeamSchedule).toHaveBeenCalledTimes(1);
        });

        fireEvent.click(await screen.findByText("Bob Equipe"));

        expect(await screen.findByText("Details du quart")).toBeInTheDocument();
        expect(screen.getByText("Vendeur")).toBeInTheDocument();
        expect(screen.getByText("Succursale Centre")).toBeInTheDocument();
        expect(screen.getByText("12:00 - 20:00")).toBeInTheDocument();
    });
});
