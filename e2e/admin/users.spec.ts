import { expect, test } from "@playwright/test";

test.describe("Gestion utilisateurs admin", () => {
	test("A3 — créer un utilisateur avec rôle dynamique", async ({ page }) => {
		const uniqueSuffix = Date.now();
		const email = `e2e-user-${uniqueSuffix}@test.local`;
		const userName = `e2euser${uniqueSuffix}`;

		await page.goto("/rh/utilisateurs");
		await expect(page.getByRole("heading", { name: "Liste des utilisateurs" })).toBeVisible();

		await page.getByRole("button", { name: "Ajouter un utilisateur" }).click();
		await expect(page.getByRole("heading", { name: "Ajouter un utilisateur" })).toBeVisible();

		await page.getByLabel(/^email$/i).fill(email);
		await page.getByLabel(/^username$/i).fill(userName);
		await page.getByLabel(/^mot de passe$/i).fill("ValidPass1!a");

		const roleSelect = page.getByLabel(/^rôle$/i);
		await expect(roleSelect).toBeVisible();
		await roleSelect.click();
		await page.getByRole("option", { name: "Employé" }).click();

		await page.getByRole("button", { name: "Ajouter" }).click();

		await expect(page.getByText(`Utilisateur ${userName} ajouté`)).toBeVisible();
		await expect(page.getByRole("gridcell", { name: userName })).toBeVisible();
		await expect(page.getByRole("gridcell", { name: "Employé" })).toBeVisible();
	});
});
