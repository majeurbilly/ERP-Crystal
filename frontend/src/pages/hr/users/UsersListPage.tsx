import userService from "../../../api/services/hr/userService";
import { usersCacheKey } from "../../../data/cacheKeys";
import { ROUTE_HR, ROUTE_USER_PROFILE } from "../../../data/routeNames";
import { useSearchableQuery } from "../../../data/hooks/useSearchableQuery";
import PageQueryWrapper from "../../../components/layouts/PageQueryWrapper";
import { CustomDataGrid } from "../../../components/data-grids/CustomDataGrid";
import { userColumns } from "../../../data/gridColumns";
import { FORM_TYPES, useFormContainer } from "../../../context/FormContext";
import { useAuth } from "../../../context/AuthContext";
import { useNavigate } from "react-router-dom";
import { useDeleteDialog } from "../../../context/DeleteDialogContext";
import { useUserMutations } from "../../../api/mutations/hr/useUserMutations";
import GenericPageLayout from "../../../components/layouts/GenericPageLayout";
import { usePermissions } from "../../../permissions/usePermissions";
import { ENTITY_TYPES } from "../../../permissions/permissions";

export default function UsersListPage() {
    const navigate = useNavigate();
    const { user: me } = useAuth();
    const { canCreate, canUpdate, canDelete } = usePermissions(ENTITY_TYPES.USER);
    const { openForm } = useFormContainer();
    const { openConfirmDeleteWindow } = useDeleteDialog();
    const { deleteUser: deleteUserMutation } = useUserMutations();

    const query = useSearchableQuery({
        queryKey: usersCacheKey.list(),
        queryFn: () => userService.getAll(),
        filterFn: (user, search) => user.userName.toLowerCase().includes(search.toLowerCase())
    });

    return (
        <PageQueryWrapper
            isLoading={query.isLoading}
            error={query.error}
            refetch={query.refetch}
            errorReturnUrl={ROUTE_HR}
            errorReturnLabel="Retour à la page RH"
        >
            <GenericPageLayout
                title="Liste des utilisateurs"
            >
                <CustomDataGrid
                    rows={query.filteredData}
                    columns={userColumns}
                    addLabel="Ajouter un utilisateur"
                    onAddClick={canCreate ? () => openForm(FORM_TYPES.USER) : undefined}
                    onEditClick={canUpdate ? (user) => openForm(FORM_TYPES.USER, user) : undefined}
                    onDeleteClick={canDelete ? (user) => openConfirmDeleteWindow({
                        id: user.id,
                        displayLabel: user.userName,
                        onDelete: deleteUserMutation
                    }) : undefined}
                    isDeleteDisabledForRow={(user) => user.id === me?.id}
                    onRowClick={(params) => navigate(ROUTE_USER_PROFILE.replace(":id", String(params.id)))}
                    {...query.searchProps}
                />
            </GenericPageLayout>
        </PageQueryWrapper>
    )
}