import { expect, test } from "@playwright/test";

test.describe("Dashboard assistant", () => {
	test("AS1 — dashboard assistant avec widgets", async ({ page }) => {
		await page.goto("/dashboard");
		await expect(page.getByRole("heading", { name: "Tableau de bord" })).toBeVisible();
		await expect(page.getByText("Prochain quart")).toBeVisible();
	});
});
