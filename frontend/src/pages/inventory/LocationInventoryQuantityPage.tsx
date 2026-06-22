import { Typography } from "@mui/material";

import { useNavigate, useParams } from "react-router-dom";

import { locationInventoryColumns, type LocationInventoryRow } from "../../data/gridColumns";

import { useSearchableQuery } from "../../data/hooks/useSearchableQuery";

import PageQueryWrapper from "../../components/layouts/PageQueryWrapper";

import { ROUTE_DASHBOARD, ROUTE_ITEM_DETAILS } from "../../data/routeNames";

import { CustomDataGrid } from "../../components/data-grids/CustomDataGrid";

import { inventoryQuantityCacheKey, locationsCacheKey } from "../../data/cacheKeys";

import locationService from "../../api/services/inventory/locationService";

import { useQuery } from "@tanstack/react-query";

import GenericPageLayout from "../../components/layouts/GenericPageLayout";

import { FORM_TYPES, useFormContainer } from "../../context/FormContext";

import itemService from "../../api/services/inventory/itemService";

import inventoryQuantityService from "../../api/services/inventory/inventoryQuantityService";

import type { InventoryQuantityFormData } from "../../data/types/inventory/inventoryQuantity";

import { useScopedPermissions } from "../../permissions/useScopedPermissions";

import LoadingSpinner from "../../components/LoadingSpinner";



export default function LocationInventoryQuantityPage() {

    const navigate = useNavigate();

    const { canUpdateInventoryOnLocation } = useScopedPermissions();



    const { id } = useParams();

    const numericLocationId = Number(id);

    const hasValidLocationId = !!id && !Number.isNaN(numericLocationId);



    const locationQuery = useQuery({

        queryKey: locationsCacheKey.details(String(id)),

        queryFn: () => locationService.getById(id!),

        enabled: hasValidLocationId,

    });

    const location = locationQuery.data;



    const itemQuery = useSearchableQuery({

        queryKey: inventoryQuantityCacheKey.locationGrid(numericLocationId),

        queryFn: async (): Promise<LocationInventoryRow[]> => {

            const [lines, items] = await Promise.all([

                inventoryQuantityService.getLinesByLocation(numericLocationId),

                itemService.getAll(),

            ]);



            const itemById = new Map(items.map((item) => [String(item.id), item]));



            return lines

                .filter((line) => itemById.has(String(line.itemId)))

                .map((line) => ({

                    id: line.itemId,

                    name: line.itemName,

                    isBook: itemById.get(String(line.itemId))?.isBook ?? false,

                    quantity: line.quantity,

                    quantityRecordId: `${line.locationId}-${line.itemId}`,

                }));

        },

        filterFn: (item, search) => (item.name ?? "").toLowerCase().includes(search.toLowerCase()),

        enabled: hasValidLocationId,

    });



    const isLocationNotFound = locationQuery.isFetched

        && !locationQuery.isFetching

        && !location;

    const isPageLoading = !hasValidLocationId

        || locationQuery.isLoading

        || itemQuery.isLoading;

    const pageError = !hasValidLocationId

        ? { message: "Identifiant de succursale invalide." }

        : locationQuery.error

        ?? itemQuery.error

        ?? (isLocationNotFound ? { message: "Cette succursale n'a pas été trouvée." } : null);



    const { openForm } = useFormContainer();

    const canManageLocationInventory = hasValidLocationId

        && canUpdateInventoryOnLocation(numericLocationId);



    if (!hasValidLocationId) {

        return <LoadingSpinner />;

    }



    return (

        <PageQueryWrapper

            isLoading={isPageLoading}

            error={pageError}

            refetch={() => {

                locationQuery.refetch();

                itemQuery.refetch();

            }}

            errorReturnUrl={ROUTE_DASHBOARD}

            errorReturnLabel="Retour au tableau de bord"

            customErrorMessage={isLocationNotFound ? "Cette succursale n'a pas été trouvée." : undefined}

        >

            <GenericPageLayout

                title={`Inventaire de ${location?.title ?? "Inconnue"}`}

            >

                {itemQuery.filteredData.length === 0 && !itemQuery.isLoading && (

                    <Typography variant="body2" color="text.secondary" sx={{ mb: 2, textAlign: "left" }}>

                        Aucun article en stock dans cette succursale. Utilisez « Ajouter au stock » pour mettre un article en rayon.

                    </Typography>

                )}

                <CustomDataGrid

                    rows={itemQuery.filteredData}

                    columns={locationInventoryColumns}

                    onRowClick={(params) => navigate(ROUTE_ITEM_DETAILS.replace(":id", String(params.id)))}

                    onEditClick={canManageLocationInventory ? (row: LocationInventoryRow) => {

                        openForm(FORM_TYPES.QUANTITY, {

                            mode: "edit",

                            id: row.quantityRecordId,

                            itemId: row.id,

                            locationId: numericLocationId,

                            quantity: row.quantity,

                            itemName: row.name,

                            locationName: location?.title ?? "Inconnue",

                        } satisfies InventoryQuantityFormData);

                    } : undefined}

                    isEditDisabledForRow={() => !canManageLocationInventory}

                    onAddClick={canManageLocationInventory ? () => openForm(FORM_TYPES.QUANTITY, {

                        mode: "add",

                        fixedLocationId: numericLocationId,

                        locationId: numericLocationId,

                        locationName: location?.title ?? "Inconnue",

                    } satisfies InventoryQuantityFormData) : undefined}

                    addLabel="Ajouter au stock"

                    {...itemQuery.searchProps}

                />

            </GenericPageLayout>

        </PageQueryWrapper>

    );

}


