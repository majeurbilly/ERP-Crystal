import { Box, Typography, useMediaQuery, useTheme } from "@mui/material";
import { DataGrid, type DataGridProps, type GridColDef, type GridValidRowModel } from "@mui/x-data-grid";
import SearchBar from "../search/SearchBar";
import { AddButton, RowActionButtons, type RowActionButton } from "../buttons/AddEditDeleteButtons";
import { getInventoryDisplayColor } from "../../data/utils/inventoryHelpers";

export type DataGridAction<Row extends GridValidRowModel> = RowActionButton<Row>;
interface CustomDataGridProps<Row extends GridValidRowModel> extends Omit<DataGridProps<Row>, "rows" | "columns"> {
	rows: Row[];
	columns: GridColDef<Row>[];
	actions?: DataGridAction<Row>[];
	showSearch?: boolean;
	searchValue?: string;
	onSearchChange?: (value: string) => void;
	addLabel?: string;
	onAddClick?: () => void;
	onEditClick?: (row: Row) => void;
	onDeleteClick?: (row: Row) => void;
	isEditDisabledForRow?: (row: Row) => boolean;
	isDeleteDisabledForRow?: (row: Row) => boolean;
}

export function CustomDataGrid<Row extends GridValidRowModel>({
	rows,
	columns,
	actions,
	showSearch = false,
	searchValue = "",
	onSearchChange,
	addLabel,
	onAddClick,
	onEditClick,
	onDeleteClick,
	isEditDisabledForRow: isEditDisabled,
	isDeleteDisabledForRow: isDeleteDisabled,
	onRowClick,
	sx,
	disableColumnMenu,
	getRowHeight,
	initialState,
	pageSizeOptions,
	...props
}: CustomDataGridProps<Row>) {
	const theme = useTheme();
	const isCompact = useMediaQuery(theme.breakpoints.down(1024));
	const useCompactLayout = isCompact;

	const actionColumn: GridColDef<Row> = {
		field: "actions",
		headerName: "Actions",
		sortable: false,
		filterable: false,
		width: isCompact ? 128 : 140,
		align: "center",
		headerAlign: "center",
		renderCell: (params) => {
			const isRowEditDisabled = isEditDisabled?.(params.row) ?? false;
			const isRowDeleteDisabled = isDeleteDisabled?.(params.row) ?? false;

			const dynamicRowActions: DataGridAction<Row>[] = actions ?? [
				...(onEditClick && !isRowEditDisabled
					? [{ type: "edit" as const, onClick: onEditClick }]
					: []
				),
				...(onDeleteClick && !isRowDeleteDisabled
					? [{ type: "delete" as const, onClick: onDeleteClick }]
					: []
				),
			];

			return (
				<Box
					onClick={(event) => event.stopPropagation()}
					onMouseDown={(event) => event.stopPropagation()}
				>
					<RowActionButtons
						row={params.row}
						actions={dynamicRowActions}
						compact={isCompact}
					/>
				</Box>
			);
		},
	};

	const getColumnValue = (row: Row, column: GridColDef<Row>) => {
		const rawValue = row[column.field];
		const valueGetter = column.valueGetter as ((value: unknown, row: Row) => unknown) | undefined;
		const valueFormatter = column.valueFormatter as ((value: unknown, row: Row) => unknown) | undefined;
		const value = valueGetter ? valueGetter(rawValue, row) : rawValue;

		return valueFormatter ? valueFormatter(value, row) : value;
	};

	const formatValue = (value: unknown) => {
		if (Array.isArray(value)) return value.join(", ");
		if (value === null || value === undefined || value === "") return "";
		return String(value);
	};

	const mobileColumns: GridColDef<Row>[] = [
		{
			field: "__mobileSummary",
			headerName: "Détails",
			flex: 1,
			minWidth: 190,
			sortable: false,
			filterable: false,
			valueGetter: (_, row) => columns[0] ? formatValue(getColumnValue(row, columns[0])) : "",
			renderCell: (params) => {
				const [titleColumn, ...detailColumns] = columns;
				const title = titleColumn ? formatValue(getColumnValue(params.row, titleColumn)) : "";

				return (
					<Box sx={{ display: "flex", flexDirection: "column", justifyContent: "center", minWidth: 0, py: 1 }}>
						<Typography variant="body1" color="text.primary" sx={{ fontWeight: 700, lineHeight: 1.35 }}>
							{title}
						</Typography>
						{detailColumns.map((column) => {
							const value = formatValue(getColumnValue(params.row, column));
							if (!value) return null;

							const isQuantityField = column.field === 'quantity';

							return (
								<Typography
									key={column.field}
									variant="body2"
									color="text.secondary"
									sx={{
										display: "-webkit-box",
										lineHeight: 1.3,
										overflow: "hidden",
										WebkitBoxOrient: "vertical",
										WebkitLineClamp: 2,
										fontWeight: isQuantityField ? 'bold' : 'normal',
										color: isQuantityField
											? (theme) => getInventoryDisplayColor(Number(value), theme)
											: "text.secondary"
									}}
								>
									{column.headerName ? `${column.headerName}: ` : ""}
									{value}
								</Typography>
							);
						})}
					</Box>
				);
			},
		},
	];

	const selectedColumns = useCompactLayout ? mobileColumns : columns;

	const hasGlobalActions = actions || onEditClick || onDeleteClick;
	const gridColumns = hasGlobalActions ? [...selectedColumns, actionColumn] : selectedColumns;

	return (
		<Box sx={{
			width: "100%",
			minWidth: 0,
			"& .MuiDataGrid-cell": { alignItems: "center", display: "flex" },
			...(useCompactLayout && {
				borderLeft: 0,
				borderRight: 0,
				"& .MuiDataGrid-cell": { px: 1 },
				"& .MuiDataGrid-columnHeader": { px: 1 },
				"& .MuiDataGrid-row": { minHeight: "76px !important" },
			}),
			...sx,
		}}>
			{(showSearch || onAddClick) && (
				<Box sx={{ mb: 2, display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 2 }}>
					<Box sx={{ flexGrow: 1 }}>
						{showSearch && (
							<SearchBar value={searchValue} onChange={onSearchChange ?? (() => { })} />
						)}
					</Box>
					{onAddClick && (
						<AddButton label={addLabel || "Ajouter"} onClick={onAddClick} />
					)}
				</Box>
			)}

			<Box sx={{ height: 600 }}>
				<DataGrid
					rows={rows}
					columns={gridColumns}
					onRowClick={onRowClick}
					disableColumnMenu={disableColumnMenu ?? useCompactLayout}
					getRowHeight={getRowHeight ?? (() => useCompactLayout ? "auto" : null)}
					initialState={initialState ?? {
						pagination: {
							paginationModel: {
								page: 0,
								pageSize: useCompactLayout ? 5 : 10,
							},
						},
					}}
					pageSizeOptions={pageSizeOptions ?? [5, 10, 25]}
					{...props}
				/>
			</Box>
		</Box>
	);
}
