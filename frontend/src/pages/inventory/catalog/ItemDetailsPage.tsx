import { Box, Chip, Divider, Paper, Stack, Typography } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import type { ReactNode } from "react";
import { useParams } from "react-router-dom";
import { useItemMutations } from "../../../api/mutations/inventory/useItemMutations";
import inventoryQuantityService from "../../../api/services/inventory/inventoryQuantityService";
import itemService from "../../../api/services/inventory/itemService";
import ItemInventoryQuantitiesTable from "../../../components/data-grids/ItemInventoryQuantityTable";
import GenericPageLayout from "../../../components/layouts/GenericPageLayout";
import PageQueryWrapper from "../../../components/layouts/PageQueryWrapper";
import { useDeleteDialog } from "../../../context/DeleteDialogContext";
import { FORM_TYPES, useFormContainer } from "../../../context/FormContext";
import { inventoryQuantityCacheKey, itemsCacheKey } from "../../../data/cacheKeys";
import { ROUTE_CATALOGUE } from "../../../data/routeNames";
import ItemImageDisplay from "../../../components/images/ItemImageDisplay";
import { usePermissions } from "../../../permissions/usePermissions";
import { useScopedPermissions } from "../../../permissions/useScopedPermissions";
import { ENTITY_TYPES } from "../../../permissions/permissions";
import type { InventoryQuantityFormData } from "../../../data/types/inventory/inventoryQuantity";

function DetailRow({
    label,
    value,
    emptyText,
}: {
    label: string;
    value: ReactNode;
    emptyText?: string;
}) {
    const isEmpty = value === null || value === undefined || value === "";

    return (
        <Box sx={{ textAlign: "left" }}>
            <Typography variant="overline" color="text.secondary">
                {label}
            </Typography>
            {isEmpty && emptyText ? (
                <Typography variant="body1" color="text.secondary" sx={{ fontStyle: "italic" }}>
                    {emptyText}
                </Typography>
            ) : (
                <Typography variant="body1">
                    {value}
                </Typography>
            )}
        </Box>
    );
}

function formatPrice(price: number) {
    return new Intl.NumberFormat("fr-CA", {
        style: "currency",
        currency: "CAD",
    }).format(price);
}

function formatDate(value?: string | null) {
    if (!value) return null;

    return new Intl.DateTimeFormat("fr-CA", {
        year: "numeric",
        month: "long",
        day: "numeric",
    }).format(new Date(value));
}

function ChipList({ values, emptyText }: { values?: string[]; emptyText: string }) {
    if (!values?.length) {
        return (
            <Typography variant="body1" color="text.secondary" sx={{ fontStyle: "italic" }}>
                {emptyText}
            </Typography>
        );
    }

    return (
        <Stack direction="row" spacing={1} useFlexGap flexWrap="wrap">
            {values.map((value) => (
                <Chip key={value} label={value} size="small" />
            ))}
        </Stack>
    );
}

