import { expect, test } from "@playwright/test";

test.describe("Horaire employé", () => {
	test("E2 — consulter l'onglet horaire dans Mon espace", async ({ page }) => {
		await page.goto("/mon-espace?tab=horaire");
		await expect(page.getByRole("heading", { name: "Mon espace" })).toBeVisible();
		await expect(page.getByRole("tab", { name: "Horaire" })).toHaveAttribute("aria-selected", "true");
	});

	test("widget Prochain quart sur le dashboard", async ({ page }) => {
		await page.goto("/dashboard");
		await expect(page.getByText("Prochain quart")).toBeVisible();
	});
});
