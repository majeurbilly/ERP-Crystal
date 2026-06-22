import { useLeaveRequestMutations } from "../../../api/mutations/hr/useLeaveRequestMutations";
import { useSearchableQuery } from "../../../data/hooks/useSearchableQuery";
import { leaveRequestsCacheKey } from "../../../data/cacheKeys";
import leaveRequestService from "../../../api/services/hr/leaveRequestService";
import PageQueryWrapper from "../../../components/layouts/PageQueryWrapper";
import { ROUTE_HR, buildLeaveRequestDetailsPath } from "../../../data/routeNames";
import { buildLeaveRequestColumns } from "../../../data/gridColumns";
import { CustomDataGrid } from "../../../components/data-grids/CustomDataGrid";
import GenericPageLayout from "../../../components/layouts/GenericPageLayout";
import { FORM_TYPES, useFormContainer } from "../../../context/FormContext";
import type { LeaveRequest } from "../../../data/types/hr/leaveRequest";
import { LEAVE_REQUEST_STATUSES } from "../../../data/types/hr/leaveRequest";
import { notifyErrorMessage, notifySuccessMessage } from "../../../data/utils/popupMessageManager";
import { extractApiErrorMessage } from "../../../data/utils/extractApiErrorMessage";
import { usePermissions } from "../../../permissions/usePermissions";
import { ENTITY_TYPES } from "../../../permissions/permissions";
import { useMemo } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";

export default function LeaveRequestsPage() {
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();
    const statusFilter = searchParams.get("status");
    const { canCreate, canUpdate } = usePermissions(ENTITY_TYPES.LEAVE_REQUEST);
    const { openForm } = useFormContainer();
    const { updateLeaveRequestStatus } = useLeaveRequestMutations();

    const query = useSearchableQuery({
        queryKey: leaveRequestsCacheKey.list(),
        queryFn: () => leaveRequestService.getAll(),
        filterFn: (p_leaveRequest: LeaveRequest, p_search: string) => {
            const normalizedSearch: string = p_search.toLowerCase();
            const employeeName: string =
                `${p_leaveRequest.employeeFirstName} ${p_leaveRequest.employeeLastName}`.toLowerCase();
            return (
                employeeName.includes(normalizedSearch)
                || p_leaveRequest.leaveType.toLowerCase().includes(normalizedSearch)
                || p_leaveRequest.status.toLowerCase().includes(normalizedSearch)
                || (p_leaveRequest.reason?.toLowerCase().includes(normalizedSearch) ?? false)
            );
        },
    });

    const displayData = useMemo(() => {
        if (!statusFilter) {
            return query.filteredData;
        }
        return query.filteredData.filter((p_item) => p_item.status === statusFilter);
    }, [query.filteredData, statusFilter]);

    const handleApprove = async (p_leaveRequest: LeaveRequest): Promise<void> => {
        try {
            await updateLeaveRequestStatus({
                id: p_leaveRequest.id,
                status: LEAVE_REQUEST_STATUSES.Approved,
            });
            notifySuccessMessage("La demande de congé a été approuvée.");
        } catch (error: unknown) {
            notifyErrorMessage(extractApiErrorMessage(error));
        }
    };

    const handleReject = async (p_leaveRequest: LeaveRequest): Promise<void> => {
        try {
            await updateLeaveRequestStatus({
                id: p_leaveRequest.id,
                status: LEAVE_REQUEST_STATUSES.Rejected,
            });
            notifySuccessMessage("La demande de congé a été refusée.");
        } catch (error: unknown) {
            notifyErrorMessage(extractApiErrorMessage(error));
        }
    };

    const columns = buildLeaveRequestColumns({
        canManageStatus: canUpdate,
        onApprove: (p_row: LeaveRequest) => {
            void handleApprove(p_row);
        },
        onReject: (p_row: LeaveRequest) => {
            void handleReject(p_row);
        },
    });

    return (
        <PageQueryWrapper
            isLoading={query.isLoading}
            error={query.error}
            refetch={query.refetch}
            errorReturnUrl={ROUTE_HR}
            errorReturnLabel="Retour au tableau de bord RH"
            customErrorMessage="Impossible de charger les demandes de congé."
        >
            <GenericPageLayout title="Congés">
                <CustomDataGrid
                    rows={displayData}
                    columns={columns}
                    addLabel="Ajouter une demande"
                    onAddClick={canCreate ? () => openForm(FORM_TYPES.LEAVE_REQUEST) : undefined}
                    onRowClick={(p_params) => navigate(buildLeaveRequestDetailsPath(p_params.id))}
                    sx={{
                        "& .MuiDataGrid-row": { cursor: "pointer" },
                    }}
                    {...query.searchProps}
                />
            </GenericPageLayout>
        </PageQueryWrapper>
    );
}
