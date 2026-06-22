import { useSearchableQuery } from "../../../data/hooks/useSearchableQuery";
import GenericPageLayout from "../../../components/layouts/GenericPageLayout";
import { authorsCachekey } from "../../../data/cacheKeys";
import authorService from "../../../api/services/inventory/authorService";
import PageQueryWrapper from "../../../components/layouts/PageQueryWrapper";
import { ROUTE_DASHBOARD } from "../../../data/routeNames";
import { CustomDataGrid } from "../../../components/data-grids/CustomDataGrid";
import { authorColumns } from "../../../data/gridColumns";
import { useNavigate } from "react-router-dom";
import { ROUTE_AUTHOR_DETAILS } from "../../../data/routeNames";
import { FORM_TYPES, useFormContainer } from "../../../context/FormContext";
import { usePermissions } from "../../../permissions/usePermissions";
import { ENTITY_TYPES } from "../../../permissions/permissions";
import { useDeleteDialog } from "../../../context/DeleteDialogContext";
import { useAuthorMutations } from "../../../api/mutations/inventory/useAuthorMutations";

export default function AuthorPage() {
    const navigate = useNavigate();
    const { canCreate, canUpdate, canDelete } = usePermissions(ENTITY_TYPES.AUTHOR);
    const { openForm } = useFormContainer();
    const { openConfirmDeleteWindow } = useDeleteDialog();
    const { deleteAuthor: deleteAuthorMutation } = useAuthorMutations();
    const query = useSearchableQuery({
        queryKey: authorsCachekey.list(),
        queryFn: () => authorService.getAll(),
        filterFn: (author, search) => author.name.toLocaleLowerCase().includes(search.toLowerCase())
    })
    return (
        <>
            <PageQueryWrapper isLoading={query.isLoading} error={query.error} refetch={query.refetch} errorReturnUrl={ROUTE_DASHBOARD} errorReturnLabel="Retourner au tableau de bord">
                <GenericPageLayout title="Liste des auteurs">
                    <CustomDataGrid
                        rows={query.filteredData}
                        columns={authorColumns}
                        addLabel="Ajouter un auteur"
                        onAddClick={canCreate ? () => openForm(FORM_TYPES.AUTHOR) : undefined}
                        onEditClick={canUpdate ? (author) => openForm(FORM_TYPES.AUTHOR, author) : undefined}
                        onDeleteClick={canDelete ? (author) => openConfirmDeleteWindow({
                            id: author.id,
                            displayLabel: author.name,
                            onDelete: deleteAuthorMutation
                        }) : undefined}
                        onRowClick={(params) => navigate(ROUTE_AUTHOR_DETAILS.replace(":id", String(params.id)))}
                        {...query.searchProps}>
                    </CustomDataGrid>
                </GenericPageLayout>
            </PageQueryWrapper>
        </>
    )
}