import { useState } from "react";
import { Box, Button, FormControl, InputLabel, MenuItem, Paper, Select, Typography, Grid, type SelectChangeEvent } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import { Link as RouterLink } from "react-router-dom";
import locationService from "../../api/services/inventory/locationService";
import GenericPageLayout from "../../components/layouts/GenericPageLayout";
import PageQueryWrapper from "../../components/layouts/PageQueryWrapper";
import DashboardWidget from "../../components/dashboard/DashboardWidget";
import StorefrontIcon from "@mui/icons-material/Storefront";
import AutoStoriesIcon from "@mui/icons-material/AutoStories";
import { locationsCacheKey } from "../../data/cacheKeys";
import { ROUTE_CATALOGUE, ROUTE_DASHBOARD, ROUTE_LOCATION_INVENTORY, ROUTE_LOCATIONS } from "../../data/routeNames";

export default function IRPage() {
    const [selectedLocationId, setSelectedLocationId] = useState<number>();

    const locationsQuery = useQuery({
        queryKey: locationsCacheKey.list(),
        queryFn: () => locationService.getAll(),
    });

    const handleLocationChange = (event: SelectChangeEvent<number>) => {
        setSelectedLocationId(Number(event.target.value));
    };

    const locations = locationsQuery.data ?? [];
    const effectiveLocationId = selectedLocationId ?? locations[0]?.id;
    const hasLocations = locations.length > 0;
    const selectedInventoryUrl = effectiveLocationId
        ? ROUTE_LOCATION_INVENTORY.replace(":id", String(effectiveLocationId))
        : ROUTE_LOCATIONS;

    return (
        <PageQueryWrapper
            isLoading={locationsQuery.isLoading}
            error={locationsQuery.error}
            refetch={locationsQuery.refetch}
            errorReturnUrl={ROUTE_DASHBOARD}
            errorReturnLabel="Retour au tableau de bord"
        >
            <GenericPageLayout title="Gestion de l'inventaire">
                <Box sx={{ display: "flex", flexDirection: "column", alignItems: "flex-start", gap: 3 }} flexGrow={1} width="100%">
                    {hasLocations ? (
                        <>
                            <Paper
                                variant="outlined"
                                sx={{
                                    display: "flex",
                                    flexDirection: "column",
                                    gap: 2,
                                    width: "100%",
                                    p: 2,
                                    borderRadius: 1,
                                }}
                            >
                                <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                                    Inventaire par succursale
                                </Typography>

                                <Box
                                    sx={{
                                        display: "flex",
                                        flexDirection: { xs: "column", sm: "row" },
                                        alignItems: "stretch",
                                        gap: 2,
                                        width: "100%",
                                    }}
                                >
                                    <FormControl
                                        sx={{
                                            width: "100%",
                                            flexBasis: { xs: "100%", sm: "75%" }
                                        }}
                                    >
                                        <InputLabel id="location-select-label">Succursale</InputLabel>
                                        <Select
                                            labelId="location-select-label"
                                            label="Succursale"
                                            value={effectiveLocationId ?? ""}
                                            onChange={handleLocationChange}
                                        >
                                            {locations.map((location) => (
                                                <MenuItem key={location.id} value={location.id}>
                                                    {location.title}
                                                </MenuItem>
                                            ))}
                                        </Select>
                                    </FormControl>

                                    <Button
                                        component={RouterLink}
                                        to={selectedInventoryUrl}
                                        variant="contained"
                                        sx={{
                                            alignSelf: "stretch",
                                            width: "100%",
                                            flexBasis: { xs: "100%", sm: "25%" },
                                            whiteSpace: "normal",
                                            textAlign: "center",
                                            lineHeight: 1.2,
                                            minHeight: 56,
                                            px: 2,
                                            py: 1,
                                            fontSize: "0.95rem",
                                            fontWeight: 600,
                                        }}
                                    >
                                        Consulter l'inventaire
                                    </Button>
                                </Box>
                            </Paper>

                            <Grid container spacing={2} sx={{ width: "100%" }}>
                                <Grid size={{ xs: 12, sm: 6 }} flexGrow={1}>
                                    <DashboardWidget
                                        title="Succursales"
                                        value=""
                                        subtitle="Consulter la liste des succursales actives"
                                        icon={<StorefrontIcon color="primary" fontSize="large" />}
                                        to={ROUTE_LOCATIONS}
                                    />
                                </Grid>
                                <Grid size={{ xs: 12, sm: 6 }} flexGrow={1}>
                                    <DashboardWidget
                                        title="Catalogue"
                                        value=""
                                        subtitle="Consulter les items dans le catalogue"
                                        icon={<AutoStoriesIcon color="secondary" fontSize="large" />}
                                        to={ROUTE_CATALOGUE}
                                    />
                                </Grid>
                            </Grid>
                        </>
                    ) : (
                        <>
                            <Typography>Aucune succursale disponible.</Typography>
                            <Grid container spacing={2} sx={{ width: "100%" }}>
                                <Grid size={{ xs: 12, sm: 6 }} flexGrow={1}>
                                    <DashboardWidget
                                        title="Succursales"
                                        value=""
                                        subtitle="Liste des succursales"
                                        icon={<StorefrontIcon color="primary" fontSize="large" />}
                                        to={ROUTE_LOCATIONS}
                                    />
                                </Grid>
                                <Grid size={{ xs: 12, sm: 6 }} flexGrow={1}>
                                    <DashboardWidget
                                        title="Catalogue"
                                        value=""
                                        subtitle="Consulter les articles"
                                        icon={<AutoStoriesIcon color="secondary" fontSize="large" />}
                                        to={ROUTE_CATALOGUE}
                                    />
                                </Grid>
                            </Grid>
                        </>
                    )}
                </Box>
            </GenericPageLayout>
        </PageQueryWrapper>
    );
}