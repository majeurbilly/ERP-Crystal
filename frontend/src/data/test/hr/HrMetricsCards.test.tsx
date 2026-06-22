import { cleanup, render, screen } from "@testing-library/react";
import { afterEach, describe, expect, it } from "vitest";
import { MemoryRouter } from "react-router-dom";
import HrMetricsCards from "../../../components/hr-components/HrMetricsCards";
import type { HrDashboardMetrics } from "../../types/hr/hrDashboardMetrics";

const fakeMetrics: HrDashboardMetrics = {
    totalActiveEmployees: 42,
    pendingTimesheetsCount: 7,
    pendingLeaveRequestsCount: 3,
    totalGrossPayroll: 125000.5,
};

describe("HrMetricsCards", () => {
    afterEach(() => {
        cleanup();
    });

    it("should render the four KPI cards with correct metric values", () => {
        render(
            <MemoryRouter>
                <HrMetricsCards metrics={fakeMetrics} />
            </MemoryRouter>
        );

        expect(screen.getByText("Employés actifs")).toBeInTheDocument();
        expect(screen.getByText("Feuilles de temps en attente")).toBeInTheDocument();
        expect(screen.getByText("Demandes de congé en attente")).toBeInTheDocument();
        expect(screen.getByText("Masse salariale brute")).toBeInTheDocument();

        expect(screen.getByText("42")).toBeInTheDocument();
        expect(screen.getByText("7")).toBeInTheDocument();
        expect(screen.getByText("3")).toBeInTheDocument();
        expect(screen.getByText(/125[\s\u00a0]*000,50\s*\$/)).toBeInTheDocument();
    });

    it("should format payroll currency using fr-CA CAD locale", () => {
        render(
            <MemoryRouter>
                <HrMetricsCards metrics={fakeMetrics} />
            </MemoryRouter>
        );

        const formattedPayroll: HTMLElement = screen.getByText(/125[\s\u00a0]*000,50\s*\$/);
        expect(formattedPayroll.textContent).toMatch(/\$/);
        expect(formattedPayroll.textContent).toContain("125");
        expect(formattedPayroll.textContent).toContain("000,50");
    });
});
