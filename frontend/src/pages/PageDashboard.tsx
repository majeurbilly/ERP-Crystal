import { Navigate/*, useParams */ } from "react-router-dom";
import { isUserRole } from "../data/devAuth";
import { useAuth } from "../context/AuthContext";
import { roleLabels } from "../data/userRoles";
import ToggleThemeButton from "../components/ToggleThemeButton";

export default function PageDashboard() {
	const { role, token, id } = useAuth();

	if (token && !role && !id) {
		return <div>Chargement...</div>;
	}

	if (!role || !isUserRole(role)) {
		return <Navigate to="/" replace />;
	}

	return (
		<>
			{console.log(role)}
			<p className="mb-0">
				Tableau de bord — <strong>{roleLabels[role]}</strong> ({role})
			</p>
			<p className="mb-0">
				Id du user — <strong>{id}</strong>
			</p>
			<ToggleThemeButton />
		</>
	);
}
