import { useState } from "react";
import { useNavigate } from "react-router-dom";
import userRoleService from "../../../api/services/hr/userRoleService";
import { CustomDataGrid } from "../../../components/data-grids/CustomDataGrid";
import GenericPageLayout from "../../../components/layouts/GenericPageLayout";
import PageQueryWrapper from "../../../components/layouts/PageQueryWrapper";
import { userRolesCacheKey } from "../../../data/cacheKeys";
import { userRoleColumns } from "../../../data/gridColumns";
import { useSearchableQuery } from "../../../data/hooks/useSearchableQuery";
import { ROUTE_DASHBOARD, ROUTE_USER_ROLE_DETAILS } from "../../../data/routeNames";
import { useDeleteDialog } from "../../../context/DeleteDialogContext";
import { useUserRoleMutations } from "../../../api/mutations/hr/useUserRoleMutations";
import { usePermissions } from "../../../permissions/usePermissions";
import { ENTITY_TYPES } from "../../../permissions/permissions";
import { useAuth } from "../../../context/AuthContext";
import UserRoleForm from "../../../components/forms/hr/UserRoleForm";
import type { DynamicUserRole } from "../../../data/types/hr/dynamicUserRole";

export default function UserRoleListPage() {
    const navigate = useNavigate();
    const { openConfirmDeleteWindow } = useDeleteDialog();
    const { deleteUserRole: deleteUserRoleMutation } = useUserRoleMutations();
    const { canUpdate, canCreate, canDelete } = usePermissions(ENTITY_TYPES.USER_ROLE);
    const { user } = useAuth();
    const [showUserRoleForm, setShowUserRoleForm] = useState(false);
    const [editUserRole, setEditUserRole] = useState<DynamicUserRole | null>(null);

    const query = useSearchableQuery({
        queryKey: userRolesCacheKey.list(),
        queryFn: () => userRoleService.getAll(),
        filterFn: (userRole, search) =>
            userRole.name.toLowerCase().includes(search.toLowerCase()),
    });

    const openAddForm = (): void => {
        setEditUserRole(null);
        setShowUserRoleForm(true);
    };

    const openEditForm = (userRole: DynamicUserRole): void => {
        setEditUserRole(userRole);
        setShowUserRoleForm(true);
    };

    return (
        <PageQueryWrapper
            isLoading={query.isLoading}
            error={query.error}
            refetch={query.refetch}
            errorReturnUrl={ROUTE_DASHBOARD}
            errorReturnLabel="Retour au tableau de bord"
        >
            <GenericPageLayout title={"Liste des rôles"}>
                <CustomDataGrid
                    rows={query.filteredData}
                    columns={userRoleColumns}
                    addLabel="Ajouter un rôle"
                    onAddClick={canCreate ? openAddForm : undefined}
                    onEditClick={canUpdate ? openEditForm : undefined}
                    onDeleteClick={canDelete ? (userRole) => openConfirmDeleteWindow({
                        id: userRole.id,
                        displayLabel: userRole.name,
                        onDelete: deleteUserRoleMutation
                    }) : undefined}
                    isDeleteDisabledForRow={canDelete ? (userRole) =>
                        userRole.isPreset === true || userRole.id === user?.dynamicRole?.id
                        : undefined}
                    onRowClick={(userRole) => navigate(ROUTE_USER_ROLE_DETAILS.replace(":id", String(userRole.id)))}
                    {...query.searchProps}
                />
            </GenericPageLayout>

            <UserRoleForm
                showUserRoleForm={showUserRoleForm}
                setShowUserRoleForm={setShowUserRoleForm}
                editUserRole={editUserRole}
                setEditUserRole={setEditUserRole}
            />
        </PageQueryWrapper>
    );
}
