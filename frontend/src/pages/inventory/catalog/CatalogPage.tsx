import itemService from "../../../api/services/inventory/itemService";
import { itemsCacheKey } from "../../../data/cacheKeys";
import { ROUTE_IR, ROUTE_ITEM_DETAILS } from "../../../data/routeNames";
import { useSearchableQuery } from "../../../data/hooks/useSearchableQuery";
import PageQueryWrapper from "../../../components/layouts/PageQueryWrapper";
import { FORM_TYPES, useFormContainer } from "../../../context/FormContext";
import { itemColumns } from "../../../data/gridColumns";
import { CustomDataGrid } from "../../../components/data-grids/CustomDataGrid";
import { useNavigate } from "react-router-dom";
import { useDeleteDialog } from "../../../context/DeleteDialogContext";
import { useItemMutations } from "../../../api/mutations/inventory/useItemMutations";
import GenericPageLayout from "../../../components/layouts/GenericPageLayout";
import { Box, Button, Collapse, FormControl, InputLabel, MenuItem, Paper, Select, Typography, type SelectChangeEvent } from "@mui/material";
import { useMemo, useState } from "react";
import { usePermissions } from "../../../permissions/usePermissions";
import { ENTITY_TYPES } from "../../../permissions/permissions";
import ExpandLessIcon from "@mui/icons-material/ExpandLess";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";

const ALL_FILTER_VALUE = "all";
const filterFormControlSx = {
	width: "100%",
	minWidth: 0,
	"& .MuiInputLabel-root": {
		color: "text.secondary",
	},
	"& .MuiInputLabel-root.Mui-focused": {
		color: "text.primary",
	},
	"& .MuiOutlinedInput-root": {
		color: "text.primary",
	},
	"& .MuiOutlinedInput-notchedOutline": {
		borderColor: "divider",
	},
	"& .MuiOutlinedInput-root:hover .MuiOutlinedInput-notchedOutline": {
		borderColor: "text.secondary",
	},
	"& .MuiOutlinedInput-root.Mui-focused .MuiOutlinedInput-notchedOutline": {
		borderColor: "text.primary",
	},
	"& .MuiSelect-icon": {
		color: "text.secondary",
	},
};

