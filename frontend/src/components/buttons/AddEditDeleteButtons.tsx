import type { MouseEvent } from "react";
import { Box, Button, IconButton, Tooltip, type ButtonProps, type SxProps, type Theme } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import CloseIcon from '@mui/icons-material/Close';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';

interface BaseButtonProps extends ButtonProps {
    label: string;
    icon?: React.ReactElement;
}

const getOutlinedActionStyles = (tokenPath: string): SxProps<Theme> => ({
    color: tokenPath,
    borderColor: tokenPath,
    "&:hover": {
        borderColor: tokenPath,
        color: tokenPath,
        boxShadow: `inset 0 0 0 2px ${tokenPath}`,
        backgroundColor: "rgba(211, 47, 47, 0.2)",
    }
});

const getContainedActionStyles = (bgToken: string, textToken: string): SxProps<Theme> => ({
    bgcolor: bgToken,
    color: textToken,
    "&:hover": {
        opacity: 0.9,
        bgcolor: bgToken
    }
});


function BaseButton({ label, onClick, variant, icon, sx, ...props }: BaseButtonProps) {
    return (
        <Button
            {...props}
            variant={variant}
            color="primary"
            onClick={onClick}
            sx={sx}
            startIcon={icon || props.startIcon}
        >
            {label}
        </Button>
    );
}

export function CancelButton(props: Omit<BaseButtonProps, "icon">) {
    return (
        <BaseButton
            {...props}
            icon={<CloseIcon />}
            variant="outlined"
            sx={getOutlinedActionStyles("actionButtons.cancel.bg")}
        />
    );
}

export function PreviousButton(props: Omit<BaseButtonProps, "icon">) {
    return (
        <BaseButton
            {...props}
            icon={<ArrowBackIcon />}
            variant="outlined"
            sx={getOutlinedActionStyles("actionButtons.cancel.bg")}
        />
    );
}

export function ConfirmButton({ startIcon = <AddIcon />, ...props }: BaseButtonProps) {
    return (
        <BaseButton
            {...props}
            startIcon={startIcon}
            variant="contained"
            sx={getContainedActionStyles("actionButtons.confirm.bg", "actionButtons.confirm.text")}
        />
    );
}

export function AddButton(props: Omit<BaseButtonProps, "icon">) {
    return (
        <BaseButton
            {...props}
            icon={<AddIcon />}
            variant="contained"
            sx={getContainedActionStyles("actionButtons.add.bg", "actionButtons.add.text")}
        />
    );
}

export function EditButton(props: Omit<BaseButtonProps, "icon">) {
    return (
        <BaseButton
            {...props}
            icon={<EditIcon />}
            variant="contained"
            sx={getContainedActionStyles("actionButtons.edit.bg", "actionButtons.edit.text")}
        />
    );
}

export function DeleteButton(props: Omit<BaseButtonProps, "icon">) {
    return (
        <BaseButton
            {...props}
            icon={<DeleteIcon />}
            variant="contained"
            sx={getContainedActionStyles("actionButtons.delete.bg", "actionButtons.delete.text")}
        />
    );
}


export type RowActionButtonType = "edit" | "delete";

export interface RowActionButton<Row> {
    type: RowActionButtonType;
    ariaLabel?: string;
    tooltip?: string;
    onClick?: (row: Row) => void;
}

interface RowActionButtonsProps<Row> {
    row: Row;
    actions: RowActionButton<Row>[];
    compact?: boolean;
}

interface IconActionButtonProps {
    ariaLabel: string;
    compact?: boolean;
    color?: string;
    icon: React.ReactElement;
    onClick: (event: MouseEvent<HTMLButtonElement>) => void;
    tooltip: string;
}

function IconActionButton({
    ariaLabel,
    compact = false,
    color,
    icon,
    onClick,
    tooltip,
}: IconActionButtonProps) {
    const sizeDimension = compact ? 44 : 36;

    return (
        <Tooltip title={tooltip}>
            <IconButton
                aria-label={ariaLabel}
                size={compact ? "medium" : "small"}
                onClick={onClick}
                sx={{
                    width: sizeDimension,
                    height: sizeDimension,
                    color: color,
                    "&:hover": {
                        bgcolor: "action.hover"
                    }
                }}
            >
                {icon}
            </IconButton>
        </Tooltip>
    );
}

export function IconEditButton(props: Omit<IconActionButtonProps, "color" | "icon">) {
    return (
        <IconActionButton
            {...props}
            color="actionButtons.edit.bg"
            icon={<EditIcon fontSize="small" />}
        />
    );
}

export function IconDeleteButton(props: Omit<IconActionButtonProps, "color" | "icon">) {
    return (
        <IconActionButton
            {...props}
            color="actionButtons.delete.bg"
            icon={<DeleteIcon fontSize="small" />}
        />
    );
}

const ROW_ACTION_DEFAULTS: Record<RowActionButtonType, { ariaLabel: string; tooltip: string }> = {
    delete: { ariaLabel: "Supprimer", tooltip: "Supprimer" },
    edit: { ariaLabel: "Modifier", tooltip: "Modifier" },
};

export function RowActionButtons<Row>({
    row,
    actions,
    compact = false,
}: RowActionButtonsProps<Row>) {
    const handleClick = (
        event: MouseEvent<HTMLButtonElement>,
        action: RowActionButton<Row>,
    ) => {
        event.stopPropagation();
        action.onClick?.(row);
    };

    return (
        <Box sx={{ display: "flex", alignItems: "center", gap: 1, height: "100%" }}>
            {actions.map((action) => {
                const defaults = ROW_ACTION_DEFAULTS[action.type];
                const buttonProps = {
                    ariaLabel: action.ariaLabel ?? defaults.ariaLabel,
                    compact,
                    onClick: (event: MouseEvent<HTMLButtonElement>) => handleClick(event, action),
                    tooltip: action.tooltip ?? defaults.tooltip,
                };

                return action.type === "delete"
                    ? <IconDeleteButton key={action.type} {...buttonProps} />
                    : <IconEditButton key={action.type} {...buttonProps} />;
            })}
        </Box>
    );
}