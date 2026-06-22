import { expect, test } from "@playwright/test";
import { expandSidebarSection, loginAs } from "../fixtures/auth";

test.describe("Navigation admin", () => {
	test.beforeEach(async ({ page }) => {
		await loginAs(page, "admin");
	});

	test("A4 — accès au menu RH complet", async ({ page }) => {
		await expandSidebarSection(page, "Ressources humaines");
		await expect(page.getByRole("link", { name: "Accueil RH" })).toBeVisible();
		await expect(page.getByRole("link", { name: "Employés" })).toBeVisible();
		await expect(page.getByRole("link", { name: "Congés" })).toBeVisible();
		await expect(page.getByRole("link", { name: "Planification" })).toBeVisible();
	});

	test("navigation vers les rôles dynamiques", async ({ page }) => {
		await page.getByRole("link", { name: "Roles" }).click();
		await expect(page).toHaveURL(/\/roles$/);
		await expect(page.getByRole("heading", { name: "Liste des rôles" })).toBeVisible();
	});
});
