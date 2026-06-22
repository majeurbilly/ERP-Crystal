import { Component, type ErrorInfo, type ReactNode } from "react";
import { Box, Button, Typography } from "@mui/material";
import { ROUTE_DASHBOARD } from "../../data/routeNames";

interface RouteErrorBoundaryProps {
    children: ReactNode;
}

interface RouteErrorBoundaryState {
    hasError: boolean;
    errorMessage: string;
}

export default class RouteErrorBoundary extends Component<
    RouteErrorBoundaryProps,
    RouteErrorBoundaryState
> {
    public constructor(p_props: RouteErrorBoundaryProps) {
        super(p_props);
        this.state = { hasError: false, errorMessage: "" };
    }

    public static getDerivedStateFromError(p_error: Error): RouteErrorBoundaryState {
        return {
            hasError: true,
            errorMessage: p_error.message || "Erreur inattendue lors du chargement de la page.",
        };
    }

    public componentDidCatch(p_error: Error, p_info: ErrorInfo): void {
        console.error("RouteErrorBoundary:", p_error, p_info.componentStack);
    }

    public render(): ReactNode {
        if (this.state.hasError) {
            return (
                <Box sx={{ py: 4, textAlign: "center" }}>
                    <Typography variant="h6" color="error" gutterBottom>
                        Impossible d&apos;afficher cette page
                    </Typography>
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                        {this.state.errorMessage}
                    </Typography>
                    <Button variant="contained" href={ROUTE_DASHBOARD}>
                        Retour au tableau de bord
                    </Button>
                </Box>
            );
        }

        return this.props.children;
    }
}
