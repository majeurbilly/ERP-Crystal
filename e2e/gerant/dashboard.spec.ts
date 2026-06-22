import { expect, test } from "@playwright/test";

test.describe("Dashboard gérant", () => {
	test("G1 — métriques RH visibles sur le dashboard", async ({ page }) => {
		await page.goto("/dashboard");
		await expect(page.getByRole("heading", { name: "Tableau de bord" })).toBeVisible();
		await expect(page.getByText("Employés actifs")).toBeVisible();
		await expect(page.getByText("Demandes de congé en attente")).toBeVisible();
		await expect(page.getByText("Feuilles de temps en attente").first()).toBeVisible();
	});
});
