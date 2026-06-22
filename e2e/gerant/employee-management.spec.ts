import { expect, test } from "@playwright/test";

test.describe("Gestion employés gérant", () => {
	test("G3 — consulter et ouvrir une fiche employé", async ({ page }) => {
		await page.goto("/rh/employes");
		await expect(page.getByRole("heading", { name: "Employés" })).toBeVisible();

		const firstDataRow = page.getByRole("row").nth(1);
		await expect(firstDataRow).toBeVisible();
		await firstDataRow.click();

		await expect(page).toHaveURL(/\/rh\/employes\/\d+/);
	});

	test("G4 — consulter le planning", async ({ page }) => {
		await page.goto("/rh/planning");
		await expect(page).toHaveURL(/\/rh\/planning$/);
		await expect(page.getByRole("heading", { name: "Planification" })).toBeVisible();
	});
});
