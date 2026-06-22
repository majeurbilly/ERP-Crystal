import { useAuth } from "../context/AuthContext";
import LoadingSpinner from "../components/LoadingSpinner";
import GenericPageLayout from "../components/layouts/GenericPageLayout";
import DashboardWidgetGrid from "../components/dashboard/DashboardWidgetGrid";
import { Typography } from "@mui/material";

export default function DashboardPage() {
    const { user, isAuthenticated } = useAuth();

    if (!isAuthenticated) {
        return <LoadingSpinner />;
    }

    return (
        <GenericPageLayout title="Tableau de bord">
            <Typography align="left" sx={{ mb: 2 }}>
                Bienvenue{user?.employeeProfile?.firstName ? `, ${user.employeeProfile.firstName}` : ""}.
            </Typography>
            <DashboardWidgetGrid />
        </GenericPageLayout>
    );
}
