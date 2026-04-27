import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import App from "./App.tsx";
import "bootstrap/dist/css/bootstrap.min.css";
import { BrowserRouter } from "react-router-dom";
import { SidebarProvider } from "./context/SidebarContext.tsx";
import { AuthProvider } from "./context/AuthContext.tsx";
import { CustomThemeProvider } from "./context/CustomThemeContext.tsx";
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';


const rootElement = document.getElementById("root");
if (!rootElement) {
	throw new Error(
		'Élément racine "#root" introuvable : impossible de monter l’application React.',
	);
}

const queryClient = new QueryClient();

createRoot(rootElement).render(
	<StrictMode>
		<BrowserRouter>
			<QueryClientProvider client={queryClient}>
				<AuthProvider>
					<CustomThemeProvider>
						<SidebarProvider>
							<App />
						</SidebarProvider>
					</CustomThemeProvider>
				</AuthProvider>
			</QueryClientProvider>
		</BrowserRouter>
	</StrictMode>,
);
