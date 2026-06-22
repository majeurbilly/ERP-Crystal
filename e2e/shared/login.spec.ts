import { expect, test } from "@playwright/test";
import { loginAs, TEST_USERS } from "../fixtures/auth";

test.describe("Login", () => {
	test("A1 — connexion employé redirige vers le dashboard", async ({ page }) => {
		await loginAs(page, "employee");
		await expect(page.getByRole("heading", { name: "Tableau de bord" })).toBeVisible();
	});

	test("affiche une erreur avec des identifiants invalides", async ({ page }) => {
		await page.goto("/");
		await page.getByLabel("Email", { exact: true }).fill("invalid@test.local");
		await page.getByLabel("Password", { exact: true }).fill("wrongpassword");
		await page.getByRole("button", { name: /log in/i }).click();
		await expect(page.getByRole("alert")).toBeVisible();
		await expect(page).toHaveURL("/");
	});

	test("chaque compte seed peut se connecter", async ({ page }) => {
		for (const role of Object.keys(TEST_USERS) as Array<keyof typeof TEST_USERS>) {
			await page.goto("/");
			await page.evaluate(() => {
				localStorage.clear();
			});
			await loginAs(page, role);
			await expect(page.getByRole("heading", { name: "Tableau de bord" })).toBeVisible();
		}
	});
});
