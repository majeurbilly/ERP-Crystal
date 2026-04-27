import { describe, expect, it } from "vitest";
import { isUserRole, mapServerRoleToFrontend } from "../devAuth";

describe("mapServerRoleToFrontend", () => {
	it.each([
		["Admin", "admin"],
		["admin", "admin"],
		["Manager", "gerant"],
		["manager", "gerant"],
		["Assistant", "assistant"],
		["Employee", "employee"],
		["employee", "employee"],
	])("traduit %s vers le segment frontend %s", (server, expected) => {
		expect(mapServerRoleToFrontend(server)).toBe(expected);
	});

	it("retourne null pour un rôle inconnu", () => {
		expect(mapServerRoleToFrontend("Pirate")).toBeNull();
		expect(mapServerRoleToFrontend("")).toBeNull();
	});
});

describe("isUserRole", () => {
	it.each([
		["gerant"],
		["assistant"],
		["employee"],
		["admin"],
	])("retourne true pour le rôle valide %s", (role) => {
		expect(isUserRole(role)).toBe(true);
	});

	it.each([
		["pirate"],
		[""],
	])("retourne false pour une chaîne invalide (%s)", (value) => {
		expect(isUserRole(value)).toBe(false);
	});
});
