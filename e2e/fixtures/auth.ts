import type { Page } from "@playwright/test";
import path from "node:path";
import { fileURLToPath } from "node:url";

const fixturesDir = path.dirname(fileURLToPath(import.meta.url));

export const TEST_USERS = {
	admin: { email: "admin@crystal.local", password: "ValidPass1!a" },
	gerant: { email: "gerant@crystal.local", password: "ValidPass1!a" },
	assistant: { email: "assistant@crystal.local", password: "ValidPass1!a" },
	employee: { email: "employee@crystal.local", password: "ValidPass1!a" },
} as const;

export type TestRole = keyof typeof TEST_USERS;

export function storagePath(p_role: TestRole): string {
	return path.join(fixturesDir, "storage", `${p_role}.json`);
}

export async function loginAs(p_page: Page, p_role: TestRole): Promise<void> {
	const credentials = TEST_USERS[p_role];
	await p_page.goto("/");
	await p_page.getByLabel("Email", { exact: true }).fill(credentials.email);
	await p_page.getByLabel("Password", { exact: true }).fill(credentials.password);
	await p_page.getByRole("button", { name: /log in/i }).click();
	await p_page.waitForURL("**/dashboard");
}

export async function expandSidebarSection(
	p_page: Page,
	p_sectionName: string,
): Promise<void> {
	await p_page.getByRole("button", { name: p_sectionName }).click();
}
