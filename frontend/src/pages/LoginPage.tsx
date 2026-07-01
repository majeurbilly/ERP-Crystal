import { isAxiosError } from "axios";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import apiClient from "../api/apiClient";
import AnimatedLogin from "../components/animated-login";
import { ROUTE_DASHBOARD } from "../data/routeNames";
import { useAuth } from "../context/AuthContext";
import { API_URL } from "../api/apiBaseUrl";

type LoginResponseBody = {
	token?: string;
};

export default function LoginPage() {
	const navigate = useNavigate();
	const [email, setEmail] = useState("");
	const [password, setPassword] = useState("");
	const [error, setError] = useState("");

	const { login, logout } = useAuth();

	const handleLoginRequest = async (
		submittedEmail: string,
		submittedPassword: string,
	) => {
		setError("");
		try {
			const response = await apiClient.post<LoginResponseBody>(
				`${API_URL}/auth/login`,
				{
					email: submittedEmail,
					password: submittedPassword,
				},
			);

			const token = response.data.token;
			if (!token) {
				setError("Réponse invalide : aucun jeton d’authentification reçu.");
				return;
			}

			login(token);

			setTimeout(() => {
				navigate(ROUTE_DASHBOARD);
			}, 10);
		} catch (err) {
			logout();

			if (isAxiosError(err) && err.response) {
				const status = err.response.status;
				const data = err.response.data as
					| { message?: string; title?: string }
					| undefined;
				const serverMsg =
					(typeof data?.message === "string" && data.message) ||
					(typeof data?.title === "string" && data.title) ||
					null;

				if (status === 401 || status === 403) {
					setError(serverMsg ?? "Courriel ou mot de passe incorrect.");
				} else {
					setError(
						serverMsg ?? `Erreur de connexion (HTTP ${String(status)}).`,
					);
				}
				return;
			}

			setError(
				"Impossible de joindre le serveur. Vérifiez votre connexion ou réessayez plus tard.",
			);
		}
	};

	return (
		<div className="min-vh-100 w-100 bg-light">
			<AnimatedLogin
				email={email}
				password={password}
				onEmailChange={(v: string) => {
					setEmail(v);
					setError("");
				}}
				onPasswordChange={(v: string) => {
					setPassword(v);
					setError("");
				}}
				onLoginRequest={handleLoginRequest}
				externalError={error}
			/>
		</div>
	);
}