import { expect, test } from "@playwright/test";

test.describe("Mon espace employé", () => {
	test("E1 — dashboard employé avec lien Mon espace", async ({ page }) => {
		await page.goto("/dashboard");
		await expect(page.getByRole("heading", { name: "Tableau de bord" })).toBeVisible();
		await expect(page.getByRole("link", { name: "Mon espace" })).toBeVisible();
	});

	test("E4 — pas de menu RH gestionnaire", async ({ page }) => {
		await page.goto("/dashboard");
		await expect(page.getByRole("button", { name: "Ressources humaines" })).toHaveCount(0);
	});

	test("E5 — redirection depuis une route RH protégée", async ({ page }) => {
		await page.goto("/rh/employes");
		await expect(page).toHaveURL(/\/dashboard$/);
	});
});
