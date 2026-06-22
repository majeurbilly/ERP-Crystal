import { cleanup, screen } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import type { UseQueryResult } from "@tanstack/react-query";
import HRPage from "../../../pages/hr/HRPage";
import type { HrDashboardMetrics } from "../../types/hr/hrDashboardMetrics";
import { renderWithHrProviders } from "./testUtils";

const fakeMetrics: HrDashboardMetrics = {
    totalActiveEmployees: 10,
    pendingTimesheetsCount: 2,
    pendingLeaveRequestsCount: 1,
    totalGrossPayroll: 85000,
};

const mockRefetch = vi.fn();

vi.mock("../../../api/queries/useHrDashboardMetrics", () => ({
    useHrDashboardMetrics: vi.fn(),
}));

vi.mock("../../../api/services/hr/timesheetService", () => ({
    default: { getAll: vi.fn().mockResolvedValue([]) },
}));

vi.mock("../../../api/services/hr/leaveRequestService", () => ({
    default: { getAll: vi.fn().mockResolvedValue([]) },
}));

vi.mock("../../../permissions/usePermissions", () => ({
    usePermissions: () => ({
        ability: {
            can: vi.fn().mockReturnValue(true),
        },
        canCreate: true,
        canRead: true,
        canUpdate: true,
        canDelete: true,
        isSuperAdmin: true,
    }),
}));

import { useHrDashboardMetrics } from "../../../api/queries/useHrDashboardMetrics";

describe("HRPage (dashboard RH)", () => {
    beforeEach(() => {
        vi.mocked(useHrDashboardMetrics).mockReturnValue({
            data: fakeMetrics,
            isLoading: false,
            error: null,
            refetch: mockRefetch,
        } as unknown as UseQueryResult<HrDashboardMetrics, Error>);
    });

    afterEach(() => {
        cleanup();
        vi.clearAllMocks();
    });

    it("should render the dashboard with correct metrics when data is loaded", () => {
        renderWithHrProviders(<HRPage />);

        expect(screen.getByText("Ressources humaines")).toBeInTheDocument();
        expect(screen.getByText("Employés actifs")).toBeInTheDocument();
        expect(screen.getByText("10")).toBeInTheDocument();
        expect(screen.getByText("2")).toBeInTheDocument();
        expect(screen.getByText("1")).toBeInTheDocument();
        expect(screen.getByText(/85[\s\u00a0]*000,00\s*\$/)).toBeInTheDocument();
        expect(screen.getByText("Actions rapides")).toBeInTheDocument();
        expect(screen.getByText("Nouvel employé")).toBeInTheDocument();
        expect(screen.getByText("Liste des utilisateurs")).toBeInTheDocument();
        expect(screen.getByRole("link", { name: "Consulter" })).toBeInTheDocument();
    });

    it("should show a loading state while metrics are fetching", () => {
        vi.mocked(useHrDashboardMetrics).mockReturnValue({
            data: undefined,
            isLoading: true,
            error: null,
            refetch: mockRefetch,
        } as unknown as UseQueryResult<HrDashboardMetrics, Error>);

        renderWithHrProviders(<HRPage />);

        expect(screen.queryByText("Employés actifs")).not.toBeInTheDocument();
    });
});
