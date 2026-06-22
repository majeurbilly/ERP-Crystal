import { expect, test } from "@playwright/test";
import { uniqueFutureDates } from "../fixtures/api";

test.describe("Congés assistant", () => {
	test("AS2 — créer une demande de congé pour soi", async ({ page }) => {
		const { startDate, endDate } = uniqueFutureDates(7000);

		await page.goto("/mon-espace?tab=conges");
		await expect(page.getByRole("heading", { name: "Mon espace" })).toBeVisible();

		await page.getByRole("button", { name: "Nouvelle demande de congé" }).click();
		await page.getByLabel(/type de congé/i).click();
		await page.getByRole("option", { name: "Vacances" }).click();
		await page.getByLabel(/date de début/i).fill(startDate);
		await page.getByLabel(/date de fin/i).fill(endDate);
		await page.getByRole("button", { name: "Ajouter" }).click();

		await expect(page.getByText("La demande de congé a été ajoutée avec succès.")).toBeVisible();
		await expect(page.getByText("En attente").first()).toBeVisible();
	});
});
