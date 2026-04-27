import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import apiClient from "../../api/apiClient";
import PageLogin from "../../pages/PageLogin";
import { AuthProvider } from "../../context/AuthContext";
import { ROUTE_DASHBOARD, ROUTE_ROOT } from "../routeNames";

vi.mock("../../api/apiClient", () => ({
	default: {
		post: vi.fn(),
	},
}));

function makeFakeJwtPayload(payload: Record<string, unknown>): string {
	const header = btoa(JSON.stringify({ alg: "none", typ: "JWT" }))
		.replace(/\+/g, "-")
		.replace(/\//g, "_")
		.replace(/=+$/, "");
	const body = btoa(JSON.stringify(payload))
		.replace(/\+/g, "-")
		.replace(/\//g, "_")
		.replace(/=+$/, "");
	return `${header}.${body}.x`;
}

describe("PageLogin (intégration UI)", () => {
	beforeEach(() => {
		localStorage.clear();
		vi.mocked(apiClient.post).mockReset();
	});

	afterEach(() => {
		localStorage.clear();
	});

	it("Happy path", async () => {
		const user = userEvent.setup();

		const token = makeFakeJwtPayload({
			"http://schemas.microsoft.com/ws/2008/06/identity/claims/role":
				"Assistant",
		});

		vi.mocked(apiClient.post).mockResolvedValue({
			status: 200,
			data: { token },
		});

		render(
			<AuthProvider>
				<MemoryRouter initialEntries={["/"]}>
					<Routes>
						<Route path={ROUTE_ROOT} element={<PageLogin />} />
						<Route path={ROUTE_DASHBOARD} element={<div>Tableau de bord</div>} />
					</Routes>
				</MemoryRouter>
			</AuthProvider>,
		);

		await user.type(screen.getByLabelText(/^email$/i), "a@b.ca");
		await user.type(screen.getByLabelText(/^password$/i), "secret123");

		await user.click(screen.getAllByRole("button", { name: /log in/i })[0]);

		expect(await screen.findByText("Tableau de bord")).toBeInTheDocument();
		expect(localStorage.getItem("token")).toBe(token);
	});

	it("affiche un message d'erreur lorsque l'API retourne une erreur", async () => {
		const user = userEvent.setup();

		vi.mocked(apiClient.post).mockRejectedValue({
			isAxiosError: true,
			response: {
				status: 401,
				data: { message: "Identifiants invalides." },
			},
		});

		render(
			<AuthProvider>
				<MemoryRouter initialEntries={["/"]}>
					<Routes>
						<Route path={ROUTE_ROOT} element={<PageLogin />} />
						<Route path={ROUTE_DASHBOARD} element={<div>Tableau de bord</div>} />
					</Routes>
				</MemoryRouter>
			</AuthProvider>,
		);

		await user.type(screen.getByLabelText(/^email$/i), "a@b.ca");
		await user.type(screen.getByLabelText(/^password$/i), "wrong");

		await user.click(screen.getAllByRole("button", { name: /log in/i })[0]);

		const alerte = await screen.findByRole("alert");
		expect(alerte).toHaveTextContent("Identifiants invalides.");
		expect(localStorage.getItem("token")).toBeNull();
	});
});
