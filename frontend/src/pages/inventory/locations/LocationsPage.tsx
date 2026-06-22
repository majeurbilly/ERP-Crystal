import { useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { Box, Chip, Typography } from "@mui/material";
import StorefrontOutlinedIcon from "@mui/icons-material/StorefrontOutlined";
import locationService from "../../../api/services/inventory/locationService";
import { useLocationMutations } from "../../../api/mutations/inventory/useLocationMutations";
import { CustomDataGrid } from "../../../components/data-grids/CustomDataGrid";
import PageQueryWrapper from "../../../components/layouts/PageQueryWrapper";
import { useDeleteDialog } from "../../../context/DeleteDialogContext";
import { FORM_TYPES, useFormContainer } from "../../../context/FormContext";
import { locationsCacheKey } from "../../../data/cacheKeys";
import { locationColumns } from "../../../data/gridColumns";
import { useSearchableQuery } from "../../../data/hooks/useSearchableQuery";
import { ROUTE_DASHBOARD, ROUTE_LOCATION_DETAILS } from "../../../data/routeNames";
import GenericPageLayout from "../../../components/layouts/GenericPageLayout";
import { usePermissions } from "../../../permissions/usePermissions";
import { useEmployeeBranchRowGuard } from "../../../permissions/useEmployeeBranchRowGuard";
import { ENTITY_TYPES } from "../../../permissions/permissions";

export default function LocationsPage() {
    const navigate = useNavigate();
    const { canCreate, canDelete, canUpdate } = usePermissions(ENTITY_TYPES.LOCATION);
    const { isOtherBranch } = useEmployeeBranchRowGuard();
    const isUpdateDisabled = (rowId?: string | number) => !canUpdate || isOtherBranch(rowId);
    const { openForm } = useFormContainer();
    const { openConfirmDeleteWindow } = useDeleteDialog();
    const { deleteLocation: deleteLocationMutation } = useLocationMutations();

    const query = useSearchableQuery({
        queryKey: locationsCacheKey.list(),
        queryFn: () => locationService.getAll(),
        filterFn: (location, search) =>
            location.title.toLowerCase().includes(search.toLowerCase())
            || location.address.toLowerCase().includes(search.toLowerCase()),
    });

    const locationCountLabel = useMemo(() => {
        const count = query.filteredData.length;
        return count === 1 ? "1 succursale" : `${count} succursales`;
    }, [query.filteredData.length]);

    return (
        <PageQueryWrapper
            isLoading={query.isLoading}
            error={query.error}
            refetch={query.refetch}
            errorReturnUrl={ROUTE_DASHBOARD}
            errorReturnLabel="Retour au tableau de bord"
        >
            <GenericPageLayout
                title="Succursales"
                subtitle="Points de vente et inventaires par emplacement"
            >
                <Box
                    sx={{
                        display: "flex",
                        alignItems: "center",
                        justifyContent: "space-between",
                        gap: 2,
                        mb: 2,
                        flexWrap: "wrap",
                    }}
                >
                    <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                        <StorefrontOutlinedIcon color="primary" />
                        <Typography variant="body2" color="text.secondary">
                            {locationCountLabel} affichée{query.filteredData.length > 1 ? "s" : ""}
                        </Typography>
                    </Box>
                    <Chip
                        label="Cliquez sur une ligne pour ouvrir la fiche"
                        size="small"
                        variant="outlined"
                    />
                </Box>

                <CustomDataGrid
                    rows={query.filteredData}
                    columns={locationColumns}
                    addLabel="Ajouter une succursale"
                    onAddClick={canCreate ? () => openForm(FORM_TYPES.LOCATION) : undefined}
                    onEditClick={canUpdate ? (location) => openForm(FORM_TYPES.LOCATION, location) : undefined}
                    onDeleteClick={canDelete ? (location) => openConfirmDeleteWindow({
                        id: location.id,
                        displayLabel: location.title,
                        onDelete: deleteLocationMutation,
                    }) : undefined}
                    onRowClick={(params) => navigate(ROUTE_LOCATION_DETAILS.replace(":id", String(params.id)))}
                    isEditDisabledForRow={(row) => isUpdateDisabled(row.id)}
                    {...query.searchProps}
                />
            </GenericPageLayout>
        </PageQueryWrapper>
    );
}
