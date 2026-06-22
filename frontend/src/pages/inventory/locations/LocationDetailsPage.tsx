import {
    Box,
    Button,
    Chip,
    Divider,
    Grid,
    Paper,
    Stack,
    Typography,
} from "@mui/material";
import StorefrontOutlinedIcon from "@mui/icons-material/StorefrontOutlined";
import LocationOnOutlinedIcon from "@mui/icons-material/LocationOnOutlined";
import Inventory2OutlinedIcon from "@mui/icons-material/Inventory2Outlined";
import ArrowForwardIcon from "@mui/icons-material/ArrowForward";
import { useQuery } from "@tanstack/react-query";
import { useNavigate, useParams } from "react-router-dom";
import inventoryQuantityService from "../../../api/services/inventory/inventoryQuantityService";
import locationService from "../../../api/services/inventory/locationService";
import GenericPageLayout from "../../../components/layouts/GenericPageLayout";
import PageQueryWrapper from "../../../components/layouts/PageQueryWrapper";
import { inventoryQuantityCacheKey, locationsCacheKey } from "../../../data/cacheKeys";
import { ROUTE_LOCATIONS, buildLocationInventoryPath } from "../../../data/routeNames";
import { FORM_TYPES, useFormContainer } from "../../../context/FormContext";
import { useDeleteDialog } from "../../../context/DeleteDialogContext";
import { useLocationMutations } from "../../../api/mutations/inventory/useLocationMutations";
import { usePermissions } from "../../../permissions/usePermissions";
import { useEmployeeBranchRowGuard } from "../../../permissions/useEmployeeBranchRowGuard";
import { ENTITY_TYPES } from "../../../permissions/permissions";

function StatTile({
    label,
    value,
    helper,
}: {
    label: string;
    value: string | number;
    helper?: string;
}) {
    return (
        <Box sx={{ textAlign: "left", minWidth: 0 }}>
            <Typography variant="overline" color="text.secondary" sx={{ lineHeight: 1.2 }}>
                {label}
            </Typography>
            <Typography variant="h5" fontWeight={700} sx={{ mt: 0.5 }}>
                {value}
            </Typography>
            {helper && (
                <Typography variant="caption" color="text.secondary">
                    {helper}
                </Typography>
            )}
        </Box>
    );
}

