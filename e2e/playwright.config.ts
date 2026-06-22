import { defineConfig, devices } from "@playwright/test";
import path from "node:path";
import { fileURLToPath } from "node:url";

const rootDir = path.dirname(fileURLToPath(import.meta.url));
const storageDir = path.join(rootDir, "fixtures", "storage");

export default defineConfig({
	testDir: rootDir,
	fullyParallel: true,
	forbidOnly: Boolean(process.env.CI),
	retries: process.env.CI ? 2 : 0,
	workers: process.env.CI ? 1 : undefined,
	reporter: process.env.CI ? [["list"], ["html", { open: "never" }]] : "html",
	timeout: 60_000,
	expect: { timeout: 15_000 },
	use: {
		baseURL: process.env.E2E_BASE_URL ?? "http://localhost:3000",
		trace: "on-first-retry",
		screenshot: "only-on-failure",
		video: "retain-on-failure",
	},
	projects: [
		{ name: "setup", testMatch: /auth\.setup\.ts/ },
		{
			name: "shared",
			testDir: path.join(rootDir, "shared"),
			use: { ...devices["Desktop Chrome"] },
		},
		{
			name: "admin",
			testDir: path.join(rootDir, "admin"),
			dependencies: ["setup"],
			use: {
				...devices["Desktop Chrome"],
				storageState: path.join(storageDir, "admin.json"),
			},
		},
		{
			name: "gerant",
			testDir: path.join(rootDir, "gerant"),
			dependencies: ["setup"],
			use: {
				...devices["Desktop Chrome"],
				storageState: path.join(storageDir, "gerant.json"),
			},
		},
		{
			name: "assistant",
			testDir: path.join(rootDir, "assistant"),
			dependencies: ["setup"],
			use: {
				...devices["Desktop Chrome"],
				storageState: path.join(storageDir, "assistant.json"),
			},
		},
		{
			name: "employee",
			testDir: path.join(rootDir, "employee"),
			dependencies: ["setup"],
			use: {
				...devices["Desktop Chrome"],
				storageState: path.join(storageDir, "employee.json"),
			},
		},
	],
});
