import { expect, test } from "@playwright/test";
import { createPendingLeaveRequest, uniqueFutureDates } from "../fixtures/api";

test.describe("Approbation congés gérant", () => {
	test.beforeAll(async ({ request }) => {
		const { startDate, endDate } = uniqueFutureDates(9000);
		await createPendingLeaveRequest(request, "employee", startDate, endDate);
	});

	test("G2 — approuver une demande de congé en attente", async ({ page }) => {
		await page.goto("/rh/absences?status=Pending");
		await expect(page.getByRole("heading", { name: "Congés" })).toBeVisible();

		const approveButton = page.getByRole("button", { name: "Approuver" }).first();
		await expect(approveButton).toBeVisible();
		await approveButton.click();

		await expect(page.getByText("La demande de congé a été approuvée.")).toBeVisible();
	});
});
