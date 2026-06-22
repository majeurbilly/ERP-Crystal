import HrMetricsCards from "../../components/hr-components/HrMetricsCards";
import HrPendingTasksPanel from "../../components/hr-components/HrPendingTasksPanel";
import HrQuickActions from "../../components/hr-components/HrQuickActions";
import GenericPageLayout from "../../components/layouts/GenericPageLayout";
import PageQueryWrapper from "../../components/layouts/PageQueryWrapper";
import { useHrDashboardMetrics } from "../../api/queries/useHrDashboardMetrics";
import { ROUTE_HR } from "../../data/routeNames";

export default function HRPage() {
    const { data: metrics, isLoading, error, refetch } = useHrDashboardMetrics();

    return (
        <PageQueryWrapper
            isLoading={isLoading}
            error={error}
            refetch={refetch}
            errorReturnUrl={ROUTE_HR}
            errorReturnLabel="Retour au tableau de bord RH"
            customErrorMessage="Impossible de charger les indicateurs RH."
        >
            <GenericPageLayout title="Ressources humaines">
                {metrics && <HrMetricsCards metrics={metrics} />}
                <HrPendingTasksPanel />
                <HrQuickActions />
            </GenericPageLayout>
        </PageQueryWrapper>
    );
}
