/// <reference types="vitest/config" />

import path from "node:path";
import { fileURLToPath } from "node:url";
import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";
import checker from "vite-plugin-checker";

const __dirname = path.dirname(fileURLToPath(import.meta.url));

export default defineConfig({
	resolve: {
		alias: {
			"jwt-decode": path.resolve(__dirname, "src/shims/jwt-decode.ts"),
		},
	},
	plugins: [
		react(),
		checker({
			biome: true,
			typescript: true,
		}),
	],
	test: {
		environment: "jsdom",
		setupFiles: ["./src/setupTests.ts"],
	},
});
