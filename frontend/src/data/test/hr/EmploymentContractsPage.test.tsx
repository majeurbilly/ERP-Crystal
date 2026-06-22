import { cleanup, screen, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import EmploymentContractsPage from "../../../pages/hr/EmploymentContractsPage";
import type { EmploymentContract } from "../../types/hr/employmentContract";
import { CONTRACT_TYPES, WAGE_TYPES } from "../../types/hr/employmentContract";
import { renderWithHrProviders } from "./testUtils";

const mockContracts: EmploymentContract[] = [
    {
        id: 1,
        employeeProfileId: 10,
        employeeFirstName: "Alice",
        employeeLastName: "Martin",
        contractType: CONTRACT_TYPES.FullTime,
        wageType: WAGE_TYPES.Fixed,
        baseRate: 52000,
        startDate: "2026-01-01",
        endDate: null,
        isDeleted: false,
    },
    {
        id: 2,
        employeeProfileId: 11,
        employeeFirstName: "Bob",
        employeeLastName: "Dupont",
        contractType: CONTRACT_TYPES.PartTime,
        wageType: WAGE_TYPES.Monthly,
        baseRate: 25,
        startDate: "2030-01-01",
        endDate: null,
        isDeleted: false,
    },
];

vi.mock("../../../api/services/hr/employmentContractService", () => ({
    default: {
        getAll: vi.fn(),
        getById: vi.fn(),
        getByEmployeeId: vi.fn(),
        add: vi.fn(),
        update: vi.fn(),
        delete: vi.fn(),
    },
}));

import employmentContractService from "../../../api/services/hr/employmentContractService";

describe("EmploymentContractsPage", () => {
    beforeEach(() => {
        vi.mocked(employmentContractService.getAll).mockResolvedValue(mockContracts);
    });

    afterEach(() => {
        cleanup();
        vi.clearAllMocks();
    });

    it("should render employment contracts from the shared page", async () => {
        renderWithHrProviders(<EmploymentContractsPage />);

        expect(await screen.findByText("Contrats de travail")).toBeInTheDocument();
        expect(employmentContractService.getAll).toHaveBeenCalledTimes(1);

        await waitFor(() => {
            expect(screen.getByText("Alice Martin")).toBeInTheDocument();
            expect(screen.getByText("Temps plein")).toBeInTheDocument();
            expect(screen.getByText("Montant fixe")).toBeInTheDocument();
        });
    });
});
