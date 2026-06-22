import PageQueryWrapper from "../../../components/layouts/PageQueryWrapper";
import { useQuery } from "@tanstack/react-query";
import { authorsCachekey, itemsCacheKey } from "../../../data/cacheKeys";
import authorService from "../../../api/services/inventory/authorService";
import { useParams, useNavigate } from "react-router-dom";
import { ROUTE_LIST_AUTHORS, ROUTE_LOCATIONS, ROUTE_ITEM_DETAILS } from "../../../data/routeNames";
import GenericPageLayout from "../../../components/layouts/GenericPageLayout";
import { usePermissions } from "../../../permissions/usePermissions";
import { useFormContainer } from "../../../context/FormContext";
import { FORM_TYPES } from "../../../context/FormContext";
import { ENTITY_TYPES } from "../../../permissions/permissions";
import { useDeleteDialog } from "../../../context/DeleteDialogContext";
import { useAuthorMutations } from "../../../api/mutations/inventory/useAuthorMutations";
import itemService from "../../../api/services/inventory/itemService";
import { CustomDataGrid } from "../../../components/data-grids/CustomDataGrid";
import { itemColumns } from "../../../data/gridColumns";
import { useSearchableQuery } from "../../../data/hooks/useSearchableQuery";
import Typography from "@mui/material/Typography";

export default function AuthorDetailsPage() {
    const navigate = useNavigate();
    const { id } = useParams();
    const authorId = Number(id);

    const { data: author, isLoading, error, refetch } = useQuery({
        queryKey: authorsCachekey.details(id!),
        queryFn: () => authorService.getById(id!),
        enabled: !!id,
    });

    const authorBooksQuery = useSearchableQuery({
        queryKey: itemsCacheKey.list({ authorId }),
        queryFn: () => itemService.getAll({ authorId, isBook: true }),
        filterFn: (book, search) => book.name.toLowerCase().includes(search.toLowerCase()),
        enabled: !!author && !Number.isNaN(authorId)
    });

    const { canUpdate, canDelete } = usePermissions(ENTITY_TYPES.AUTHOR);
    const { openForm } = useFormContainer();
    const { openConfirmDeleteWindow } = useDeleteDialog();
    const { deleteAuthor: deleteAuthorMutation } = useAuthorMutations();

    return (
        <PageQueryWrapper
            isLoading={isLoading || authorBooksQuery.isLoading}
            error={error || (!author && !isLoading ? { message: "Auteur introuvable" } : null)}
            refetch={refetch}
            errorReturnUrl={ROUTE_LOCATIONS}
            errorReturnLabel="Retour aux auteurs"
        >
            {author && (
                <GenericPageLayout
                    title={author.name}
                    subtitle={`Livres de ${author.name}`}
                    onEditClick={canUpdate ? () => openForm(FORM_TYPES.AUTHOR, author) : undefined}
                    onDeleteClick={canDelete ? () => openConfirmDeleteWindow({
                        id: author.id,
                        displayLabel: author.name,
                        onDelete: deleteAuthorMutation,
                        redirectUrl: ROUTE_LIST_AUTHORS,
                    }) : undefined}
                >
                    {authorBooksQuery.filteredData.length > 0 ? (
                        <>
                            <CustomDataGrid
                                rows={authorBooksQuery.filteredData}
                                columns={itemColumns}
                                onRowClick={(params) =>
                                    navigate(ROUTE_ITEM_DETAILS.replace(":id", String(params.id)))
                                }
                                {...authorBooksQuery.searchProps}
                            />
                        </>
                    ) : (
                        <Typography variant="body1">
                            Cet auteur n'a aucun livre associé.
                        </Typography>
                    )}
                </GenericPageLayout>
            )}
        </PageQueryWrapper>
    );
}
