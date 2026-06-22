import { cleanup, fireEvent, screen, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import PayrollPage from "../../../pages/hr/PayrollPage";
import type { PayStub } from "../../types/hr/payStub";
import { renderWithHrProviders } from "./testUtils";

const grossPayFormatter = new Intl.NumberFormat("fr-CA", {
    style: "currency",
    currency: "CAD",
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
});

function normalizeCurrencyText(p_text: string): string {
    return p_text.replace(/\u00a0/g, " ").trim();
}

const mockPayStubs: PayStub[] = [
    {
        id: 1,
        payPeriodId: 3,
        employeeProfileId: 10,
        employeeFirstName: "Alice",
        employeeLastName: "Martin",
        periodStartDate: "2026-06-01",
        periodEndDate: "2026-06-30",
        totalHours: 16,
        grossPay: 400,
        isPublished: false,
        isDeleted: false,
    },
    {
        id: 2,
        payPeriodId: 3,
        employeeProfileId: 11,
        employeeFirstName: "Bob",
        employeeLastName: "Dupont",
        periodStartDate: "2026-06-01",
        periodEndDate: "2026-06-30",
        totalHours: 37.5,
        grossPay: 937.5,
        isPublished: true,
        isDeleted: false,
    },
];

const payrollMutationMocks = vi.hoisted(() => ({
    publishPayStub: vi.fn().mockResolvedValue(undefined),
}));

vi.mock("../../../api/services/hr/payrollService", () => ({
    default: {
        getStubs: vi.fn(),
        generatePayStub: vi.fn(),
        publishPayStub: vi.fn(),
    },
}));

vi.mock("../../../api/mutations/hr/usePayrollMutations", () => ({
    usePayrollMutations: () => ({
        generatePayStub: vi.fn(),
        isGeneratingPayStub: false,
        generatePayStubError: null,
        generatedPayStub: undefined,
        publishPayStub: payrollMutationMocks.publishPayStub,
        isPublishingPayStub: false,
        publishPayStubError: null,
        publishedPayStub: undefined,
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

vi.mock("../../../permissions/usePermissions", () => ({
    usePermissions: () => ({
        canCreate: true,
        canRead: true,
        canUpdate: true,
        canDelete: true,
    }),
}));

import payrollService from "../../../api/services/hr/payrollService";

describe("PayrollPage", () => {
    beforeEach(() => {
        vi.mocked(payrollService.getStubs).mockResolvedValue(mockPayStubs);
        payrollMutationMocks.publishPayStub.mockClear();
    });

    afterEach(() => {
        cleanup();
        vi.clearAllMocks();
    });

    it("should render pay stubs in the data grid with CAD currency formatting", async () => {
        renderWithHrProviders(<PayrollPage />);

        expect(await screen.findByText("Paie")).toBeInTheDocument();
        expect(payrollService.getStubs).toHaveBeenCalledTimes(1);

        await waitFor(() => {
            expect(screen.getByText("Alice Martin")).toBeInTheDocument();
            expect(screen.getByText("Bob Dupont")).toBeInTheDocument();
            expect(screen.getByText("16")).toBeInTheDocument();
            expect(screen.getByText("37.5")).toBeInTheDocument();

            expect(screen.getAllByText("Publiée").length).toBeGreaterThanOrEqual(1);

            const bodyText = normalizeCurrencyText(document.body.textContent ?? "");
            expect(bodyText).toContain(normalizeCurrencyText(grossPayFormatter.format(400)));
            expect(bodyText).toContain(normalizeCurrencyText(grossPayFormatter.format(937.5)));
        });
    });

    it("should publish a draft pay stub from the data grid", async () => {
        renderWithHrProviders(<PayrollPage />);
        expect(await screen.findByText("Alice Martin")).toBeInTheDocument();

        const publishButton = await screen.findByRole("button", { name: "Publier" });
        fireEvent.click(publishButton);

        await waitFor(() => {
            expect(payrollMutationMocks.publishPayStub).toHaveBeenCalledTimes(1);
            expect(payrollMutationMocks.publishPayStub).toHaveBeenCalledWith(1);
        });
    });
});