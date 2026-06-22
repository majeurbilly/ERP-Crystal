import { Route, Routes } from "react-router-dom";
import {
	ROUTE_DASHBOARD,
	ROUTE_MY_PROFILE,
	ROUTE_ROOT,
	ROUTE_HR,
	ROUTE_LIST_USERS,
	ROUTE_USER_PROFILE,
	ROUTE_JOB_POSITIONS,
	ROUTE_EMPLOYEE_PROFILES,
	ROUTE_EMPLOYEE_PROFILE_DETAILS,
	ROUTE_EMPLOYMENT_CONTRACTS,
	ROUTE_LEAVE_REQUESTS,
	ROUTE_LEAVE_REQUEST_DETAILS,
	ROUTE_SCHEDULES,
	ROUTE_TIME_ENTRIES,
	ROUTE_TIMESHEETS,
	ROUTE_TIMESHEET_DETAILS,
	ROUTE_PAYROLL, ROUTE_CATALOGUE,
	ROUTE_ITEM_DETAILS,
	ROUTE_CATEGORY,
	ROUTE_IR,
	ROUTE_CATEGORY_DETAILS,
	ROUTE_LIST_USER_ROLES,
	ROUTE_USER_ROLE_DETAILS,
	ROUTE_LIST_AUTHORS,
	ROUTE_AUTHOR_DETAILS,
	ROUTE_MON_ESPACE,
	ROUTE_LOCATIONS,
	ROUTE_LOCATION_DETAILS,
	ROUTE_LOCATION_INVENTORY
} from "./data/routeNames"
import LoginPage from "./pages/LoginPage";
import "./App.css";
import DashboardPage from "./pages/DashboardPage";
import AppLayout from "./components/layouts/AppLayout";
import HRPage from "./pages/hr/HRPage";
import MyProfilePage from "./pages/MyProfilePage";
import { AuthenticatedExclusiveRoute, EmploymentContractExclusiveRoute, HrDashboardExclusiveRoute, PayrollExclusiveRoute, TimesheetExclusiveRoute } from "./components/routes/RestrictedRoute";
import UsersListPage from "./pages/hr/users/UsersListPage";
import UserProfilePage from "./pages/hr/users/UserProfilePage";
import CatalogPage from "./pages/inventory/catalog/CatalogPage";
import ItemDetailsPage from "./pages/inventory/catalog/ItemDetailsPage";
import LocationsPage from "./pages/inventory/locations/LocationsPage";
import LocationDetailsPage from "./pages/inventory/locations/LocationDetailsPage";
import { ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import NotFoundPage from "./pages/NotFoundPage";
import { FormProvider } from "./context/FormContext";
import { DeleteDialogProvider } from "./context/DeleteDialogContext";
import CategoriesListPage from "./pages/inventory/categories/CategoriesListPage";
import IRPage from "./pages/inventory/IRPage";
import LocationInventoryQuantityPage from "./pages/inventory/LocationInventoryQuantityPage";
import CategoryDetailPage from "./pages/inventory/categories/CategoryDetailsPage";
import JobPositionsPage from "./pages/hr/JobPositionsPage";
import EmployeeProfileDetailsPage from "./pages/hr/employee-profiles/EmployeeProfileDetailsPage";
import EmploymentContractsPage from "./pages/hr/EmploymentContractsPage";
import LeaveRequestsPage from "./pages/hr/leave-requests/LeaveRequestsPage";
import LeaveRequestDetailsPage from "./pages/hr/leave-requests/LeaveRequestDetailsPage";
import SchedulesPage from "./pages/hr/SchedulesPage";
import TimeEntriesPage from "./pages/hr/TimeEntriesPage";
import TimesheetsPage from "./pages/hr/timesheets/TimesheetsPage";
import TimesheetDetailsPage from "./pages/hr/timesheets/TimesheetDetailsPage";
import PayrollPage from "./pages/hr/PayrollPage";
import AuthorPage from "./pages/inventory/authors/AuthorsListPage";
import UserRoleListPage from "./pages/hr/user-roles/UserRoleListPage";
import UserRoleDetailsPage from "./pages/hr/user-roles/UserRoleDetailsPage";
import { useAuth } from "./context/AuthContext";

import { PermissionProvider } from "./permissions/AppPermissionContext";
import MySpacePage from "./pages/MySpacePage";
import EmployeeProfilesPage from "./pages/hr/employee-profiles/EmployeeProfilesPage";
import AuthorDetailsPage from "./pages/inventory/authors/AuthorDetails";

function App() {
	const { user, isAuthenticated } = useAuth();

	return (
		<>
			<ToastContainer />
			<PermissionProvider user={isAuthenticated ? user : null}>
				<FormProvider>
					<DeleteDialogProvider>
						<Routes>
							<Route path={ROUTE_ROOT} element={<LoginPage />} />

							<Route element={<AuthenticatedExclusiveRoute />}>
								<Route element={<AppLayout />}>
									<Route path={ROUTE_DASHBOARD} element={<DashboardPage />} />
									<Route path={ROUTE_MY_PROFILE} element={<MyProfilePage />} />
									<Route path={ROUTE_MON_ESPACE} element={<MySpacePage />} />
									<Route path={ROUTE_CATALOGUE} element={<CatalogPage />} />
									<Route path={ROUTE_ITEM_DETAILS} element={<ItemDetailsPage />} />
									<Route path={ROUTE_LOCATIONS} element={<LocationsPage />} />
									<Route path={ROUTE_LOCATION_INVENTORY} element={<LocationInventoryQuantityPage />} />
									<Route path={ROUTE_LOCATION_DETAILS} element={<LocationDetailsPage />} />
									<Route path={ROUTE_CATEGORY} element={<CategoriesListPage />} />
									<Route path={ROUTE_CATEGORY_DETAILS} element={<CategoryDetailPage />} />
									<Route path={ROUTE_LIST_AUTHORS} element={<AuthorPage />} />
									<Route path={ROUTE_AUTHOR_DETAILS} element={<AuthorDetailsPage />} />
									<Route path={ROUTE_LIST_USER_ROLES} element={<UserRoleListPage />} />
									<Route path={ROUTE_USER_ROLE_DETAILS} element={<UserRoleDetailsPage />} />
									<Route path={ROUTE_LEAVE_REQUEST_DETAILS} element={<LeaveRequestDetailsPage />} />
									<Route element={<EmploymentContractExclusiveRoute />}>
										<Route path={ROUTE_EMPLOYMENT_CONTRACTS} element={<EmploymentContractsPage />} />
									</Route>

									<Route element={<PayrollExclusiveRoute />}>
										<Route path={ROUTE_PAYROLL} element={<PayrollPage />} />
									</Route>

									<Route element={<TimesheetExclusiveRoute />}>
										<Route path={ROUTE_TIMESHEET_DETAILS} element={<TimesheetDetailsPage />} />
									</Route>

									<Route element={<HrDashboardExclusiveRoute />} >
										<Route path={ROUTE_HR} element={<HRPage />} />
										<Route path={ROUTE_JOB_POSITIONS} element={<JobPositionsPage />} />
										<Route path={ROUTE_EMPLOYEE_PROFILES} element={<EmployeeProfilesPage />} />
										<Route path={ROUTE_EMPLOYEE_PROFILE_DETAILS} element={<EmployeeProfileDetailsPage />} />
										<Route path={ROUTE_EMPLOYMENT_CONTRACTS} element={<EmploymentContractsPage />} />
										<Route path={ROUTE_LEAVE_REQUESTS} element={<LeaveRequestsPage />} />
										<Route path={ROUTE_SCHEDULES} element={<SchedulesPage />} />
										<Route path={ROUTE_TIME_ENTRIES} element={<TimeEntriesPage />} />
										<Route path={ROUTE_TIMESHEETS} element={<TimesheetsPage />} />
										<Route path={ROUTE_TIMESHEET_DETAILS} element={<TimesheetDetailsPage />} />
										<Route path={ROUTE_PAYROLL} element={<PayrollPage />} />
										<Route path={ROUTE_LIST_USERS} element={<UsersListPage />} />
										<Route path={ROUTE_USER_PROFILE} element={<UserProfilePage />} />
										<Route path={ROUTE_IR} element={<IRPage />} />
									</Route>

									<Route path="*" element={<NotFoundPage />} />
								</Route>
							</Route>
						</Routes>
					</DeleteDialogProvider>
				</FormProvider>
			</PermissionProvider>
		</>
	);
}

export default App;