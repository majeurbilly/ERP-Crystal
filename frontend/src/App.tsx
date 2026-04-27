import { Route, Routes } from "react-router-dom";

import { ROUTE_DASHBOARD, ROUTE_MY_PROFILE, ROUTE_ROOT, ROUTE_HR } from "./data/routeNames";
import PageLogin from "./pages/PageLogin";
import "./App.css";
import PageDashboard from "./pages/PageDashboard";
import Layout from "./components/Layout";
import HRPage from "./pages/HRPage";
import MyProfilePage from "./pages/MyProfilePage";

function App() {
	return (
		<Routes>
			<Route path={ROUTE_ROOT} element={<PageLogin />} />

			<Route element={<Layout />}>
				<Route path={ROUTE_DASHBOARD} element={<PageDashboard />} />
				<Route path={ROUTE_HR} element={<HRPage />} />
				<Route path={ROUTE_MY_PROFILE} element={<MyProfilePage />} />
				<Route path="*" element={<p>page not found</p>} />
			</Route>

		</Routes>
	);
}

export default App;
