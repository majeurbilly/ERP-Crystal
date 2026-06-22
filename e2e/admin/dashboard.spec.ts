import { expect, test } from "@playwright/test";

test.describe("Dashboard admin", () => {
	test("A1 — widgets admin visibles sur le dashboard", async ({ page }) => {
		await page.goto("/dashboard");
		await expect(page.getByRole("heading", { name: "Tableau de bord" })).toBeVisible();
		await expect(page.getByText("Rôles et permissions")).toBeVisible();
		await expect(page.getByText("Employés actifs")).toBeVisible();
	});
});
