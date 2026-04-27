import { createTheme, ThemeProvider } from "@mui/material/styles";
import {
	DataGrid,
	type DataGridProps,
	type GridColDef,
	type GridRowsProp,
} from "@mui/x-data-grid";

const darkTheme = createTheme({
	palette: {
		mode: "dark",
	},
});

interface CustomDataGridProps extends DataGridProps {
	rows: GridRowsProp;
	columns: GridColDef[];
}

export function CustomDataGrid({ ...props }: CustomDataGridProps) {
	return (
		<ThemeProvider theme={darkTheme}>
			<div>
				<DataGrid {...props} />
			</div>
		</ThemeProvider>
	);
}
