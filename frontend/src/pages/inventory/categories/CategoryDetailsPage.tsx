import { useParams, useNavigate } from "react-router-dom";
import categoryService from "../../../api/services/inventory/categoryService";
import PageQueryWrapper from "../../../components/layouts/PageQueryWrapper";
import { useQuery } from "@tanstack/react-query";
import { categoriesCachekey, itemsCacheKey } from "../../../data/cacheKeys";
import { ROUTE_CATEGORY, ROUTE_LOCATIONS, ROUTE_ITEM_DETAILS } from "../../../data/routeNames";
import GenericPageLayout from "../../../components/layouts/GenericPageLayout";
import { useCategoryMutations } from "../../../api/mutations/inventory/useCategoryMutations";
import { useDeleteDialog } from "../../../context/DeleteDialogContext";
import { FORM_TYPES } from "../../../context/FormContext";
import { useFormContainer } from "../../../context/FormContext";
import { usePermissions } from "../../../permissions/usePermissions";
import { ENTITY_TYPES } from "../../../permissions/permissions";
import itemService from "../../../api/services/inventory/itemService";
import { CustomDataGrid } from "../../../components/data-grids/CustomDataGrid";
import { itemColumns } from "../../../data/gridColumns";
import { useSearchableQuery } from "../../../data/hooks/useSearchableQuery";

export default function CategoryDetailPage() {
    const navigate = useNavigate();
    const { id } = useParams();
    const { deleteCategory: deleteCategoryMutation } = useCategoryMutations();

    const { openConfirmDeleteWindow } = useDeleteDialog();
    const { data: category, isLoading, error, refetch } = useQuery({
        queryKey: categoriesCachekey.details(id!),
        queryFn: () => categoryService.getById(id!),
        enabled: !!id,
    });

    const categoryId = Number(category?.id);

    const { filteredData: items, searchProps, isLoading: isItemsLoading } =
        useSearchableQuery({
            queryKey: itemsCacheKey.list({ categoryIds: [categoryId] }),
            queryFn: () => itemService.getAll({
                categoryIds: [categoryId],
                isBook: true,
            }),
            filterFn: (item, search) =>
                item.name.toLowerCase().includes(search.toLowerCase()),
            enabled: !!category && !Number.isNaN(categoryId),
        });

    const { canUpdate, canDelete } = usePermissions(ENTITY_TYPES.CATEGORY);
    const { openForm } = useFormContainer();

    return (
        <PageQueryWrapper
            isLoading={isLoading || isItemsLoading}
            error={error || (!category && !isLoading ? { message: "Catégorie introuvable" } : null)}
            refetch={refetch}
            errorReturnUrl={ROUTE_LOCATIONS}
            errorReturnLabel="Retour aux catégories"
        >
            {category && (
                <GenericPageLayout
                    title={category.name}
                    onEditClick={canUpdate ? () => openForm(FORM_TYPES.CATEGORY, category) : undefined}
                    onDeleteClick={canDelete ? () => openConfirmDeleteWindow({
                        id: category.id,
                        displayLabel: category.name,
                        onDelete: deleteCategoryMutation,
                        redirectUrl: ROUTE_CATEGORY

                    }) : undefined}
                >
                    <CustomDataGrid
                        rows={items}
                        columns={itemColumns}
                        onRowClick={(params) =>
                            navigate(
                                ROUTE_ITEM_DETAILS.replace(
                                    ":id",
                                    String(params.id)
                                )
                            )
                        }
                        {...searchProps}
                    />
                </GenericPageLayout>
            )}
        </PageQueryWrapper>
    );
}
