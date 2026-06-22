import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { useParams } from "react-router-dom";
import { Box, Chip, Typography } from "@mui/material";
import { userRolesCacheKey } from "../../../data/cacheKeys";
import userRoleService from "../../../api/services/hr/userRoleService";
import PageQueryWrapper from "../../../components/layouts/PageQueryWrapper";
import { ROUTE_LIST_USER_ROLES } from "../../../data/routeNames";
import { useUserRoleMutations } from "../../../api/mutations/hr/useUserRoleMutations";
import { useDeleteDialog } from "../../../context/DeleteDialogContext";
import { usePermissions } from "../../../permissions/usePermissions";
import { ENTITY_TYPES } from "../../../permissions/permissions";
import { useAuth } from "../../../context/AuthContext";
import GenericPageLayout from "../../../components/layouts/GenericPageLayout";
import UserRoleForm from "../../../components/forms/hr/UserRoleForm";
import PermissionListView from "../../../components/hr-components/PermissionListView";

export default function UserRoleDetailsPage() {
    const { id } = useParams();
    const { deleteUserRole: deleteUserRoleMutation } = useUserRoleMutations();
    const { openConfirmDeleteWindow } = useDeleteDialog();
    const { canUpdate, canDelete } = usePermissions(ENTITY_TYPES.USER_ROLE);
    const { user } = useAuth();
    const [showUserRoleForm, setShowUserRoleForm] = useState(false);

    const { data: userRole, isLoading, error, refetch } = useQuery({
        queryKey: userRolesCacheKey.details(id!),
        queryFn: () => userRoleService.getById(id!),
        enabled: !!id,
    });

    const permissions = userRole ? userRole.permissions : [];
    const canDeleteRole = canDelete
        && userRole
        && !userRole.isPreset
        && id !== user?.dynamicRole?.id;
    const canEditRole = canUpdate && userRole && !userRole.isPreset;

    return (
        <PageQueryWrapper
            isLoading={isLoading}
            error={error || (!userRole && !isLoading ? { message: "introuvable" } : null)}
            refetch={refetch}
            errorReturnUrl={ROUTE_LIST_USER_ROLES}
            errorReturnLabel="Retour à la liste des rôles"
        >
            {userRole &&
                <GenericPageLayout
                    title={userRole.name}
                    subtitle={
                        userRole.isPreset
                            ? "Rôle prédéfini — consultation seulement"
                            : "Rôle personnalisé — droits configurables"
                    }
                    onEditClick={canEditRole ? () => setShowUserRoleForm(true) : undefined}
                    onDeleteClick={canDeleteRole ? () => openConfirmDeleteWindow({
                        id: userRole.id,
                        displayLabel: userRole.name,
                        onDelete: deleteUserRoleMutation,
                        redirectUrl: ROUTE_LIST_USER_ROLES
                    }) : undefined}
                >
                    <Box sx={{ mb: 2, display: "flex", alignItems: "center", gap: 1, flexWrap: "wrap" }}>
                        <Typography variant="body2" color="text.secondary">
                            {permissions.length} droit{permissions.length > 1 ? "s" : ""} d&apos;accès
                        </Typography>
                        {userRole.isPreset && (
                            <Chip label="Modèle par défaut" size="small" color="info" variant="outlined" />
                        )}
                    </Box>

                    <PermissionListView permissions={permissions} />
                </GenericPageLayout>
            }

            {userRole && (
                <UserRoleForm
                    showUserRoleForm={showUserRoleForm}
                    setShowUserRoleForm={setShowUserRoleForm}
                    editUserRole={showUserRoleForm ? userRole : null}
                />
            )}
        </PageQueryWrapper>
    );
}