export default function ItemDetailsPage() {
    const { id } = useParams();
    const { openForm } = useFormContainer();

    const { openConfirmDeleteWindow } = useDeleteDialog();
    const { deleteItem: deleteItemMutation } = useItemMutations();

    const { canUpdate: canUpdateItem, canDelete: canDeleteItem } = usePermissions(ENTITY_TYPES.ITEM);
    const { canUpdateInventoryAnywhere } = useScopedPermissions();

    const itemQuery = useQuery({
        queryKey: itemsCacheKey.details(id!),
        queryFn: () => itemService.getById(id!),
        enabled: !!id,
    });
    const item = itemQuery.data;

    const inventoryLinesQuery = useQuery({
        queryKey: inventoryQuantityCacheKey.itemLines(Number(item?.id)),
        queryFn: () => inventoryQuantityService.getLinesByItem(Number(item!.id)),
        enabled: !!item,
    });

    const inventoryLines = inventoryLinesQuery.data ?? [];
    const displayedTotalQuantity = inventoryLines.reduce((total, line) => total + line.quantity, 0);
    const hasInventory = inventoryLines.length > 0;
    const isDisplayedLowStock = hasInventory && item
        ? displayedTotalQuantity <= item.alertQuantity
        : false;

    const isPageLoading = itemQuery.isLoading || inventoryLinesQuery.isLoading;
    const hasPageError = itemQuery.isError || inventoryLinesQuery.isError;

    const handleAddToStock = canUpdateInventoryAnywhere
        ? () => openForm(FORM_TYPES.QUANTITY, {
            mode: "add",
            fixedItemId: Number(item!.id),
            itemId: item!.id,
            itemName: item!.name,
        } satisfies InventoryQuantityFormData)
        : undefined;

    return (
        <PageQueryWrapper
            isLoading={isPageLoading}
            error={hasPageError || (!item && !itemQuery.isLoading ? { message: "Item introuvable" } : null)}
            refetch={() => {
                itemQuery.refetch();
                inventoryLinesQuery.refetch();
            }}
            errorReturnUrl={ROUTE_CATALOGUE}
            errorReturnLabel="Retour au catalogue"
        >
            {item && (
                <GenericPageLayout
                    title={item.name}
                    subtitle={item.isBook ? "Livre" : "Produit"}
                    onEditClick={canUpdateItem ? () => openForm(FORM_TYPES.ITEM, item) : undefined}
                    onDeleteClick={canDeleteItem ? () => openConfirmDeleteWindow({
                        id: item.id,
                        displayLabel: item.name,
                        onDelete: deleteItemMutation,
                        redirectUrl: ROUTE_CATALOGUE,
                    }) : undefined}
                >
                    <Box sx={{ display: "flex", flexDirection: "column", alignItems: "stretch", gap: 3, textAlign: "left" }}>
                        <Paper variant="outlined" sx={{ p: 2.5, borderRadius: 1 }}>
                            <Stack
                                direction={{ xs: "column", md: "row" }}
                                spacing={2}
                                divider={<Divider orientation="vertical" flexItem />}
                            >
                                <ItemImageDisplay
                                    name={"image"}
                                    isBook={item.isBook}
                                    imageUrl={item.imageUrl ?? null}
                                />
                                <DetailRow label="Prix" value={formatPrice(item.price)} />
                                <DetailRow label="Quantité totale" value={String(displayedTotalQuantity)} />
                                <DetailRow label="Seuil d'alerte" value={item.alertQuantity} />
                                <Box>
                                    <Typography variant="overline" color="text.secondary" sx={{ textAlign: "left" }}>
                                        État du stock
                                    </Typography>
                                    <Box sx={{ textAlign: "left" }}>
                                        {!hasInventory ? (
                                            <Chip label="Hors inventaire" color="default" size="small" />
                                        ) : (
                                            <Chip
                                                label={isDisplayedLowStock ? "Stock bas" : "Stock correct"}
                                                color={isDisplayedLowStock ? "warning" : "success"}
                                                size="small"
                                            />
                                        )}
                                    </Box>
                                </Box>
                            </Stack>
                        </Paper>

                        <Box
                            sx={{
                                display: "grid",
                                gridTemplateColumns: { xs: "1fr", lg: "minmax(320px, 1fr) minmax(420px, 1.1fr)" },
                                alignItems: "start",
                                gap: 3,
                            }}
                        >
                            <Paper variant="outlined" sx={{ p: 2.5, borderRadius: 1 }}>
                                <Typography variant="h6" sx={{ mb: 2, textAlign: "left" }}>
                                    {item.isBook ? "Détails du livre" : "Détails du produit"}
                                </Typography>

                                <Box sx={{ display: "grid", gridTemplateColumns: "1fr", gap: 2 }}>
                                    {!item.isBook && (
                                        <DetailRow
                                            label="Distributeur"
                                            value={item.distributor}
                                            emptyText="Aucun distributeur indiqué"
                                        />
                                    )}
                                    {item.isBook && (
                                        <>
                                            <DetailRow label="Auteur(s)" value={<ChipList values={item.authors} emptyText="Aucun auteur indiqué" />} />
                                            <DetailRow label="ISBN" value={item.isbn} emptyText="Aucun ISBN indiqué" />
                                            <DetailRow
                                                label="Date de publication"
                                                value={formatDate(item.publicationDate)}
                                                emptyText="Date de publication non précisée"
                                            />
                                            <DetailRow label="Catégorie(s)" value={<ChipList values={item.categories} emptyText="Aucune catégorie associée" />} />
                                            <DetailRow label="Éditeur" value={<ChipList values={item.publishers} emptyText="Aucun éditeur indiqué" />} />
                                        </>
                                    )}
                                </Box>

                                <Divider sx={{ my: 2 }} />
                                <DetailRow
                                    label="Description"
                                    value={item.description}
                                    emptyText="Aucune description pour cet article"
                                />
                            </Paper>

                            <Box sx={{ width: "100%", minWidth: 0 }}>
                                <Typography variant="h6" sx={{ mb: 2, textAlign: "left" }}>
                                    Inventaire par succursale
                                </Typography>
                                <ItemInventoryQuantitiesTable
                                    inventoryLines={inventoryLines}
                                    itemId={item.id}
                                    itemName={item.name}
                                    onAddClick={canUpdateInventoryAnywhere ? handleAddToStock : undefined}
                                />
                            </Box>
                        </Box>
                    </Box>
                </GenericPageLayout>
            )}
        </PageQueryWrapper>
    );
}