export default function LocationDetailsPage() {
    const navigate = useNavigate();
    const { id } = useParams();
    const numericId = Number(id);
    const { canUpdate, canDelete } = usePermissions(ENTITY_TYPES.LOCATION);
    const { isOtherBranch } = useEmployeeBranchRowGuard();
    const isUpdateDisabled = (rowId?: string | number) => !canUpdate || isOtherBranch(rowId);
    const isDeleteDisabled = (rowId?: string | number) => !canDelete || isOtherBranch(rowId);
    const { openConfirmDeleteWindow } = useDeleteDialog();
    const { openForm } = useFormContainer();
    const { deleteLocation: deleteLocationMutation, isDeletingLocation } = useLocationMutations();

    const locationQuery = useQuery({
        queryKey: locationsCacheKey.details(id!),
        queryFn: () => locationService.getById(id!),
        enabled: !!id,
    });
    const location = locationQuery.data;

    const inventoryQuery = useQuery({
        queryKey: inventoryQuantityCacheKey.locationLines(numericId),
        queryFn: () => inventoryQuantityService.getLinesByLocation(numericId),
        enabled: !!id && !Number.isNaN(numericId),
    });

    const inventoryLines = inventoryQuery.data ?? [];
    const stockedItemCount = inventoryLines.length;
    const totalUnits = inventoryLines.reduce((total, line) => total + line.quantity, 0);

    const handleOpenInventory = (): void => {
        if (id) {
            navigate(buildLocationInventoryPath(id));
        }
    };

    const isPageLoading = locationQuery.isLoading || inventoryQuery.isLoading;
    const hasPageError = locationQuery.error
        || (!location && !locationQuery.isLoading ? { message: "Succursale introuvable" } : null);

    return (
        <PageQueryWrapper
            isLoading={isPageLoading}
            error={hasPageError}
            refetch={() => {
                locationQuery.refetch();
                inventoryQuery.refetch();
            }}
            errorReturnUrl={ROUTE_LOCATIONS}
            errorReturnLabel="Retour aux succursales"
        >
            {location && id && (
                <GenericPageLayout
                    title={location.title}
                    subtitle="Fiche succursale"
                    onEditClick={!isUpdateDisabled(id) ? () => openForm(FORM_TYPES.LOCATION, location) : undefined}
                    onDeleteClick={!isDeleteDisabled(id) ? () => openConfirmDeleteWindow({
                        id: location.id,
                        displayLabel: location.title,
                        onDelete: deleteLocationMutation,
                        isDeleting: isDeletingLocation,
                        redirectUrl: ROUTE_LOCATIONS,
                    }) : undefined}
                >
                    <Stack spacing={3} sx={{ width: "100%", textAlign: "left" }}>
                        <Paper
                            variant="outlined"
                            sx={{
                                p: { xs: 2, md: 2.5 },
                                borderRadius: 2,
                                background: (theme) => theme.palette.mode === "dark"
                                    ? "linear-gradient(135deg, rgba(25, 118, 210, 0.12) 0%, rgba(0,0,0,0) 55%)"
                                    : "linear-gradient(135deg, rgba(25, 118, 210, 0.06) 0%, rgba(255,255,255,1) 55%)",
                            }}
                        >
                            <Stack
                                direction="row"
                                spacing={2}
                                alignItems="center"
                            >
                                <Box
                                    sx={{
                                        width: 56,
                                        height: 56,
                                        borderRadius: 2,
                                        display: "flex",
                                        alignItems: "center",
                                        justifyContent: "center",
                                        bgcolor: "primary.main",
                                        color: "primary.contrastText",
                                        flexShrink: 0,
                                    }}
                                >
                                    <StorefrontOutlinedIcon fontSize="large" />
                                </Box>
                                <Box>
                                    <Stack direction="row" spacing={1} alignItems="center" flexWrap="wrap" useFlexGap>
                                        <Typography variant="h5" fontWeight={700}>
                                            {location.title}
                                        </Typography>
                                        <Chip label="Succursale active" size="small" color="success" variant="outlined" />
                                    </Stack>
                                    <Stack direction="row" spacing={0.75} alignItems="center" sx={{ mt: 0.75 }}>
                                        <LocationOnOutlinedIcon sx={{ fontSize: 18, color: "text.secondary" }} />
                                        <Typography variant="body2" color="text.secondary">
                                            {location.address}
                                        </Typography>
                                    </Stack>
                                </Box>
                            </Stack>

                            <Divider sx={{ my: 2 }} />

                            <Grid container spacing={3}>
                                <Grid size={{ xs: 12, sm: 4 }}>
                                    <StatTile
                                        label="Articles en stock"
                                        value={stockedItemCount}
                                        helper={stockedItemCount === 0 ? "Aucun article en rayon" : "Références distinctes"}
                                    />
                                </Grid>
                                <Grid size={{ xs: 12, sm: 4 }}>
                                    <StatTile
                                        label="Unités totales"
                                        value={totalUnits}
                                        helper="Somme des quantités"
                                    />
                                </Grid>
                                <Grid size={{ xs: 12, sm: 4 }}>
                                    <StatTile
                                        label="Identifiant"
                                        value={`#${location.id}`}
                                        helper="Référence interne"
                                    />
                                </Grid>
                            </Grid>
                        </Paper>

                        <Grid container spacing={3} alignItems="stretch">
                            <Grid size={{ xs: 12, md: 7 }}>
                                <Paper variant="outlined" sx={{ p: 2.5, borderRadius: 2, height: "100%" }}>
                                    <Typography variant="h6" sx={{ mb: 2 }}>
                                        Informations
                                    </Typography>
                                    <Stack spacing={2.5}>
                                        <Box>
                                            <Typography variant="overline" color="text.secondary">
                                                Adresse complète
                                            </Typography>
                                            <Typography variant="body1">
                                                {location.address}
                                            </Typography>
                                        </Box>
                                        <Box>
                                            <Typography variant="overline" color="text.secondary">
                                                Description
                                            </Typography>
                                            <Typography
                                                variant="body1"
                                                color={location.description?.trim() ? "text.primary" : "text.secondary"}
                                                sx={!location.description?.trim() ? { fontStyle: "italic" } : undefined}
                                            >
                                                {location.description?.trim() || "Aucune description pour cette succursale."}
                                            </Typography>
                                        </Box>
                                    </Stack>
                                </Paper>
                            </Grid>

                            <Grid size={{ xs: 12, md: 5 }}>
                                <Paper
                                    variant="outlined"
                                    sx={{
                                        p: 2.5,
                                        borderRadius: 2,
                                        height: "100%",
                                        display: "flex",
                                        flexDirection: "column",
                                        gap: 2,
                                    }}
                                >
                                    <Typography variant="h6">
                                        Actions rapides
                                    </Typography>
                                    <Typography variant="body2" color="text.secondary">
                                        Consultez les stocks, ajustez les quantités ou ajoutez de nouveaux articles au rayon de cette succursale.
                                    </Typography>

                                    <Button
                                        variant="outlined"
                                        startIcon={<Inventory2OutlinedIcon />}
                                        endIcon={<ArrowForwardIcon />}
                                        onClick={handleOpenInventory}
                                        sx={{ mt: "auto", justifyContent: "space-between", py: 1.25 }}
                                    >
                                        Gérer l'inventaire
                                    </Button>
                                </Paper>
                            </Grid>
                        </Grid>
                    </Stack>
                </GenericPageLayout>
            )}
        </PageQueryWrapper>
    );
}
