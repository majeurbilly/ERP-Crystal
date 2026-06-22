import { useNavigate } from "react-router-dom";
import { useDeleteDialog } from "../../../context/DeleteDialogContext";
import { useCategoryMutations } from "../../../api/mutations/inventory/useCategoryMutations";
import { useSearchableQuery } from "../../../data/hooks/useSearchableQuery";
import { categoriesCachekey } from "../../../data/cacheKeys";
import categoryService from "../../../api/services/inventory/categoryService";
import PageQueryWrapper from "../../../components/layouts/PageQueryWrapper";
import { ROUTE_CATEGORY_DETAILS, ROUTE_DASHBOARD } from "../../../data/routeNames";
import { categoryColumns } from "../../../data/gridColumns";
import { CustomDataGrid } from "../../../components/data-grids/CustomDataGrid";
import GenericPageLayout from "../../../components/layouts/GenericPageLayout";
import { FORM_TYPES, useFormContainer } from "../../../context/FormContext";
import { usePermissions } from "../../../permissions/usePermissions";
import { ENTITY_TYPES } from "../../../permissions/permissions";

export default function CategoriesListPage() {
    const navigate = useNavigate();
    const { canCreate, canUpdate, canDelete } = usePermissions(ENTITY_TYPES.CATEGORY);
    const { openForm } = useFormContainer();
    const { openConfirmDeleteWindow } = useDeleteDialog();
    const { deleteCategory: deleteCategoryMutation } = useCategoryMutations();

    const query = useSearchableQuery({
        queryKey: categoriesCachekey.list(),
        queryFn: () => categoryService.getAll(),
        filterFn: (category, search) => category.name.toLowerCase().includes(search.toLowerCase())
    });


    return (
        <PageQueryWrapper
            isLoading={query.isLoading}
            error={query.error}
            refetch={query.refetch}
            errorReturnUrl={ROUTE_DASHBOARD}
            errorReturnLabel="Retour au tableau de bord"
        >
            <GenericPageLayout
                title="Liste des catégories"
            >
                <CustomDataGrid
                    rows={query.filteredData}
                    columns={categoryColumns}
                    addLabel="Ajouter une catégorie"
                    onAddClick={canCreate ? () => openForm(FORM_TYPES.CATEGORY) : undefined}
                    onEditClick={canUpdate ? (category) => openForm(FORM_TYPES.CATEGORY, category) : undefined}
                    onDeleteClick={canDelete ? (category) => openConfirmDeleteWindow({
                        id: category.id,
                        displayLabel: category.name,
                        onDelete: deleteCategoryMutation
                    }) : undefined}
                    onRowClick={(params) => navigate(ROUTE_CATEGORY_DETAILS.replace(":id", String(params.id)))}
                    {...query.searchProps}
                />
            </GenericPageLayout>
        </PageQueryWrapper>
    );
}