export default function CatalogPage() {
	const navigate = useNavigate();



	const { canCreate, canUpdate, canDelete } = usePermissions(ENTITY_TYPES.ITEM);



	const { openForm } = useFormContainer();
	const { openConfirmDeleteWindow } = useDeleteDialog();
	const { deleteItem: deleteItemMutation } = useItemMutations();
	const [publisherFilter, setPublisherFilter] = useState(ALL_FILTER_VALUE);
	const [distributorFilter, setDistributorFilter] = useState(ALL_FILTER_VALUE);
	const [categoryFilter, setCategoryFilter] = useState(ALL_FILTER_VALUE);
	const [showBookFilters, setShowBookFilters] = useState(false);

	const query = useSearchableQuery({
		queryKey: itemsCacheKey.list(),
		queryFn: () => itemService.getAll(),
		filterFn: (item, search) => item.name.toLowerCase().includes(search.toLowerCase())
	});

	const publisherOptions = useMemo(() => {
		const values = (query.data ?? [])
			.filter((item) => item.isBook)
			.flatMap((item) => item.publishers ?? []);

		return Array.from(new Set(values)).sort((a, b) => a.localeCompare(b));
	}, [query.data]);

	const distributorOptions = useMemo(() => {
		const values = (query.data ?? [])
			.filter((item) => !item.isBook)
			.flatMap((item) => item.distributor ? [item.distributor] : []);

		return Array.from(new Set(values)).sort((a, b) => a.localeCompare(b));
	}, [query.data]);

	const categoryOptions = useMemo(() => {
		const values = (query.data ?? [])
			.filter((item) => item.isBook)
			.flatMap((item) => item.categories ?? []);

		return Array.from(new Set(values)).sort((a, b) => a.localeCompare(b));
	}, [query.data]);

	const filteredItems = query.filteredData.filter((item) => {
		const matchesPublisher = publisherFilter === ALL_FILTER_VALUE
			|| (item.isBook && (item.publishers ?? []).includes(publisherFilter));

		const matchesDistributor = distributorFilter === ALL_FILTER_VALUE
			|| (!item.isBook && item.distributor === distributorFilter);

		const matchesCategory = categoryFilter === ALL_FILTER_VALUE
			|| (item.isBook && (item.categories ?? []).includes(categoryFilter));

		return matchesPublisher && matchesDistributor && matchesCategory;
	});

	const handlePublisherChange = (event: SelectChangeEvent) => {
		setPublisherFilter(event.target.value);
	};

	const handleDistributorChange = (event: SelectChangeEvent) => {
		setDistributorFilter(event.target.value);
	};

	const handleCategoryChange = (event: SelectChangeEvent) => {
		setCategoryFilter(event.target.value);
	};

	return (
		<>
			<PageQueryWrapper
				isLoading={query.isLoading}
				error={query.error}
				refetch={query.refetch}
				errorReturnUrl={ROUTE_IR}
				errorReturnLabel="Retour à la page de gestion d'inventaire"
			>
				<GenericPageLayout
					title="Catalogue"
				>
					<Box sx={{ mb: 2, display: "flex", flexDirection: "column", alignItems: "flex-start", gap: 1.5, width: "100%", minWidth: 0 }}>
						<Button
							variant="text"
							onClick={() => setShowBookFilters((current) => !current)}
							startIcon={showBookFilters ? <ExpandLessIcon /> : <ExpandMoreIcon />}
							sx={{
								fontWeight: 500,
								color: "text.secondary",
								textTransform: "none",
								"&:hover": {
									color: "text.primary",
									bgcolor: "action.hover",
								},
							}}
						>
							{showBookFilters ? "Masquer les filtres supplémentaires" : "Afficher les filtres supplémentaires"}
						</Button>

						<Collapse in={showBookFilters} sx={{ width: "100%" }}>
							<Paper
								variant="outlined"
								sx={{
									display: "flex",
									flexDirection: "column",
									gap: 2,
									p: 2,
									borderRadius: 1,
									width: "100%",
									minWidth: 0,
									boxSizing: "border-box",
								}}
							>
								<Typography variant="subtitle1" sx={{ fontWeight: 600, textAlign: "left", overflowWrap: "anywhere" }}>
									Filtres
								</Typography>

								<Box
									sx={{
										display: "grid",
										gridTemplateColumns: { xs: "1fr", md: "repeat(auto-fit, minmax(180px, 1fr))" },
										gap: 2,
										width: "100%",
										minWidth: 0,
									}}
								>
									<FormControl size="small" sx={filterFormControlSx}>
										<InputLabel id="publisher-filter-label">Éditeur</InputLabel>
										<Select
											labelId="publisher-filter-label"
											label="Éditeur"
											value={publisherFilter}
											onChange={handlePublisherChange}
										>
											<MenuItem value={ALL_FILTER_VALUE}>Tous</MenuItem>
											{publisherOptions.map((option) => (
												<MenuItem key={option} value={option}>
													{option}
												</MenuItem>
											))}
										</Select>
									</FormControl>

									<FormControl size="small" sx={filterFormControlSx}>
										<InputLabel id="distributor-filter-label">Distributeur</InputLabel>
										<Select
											labelId="distributor-filter-label"
											label="Distributeur"
											value={distributorFilter}
											onChange={handleDistributorChange}
										>
											<MenuItem value={ALL_FILTER_VALUE}>Tous</MenuItem>
											{distributorOptions.map((option) => (
												<MenuItem key={option} value={option}>
													{option}
												</MenuItem>
											))}
										</Select>
									</FormControl>

									<FormControl size="small" sx={filterFormControlSx}>
										<InputLabel id="category-filter-label">Catégorie de livre</InputLabel>
										<Select
											labelId="category-filter-label"
											label="Catégorie de livre"
											value={categoryFilter}
											onChange={handleCategoryChange}
										>
											<MenuItem value={ALL_FILTER_VALUE}>Toutes</MenuItem>
											{categoryOptions.map((option) => (
												<MenuItem key={option} value={option}>
													{option}
												</MenuItem>
											))}
										</Select>
									</FormControl>
								</Box>
							</Paper>
						</Collapse>
					</Box>

					<CustomDataGrid
						rows={filteredItems}
						columns={itemColumns}
						addLabel="Ajouter un item"
						onAddClick={canCreate ? () => openForm(FORM_TYPES.ITEM) : undefined}
						onEditClick={canUpdate ? (item) => openForm(FORM_TYPES.ITEM, item) : undefined}
						onDeleteClick={canDelete ? (item) => openConfirmDeleteWindow({
							id: item.id,
							displayLabel: item.name,
							onDelete: deleteItemMutation
						}) : undefined}
						onRowClick={(item) => navigate(ROUTE_ITEM_DETAILS.replace(":id", String(item.id)))}
						{...query.searchProps}
					/>
				</GenericPageLayout>
			</PageQueryWrapper>

		</>
	);

}
