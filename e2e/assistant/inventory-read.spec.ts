import { expect, test } from "@playwright/test";

test.describe("Inventaire assistant", () => {
	test("AS3 — consulter le catalogue en lecture", async ({ page }) => {
		await page.goto("/catalogue");
		await expect(page).toHaveURL(/\/catalogue$/);
		await expect(page.getByRole("heading", { name: "Catalogue" })).toBeVisible();
	});

	test("AS4 — pas d'accès à la paie (API refusée)", async ({ page }) => {
		await page.goto("/rh/paie");
		await expect(page.getByText("Impossible de charger les bulletins de paie.")).toBeVisible();
	});

	test("AS4 — pas d'accès à la liste utilisateurs", async ({ page }) => {
		await page.goto("/rh/utilisateurs");
		await expect(page.getByText("Oups!")).toBeVisible();
		await expect(page.getByText("Une erreur est survenue.")).toBeVisible();
	});
});
