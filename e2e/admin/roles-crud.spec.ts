import { expect, test } from "@playwright/test";

test.describe("Rôles dynamiques admin", () => {
	test("A2 — créer un rôle depuis le preset Employé", async ({ page }) => {
		const roleName = `E2E Employé ${Date.now()}`;

		await page.goto("/roles");
		await expect(page.getByRole("heading", { name: "Liste des rôles" })).toBeVisible();

		await page.getByRole("button", { name: "Ajouter un rôle" }).click();
		await expect(page.getByRole("heading", { name: "Ajouter un rôle" })).toBeVisible();

		await page.getByRole("button", { name: "Employé" }).click();
		await page.getByLabel("Nom du rôle").fill(roleName);
		await page.locator('button[type="submit"]').click();

		await expect(page.getByText(/a été ajouté avec succès/i)).toBeVisible();
		await expect(page.getByRole("gridcell", { name: roleName })).toBeVisible();
	});
});
