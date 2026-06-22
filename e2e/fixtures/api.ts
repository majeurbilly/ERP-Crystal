import type { APIRequestContext } from "@playwright/test";
import { expect } from "@playwright/test";
import { TEST_USERS, type TestRole } from "./auth";

const API_BASE = process.env.E2E_API_URL ?? "http://localhost:8080";

export async function apiLogin(
	p_request: APIRequestContext,
	p_role: TestRole,
): Promise<string> {
	const credentials = TEST_USERS[p_role];
	const response = await p_request.post(`${API_BASE}/api/auth/login`, {
		data: {
			email: credentials.email,
			password: credentials.password,
		},
	});
	expect(response.ok()).toBeTruthy();
	const body = (await response.json()) as { token: string };
	return body.token;
}

export async function getEmployeeProfileId(
	p_request: APIRequestContext,
	p_role: TestRole,
): Promise<number> {
	const token = await apiLogin(p_request, p_role);
	const response = await p_request.get(`${API_BASE}/api/employee-profiles/me`, {
		headers: { Authorization: `Bearer ${token}` },
	});
	expect(response.ok()).toBeTruthy();
	const body = (await response.json()) as { id: number };
	return body.id;
}

export async function createPendingLeaveRequest(
	p_request: APIRequestContext,
	p_role: TestRole,
	p_startDate: string,
	p_endDate: string,
): Promise<number> {
	const token = await apiLogin(p_request, p_role);
	const profileId = await getEmployeeProfileId(p_request, p_role);

	const response = await p_request.post(`${API_BASE}/api/leave-requests`, {
		headers: { Authorization: `Bearer ${token}` },
		data: {
			employeeProfileId: profileId,
			leaveType: 0,
			startDate: p_startDate,
			endDate: p_endDate,
			reason: "E2E Playwright",
		},
	});
	if (!response.ok()) {
		const errorBody = await response.text();
		throw new Error(`Create leave failed (${response.status()}): ${errorBody}`);
	}
	const body = (await response.json()) as { id: number };
	return body.id;
}

export function uniqueFutureDates(p_salt = 0): { startDate: string; endDate: string } {
	const offset = 500 + p_salt + (Date.now() % 5000);
	const start = new Date();
	start.setDate(start.getDate() + offset);
	const end = new Date(start);
	end.setDate(end.getDate() + 3);
	const format = (p_date: Date): string => p_date.toISOString().substring(0, 10);
	return { startDate: format(start), endDate: format(end) };
}
