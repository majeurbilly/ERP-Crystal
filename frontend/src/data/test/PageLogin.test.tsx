import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { AuthProvider } from "../../context/AuthContext";
import { LanguageProvider } from "../../context/TranslationContext";
import { ROUTE_DASHBOARD, ROUTE_ROOT } from "../routeNames";
import LoginPage from "../../pages/LoginPage";

vi.mock("../../api/apiClient", () => ({
	default: {
		post: vi.fn(),
	},
}));

import apiClient from "../../api/apiClient";

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
		const token = makeFakeJwtPayload({
			"http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "Assistant",
		});

		vi.mocked(apiClient.post).mockResolvedValue({
			status: 200,
			data: { token },
		});

		render(
			<LanguageProvider>
				<AuthProvider>
					<MemoryRouter initialEntries={[ROUTE_ROOT]}>
						<Routes>
							<Route path={ROUTE_ROOT} element={<LoginPage />} />
							<Route path={ROUTE_DASHBOARD} element={<div>Tableau de bord</div>} />
						</Routes>
					</MemoryRouter>
				</AuthProvider>
			</LanguageProvider>
		);

		const emailInput = screen.getByLabelText(/email|courriel/i, { selector: "input" });
		const passwordInput = screen.getByLabelText(/password|mot de passe/i, { selector: "input" });

		fireEvent.change(emailInput, { target: { value: "a@b.ca" } });
		fireEvent.change(passwordInput, { target: { value: "secret123" } });

		const loginButton = screen.getAllByRole("button", { name: /log in|connexion/i })[0];
		fireEvent.click(loginButton);

		await waitFor(() => {
			expect(screen.getByText("Tableau de bord")).toBeInTheDocument();
			expect(localStorage.getItem("token")).toBe(token);
		});
	});

	it("affiche un message d'erreur lorsque l'API retourne une erreur", async () => {
		vi.mocked(apiClient.post).mockRejectedValue({
			isAxiosError: true,
			response: {
				status: 401,
				data: { message: "Identifiants invalides." },
			},
		});

		render(
			<LanguageProvider>
				<AuthProvider>
					<MemoryRouter initialEntries={[ROUTE_ROOT]}>
						<Routes>
							<Route path={ROUTE_ROOT} element={<LoginPage />} />
							<Route path={ROUTE_DASHBOARD} element={<div>Tableau de bord</div>} />
						</Routes>
					</MemoryRouter>
				</AuthProvider>
			</LanguageProvider>
		);

		const emailInput = screen.getByLabelText(/email|courriel/i, { selector: "input" });
		const passwordInput = screen.getByLabelText(/password|mot de passe/i, { selector: "input" });

		fireEvent.change(emailInput, { target: { value: "a@b.ca" } });
		fireEvent.change(passwordInput, { target: { value: "wrong" } });

		const loginButton = screen.getAllByRole("button", { name: /log in|connexion/i })[0];
		fireEvent.click(loginButton);

		const alerte = await screen.findByRole("alert");
		expect(alerte).toHaveTextContent("Identifiants invalides.");
		expect(localStorage.getItem("token")).toBeNull();
	});
});