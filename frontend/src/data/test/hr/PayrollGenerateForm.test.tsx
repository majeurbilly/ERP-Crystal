import { cleanup, fireEvent, render, screen, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import PayrollGenerateForm from "../../../components/forms/hr/PayrollGenerateForm";
import type { GeneratePayrollForPeriodRequest } from "../../types/hr/payStub";
import type { PayPeriod } from "../../types/hr/payPeriod";
import type { Location } from "../../types/inventory/location";

const mockPayPeriods: PayPeriod[] = [
    {
        id: 3,
        startDate: "2026-06-08",
        endDate: "2026-06-14",
        isProcessed: false,
    },
];

const mockLocations: Location[] = [
    { id: 1, title: "Succursale Québec", address: "123 Rue", description: "" },
    { id: 2, title: "Succursale Sainte-Foy", address: "456 Rue", description: "" },
];

const generatePayrollForPeriodMock = vi.fn().mockResolvedValue({
    payPeriodId: 3,
    periodStartDate: "2026-06-08",
    periodEndDate: "2026-06-14",
    locationId: null,
    createdCount: 2,
    existingCount: 1,
    skippedCount: 0,
    payStubs: [],
});
const setShowPayrollGenerateFormMock = vi.fn();

vi.mock("../../../api/services/hr/payrollService", () => ({
    default: {
        getPeriods: vi.fn(),
        createPeriod: vi.fn(),
    },
}));

vi.mock("../../../api/services/inventory/locationService", () => ({
    default: {
        getAll: vi.fn(),
    },
}));

vi.mock("../../../api/mutations/hr/usePayrollMutations", () => ({
    usePayrollMutations: () => ({
        generatePayStub: vi.fn(),
        isGeneratingPayStub: false,
        generatePayStubError: null,
        generatedPayStub: undefined,
        generatePayrollForPeriod: generatePayrollForPeriodMock,
        isGeneratingPayrollForPeriod: false,
        generatePayrollForPeriodError: null,
        generatedPayrollForPeriod: undefined,
        publishPayStub: vi.fn(),
        isPublishingPayStub: false,
        publishPayStubError: null,
        publishedPayStub: undefined,
    }),
}));

vi.mock("../../../context/AuthContext", () => ({
    useAuth: () => ({
        user: {
            id: "admin-user-id",
            userName: "admin",
            email: "admin@test.local",
            employeeProfile: { locationId: 1 },
            dynamicRole: null,
        },
    }),
}));

vi.mock("../../../permissions/usePermissions", () => ({
    usePermissions: () => ({
        isSuperAdmin: true,
    }),
}));

vi.mock("../../../data/utils/popupMessageManager", () => ({
    notifySuccessMessage: vi.fn(),
    notifyErrorMessage: vi.fn(),
}));

import payrollService from "../../../api/services/hr/payrollService";
import locationService from "../../../api/services/inventory/locationService";

function renderPayrollGenerateForm(): ReturnType<typeof render> {
    const queryClient = new QueryClient({
        defaultOptions: {
            queries: { retry: false },
            mutations: { retry: false },
        },
    });

    return render(
        <QueryClientProvider client={queryClient}>
            <PayrollGenerateForm
                showPayrollGenerateForm={true}
                setShowPayrollGenerateForm={setShowPayrollGenerateFormMock}
            />
        </QueryClientProvider>
    );
}

describe("PayrollGenerateForm", () => {
    beforeEach(() => {
        vi.useFakeTimers({ toFake: ["Date"] });
        vi.setSystemTime(new Date("2026-06-17T12:00:00"));
        vi.mocked(payrollService.getPeriods).mockResolvedValue(mockPayPeriods);
        vi.mocked(payrollService.createPeriod).mockResolvedValue(mockPayPeriods[0]);
        vi.mocked(locationService.getAll).mockResolvedValue(mockLocations);
        generatePayrollForPeriodMock.mockClear();
        setShowPayrollGenerateFormMock.mockClear();
    });

    afterEach(() => {
        cleanup();
        vi.useRealTimers();
        vi.clearAllMocks();
    });

    it("should render branch and completed week controls from mocked queries", async () => {
        renderPayrollGenerateForm();

        expect(document.querySelector("h6")).toHaveTextContent("Générer les fiches de paie");
        expect(payrollService.getPeriods).toHaveBeenCalled();
        expect(locationService.getAll).toHaveBeenCalled();

        const dateInput = document.querySelector('input[type="date"]') as HTMLInputElement;
        expect(dateInput).toHaveValue("2026-06-08");
        expect(dateInput).toHaveAttribute("max", "2026-06-08");
        expect(dateInput).toHaveAttribute("step", "7");

        fireEvent.mouseDown(screen.getAllByRole("combobox")[0]);
        expect(await screen.findByText("Toutes les succursales")).toBeInTheDocument();
    });

    it("should generate payroll for the selected completed week and branch scope on submit", async () => {
        renderPayrollGenerateForm();

        const submitButton = await screen.findByRole("button", { name: "Générer" });

        await waitFor(() => {
            expect(submitButton).not.toBeDisabled();
        });

        fireEvent.click(submitButton);

        const expectedPayload: GeneratePayrollForPeriodRequest = {
            payPeriodId: 3,
            locationId: null,
        };

        await waitFor(() => {
            expect(payrollService.createPeriod).not.toHaveBeenCalled();
            expect(generatePayrollForPeriodMock).toHaveBeenCalledTimes(1);
            expect(generatePayrollForPeriodMock).toHaveBeenCalledWith(expectedPayload);
            expect(setShowPayrollGenerateFormMock).toHaveBeenCalledWith(false);
        });
    });

    it("should reject current or future weeks before submitting", async () => {
        renderPayrollGenerateForm();

        const periodStartInput = document.querySelector('input[type="date"]') as HTMLInputElement;
        fireEvent.change(periodStartInput, { target: { value: "2026-06-15" } });

        expect(periodStartInput.checkValidity()).toBe(false);

        const submitButton = await screen.findByRole("button", { name: "Générer" });
        fireEvent.click(submitButton);

        expect(generatePayrollForPeriodMock).not.toHaveBeenCalled();
    });
});