import { test as setup } from "@playwright/test";
import { loginAs, storagePath, TEST_USERS, type TestRole } from "./auth";

const roles = Object.keys(TEST_USERS) as TestRole[];

for (const role of roles) {
	setup(`authenticate as ${role}`, async ({ page }) => {
		await loginAs(page, role);
		await page.context().storageState({ path: storagePath(role) });
	});
}
