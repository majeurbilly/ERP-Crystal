import { cleanup, screen, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import JobPositionsPage from "../../../pages/hr/JobPositionsPage";
import type { JobPosition } from "../../types/hr/jobPosition";
import { renderWithHrProviders } from "./testUtils";

const mockJobPositions: JobPosition[] = [
    {
        id: 1,
        name: "Développeur backend",
        description: "Conception d'APIs",
        isDeleted: false,
    },
    {
        id: 2,
        name: "Analyste fonctionnel",
        description: "Spécifications métier",
        isDeleted: false,
    },
];

vi.mock("../../../api/services/hr/jobPositionService", () => ({
    default: {
        getAll: vi.fn(),
        getById: vi.fn(),
        add: vi.fn(),
        update: vi.fn(),
        delete: vi.fn(),
    },
}));

vi.mock("../../../api/mutations/useJobPositionMutations", () => ({
    useJobPositionMutations: () => ({
        addJobPosition: vi.fn(),
        isAddingJobPosition: false,
        deleteJobPosition: vi.fn(),
        isDeletingJobPosition: false,
        updateJobPosition: vi.fn(),
        isUpdatingJobPosition: false,
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

import jobPositionService from "../../../api/services/hr/jobPositionService";

describe("JobPositionsPage", () => {
    beforeEach(() => {
        vi.mocked(jobPositionService.getAll).mockResolvedValue(mockJobPositions);
    });

    afterEach(() => {
        cleanup();
        vi.clearAllMocks();
    });

    it("should render job positions in the data grid after loading", async () => {
        renderWithHrProviders(<JobPositionsPage />);

        expect(await screen.findByText("Postes")).toBeInTheDocument();
        expect(jobPositionService.getAll).toHaveBeenCalledTimes(1);

        await waitFor(() => {
            expect(screen.getByText("Développeur backend")).toBeInTheDocument();
            expect(screen.getByText("Analyste fonctionnel")).toBeInTheDocument();
        });
    });
});