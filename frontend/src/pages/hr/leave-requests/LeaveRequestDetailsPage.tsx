import { Box, Button } from "@mui/material";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import { useQuery } from "@tanstack/react-query";
import { useNavigate, useParams } from "react-router-dom";
import leaveRequestService from "../../../api/services/hr/leaveRequestService";
import PageQueryWrapper from "../../../components/layouts/PageQueryWrapper";
import LeaveRequestApprovalActions from "../../../components/hr-components/LeaveRequestApprovalActions";
import LeaveRequestSummaryCard from "../../../components/hr-components/LeaveRequestSummaryCard";
import { leaveRequestsCacheKey } from "../../../data/cacheKeys";
import { ROUTE_LEAVE_REQUESTS, ROUTE_MON_ESPACE } from "../../../data/routeNames";
import type { LeaveRequest } from "../../../data/types/hr/leaveRequest";
import { LEAVE_REQUEST_STATUSES } from "../../../data/types/hr/leaveRequest";
import { useLeaveRequestMutations } from "../../../api/mutations/hr/useLeaveRequestMutations";
import { notifyErrorMessage, notifySuccessMessage } from "../../../data/utils/popupMessageManager";
import { extractApiErrorMessage } from "../../../data/utils/extractApiErrorMessage";
import { usePermissions } from "../../../permissions/usePermissions";
import { ENTITY_TYPES } from "../../../permissions/permissions";

export default function LeaveRequestDetailsPage() {
    const { id } = useParams();
    const navigate = useNavigate();
    const { canUpdate: canManageStatus } = usePermissions(ENTITY_TYPES.LEAVE_REQUEST);
    const { canRead: canReadHrDashboard } = usePermissions(ENTITY_TYPES.HR_DASHBOARD);
    const { updateLeaveRequestStatus, isUpdatingLeaveRequestStatus } = useLeaveRequestMutations();

    const leaveRequestId: number = Number(id);
    const isValidLeaveRequestId: boolean = Number.isInteger(leaveRequestId) && leaveRequestId > 0;

    const leaveRequestQuery = useQuery<LeaveRequest, Error>({
        queryKey: leaveRequestsCacheKey.details(id ?? ""),
        queryFn: () => leaveRequestService.getById(id ?? ""),
        enabled: isValidLeaveRequestId,
    });

    const leaveRequest: LeaveRequest | undefined = leaveRequestQuery.data;
    const hasError: boolean =
        !!leaveRequestQuery.error
        || !isValidLeaveRequestId
        || (!leaveRequest && !leaveRequestQuery.isLoading);

    const returnUrl: string = canReadHrDashboard
        ? ROUTE_LEAVE_REQUESTS
        : `${ROUTE_MON_ESPACE}?tab=conges`;
    const returnLabel: string = canReadHrDashboard
        ? "Retour aux congés"
        : "Retour à mon espace";

    const handleStatusChange = async (p_status: LeaveRequest["status"]): Promise<void> => {
        if (!leaveRequest) {
            return;
        }

        try {
            await updateLeaveRequestStatus({ id: leaveRequest.id, status: p_status });
            const message: string =
                p_status === LEAVE_REQUEST_STATUSES.Approved
                    ? "La demande de congé a été approuvée."
                    : "La demande de congé a été refusée.";
            notifySuccessMessage(message);
        } catch (error: unknown) {
            notifyErrorMessage(extractApiErrorMessage(error));
        }
    };

    const showApprovalButtons: boolean =
        canManageStatus
        && !!leaveRequest
        && leaveRequest.status === LEAVE_REQUEST_STATUSES.Pending;

    return (
        <PageQueryWrapper
            isLoading={leaveRequestQuery.isLoading}
            error={
                hasError
                    ? (leaveRequestQuery.error ?? { message: "Demande de congé introuvable" })
                    : null
            }
            refetch={leaveRequestQuery.refetch}
            errorReturnUrl={returnUrl}
            errorReturnLabel={returnLabel}
            customErrorMessage="Impossible de charger la demande de congé."
        >
            {leaveRequest && (
                <Box sx={{ textAlign: "left" }}>
                    <Button
                        startIcon={<ArrowBackIcon />}
                        onClick={() => navigate(returnUrl)}
                        sx={{ mb: 2 }}
                    >
                        {returnLabel}
                    </Button>

                    <LeaveRequestSummaryCard
                        leaveRequest={leaveRequest}
                        footer={
                            showApprovalButtons ? (
                                <LeaveRequestApprovalActions
                                    disabled={isUpdatingLeaveRequestStatus}
                                    onApprove={() => {
                                        void handleStatusChange(LEAVE_REQUEST_STATUSES.Approved);
                                    }}
                                    onReject={() => {
                                        void handleStatusChange(LEAVE_REQUEST_STATUSES.Rejected);
                                    }}
                                />
                            ) : undefined
                        }
                    />
                </Box>
            )}
        </PageQueryWrapper>
    );
}
